using System; // позволяет использовать базовые классы .NET
using System.Collections.Generic; // позволяет использовать коллекции, такие как List<T>
using System.Net.Http; // позволяет выполнять HTTP-запросы
using System.Text.Json; // позволяет работать с JSON-данными
using System.Threading.Tasks; // позволяет использовать асинхронное программирование

namespace MoneyMorph
{
    // Представляет информацию о валюте и её курсе
    public class CurrencyInfo
    {
        public string Code; // Код валюты (ISO 4217)

        public decimal PriceInUsd; // Стоимость одной единицы валюты в долларах США

        // Инициализирует новый экземпляр информации о валюте
        public CurrencyInfo(string code, decimal priceInUsd)
        {
            Code = code; // Сохраняет код валюты
            PriceInUsd = priceInUsd; // Сохраняет курс относительно USD
        }
    }

    // Выполняет конвертацию между различными валютами
    public class CurrencyConverter
    {
        private readonly List<CurrencyInfo> _allCurrencies; // Коллекция всех поддерживаемых валют
        private static readonly HttpClient _httpClient = new HttpClient(); // HTTP-клиент для запросов к API
        private const string ApiUrl = "https://open.er-api.com/v6/latest/USD"; // Адрес API для получения курсов валют

        public DateTime LastSuccessfulUpdate { get; private set; } // Время последнего успешного обновления курсов

        // Инициализирует конвертер с предустановленными курсами валют
        public CurrencyConverter()
        {
            // Создаёт список валют с начальными курсами
            _allCurrencies = new List<CurrencyInfo>
            {
                new CurrencyInfo("USD", 1.00m), // Доллар США (базовая валюта)
                new CurrencyInfo("EUR", 1.09m), // Евро
                new CurrencyInfo("GBP", 1.28m), // Фунт стерлингов
                new CurrencyInfo("JPY", 0.0070m), // Японская иена
                new CurrencyInfo("RUB", 0.011m), // Российский рубль
                new CurrencyInfo("CNY", 0.14m), // Китайский юань
                new CurrencyInfo("CHF", 1.12m), // Швейцарский франк
                new CurrencyInfo("CAD", 0.73m), // Канадский доллар
                new CurrencyInfo("AUD", 0.67m), // Австралийский доллар
                new CurrencyInfo("TRY", 0.031m), // Турецкая лира
                new CurrencyInfo("SEK", 0.095m), // Шведская крона
                new CurrencyInfo("NOK", 0.094m), // Норвежская крона
                new CurrencyInfo("PLN", 0.25m), // Польский злотый
                new CurrencyInfo("INR", 0.012m), // Индийская рупия
                new CurrencyInfo("BRL", 0.20m), // Бразильский реал
                new CurrencyInfo("ZAR", 0.054m) // Южноафриканский рэнд
            };
        }

        // Возвращает массив кодов всех доступных валют
        public string[] GetCurrencyCodes()
        {
            string[] codes = new string[_allCurrencies.Count]; // Создаёт массив для кодов валют с размером списка валют
            for (int i = 0; i < _allCurrencies.Count; i++) // Перебирает все валюты
            {
                codes[i] = _allCurrencies[i].Code; // Копирует код валюты в массив
            }
            return codes; // Возвращает массив кодов
        }

        // Ищет валюту по коду без учёта регистра
        private CurrencyInfo? FindCurrency(string code) // Ищет валюту по её коду может вернуть null
        {
            foreach (CurrencyInfo info in _allCurrencies) // Перебирает все валюты в списке
            {
                if (string.Equals(info.Code, code, StringComparison.OrdinalIgnoreCase)) // Сравнивает коды без учёта регистра
                {
                    return info; // Возвращает найденную валюту
                }
            }
            return null; // Возвращает null если валюта не найдена
        }

