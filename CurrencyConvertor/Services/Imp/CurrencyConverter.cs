using CurrencyConvertor.Services.Int;
using System.Security.Cryptography.X509Certificates;

namespace CurrencyConvertor.Services.Imp
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly object lockObject = new object();
        private Dictionary<string, Dictionary<string, double>> conversionGraph;

        private static CurrencyConverter instance;

        public CurrencyConverter()
        {
            ClearConfiguration();
        }

        public static CurrencyConverter Instance
        {
            get
            {
                if (instance == null)
                    lock (typeof(CurrencyConverter))
                        if (instance == null)
                            instance = new CurrencyConverter();

                return instance;
            }
        }

        public void ClearConfiguration()
        {
            lock (lockObject)
                conversionGraph = new Dictionary<string, Dictionary<string, double>>();
        }

        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            lock (lockObject)
            {
                foreach (var rate in conversionRates)
                {
                    string fromCurrency = rate.Item1;
                    string toCurrency = rate.Item2;
                    double rateValue = rate.Item3;

                    if (!conversionGraph.ContainsKey(fromCurrency))
                    {
                        conversionGraph[fromCurrency] = new Dictionary<string, double>();
                    }

                    conversionGraph[fromCurrency][toCurrency] = rateValue;

                    // Add reverse conversion for indirect paths
                    if (!conversionGraph.ContainsKey(toCurrency))
                    {
                        conversionGraph[toCurrency] = new Dictionary<string, double>();
                    }

                    conversionGraph[toCurrency][fromCurrency] = 1 / rateValue;
                }
            }
        }

        public double FindConversionPath(string fromCurrency, string toCurrency, double amount, List<string> path = null, HashSet<string> visitedCurrencies = null)
        {
            if (path == null)
            {
                path = new List<string>();
            }

            if (visitedCurrencies == null)
            {
                visitedCurrencies = new HashSet<string>();
            }

            if (conversionGraph.ContainsKey(fromCurrency))
            {
                // Check if there's a direct path from fromCurrency to toCurrency
                if (conversionGraph[fromCurrency].ContainsKey(toCurrency))
                {
                    Console.WriteLine($"Found direct path: {fromCurrency} -> {toCurrency}");
                    path.Add(fromCurrency);
                    path.Add(toCurrency);
                    return amount * conversionGraph[fromCurrency][toCurrency];
                }
                else
                {
                    foreach (var nextCurrency in conversionGraph[fromCurrency].Keys)
                    {
                        if (!visitedCurrencies.Contains(nextCurrency))
                        {
                            visitedCurrencies.Add(nextCurrency);
                            path.Add(fromCurrency);
                            double result = FindConversionPath(nextCurrency, toCurrency, amount * conversionGraph[fromCurrency][nextCurrency], path, visitedCurrencies);

                            if (result != -1)
                            {
                                Console.WriteLine($"Found indirect path: {string.Join(" -> ", path)}");
                                return result;
                            }

                            // Backtrack
                            path.RemoveAt(path.Count - 1);
                        }
                    }
                }
            }

            return -1; // Indicate that the path was not found
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            lock (lockObject)
            {
                if (!conversionGraph.ContainsKey(fromCurrency) || !conversionGraph.ContainsKey(toCurrency))
                {
                    throw new ArgumentException("Invalid currency codes");
                }


                List<string> path = new List<string>();
                double result = FindConversionPath(fromCurrency, toCurrency, amount, path);

                if (result == -1)
                {
                    Console.WriteLine($"No conversion path found from {fromCurrency} to {toCurrency}");
                }
                else
                {
                    Console.WriteLine($"Final result: {result}");
                }

                return result;
            }
        }
    }
}
