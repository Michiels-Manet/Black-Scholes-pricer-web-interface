using System;
using MathNet.Numerics.Distributions;

namespace Pricer.Numerics
{
    public sealed class CorrelatedBrownianMotionResult
    {
        public BrownianPath W1 { get; }
        public BrownianPath W2 { get; }
        public BrownianPath W3 { get; }

        public CorrelatedBrownianMotionResult(BrownianPath w1, BrownianPath w2, BrownianPath w3) // constructor, als we dit oproepen moeten we 3 BMpaden meegeven
        {
            W1 = w1 ?? throw new ArgumentNullException(nameof(w1));
            W2 = w2 ?? throw new ArgumentNullException(nameof(w2));
            W3 = w3 ?? throw new ArgumentNullException(nameof(w3));
        }
    }

    public sealed class IndependentBrownianMotions
    {
        public BrownianPath W1 { get; }
        public BrownianPath W2 { get; }

        public IndependentBrownianMotions(BrownianPath w1, BrownianPath w2) // constructor, als we dit oproepen moeten we 2 onafhankelijke BMpaden meegeven
        {
            W1 = w1 ?? throw new ArgumentNullException(nameof(w1));
            W2 = w2 ?? throw new ArgumentNullException(nameof(w2));
        }
    }

    public static class CorrelatedBrownianMotion // verzameling van methodes/functies
    {
        // methode GenerateIndependentPaths genereert eerst 2 onafhankelijke Brownian motions W1 en W2
        public static IndependentBrownianMotions GenerateIndependentPaths(
            double maturity,
            int numberOfSteps,
            int? seed = null)
        {
            if (maturity <= 0.0)
                throw new ArgumentException("Maturity must be strictly positive.", nameof(maturity));

            if (numberOfSteps <= 0)
                throw new ArgumentException("Number of steps must be strictly positive.", nameof(numberOfSteps));

            Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

            double dt = maturity / numberOfSteps; // Delta t = T/n
            double sqrtDt = Math.Sqrt(dt);

            double[] times = new double[numberOfSteps + 1];
            double[] w1Values = new double[numberOfSteps + 1];
            double[] w2Values = new double[numberOfSteps + 1];

            // beginwaarden (op tijdstip 0)
            times[0] = 0.0;
            w1Values[0] = 0.0;
            w2Values[0] = 0.0;

            for (int i = 1; i <= numberOfSteps; i++)
            {
                // Twee onafhankelijke standaardnormale variabelen
                double z1 = Normal.Sample(rng, 0.0, 1.0);
                double z2 = Normal.Sample(rng, 0.0, 1.0);

                // Increments van W1 en W2
                double dW1 = sqrtDt * z1;
                double dW2 = sqrtDt * z2;

                times[i] = i * dt;

                // waarden van de paden updaten
                w1Values[i] = w1Values[i - 1] + dW1;
                w2Values[i] = w2Values[i - 1] + dW2;
            }

            // Brownian paden maken
            BrownianPath w1 = new BrownianPath(times, w1Values);
            BrownianPath w2 = new BrownianPath(times, w2Values);

            return new IndependentBrownianMotions(w1, w2);
        }

        // methode ConstructCorrelatedPath construeert W3 uit reeds bestaande W1 en W2
        public static BrownianPath ConstructCorrelatedPath(BrownianPath w1, BrownianPath w2, double rho)
        {
            if (w1 is null)
                throw new ArgumentNullException(nameof(w1));

            if (w2 is null)
                throw new ArgumentNullException(nameof(w2));

            if (rho < -1.0 || rho > 1.0)
                throw new ArgumentException("Correlation coefficient rho must be between -1 and 1.", nameof(rho));

            if (w1.Values.Length != w2.Values.Length)
                throw new ArgumentException("W1 and W2 must have the same number of points.");

            if (w1.Times.Length != w2.Times.Length)
                throw new ArgumentException("W1 and W2 must have the same number of time points.");

            double sqrtOneMinusRhoSquared = Math.Sqrt(1.0 - rho * rho);

            int n = w1.Values.Length - 1;
            double[] times = new double[w1.Times.Length];
            double[] w3Values = new double[w1.Values.Length];

            Array.Copy(w1.Times, times, w1.Times.Length);

            // beginwaarde van W3
            w3Values[0] = 0.0;

            for (int i = 1; i <= n; i++)
            {
                // incrementen van de reeds gegenereerde paden W1 en W2
                double dW1 = w1.Values[i] - w1.Values[i - 1];
                double dW2 = w2.Values[i] - w2.Values[i - 1];

                // Constructie van W3 uit W1 en W2
                double dW3 = rho * dW1 + sqrtOneMinusRhoSquared * dW2;

                // waarden van pad W3 updaten
                w3Values[i] = w3Values[i - 1] + dW3;
            }

            // Brownian pad maken
            BrownianPath w3 = new BrownianPath(times, w3Values);

            return w3;
        }

        // deze methode mag blijven bestaan als gecombineerde methode
        public static CorrelatedBrownianMotionResult GeneratePaths(
            double maturity,
            int numberOfSteps,
            double rho,
            int? seed = null)
        {
            var independentPaths = GenerateIndependentPaths(maturity, numberOfSteps, seed);
            BrownianPath w3 = ConstructCorrelatedPath(independentPaths.W1, independentPaths.W2, rho);

            return new CorrelatedBrownianMotionResult(independentPaths.W1, independentPaths.W2, w3);
        }

        // methode ComputeEmpiricalCorrelation neemt 2 inputs (twee BMpaden)
        public static double ComputeEmpiricalCorrelation(BrownianPath pathA, BrownianPath pathB)
        {
            if (pathA is null)
                throw new ArgumentNullException(nameof(pathA));

            if (pathB is null)
                throw new ArgumentNullException(nameof(pathB));

            if (pathA.Values.Length != pathB.Values.Length)
                throw new ArgumentException("The two paths must have the same number of points.");

            int n = pathA.Values.Length - 1;
            if (n <= 0)
                throw new ArgumentException("Paths must contain at least one increment.");

            double sumDx = 0.0; // som van alle incrementen van pad A
            double sumDy = 0.0; // som van alle incrementen van pad B

            for (int i = 1; i <= n; i++)
            {
                double dx = pathA.Values[i] - pathA.Values[i - 1];
                double dy = pathB.Values[i] - pathB.Values[i - 1];

                sumDx += dx;
                sumDy += dy;
            }

            // gemiddelde increment van elk pad
            double meanDx = sumDx / n;
            double meanDy = sumDy / n;

            double covariance = 0.0;
            double varianceX = 0.0;
            double varianceY = 0.0;

            // (co)variantie berekenen
            for (int i = 1; i <= n; i++)
            {
                double dx = (pathA.Values[i] - pathA.Values[i - 1]) - meanDx;
                double dy = (pathB.Values[i] - pathB.Values[i - 1]) - meanDy;

                covariance += dx * dy;
                varianceX += dx * dx;
                varianceY += dy * dy;
            }

            if (varianceX <= 0.0 || varianceY <= 0.0)
                throw new InvalidOperationException("Cannot compute correlation because one variance is zero.");

            return covariance / Math.Sqrt(varianceX * varianceY);
        }
    }
}