        // Конвертирует сумму из одной валюты в другую
        public decimal Convert(string fromCode, string toCode, decimal amount, int decimals) // Конвертирует сумму между валютами
        {
            CurrencyInfo? fromCurrency = FindCurrency(fromCode); // Находит исходную валюту
            CurrencyInfo? toCurrency = FindCurrency(toCode); // Находит целевую валюту

            if (fromCurrency == null || toCurrency == null) // Проверяет наличие обеих валют
            {
                throw new ArgumentException("Выбрана валюта, которой нет в списке."); // Выбрасывает исключение при отсутствии валюты
            }

            decimal usdValue = amount * fromCurrency.PriceInUsd; // Конвертирует сумму в доллары США
            decimal targetValue = usdValue / toCurrency.PriceInUsd; // Конвертирует из долларов в целевую валюту
            int safeDecimals = Math.Clamp(decimals, 0, 6); // Ограничивает количество знаков после запятой 
            return decimal.Round(targetValue, safeDecimals); // Округляет и возвращает результат 
        }

        // Возвращает копию массива всех валют
        public CurrencyInfo[] GetAllCurrencies()
        {
            CurrencyInfo[] copy = new CurrencyInfo[_allCurrencies.Count]; // Создаёт новый массив
            for (int i = 0; i < _allCurrencies.Count; i++) // Перебирает все валюты
            {
                CurrencyInfo original = _allCurrencies[i]; // Получает оригинальную валюту
                copy[i] = new CurrencyInfo(original.Code, original.PriceInUsd); // Создаёт копию валюты
            }

            return copy; // Возвращает массив копий
        }

        // Асинхронно обновляет курсы валют из внешнего API
        public async Task<bool> UpdateRatesFromInternetAsync()
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(ApiUrl); // Выполняет GET-запрос к API
                if (!response.IsSuccessStatusCode) // Проверяет успешность HTTP-ответа, если 200 то ок, если нет то ошибка
                {
                    return false; // Возвращает false при ошибке запроса
                }

                string json = await response.Content.ReadAsStringAsync(); // Читает тело ответа как строку
                using JsonDocument document = JsonDocument.Parse(json); // Парсит JSON-документ

                if (!document.RootElement.TryGetProperty("result", out JsonElement resultElement)) // Получает поле result
                {
                    return false; // Возвращает false если поле отсутствует
                }

                string? resultText = resultElement.GetString(); // Извлекает текст из поля result
                if (!string.Equals(resultText, "success", StringComparison.OrdinalIgnoreCase)) // Проверяет статус ответа
                {
                    return false; // Возвращает false если статус не success
                }

                if (!document.RootElement.TryGetProperty("rates", out JsonElement ratesElement)) // Получает объект rates
                {
                    return false; // Возвращает false если объект отсутствует
                }

                foreach (CurrencyInfo info in _allCurrencies) // Перебирает все валюты
                {
                    if (string.Equals(info.Code, "USD", StringComparison.OrdinalIgnoreCase)) // Проверяет является ли валюта USD
                    {
                        info.PriceInUsd = 1m; // Устанавливает курс USD как 1. m обозначает decimal
                        continue; // Переходит к следующей валюте
                    }

                    if (ratesElement.TryGetProperty(info.Code, out JsonElement rateNode)) // Пытается получить курс валюты
                    {
                        double rateToCurrency = rateNode.GetDouble(); // Извлекает курс как double
                        if (rateToCurrency > 0) // Проверяет положительность курса для избежания деления на ноль
                        {
                            decimal safeRate = System.Convert.ToDecimal(rateToCurrency); // Преобразует в decimal
                            decimal newPrice = 1m / safeRate; // Вычисляет обратный курс (цену в USD)
                            info.PriceInUsd = decimal.Round(newPrice, 6, MidpointRounding.AwayFromZero); // Округляет и сохраняет курс
                        }
                    }
                }

                LastSuccessfulUpdate = DateTime.Now; // Сохраняет время успешного обновления
                return true; // Возвращает true при успешном обновлении
            }
            catch
            {
                return false; // Возвращает false при любой ошибке
            }
        }
    }
}