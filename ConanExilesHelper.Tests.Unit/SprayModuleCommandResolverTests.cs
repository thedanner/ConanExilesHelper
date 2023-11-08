using Discord;
using FakeItEasy;
using FluentAssertions;
using ConanExilesHelper.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ConanExilesHelper.Tests.Unit;

[TestFixture]
public class SprayModuleCommandResolverTests
{
    private const string UrlExample = "https://example.com/sourceImage.jpg";

    private SprayModuleCommandResolver _resolver;

    [SetUp]
    public void SetUp()
    {
        var serviceCollection = new ServiceCollection()
            .AddLogging(builder => builder.AddDebug());

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        _resolver = new SprayModuleCommandResolver(
            loggerFactory.CreateLogger<SprayModuleCommandResolver>());
    }

    [Test]
    public void Resolve_NullMessaage_ThrowsAne()
    {
        // Arrange


        // Act
        Action call = () => _resolver.TryResolve(null, null, null, out var result);

        // Assert
        call.Should().Throw<ArgumentNullException>().Where(ex => ex.ParamName == "message");
    }

    // Supported scenarios:
    // 1. Message content starts with a URL.
    // 2. Message content starts with a filename and URL.
    // 3. Message content starts with a filename and attachment.
    // 4. Message content starts with a filename and is a reply to a message with only a URL as its content.
    // 5. Message content starts with a filename and is a reply to a message with an attachement.
    // 6. Message content is empty and has an attachment.
    // 7. Message content is empty and is a reply to a message with an attachment.
    // 8. Message content is empty and is a reply to a message with only a URL as its content.

