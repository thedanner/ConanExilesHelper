#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR "/src"
COPY ["ConanExilesHelper/ConanExilesHelper.csproj", "ConanExilesHelper/"]
COPY ["ConanExilesHelper.Sprays/ConanExilesHelper.Sprays.csproj", "ConanExilesHelper.Sprays/"]
COPY ["ConanExilesHelper.ImageSharpExtensions/ConanExilesHelper.ImageSharpExtensions.csproj", "ConanExilesHelper.ImageSharpExtensions/"]
COPY ["ConanExilesHelper.Tests.Unit/ConanExilesHelper.Tests.Unit.csproj", "ConanExilesHelper.Tests.Unit/"]
RUN dotnet restore "ConanExilesHelper/ConanExilesHelper.csproj"
RUN dotnet restore "ConanExilesHelper.Tests.Unit/ConanExilesHelper.Tests.Unit.csproj"
COPY . .
WORKDIR "/src/ConanExilesHelper"
RUN dotnet build "ConanExilesHelper.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR "/src/ConanExilesHelper.Tests.Unit"
RUN dotnet test -c Release "ConanExilesHelper.Tests.Unit.csproj"

FROM build AS publish
WORKDIR "/src/ConanExilesHelper"
RUN dotnet publish "ConanExilesHelper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConanExilesHelper.dll"]
