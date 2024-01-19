using CurrencyConvertor.Services.Int;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

[ApiController]
[Route("[controller]")]
public class LoadTestController : ControllerBase
{
    private readonly ICurrencyConverter _currencyConverter;
    private readonly ILogger<LoadTestController> _logger;

    public LoadTestController(ILogger<LoadTestController> logger, ICurrencyConverter currencyConverter)
    {
        _logger = logger;
        _currencyConverter = currencyConverter;
    }

    [HttpPost("runLoadTest")]
    public async Task<IActionResult> RunLoadTest([FromQuery] int numberOfRequests)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            List<Task<double>> tasks = new List<Task<double>>();

            for (int i = 0; i < numberOfRequests; i++)
            {
                tasks.Add(Task.Run(async () => await ExecuteSingleLoadTestRequest()));
            }

            await Task.WhenAll(tasks);

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var averageResponseTime = tasks.Select(t => t.Result).Average();

            var resultSummary = new
            {
                NumberOfRequests = numberOfRequests,
                ElapsedTime = elapsedMilliseconds,
                AverageResponseTime = averageResponseTime
            };

            _logger.LogInformation($"Load test completed. {resultSummary}");

            return Ok(resultSummary);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error running load test: {ex.Message}");
        }
    }

    private async Task<double> ExecuteSingleLoadTestRequest()
    {
        // Simulate a single conversion request
        string fromCurrency = "CAD";
        string toCurrency = "EUR";
        double amount = 100.0;

        // Add more randomness or variation in currencies and amounts as needed

        var stopwatch = Stopwatch.StartNew();
        double result = _currencyConverter.Convert(fromCurrency, toCurrency, amount);
        stopwatch.Stop();

        _logger.LogInformation($"Load test request completed in {stopwatch.ElapsedMilliseconds} ms. Result: {result}");

        return stopwatch.ElapsedMilliseconds;
    }
}
