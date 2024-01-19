using CurrencyConvertor.Services.Int;
using System.Collections.Concurrent;

namespace CurrencyConvertor.Services.Imp
{
    public class CurrencyConverter_Dijkstra : ICurrencyConverter
    {
        private readonly object lockObject = new object();
        private Lazy<Dictionary<string, Dictionary<string, double>>> conversionGraph;
        private readonly ConcurrentDictionary<string, double> conversionCache = new ConcurrentDictionary<string, double>();

        private static CurrencyConverter_Dijkstra instance;

        public static CurrencyConverter_Dijkstra Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(CurrencyConverter_Dijkstra))
                    {
                        if (instance == null)
                        {
                            instance = new CurrencyConverter_Dijkstra();
                        }
                    }
                }

                return instance;
            }
        }

        public CurrencyConverter_Dijkstra()
        {
            ClearConfiguration();
            conversionGraph = new Lazy<Dictionary<string, Dictionary<string, double>>>(InitializeConversionGraph);
        }

        private Dictionary<string, Dictionary<string, double>> InitializeConversionGraph()
        {
            Dictionary<string, Dictionary<string, double>> graph = new Dictionary<string, Dictionary<string, double>>();

            // Initialize the graph with the provided conversion rates

            return graph;
        }

        public void ClearConfiguration()
        {
            lock (lockObject)
            {
                // Clear any existing configuration
                conversionGraph = new Lazy<Dictionary<string, Dictionary<string, double>>>(InitializeConversionGraph);
            }
        }

        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            lock (lockObject)
            {
                // Update the conversion graph with new rates
                foreach (var rate in conversionRates)
                {
                    string fromCurrency = rate.Item1;
                    string toCurrency = rate.Item2;
                    double rateValue = rate.Item3;

                    if (!conversionGraph.Value.ContainsKey(fromCurrency))
                    {
                        conversionGraph.Value[fromCurrency] = new Dictionary<string, double>();
                    }

                    conversionGraph.Value[fromCurrency][toCurrency] = rateValue;

                    // Add reverse conversion for indirect paths
                    if (!conversionGraph.Value.ContainsKey(toCurrency))
                    {
                        conversionGraph.Value[toCurrency] = new Dictionary<string, double>();
                    }

                    conversionGraph.Value[toCurrency][fromCurrency] = 1 / rateValue;
                }
            }
        }

        private Dictionary<string, double> Dijkstra(string startCurrency)
        {
            Dictionary<string, double> distances = new Dictionary<string, double>();
            HashSet<string> visitedCurrencies = new HashSet<string>();

            foreach (var currency in conversionGraph.Value.Keys)
            {
                distances[currency] = double.MaxValue;
            }

            distances[startCurrency] = 0;

            while (true)
            {
                string currentCurrency = GetMinDistanceCurrency(distances, visitedCurrencies);

                if (currentCurrency == null)
                {
                    break;
                }

                visitedCurrencies.Add(currentCurrency);

                foreach (var neighbor in conversionGraph.Value[currentCurrency])
                {
                    double alternativeRoute = distances[currentCurrency] + neighbor.Value;

                    if (alternativeRoute < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alternativeRoute;
                    }
                }
            }

            return distances;
        }

        private string GetMinDistanceCurrency(Dictionary<string, double> distances, HashSet<string> visitedCurrencies)
        {
            double minDistance = double.MaxValue;
            string minDistanceCurrency = null;

            foreach (var currency in distances.Keys)
            {
                if (!visitedCurrencies.Contains(currency) && distances[currency] < minDistance)
                {
                    minDistance = distances[currency];
                    minDistanceCurrency = currency;
                }
            }

            return minDistanceCurrency;
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            lock (lockObject)
            {
                // Check if the conversion is in the cache
                string cacheKey = $"{fromCurrency}_{toCurrency}";
                if (conversionCache.TryGetValue(cacheKey, out double cachedResult))
                {
                    Console.WriteLine($"Found in cache: {fromCurrency} -> {toCurrency}");
                    return cachedResult;
                }

                Dictionary<string, double> distances = Dijkstra(fromCurrency);

                // Check if there's a direct path from fromCurrency to toCurrency
                if (conversionGraph.Value.ContainsKey(fromCurrency) && conversionGraph.Value[fromCurrency].ContainsKey(toCurrency))
                {
                    Console.WriteLine($"Found direct path: {fromCurrency} -> {toCurrency}");
                    double result = amount * conversionGraph.Value[fromCurrency][toCurrency];

                    // Cache the result
                    conversionCache[cacheKey] = result;

                    return result;
                }
                else
                {
                    // Find the shortest path and calculate the conversion
                    string intermediateCurrency = GetIntermediateCurrency(distances, toCurrency);
                    if (intermediateCurrency != null)
                    {
                        double result = amount * CalculateConversionPath(fromCurrency, intermediateCurrency, toCurrency);
                        Console.WriteLine($"Found indirect path: {fromCurrency} -> {intermediateCurrency} -> {toCurrency}");

                        // Cache the result
                        conversionCache[cacheKey] = result;

                        return result;
                    }
                }

                Console.WriteLine($"No conversion path found from {fromCurrency} to {toCurrency}");
                return -1;
            }
        }

        private string GetIntermediateCurrency(Dictionary<string, double> distances, string toCurrency)
        {
            // Find the intermediate currency with the shortest path to toCurrency
            double minDistance = double.MaxValue;
            string intermediateCurrency = null;

            foreach (var currency in distances.Keys)
            {
                if (distances[currency] < minDistance && conversionGraph.Value[currency].ContainsKey(toCurrency))
                {
                    minDistance = distances[currency];
                    intermediateCurrency = currency;
                }
            }

            return intermediateCurrency;
        }

        private double CalculateConversionPath(string fromCurrency, string intermediateCurrency, string toCurrency)
        {
            double rate1 = conversionGraph.Value[fromCurrency][intermediateCurrency];
            double rate2 = conversionGraph.Value[intermediateCurrency][toCurrency];
            return rate1 * rate2;
        }
    }

}