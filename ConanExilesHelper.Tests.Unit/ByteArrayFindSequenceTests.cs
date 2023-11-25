using System;
using FluentAssertions;
using ConanExilesHelper.SourceQuery;
using NUnit.Framework;

namespace ConanExilesHelper.Tests.Unit;

[TestFixture]
public class VtfSprayTests
{
    [Test]
    public void FindSequence2arg_NullSource_Throws()
    {
        // Arrange
        byte[] source = null;
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*source*");
    }


    [Test]
    public void FindSequence2arg_EmptySource_Throws()
    {
        // Arrange
        byte[] source = new byte[0];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*source*");
    }
    [Test]
    public void FindSequence4arg_NullSource_Throws()
    {
        // Arrange
        byte[] source = null;
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, 0, 100);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*source*");
    }


    [Test]
    public void FindSequence4arg_EmptySource_Throws()
    {
        // Arrange
        byte[] source = new byte[0];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, 0, 100);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*source*");
    }

    [Test]
    public void FindSequence_NullPattern_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = null;

        // Act
        Action act = () => source.FindSequence(pattern, 0, source.Length);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*pattern*");
    }

    [Test]
    public void FindSequence_EmptyPattern_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[0];

        // Act
        Action act = () => source.FindSequence(pattern, 0, source.Length);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*pattern*");
    }

    [Test]
    public void FindSequence_StartIndexTooLow_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, -1, source.Length);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*startIndex*");
    }

    [Test]
    public void FindSequence_StartIndexTooHigh_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, 2, source.Length);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*startIndex*");
    }

    [Test]
    public void FindSequence_EndIndexTooLow_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, 0, -1);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*endIndex*");
    }

    [Test]
    public void FindSequence_EndIndexTooHigh_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, 0, 2);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*endIndex*");
    }

    [Test]
    public void FindSequence_PatternBiggerThanSource_Throws()
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[2];

        // Act
        Action act = () => source.FindSequence(pattern, 0, 1);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*pattern*source*");
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(1, 0)]
    public void FindSequence_EndIndexOnOrBeforeStartIndex_Throws(int startIndex, int endIndex)
    {
        // Arrange
        byte[] source = new byte[1];
        byte[] pattern = new byte[1];

        // Act
        Action act = () => source.FindSequence(pattern, startIndex, endIndex);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*start*end*");
    }

    [Test]
    public void FindSequence_ValidSingleCharPatternAtStart_ReturnsCorrectIndex()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3 };
        byte[] pattern = new byte[] { 1 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(0);
    }

    [Test]
    public void FindSequence_ValidSingleCharPatternAtMiddle_ReturnsCorrectIndex()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3 };
        byte[] pattern = new byte[] { 2 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(1);
    }

    [Test]
    public void FindSequence_ValidSingleCharPatternAtEnd_ReturnsCorrectIndex()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3 };
        byte[] pattern = new byte[] { 3 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(2);
    }

    [Test]
    public void FindSequence_PatternNotInSource_ReturnsNegativeOne()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3 };
        byte[] pattern = new byte[] { 4 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(-1);
    }



    [Test]
    public void FindSequence_ValidMultipleCharPatternAtStart_ReturnsCorrectIndex()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3, 4, 5, 6 };
        byte[] pattern = new byte[] { 1, 2 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(0);
    }

    [Test]
    public void FindSequence_ValidMultipleCharPatternAtMiddle_ReturnsCorrectIndex()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3, 4, 5, 6 };
        byte[] pattern = new byte[] { 3, 4 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(2);
    }

    [Test]
    public void FindSequence_ValidMultipleCharPatternAtEnd_ReturnsCorrectIndex()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 3, 4, 5, 6 };
        byte[] pattern = new byte[] { 5, 6 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(4);
    }

    [Test]
    public void FindSequence_PatternCutOffAtEnd_NegativeOne()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2 };
        byte[] pattern = new byte[] { 2, 3 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(-1);
    }

    [Test]
    public void FindSequence_PartialMatchFirstFullMatchLater_NegativeOne()
    {
        // Arrange
        byte[] source = new byte[] { 1, 2, 1, 2, 3};
        byte[] pattern = new byte[] { 1, 2, 3 };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(-1);
    }

    [Test]
    public void FindSequence_ActualExample_ReturnsCorrectResult()
    {
        // Arrange
        byte[] source = new byte[] {
            254, 255, 255, 255, 222,  16,   0,   0,
              4,   0, 164,   4, 255, 255, 255, 255,
             69, 165,   0,  98,  97
        };
        byte[] pattern = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

        // Act
        var index = source.FindSequence(pattern);

        // Assert
        index.Should().Be(12);
    }
}
