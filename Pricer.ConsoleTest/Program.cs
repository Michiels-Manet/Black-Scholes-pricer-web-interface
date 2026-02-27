using Pricer.Numerics;
using System;
using static Pricer.Numerics.BlackScholes;

// Testprogramma om de Black-Scholes formule te gebruiken en de resultaten te tonen
double S = 100.0;
double K = 100.0;
double r = 0.05;
double q = 0.0;
double T = 1.0;
double sigma = 0.20;

var call = new BlackScholes(OptionType.Call, r, T, sigma, K, S, q);
var put = new BlackScholes(OptionType.Put, r, T, sigma, K, S, q);

double callPrice = call.Price();
double putPrice = put.Price();

double callvega = call.Vega();
double putvega = put.Vega();

double leftSide = callPrice - putPrice;
double rightSide = S * Math.Exp(-q * T) - K * Math.Exp(-r * T);

Console.WriteLine($"Call price   = {callPrice}");
Console.WriteLine($"Put price    = {putPrice}");
Console.WriteLine($"Call Vega : {callvega}");
Console.WriteLine($"Put Vega : {putvega}");
Console.WriteLine($"Call - Put   = {leftSide}");
Console.WriteLine($"Parity RHS   = {rightSide}");
Console.WriteLine($"Difference   = {Math.Abs(leftSide - rightSide)}");

// Testen van de implied volatility calculator
double marketPrice = 10.450583572185565;

double impliedVol = ImpliedVolatilityCalculator.Compute(
    OptionType.Call,
    marketPrice,
    0.05,
    1.0,
    100.0,
    100.0);

Console.WriteLine($"Implied vol = {impliedVol}");

double optionPrice = 100.0 * 0.20 / Math.Sqrt(2.0 * Math.PI);

double sigmaB = ImpliedVolatilityCalculator.BachelierImpliedVolATM(
    optionPrice,
    100.0,
    1.0,
    0.05);

Console.WriteLine($"Bachelier ATM implied vol = {sigmaB}");


