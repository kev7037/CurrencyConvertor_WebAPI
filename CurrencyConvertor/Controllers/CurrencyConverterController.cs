using CurrencyConvertor.Models;
using CurrencyConvertor.Services.Imp;
using CurrencyConvertor.Services.Int;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConvertor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ICurrencyConverter _currencyConverter;

        private static readonly CurrencyConverterModel[] ExchangePaths = new[]
        {
            new CurrencyConverterModel
            {
                ExchangeFrom = "USD",
                ExchangeTo = "CAD",
                ExchangeRate = (float) 1.34
            },
            new CurrencyConverterModel
            {
                ExchangeFrom = "CAD",
                ExchangeTo = "GBP",
                ExchangeRate = (float) 0.58
            },
            new CurrencyConverterModel
            {
                ExchangeFrom = "USD",
                ExchangeTo = "EUR",
                ExchangeRate = (float) 0.86
            }
        };

        private readonly ILogger<CurrencyConverterController> _logger;

        public CurrencyConverterController(ILogger<CurrencyConverterController> logger,
                                           ICurrencyConverter currencyConverter)
        {
            _logger = logger;
            _currencyConverter = currencyConverter;
        }

        [HttpGet(Name = "GetCurrencyPaths")]
        public IActionResult GetCurrencyPaths()
        {
            _logger.LogInformation("Currency paths retrieved");
            return Ok(ExchangePaths);
        }

        [HttpPost("configure")]
        public IActionResult ConfigureConversionRates([FromBody] IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            try
            {
                _currencyConverter.ClearConfiguration();
                _currencyConverter.UpdateConfiguration(conversionRates);
                return Ok("Configuration updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error configuring conversion rates: {ex.Message}");
            }
        }

        [HttpGet("convert")]
        public IActionResult ConvertCurrency([FromQuery] string fromCurrency, [FromQuery] string toCurrency, [FromQuery] double amount)
        {
            try
            {
                double result = _currencyConverter.Convert(fromCurrency, toCurrency, amount);
                return Ok($"Converted amount: {result}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Error converting currency: {ex.Message}");
            }
        }
    }
}
