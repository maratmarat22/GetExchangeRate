using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace GetExchangeRate
{
    internal class ExchangerClient
    {
        private readonly string _url;

        internal ExchangerClient(string url)
        {
            _url = url;
        }

        internal async Task<List<Currency>> GetExchangeRatesByDate(DateTime dateTime)
        {
            string requestBody = CreateRequest(dateTime);
            
            using HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Post, _url);
            request.Headers.Add("Accept", "application/soap+xml");
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/soap+xml");

            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseString);
        }

        private static string CreateRequest(DateTime dateTime)
        {
            string dateTimeString = dateTime.ToString("yyyy-MM-dd");
            
            return $"""
                <?xml version="1.0" encoding="utf-8"?>
                <soap12:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap12="http://www.w3.org/2003/05/soap-envelope">
                  <soap12:Body>
                    <GetCursOnDateXML xmlns="http://web.cbr.ru/">
                      <On_date>{dateTimeString}</On_date>
                    </GetCursOnDateXML>
                  </soap12:Body>
                </soap12:Envelope>
                """;
        }

        private static List<Currency> ParseResponse(string response)
        {
            XDocument doc = XDocument.Parse(response);

            return doc.Descendants("ValuteCursOnDate")
                .Select(valute => new Currency
                (
                    name: valute.Element("Vname")?.Value.Trim(),
                    code: valute.Element("VchCode")?.Value,
                    nominal: int.TryParse(valute.Element("Vnom")?.Value, out int nominal) ? nominal : 1,
                    rate: decimal.TryParse(valute.Element("Vcurs")?.Value, CultureInfo.InvariantCulture, out decimal rate) ? rate : 0
                ))
                .ToList();
        }
    }
}
