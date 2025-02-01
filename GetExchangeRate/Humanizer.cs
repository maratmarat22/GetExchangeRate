using System.Xml.Linq;

namespace GetExchangeRate
{
    internal class ExchangeRateHumanizer
    {
        private readonly string _apiToken;
        
        internal ExchangeRateHumanizer(string apiToken)
        {
            _apiToken = apiToken;
        }
        
        internal async Task<string> Humanize(Currency currency, DateTime date)
        {
            string CurrencyNameMaybePlural = currency.Nominal > 1 ? await GetPlural(currency.Nominal, currency.Name, "Р") : currency.Name;
            string costWord = currency.Nominal > 1 ? "Стоят" : "Стоит";

            return $"""
                Курс {await DeclineWord(currency.Name, "Р")} на дату {FormatDate(date)}:
                {currency.Nominal} {CurrencyNameMaybePlural}
                {costWord} {currency.Rate} {await GetPlural((int)currency.Rate, "рубль", "И")}
                """;
        }

        private async Task<string> DeclineWord(string word, string @case)
        {
            using HttpClient client = new();
            string url = $"https://ws3.morpher.ru/russian/declension?s={Uri.EscapeDataString(word)}&token={_apiToken}";

            HttpResponseMessage response = await client.GetAsync(url);
            string responseString = await response.Content.ReadAsStringAsync();
            
            XDocument doc = XDocument.Parse(responseString);
            var declensions = new Dictionary<string, string>();

            if (doc.Root == null) throw new HttpRequestException("Unable to parse Morpher response");

            foreach (var element in doc.Root.Elements())
            {
                if (element.Name.LocalName == @case)
                {
                    return element.Value;
                }
            }

            return string.Empty;
        }

        private static string FormatDate(DateTime date)
        {
            string year = date.Year.ToString();
            int month = date.Month;
            string day = date.Day.ToString();

            string monthString = month switch
            {
                1 => "января",
                2 => "февраля",
                3 => "марта",
                4 => "апреля",
                5 => "мая",
                6 => "июня",
                7 => "июля",
                8 => "августа",
                9 => "сентября",
                10 => "октября",
                11 => "ноября",
                12 => "декабря",
                _ => throw new ArgumentException($"No such month: {month}")
            };

            return $"{day} {monthString} {year}";
        }

        private async Task<string?> GetPlural(int n, string word, string @case)
        {
            using HttpClient client = new();
            string url = $"https://ws3.morpher.ru/russian/spell?n={n}&unit={word}&token={_apiToken}";
            HttpResponseMessage response = await client.GetAsync(url);

            string responseString = await response.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(responseString);

            return doc.Descendants("unit").Elements(@case).FirstOrDefault()?.Value;
        }
    }
}
