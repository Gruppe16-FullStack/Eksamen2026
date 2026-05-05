namespace Pendlerapp.Services
{
    /// <summary>
    /// Service for kommunikasjon med Entur Geocoder og Journey Planner API.
    /// </summary>
    public class EnturService
    {
        private readonly HttpClient _httpClient;

        public EnturService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Søker etter stoppesteder basert på navn via Geocoder API.
        /// </summary>
        public async Task<string> SearchStopPlace(string name)
        {
            var url = $"https://api.entur.io/geocoder/v1/autocomplete?text={Uri.EscapeDataString(name)}&lang=no&size=5";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Henter avgangstider for et stoppested via Journey Planner API.
        /// </summary>
        public async Task<string> GetDepartures(string stopPlaceId)
        {
            var query = $$"""
            {
                "query": "{ stopPlace(id: \"{{stopPlaceId}}\") { id name estimatedCalls(timeRange: 72100, numberOfDepartures: 5) { realtime aimedDepartureTime expectedDepartureTime destinationDisplay { frontText } serviceJourney { journeyPattern { line { id transportMode } } } } } }"
            }
            """;

            var content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.entur.io/journey-planner/v3/graphql", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}