using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests.Domain;

public class ValueObjectTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(7.5)]
    public void Score_NonNegative_IsCreated(double value)
    {
        Assert.Equal((decimal)value, Score.Of((decimal)value).Value);
    }

    [Fact]
    public void Score_Negative_Throws()
    {
        Assert.Throws<InvalidValueException>(() => Score.Of(-0.01m));
    }

    [Fact]
    public void Score_IsComparedByValue()
    {
        Assert.Equal(Score.Of(5), Score.Of(5));
        Assert.NotEqual(Score.Of(5), Score.Of(6));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void MaxScoreAndWeight_ZeroOrNegative_Throw(int value)
    {
        Assert.Throws<InvalidValueException>(() => MaxScore.Of(value));
        Assert.Throws<InvalidValueException>(() => Weight.Of(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void PersonName_Blank_Throws(string? value)
    {
        Assert.Throws<InvalidValueException>(() => PersonName.Of(value));
    }

    [Fact]
    public void PersonName_TooLong_Throws()
    {
        Assert.Throws<InvalidValueException>(() => PersonName.Of(new string('x', PersonName.MaxLength + 1)));
    }
}
