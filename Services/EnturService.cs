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
        public async Task<string> GetDepartures(string stopPlaceId, int antall = 5)
        {
            var query = $$"""
            {
                "query": "{ stopPlace(id: \"{{stopPlaceId}}\") { id name estimatedCalls(timeRange: 72100, numberOfDepartures: {{antall}}) { realtime aimedDepartureTime expectedDepartureTime aimedArrivalTime expectedArrivalTime quay { name publicCode } destinationDisplay { frontText } serviceJourney { id journeyPattern { line { id publicCode name transportMode } } } } } }"
            }
            """;

            var content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.entur.io/journey-planner/v3/graphql", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Henter reiseruter mellom to stoppesteder via Journey Planner API.
        /// </summary>
        public async Task<string> GetTrip(string fromId, string toId)
        {
            var query = $$"""
            {
                "query": "{ trip(from: {place: \"{{fromId}}\"}, to: {place: \"{{toId}}\"}, numTripPatterns: 5) { tripPatterns { aimedStartTime expectedStartTime expectedEndTime duration legs { fromPlace { name } toPlace { name } expectedStartTime expectedEndTime mode line { publicCode name } } } } }"
            }
            """;

            var content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.entur.io/journey-planner/v3/graphql", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Henter alle stopp for en serviceJourney inkludert stopPlace ID.
        /// </summary>
        public async Task<string> GetServiceJourneyStops(string serviceJourneyId)
        {
            var query = $$"""
            {
                "query": "{ serviceJourney(id: \"{{serviceJourneyId}}\") { passingTimes { quay { id name stopPlace { id } } } } }"
            }
            """;

            var content = new StringContent(query, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.entur.io/journey-planner/v3/graphql", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}