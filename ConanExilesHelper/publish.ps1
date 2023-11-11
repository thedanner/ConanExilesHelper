Set-Location $PSScriptRoot

$Runtime = 'win-x64'
$Framework = 'net7.0'
$Configuration = 'Release'
$ServiceName = "ConanExilesHelper"
$ServiceDescription = "Conan Exiles Helper bot service"

$DeployDir = "dist"
$ExeName = "ConanExilesHelper.exe"

$TestProjectDirs = @(
    Join-Path ".." "ConanExilesHelper.Tests.Unit"
)


$CurrentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$IsAdmin = $CurrentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

$Service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($null -eq $Service)
{
    if (-not $IsAdmin)
    {
        Write-Error "The bot's service is not installed. Please run this script with administrator permissions to create it."
        exit 1
    }

    Write-Output "The bot's service will be installed after a successful build."
}


dotnet clean --nologo --configuration $Configuration
dotnet restore --nologo --runtime $Runtime
dotnet build --nologo --framework $Framework --runtime $Runtime --self-contained --configuration $Configuration --no-restore

foreach ($TestProjectDir in $TestProjectDirs)
{
    Push-Location $TestProjectDir

    dotnet restore --nologo --runtime $Runtime
    dotnet build --nologo --framework $Framework --runtime $Runtime --self-contained --configuration $Configuration --no-restore
    dotnet test --nologo --no-restore --no-build --framework $Framework --runtime $Runtime --configuration $Configuration

    Pop-Location
}


if ($LASTEXITCODE -ne 0)
{
    Write-Warning "Tests failed; cannot publish."
    exit $LASTEXITCODE
}

# Tests passed, we can deploy/publish/whatever you want to call it.
if ($Service.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Running)
{
    try
    {
        $Service.Stop()
        $Service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
    }
    catch [System.Management.Automation.MethodInvocationException]
    {
        Write-Error "The current user doesn't have permission to stop the bot service. Fix the service permissions,"
        Write-Error "or delete the service and run this script with administrator permissions to recreate it correctly."
        exit 2
    }
}

Remove-Item -Recurse (Join-Path $DeployDir "*") -ErrorAction SilentlyContinue
dotnet publish --nologo --no-restore --no-build  `
    --configuration $Configuration --framework $Framework --runtime $Runtime --self-contained true `
    --output dist

if ($Service)
{
    $Service.Start()
}
else
{
    if ($IsAdmin)
    {
        $BinPath = (Get-Item (Join-Path $DeployDir $ExeName)).FullName
        $Service = New-Service -Name $ServiceName -Description $ServiceDescription `
            -BinaryPathName $BinPath -StartupType Automatic -DependsOn "TcpIp"
        $Service.Start()

        $Sid = (wmic useraccount where name=`'$([Environment]::UserName)`' get sid)[2].Trim()
        $CurrentSD = (sc.exe sdshow "$ServiceName")[1].Trim()

        # Since SubInACL isn't hosted anymore, do the same thing it did by hand.

        # http://woshub.com/set-permissions-on-windows-service/ was helpful here.
        # Add to the discretionary list (starts with "D:"):
        $SdToAdd = "(A;;RPWPDT;;;$Sid)"
        #               ^^^^^^ RP = SERVICE_START, WP = SERVICE_STOP, DT = SERVICE_PAUSE_CONTINUE
        #            ^ A = Allow (D = Deny)

        if ($CurrentSd.Contains("S:("))
        {
            $NewSd = $CurrentSD -replace 'S:\(', ($SdToAdd + "S:(")
        }
        else
        {
            $NewSD = $CurrentSD + $SdToAdd
        }

        sc.exe sdset "$ServiceName" "$NewSd"
    }
    else
    {
        Write-Error "Please re-run this script with administrator permissions to properly install and configure the bot service."
        exit 3
    }
}
