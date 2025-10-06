using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MoneyMorph
{
    // Этот класс описывает одну валюту и её курс
    public class CurrencyInfo
    {
        // Этот текст хранит код валюты в виде понятных букв
        public string Code;

        // Это число показывает, сколько стоит одна единица валюты в долларах США
        public decimal PriceInUsd;

        // Этот конструктор просто сохраняет код и курс, которые мы передаём
        public CurrencyInfo(string code, decimal priceInUsd)
        {
            Code = code;
            PriceInUsd = priceInUsd;
        }
    }

    // Этот класс делает простейшие вычисления по конвертации валют
    public class CurrencyConverter
    {
        // Здесь мы храним все валюты в обычном списке для простоты понимания
        private readonly List<CurrencyInfo> _allCurrencies;
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";

        // Здесь мы запоминаем момент успешного обновления, чтобы показать его пользователю
        public DateTime LastSuccessfulUpdate { get; private set; }

        // Этот конструктор наполняет список валют предустановленными значениями
        public CurrencyConverter()
        {
            _allCurrencies = new List<CurrencyInfo>
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
        }

        // Эта функция возвращает список кодов всех доступных валют
        public string[] GetCurrencyCodes()
        {
            string[] codes = new string[_allCurrencies.Count];
            for (int i = 0; i < _allCurrencies.Count; i++)
            {
                codes[i] = _allCurrencies[i].Code;
            }
            return codes;
        }

        // Эта функция ищет валюту по коду и возвращает найденный объект или null
        private CurrencyInfo? FindCurrency(string code)
        {
            foreach (CurrencyInfo info in _allCurrencies)
            {
                if (string.Equals(info.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    return info;
                }
            }
            return null;
        }

        // Эта функция выполняет пересчёт суммы из одной валюты в другую
        public decimal Convert(string fromCode, string toCode, decimal amount, int decimals)
        {
            CurrencyInfo? fromCurrency = FindCurrency(fromCode);
            CurrencyInfo? toCurrency = FindCurrency(toCode);

            if (fromCurrency == null || toCurrency == null)
            {
                throw new ArgumentException("Выбрана валюта, которой нет в списке.");
            }

            decimal usdValue = amount * fromCurrency.PriceInUsd;
            decimal targetValue = usdValue / toCurrency.PriceInUsd;
            int safeDecimals = Math.Clamp(decimals, 0, 6);
            return decimal.Round(targetValue, safeDecimals);
        }

        // Эта функция возвращает копию списка валют, чтобы показать его в таблице
        public CurrencyInfo[] GetAllCurrencies()
        {
            CurrencyInfo[] copy = new CurrencyInfo[_allCurrencies.Count];
            for (int i = 0; i < _allCurrencies.Count; i++)
            {
                CurrencyInfo original = _allCurrencies[i];
                copy[i] = new CurrencyInfo(original.Code, original.PriceInUsd);
            }

            return copy;
        }

        // Эта функция обращается к бесплатному API и обновляет курсы в списке
        public async Task<bool> UpdateRatesFromInternetAsync()
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(ApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty("result", out JsonElement resultElement))
                {
                    return false;
                }

                string? resultText = resultElement.GetString();
                if (!string.Equals(resultText, "success", StringComparison.OrdinalIgnoreCase))
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

                    if (ratesElement.TryGetProperty(info.Code, out JsonElement rateNode))
                    {
                        double rateToCurrency = rateNode.GetDouble();
                        if (rateToCurrency > 0)
                        {
                            decimal safeRate = Convert.ToDecimal(rateToCurrency);
                            decimal newPrice = 1m / safeRate;
                            info.PriceInUsd = decimal.Round(newPrice, 6, MidpointRounding.AwayFromZero);
                        }
                    }
                }

                LastSuccessfulUpdate = DateTime.Now;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
