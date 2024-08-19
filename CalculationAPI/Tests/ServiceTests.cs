using Xunit;
using CalculationAPI.Services;
using System;

public class ServiceTests
{
    private readonly ExpressionEvaluator _evaluator;

    public ServiceTests()
    {
        _evaluator = new ExpressionEvaluator();
    }

    [Fact]
    public void AdditionTest()
    {
        var result = _evaluator.Evaluate("3+5");
        Assert.Equal(8, result);
    }

    [Fact]
    public void SubractionTest()
    {
        var result = _evaluator.Evaluate("10-4");
        Assert.Equal(6, result);
    }

    [Fact]
    public void MultiplicationTest()
    {
        var result = _evaluator.Evaluate("7*6");
        Assert.Equal(42, result);
    }

    [Fact]
    public void DivisionTest()
    {
        var result = _evaluator.Evaluate("8/2");
        Assert.Equal(4, result);
    }

    [Fact]
    public void ExponantiationTest()
    {
        var result = _evaluator.Evaluate("2^3");
        Assert.Equal(8, result);
    }

    [Fact]
    public void ComplexOperationTest()
    {
        var result = _evaluator.Evaluate("3+5*2-4/2");
        Assert.Equal(10, result);
    }

    [Fact]
    public void ParserTest()
    {
        var result = _evaluator.Evaluate("(3+5)*2");
        Assert.Equal(16, result);
    }

    [Fact]
    public void ParserTest2()
    {
        var result = _evaluator.Evaluate("((2+3)*2)^2");
        Assert.Equal(100, result);
    }

    
}
