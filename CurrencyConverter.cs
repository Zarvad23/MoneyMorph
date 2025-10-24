using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MoneyMorph
{
    public sealed class CurrencyInfo
    {
        public CurrencyInfo(string code, decimal priceInUsd)
        {
            Code = code;
            PriceInUsd = priceInUsd;
        }

        public string Code { get; }

        public decimal PriceInUsd { get; set; }
    }

    public class CurrencyConverter
    {
        private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly List<CurrencyInfo> _allCurrencies = new()
        {
            new CurrencyInfo("USD", 1.00m),
            new CurrencyInfo("EUR", 1.09m),
            new CurrencyInfo("GBP", 1.28m),
            new CurrencyInfo("JPY", 0.0070m),
            new CurrencyInfo("RUB", 0.011m),
            new CurrencyInfo("CNY", 0.14m),
            new CurrencyInfo("CHF", 1.12m),
            new CurrencyInfo("CAD", 0.73m),
            new CurrencyInfo("AUD", 0.67m),
            new CurrencyInfo("TRY", 0.031m),
            new CurrencyInfo("SEK", 0.095m),
            new CurrencyInfo("NOK", 0.094m),
            new CurrencyInfo("PLN", 0.25m),
            new CurrencyInfo("INR", 0.012m),
            new CurrencyInfo("BRL", 0.20m),
            new CurrencyInfo("ZAR", 0.054m)
        };

        public DateTime LastSuccessfulUpdate { get; private set; }

        public string[] GetCurrencyCodes()
        {
            return _allCurrencies.Select(info => info.Code).ToArray();
        }

        public decimal Convert(string fromCode, string toCode, decimal amount, int decimals)
        {
            CurrencyInfo? fromCurrency = FindCurrency(fromCode);
            CurrencyInfo? toCurrency = FindCurrency(toCode);

            if (fromCurrency is null || toCurrency is null)
            {
                throw new ArgumentException("Указан неизвестный код валюты.");
            }

            decimal usdValue = amount * fromCurrency.PriceInUsd;
            decimal targetValue = toCurrency.PriceInUsd <= 0m
                ? 0m
                : usdValue / toCurrency.PriceInUsd;

            int safeDecimals = Math.Clamp(decimals, 0, 6);
            return decimal.Round(targetValue, safeDecimals, MidpointRounding.AwayFromZero);
        }

        public CurrencyInfo[] GetAllCurrencies()
        {
            return _allCurrencies
                .Select(info => new CurrencyInfo(info.Code, info.PriceInUsd))
                .ToArray();
        }

        public async Task<bool> UpdateRatesFromInternetAsync()
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync(ApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                using JsonDocument document = await JsonDocument.ParseAsync(contentStream);

                if (!document.RootElement.TryGetProperty("result", out JsonElement resultElement))
                {
                    return false;
                }

                if (!string.Equals(resultElement.GetString(), "success", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!document.RootElement.TryGetProperty("rates", out JsonElement ratesElement))
                {
                    return false;
                }

                foreach (CurrencyInfo info in _allCurrencies)
                {
                    if (string.Equals(info.Code, "USD", StringComparison.OrdinalIgnoreCase))
                    {
                        info.PriceInUsd = 1m;
                        continue;
                    }

                    if (!ratesElement.TryGetProperty(info.Code, out JsonElement rateNode))
                    {
                        continue;
                    }

                    double quotedRate = rateNode.GetDouble();
                    if (quotedRate <= 0d)
                    {
                        continue;
                    }

                    decimal safeRate = System.Convert.ToDecimal(quotedRate, CultureInfo.InvariantCulture);
                    decimal usdPrice = decimal.Round(1m / safeRate, 6, MidpointRounding.AwayFromZero);
                    info.PriceInUsd = usdPrice;
                }

                LastSuccessfulUpdate = DateTime.Now;
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private CurrencyInfo? FindCurrency(string code)
        {
            return _allCurrencies.FirstOrDefault(info => string.Equals(info.Code, code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
