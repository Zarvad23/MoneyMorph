using System;
using System.Collections.Generic;

namespace MoneyMorph
{
    // Этот класс хранит данные об одной валюте
    public class CurrencyRate
    {
        // Это свойство хранит буквенный код валюты
        public string Code { get; set; }

        // Это свойство хранит курс одной единицы валюты к доллару
        public decimal RateToUsd { get; set; }

        // Этот конструктор создаёт валюту с заданным кодом и курсом
        public CurrencyRate(string code, decimal rateToUsd)
        {
            Code = code;
            RateToUsd = rateToUsd;
        }
    }

    // Этот класс выполняет простые операции конвертации валют
    public class CurrencyConverter
    {
        private readonly Dictionary<string, CurrencyRate> _rates;

        // Этот конструктор заполняет словарь доступных валют
        public CurrencyConverter()
        {
            _rates = new Dictionary<string, CurrencyRate>(StringComparer.OrdinalIgnoreCase)
            {
                { "USD", new CurrencyRate("USD", 1.00m) },
                { "EUR", new CurrencyRate("EUR", 1.09m) },
                { "GBP", new CurrencyRate("GBP", 1.28m) },
                { "JPY", new CurrencyRate("JPY", 0.0070m) },
                { "RUB", new CurrencyRate("RUB", 0.011m) }
            };
        }

        // Эта функция проверяет, есть ли валюта в словаре
        public bool HasCurrency(string code)
        {
            return _rates.ContainsKey(code);
        }

        // Эта функция возвращает список кодов доступных валют
        public List<string> GetSupportedCodes()
        {
            return new List<string>(_rates.Keys);
        }

        // Эта функция конвертирует сумму из одной валюты в другую
        public decimal Convert(string fromCode, string toCode, decimal amount)
        {
            if (!HasCurrency(fromCode) || !HasCurrency(toCode))
            {
                throw new ArgumentException("Неизвестный код валюты");
            }

            decimal usdAmount = amount * _rates[fromCode].RateToUsd;
            decimal targetAmount = usdAmount / _rates[toCode].RateToUsd;
            return decimal.Round(targetAmount, 2);
        }
    }
}
