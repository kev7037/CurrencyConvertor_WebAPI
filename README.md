# Currency Converter WebAPI

This is a Currency Converter WebAPI developed using .NET 8.

## Goal

The primary goal of this project is to provide a configurable module that, given a list of currency exchange rates, can efficiently convert currencies to one another. The implementation utilizes the ICurrencyConverter interface, with two classes to demonstrate different conversion strategies. One class employs a recursive method for finding conversion paths eagerly, while the other uses Dijkstra's algorithm to find the shortest conversion path. The CurrencyConverter is defined as a singleton to prevent locks under heavy loads, and concurrent method calls are managed using locking.

## Sample Configuration Data

The configuration data consists of a list of currency exchange rates:

```json
[
  { "item1": "USD", "item2": "CAD", "item3": 1.34 },
  { "item1": "CAD", "item2": "GBP", "item3": 0.58 },
  { "item1": "USD", "item2": "EUR", "item3": 0.86 },
  { "item1": "GBP", "item2": "JPY", "item3": 153.20 },
  { "item1": "EUR", "item2": "JPY", "item3": 141.50 },
  { "item1": "CAD", "item2": "AUD", "item3": 1.03 },
  { "item1": "AUD", "item2": "NZD", "item3": 1.06 },
  { "item1": "USD", "item2": "JPY", "item3": 114.30 },
  { "item1": "NZD", "item2": "USD", "item3": 0.73 }
]
```

## Sample Conversion Request

To convert a specific amount from one currency to another, make a conversion request:

```json
{ "fromCurrency": "CAD", "toCurrency": "EUR", "amount": 100 }
```

## CurrencyConvertorController

The `CurrencyConvertorController` provides endpoints for interacting with the Currency Converter. Here are some key endpoints:

### GetCurrencyPaths

Retrieve information about available currency exchange paths:

- **Method:** `GET`
- **Endpoint:** `/currencyconvertor/getcurrencypaths`

### ConfigureConversionRates

Configure or update the currency exchange rates:

- **Method:** `POST`
- **Endpoint:** `/currencyconvertor/configure`
- **Request Body:** A collection of tuples representing currency conversion rates.

### ConvertCurrency

Convert a specified amount from one currency to another:

- **Method:** `GET`
- **Endpoint:** `/currencyconvertor/convert`
- **Query Parameters:** `fromCurrency`, `toCurrency`, `amount`

Example:

```http
GET /currencyconvertor/convert?fromCurrency=CAD&toCurrency=EUR&amount=100
```

# Load Test Controller

The `LoadTestController` facilitates load testing for the Currency Converter WebAPI, providing insights into the system's performance under heavy concurrent requests.

## Endpoint

### RunLoadTest

Execute a load test to simulate multiple concurrent currency conversion requests:

- **Method:** `POST`
- **Endpoint:** `/loadtest/runLoadTest`
- **Request Body:** The number of requests to simulate (`numberOfRequests`).

### Example

```http
POST /loadtest/runLoadTest
Content-Type: application/json

{
  "numberOfRequests": 1000
}
```

## Execution

The controller runs a specified number of concurrent conversion requests, measuring the elapsed time and average response time. It logs detailed information about each request, providing insights into the system's performance under load.

### Configuration

- **ICurrencyConverter:** The controller uses the same `ICurrencyConverter` interface as the main `CurrencyConvertorController`.

### Load Test Request

Each simulated load test request involves converting a fixed amount from one currency to another:

- **From Currency:** CAD
- **To Currency:** EUR
- **Amount:** 100.0

You can customize the load test request parameters in the `ExecuteSingleLoadTestRequest` method for further variation.

## Results

Upon completion, the controller returns a summary of the load test results, including:

- **Number of Requests:** The total number of simulated requests.
- **Elapsed Time:** The total time taken for all requests to complete.
- **Average Response Time:** The average time taken for each individual request.

These results can be analyzed to evaluate the Currency Converter's performance under different load conditions.

This Load Test Controller is a valuable tool for assessing the robustness and efficiency of the Currency Converter WebAPI in handling concurrent requests.

Certainly! Below is an extended section for your README that explains the flexibility provided by choosing between the `CurrencyConverter` and `CurrencyConverter_Dijkstra` implementations:

## Customizable Currency Conversion Strategies

The Currency Converter WebAPI offers flexibility in choosing the currency conversion strategy. You can opt for either the recursive method or Dijkstra's algorithm by selecting the appropriate implementation when configuring services in the `Program.cs` file.

### CurrencyConverter (Recursive Method)

To use the recursive method for finding conversion paths eagerly, uncomment the following line:

```csharp
// builder.Services.AddSingleton<ICurrencyConverter, CurrencyConverter>();
```

### CurrencyConverter_Dijkstra (Dijkstra's Algorithm)

To use Dijkstra's algorithm for finding the shortest conversion path, uncomment the following line:

```csharp
// builder.Services.AddSingleton<ICurrencyConverter, CurrencyConverter_Dijkstra>();
```

Choose the strategy that best fits your requirements. The recursive method provides an eager approach to finding conversion paths, while Dijkstra's algorithm focuses on identifying the shortest path. You can easily switch between these implementations to tailor the Currency Converter to your specific use cases.

Feel free to experiment with both methods and determine which one suits your application's needs for currency conversion. The ability to switch between strategies seamlessly allows for optimal customization based on performance and behavior considerations.


# Load Test Results

We conducted a load test with 2000 concurrent requests to evaluate the performance of the Currency Converter WebAPI using both Dijkstra's algorithm and the recursive method.

## Dijkstra's Algorithm

- **Number of Requests:** 2000
- **Elapsed Time:** 12456 ms
- **Average Response Time:** 4.1735 ms

## Recursive Method

- **Number of Requests:** 2000
- **Elapsed Time:** 5237 ms
- **Average Response Time:** 44.0415 ms

These results showcase the efficiency of Dijkstra's algorithm in handling concurrent conversion requests. With a significantly lower average response time, it outperforms the recursive method under heavy loads. Consider these results when choosing the appropriate conversion strategy based on your application's performance requirements.
