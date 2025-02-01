using System.Globalization;

namespace GetExchangeRate
{
    internal class Program
    {
        static async Task Main()
        {
            string url = "https://cbr.ru/DailyInfoWebServ/DailyInfo.asmx";
            ExchangerClient exchanger = new(url);

            string morpherApiToken = "morpher-api-key";
            ExchangeRateHumanizer humanizer = new(morpherApiToken);
            
            while (true)
            {
                (string? currencyCode, DateTime date) = GetRequestData();
                List<Currency> currencies = await exchanger.GetExchangeRatesByDate(date);
                Currency? requestedCurrency = currencies.FirstOrDefault(c => c.Code.Equals(currencyCode, StringComparison.CurrentCultureIgnoreCase));

                if (requestedCurrency != null)
                {
                    Console.WriteLine(await humanizer.Humanize(requestedCurrency, date));
                }
                else
                {
                    Console.WriteLine("Такой валюты нет");
                }
            }
        }

        static (string?, DateTime) GetRequestData()
        {
            Console.WriteLine("Введите код валюты:");
            string? currencyCode = Console.ReadLine();

            string dateFormat = "yyyy-MM-dd";
            Console.WriteLine($"Введите дату в формате {dateFormat}:");
            string? dateString = Console.ReadLine();
            DateTime date;

            while (!DateTime.TryParseExact(dateString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                Console.WriteLine("Неправильный формат даты, попробуйте еще раз:");
                dateString = Console.ReadLine();
            }

            return (currencyCode, date);
        }
    }
}
