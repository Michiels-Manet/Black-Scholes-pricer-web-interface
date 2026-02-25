using MathNet.Numerics.Distributions;


namespace Pricer.Numerics;

public enum OptionType
{
    Call,
    Put
}

public class BlackScholes
{
    private OptionType option;
    private double r; // Risk-free interest rate
    private double T; // Time to maturity
    private double sigma; // Volatility
    private double K; // Strike price
    private double S; // Underlying asset price
    private double q; // Dividend yield

    public BlackScholes(
        OptionType optionType,
        double riskFreeRate,
        double timeToMaturity,
        double volatility,
        double strike,
        double underlyingPrice,
        double dividendYield = 0.0)
    {
        option = optionType;
        r = riskFreeRate;
        T = timeToMaturity;
        sigma = volatility;
        K = strike;
        S = underlyingPrice;
        q = dividendYield;
    }

    // Cumulative normal distribution
    private static double N(double x)
        => Normal.CDF(0.0, 1.0, x);

    // Normal probability density function
    private static double n(double x)
        => throw new NotImplementedException("Normal PDF not implemented;");


    // Black–Scholes price
    public double Price()
    {
        throw new NotImplementedException("Price calculation not implemented");
    }

    public double Vega() => throw new NotImplementedException("Vega not implemented");

}

// Test price compared with party at the mooonlight option calculator
// Test put call parity for consistency


public static class ImpliedVolatilityCalculator
{
    public static double Compute(
        OptionType optionType,
        double marketPrice,
        double interestRate,
        double timeToMaturity,
        double strike,
        double underlyingPrice,
        double initialGuess = 0.2,
        double tolerance = 1e-8,
        int maxIterations = 100)
    {
        throw new NotImplementedException("Implied volatility calculation not implemented");
    }

    public static double BachelierImpliedVolATM(
        double optionPrice,
        double underlyingPrice,
        double timeToMaturity,
        double interestRate)
    {
       throw new NotImplementedException("Bachelier implied volatility calculation not implemented");
    }
}