    [Test]
    public void Resolve_EmptyAndNullValues_ReturnsNull()
    {
        // Arrange
        var message = A.Fake<IUserMessage>();

        // Act
        var success = _resolver.TryResolve(null, null, message, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Test]
    public void Resolve_NothingProvided_ReturnsNull()
    {
        // Arrange
        var message = A.Fake<IUserMessage>();

        // Act
        var success = _resolver.TryResolve(null, null, message, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Test]
    public void Resolve_Arg1IsUrl_ReturnsExpectedValue()
    {
        // 1. Message content starts with a URL.

        // Arrange
        var message = A.Fake<IUserMessage>();

        // Act
        var success = _resolver.TryResolve(UrlExample, null, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(UrlExample);
        result.FileName.Should().BeNull();
    }

    [Test]
    [TestCase("http")]
    [TestCase("HTTP")]
    [TestCase("HTtp")]
    [TestCase("https")]
    [TestCase("HTTPs")]
    [TestCase("HTtps")]
    public void Resolve_Arg1HasSupportedProtocol_ReturnsExpectedValue(string protocol)
    {
        // Arrange
        var message = A.Fake<IUserMessage>();
        var url = $"{protocol}://example.com/";

        // Act
        var success = _resolver.TryResolve(url, null, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(url.ToLower());
        result.FileName.Should().BeNull();
    }

    [Test]
    [TestCase("ftp")]
    [TestCase("sftp")]
    [TestCase("net.tcp")]
    public void Resolve_Arg1HasUnsupportedProtocol_ReturnsExpectedValue(string protocol)
    {
        // Arrange
        var message = A.Fake<IUserMessage>();
        var url = $"{protocol}://example.com";

        // Act
        var success = _resolver.TryResolve(url, null, message, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Test]
    public void Resolve_Arg1IsFileNameArg2IsUrl_ReturnsExpectedValue()
    {
        // 2. Message content starts with a filename and URL.

        // Arrange
        const string givenFileName = "given.jpg";
        var message = A.Fake<IUserMessage>();

        // Act
        var success = _resolver.TryResolve(givenFileName, UrlExample, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(UrlExample);
        result.FileName.Should().Be(givenFileName);
    }

    [Test]
    public void Resolve_NeitherArgIsUrl_ReturnsNull()
    {
        // Arrange
        var message = A.Fake<IUserMessage>();

        // Act
        var success = _resolver.TryResolve("not a URL", "not a URL", message, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Test]
    [TestCase(null)]
    [TestCase("some value")]
    public void Resolve_Arg1IsFileNameWithAttachment_ReturnsExpectedValue(string arg2)
    {
        // 3. Message content starts with a filename and attachment.

        // Arrange
        const string sourceImageUrl = "https://example.com/proxy/src.jpg";
        const string givenFileName = "given.jpg";

        var attachment = A.Fake<IAttachment>();
        A.CallTo(() => attachment.Filename).Returns("attachment.jpg");
        A.CallTo(() => attachment.Url).Returns("https://example.com/image.jpg");
        A.CallTo(() => attachment.ProxyUrl).Returns(sourceImageUrl);

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.Attachments).Returns(new List<IAttachment>() { attachment }.AsReadOnly());

        // Act
        var success = _resolver.TryResolve(givenFileName, arg2, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(sourceImageUrl);
        result.FileName.Should().Be(givenFileName);
    }

    [Test]
    [TestCase(null)]
    [TestCase("some value")]
    public void Resolve_Arg1IsFileNameReferencedMessageWithUrl_ReturnsExpectedValue(string arg2)
    {
        // 4. Message content starts with a filename and is a reply to a message with only a URL as its content.

        // Arrange
        const string givenFileName = "given.jpg";

        var referencedMessage = A.Fake<IUserMessage>();
        A.CallTo(() => referencedMessage.Content).Returns(UrlExample);

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.ReferencedMessage).Returns(referencedMessage);

        // Act
        var success = _resolver.TryResolve(givenFileName, arg2, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(UrlExample);
        result.FileName.Should().Be(givenFileName);
        A.CallTo(() => message.Attachments).MustHaveHappened();
    }

    [Test]
    [TestCase(null)]
    [TestCase("some value")]
    public void Resolve_Arg1IsFileNameReferencedMessageWithAttachment_ReturnsExpectedValue(string arg2)
    {
        // 5. Message content starts with a filename and is a reply to a message with an attachement.

        // Arrange
        const string givenFileName = "given.jpg";
        const string sourceImageUrl = "https://example.com/proxy/src.jpg";

        var attachment = A.Fake<IAttachment>();
        A.CallTo(() => attachment.Filename).Returns("attachment.jpg");
        A.CallTo(() => attachment.Url).Returns("https://example.com/image.jpg");
        A.CallTo(() => attachment.ProxyUrl).Returns(sourceImageUrl);

        var referencedMessage = A.Fake<IUserMessage>();
        A.CallTo(() => referencedMessage.Attachments).Returns(new List<IAttachment>() { attachment }.AsReadOnly());

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.ReferencedMessage).Returns(referencedMessage);

        // Act
        var success = _resolver.TryResolve(givenFileName, arg2, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(sourceImageUrl);
        result.FileName.Should().Be(givenFileName);
        A.CallTo(() => message.Attachments).MustHaveHappened();
    }

    [Test]
    public void Resolve_ReferencedMessageWithNoImage_ReturnsNull()
    {
        // Arrange
        var referencedMessage = A.Fake<IUserMessage>();
        A.CallTo(() => referencedMessage.Content).Returns("some content");

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.ReferencedMessage).Returns(referencedMessage);

        // Act
        var success = _resolver.TryResolve(null, null, message, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
        A.CallTo(() => message.Attachments).MustHaveHappened();
        A.CallTo(() => referencedMessage.Content).MustHaveHappened();
        A.CallTo(() => referencedMessage.Attachments).MustHaveHappened();
    }

    [Test]
    public void Resolve_EmptyMessageWithAttachment_ReturnsExpectedValue()
    {
        // 6. Message content is empty and has an attachment.

        // Arrange
        const string sourceImageUrl = "https://example.com/proxy/src.jpg";

        var attachment = A.Fake<IAttachment>();
        A.CallTo(() => attachment.Filename).Returns("attachment.jpg");
        A.CallTo(() => attachment.Url).Returns("https://example.com/image.jpg");
        A.CallTo(() => attachment.ProxyUrl).Returns(sourceImageUrl);

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.Attachments).Returns(new List<IAttachment>() { attachment }.AsReadOnly());

        // Act
        var success = _resolver.TryResolve(null, null, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(sourceImageUrl);
        result.FileName.Should().BeNull();
    }

    [Test]
    public void Resolve_ReferencedMessageWithUrl_ReturnsExpectedValue()
    {
        // 7. Message content is empty and is a reply to a message with only a URL as its content.

        // Arrange
        var referencedMessage = A.Fake<IUserMessage>();
        A.CallTo(() => referencedMessage.Content).Returns(UrlExample);

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.ReferencedMessage).Returns(referencedMessage);

        // Act
        var success = _resolver.TryResolve(null, null, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(UrlExample);
        result.FileName.Should().BeNull();
        A.CallTo(() => message.Attachments).MustHaveHappened();
    }

    [Test]
    public void Resolve_ReferencedMessageWithAttachment_ReturnsExpectedValue()
    {
        // 8. Message content is empty and is a reply to a message with an attachment.

        // Arrange
        const string sourceImageUrl = "https://example.com/proxy/src.jpg";

        var attachment = A.Fake<IAttachment>();
        A.CallTo(() => attachment.Filename).Returns("attachment.jpg");
        A.CallTo(() => attachment.Url).Returns("https://example.com/image.jpg");
        A.CallTo(() => attachment.ProxyUrl).Returns(sourceImageUrl);

        var referencedMessage = A.Fake<IUserMessage>();
        A.CallTo(() => referencedMessage.Attachments).Returns(new List<IAttachment>() { attachment }.AsReadOnly());

        var message = A.Fake<IUserMessage>();
        A.CallTo(() => message.ReferencedMessage).Returns(referencedMessage);

        // Act
        var success = _resolver.TryResolve(null, null, message, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.SourceImageUri.AbsoluteUri.Should().Be(sourceImageUrl);
        result.FileName.Should().BeNull();
        A.CallTo(() => message.Attachments).MustHaveHappened();
    }
}
