namespace OOD_Project2026_maxbatrak.Models
{
    
    /* Holds the country data returned from the RestCountries API.
     Only the fields we actually use are mapped here. */
    
    public class CountryInfo
    {
        public string CommonName { get; set; }
        public string Capital { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string FlagUrl { get; set; }
        public string Region { get; set; }
    }
}
