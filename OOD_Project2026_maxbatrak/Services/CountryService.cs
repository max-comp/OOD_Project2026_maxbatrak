using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using OOD_Project2026_maxbatrak.Models;

namespace OOD_Project2026_maxbatrak.Services
{
    
    /* Calls the free RestCountries API to fetch country information.
     No API key required.
     API docs: https://restcountries.com
    */
    public class CountryService
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        /// <summary>
        /// Looks up a country by name and returns a CountryInfo object.
        /// Returns null if the country is not found or the request fails.
        /// </summary>
        public static CountryInfo GetCountryInfo(string countryName)
        {
            try
            {
                string url = $"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(countryName)}?fields=name,capital,currencies,flags,region";

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                    return null;

                string json = response.Content.ReadAsStringAsync().Result;
                JArray results = JArray.Parse(json);

                if (results.Count == 0)
                    return null;

                // Take the first matching country
                JObject country = (JObject)results[0];

                string commonName = country["name"]?["common"]?.ToString();
                string region     = country["region"]?.ToString();
                string flagUrl    = country["flags"]?["png"]?.ToString();

                // Capital is an array — take the first entry
                string capital = null;
                JArray capitals = country["capital"] as JArray;
                if (capitals != null && capitals.Count > 0)
                    capital = capitals[0].ToString();

                // Currencies is an object keyed by currency code e.g. { "EUR": { "name": "Euro" } }
                string currencyCode = null;
                string currencyName = null;
                JObject currencies = country["currencies"] as JObject;
                if (currencies != null)
                {
                    foreach (var currency in currencies)
                    {
                        currencyCode = currency.Key;
                        currencyName = currency.Value["name"]?.ToString();
                        break;
                    }
                }

                return new CountryInfo
                {
                    CommonName   = commonName,
                    Capital      = capital,
                    CurrencyCode = currencyCode,
                    CurrencyName = currencyName,
                    FlagUrl      = flagUrl,
                    Region       = region
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
