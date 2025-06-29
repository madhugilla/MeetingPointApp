using MeetingPointApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace MeetingPointApi.Services
{
    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _geocodingApiKey;
        private readonly string _directionsApiKey;
        private readonly string _distanceMatrixApiKey;

        public GoogleMapsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("GoogleMapsClient");
            _configuration = configuration;
            _geocodingApiKey = _configuration["GoogleApiKeys:GeocodingApiKey"];
            _directionsApiKey = _configuration["GoogleApiKeys:DirectionsApiKey"];
            _distanceMatrixApiKey = _configuration["GoogleApiKeys:DistanceMatrixApiKey"];
        }

        public async Task<LatLng> GeocodeAddress(string address)
        {
            var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_geocodingApiKey}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("results", out JsonElement results) && results.GetArrayLength() > 0)
                {
                    JsonElement location = results[0].GetProperty("geometry").GetProperty("location");
                    return new LatLng
                    {
                        Lat = location.GetProperty("lat").GetDouble(),
                        Lng = location.GetProperty("lng").GetDouble()
                    };
                }
            }
            throw new Exception($"Could not geocode address: {address}");
        }

        public async Task<string> ReverseGeocode(LatLng latLng)
        {
            var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/geocode/json?latlng={latLng.Lat},{latLng.Lng}&key={_geocodingApiKey}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("results", out JsonElement results) && results.GetArrayLength() > 0)
                {
                    return results[0].GetProperty("formatted_address").GetString();
                }
            }
            return $"Unknown location ({latLng.Lat}, {latLng.Lng})";
        }

        public async Task<TravelTimeInfo> CalculateRoute(LatLng origin, LatLng destination)
        {
            var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/directions/json?origin={origin.Lat},{origin.Lng}&destination={destination.Lat},{destination.Lng}&key={_directionsApiKey}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("routes", out JsonElement routes) && routes.GetArrayLength() > 0)
                {
                    JsonElement leg = routes[0].GetProperty("legs")[0];
                    return new TravelTimeInfo
                    {
                        DurationText = leg.GetProperty("duration").GetProperty("text").GetString(),
                        DurationSeconds = leg.GetProperty("duration").GetProperty("value").GetInt32(),
                        DistanceText = leg.GetProperty("distance").GetProperty("text").GetString(),
                        DistanceMeters = leg.GetProperty("distance").GetProperty("value").GetInt32()
                    };
                }
            }
            throw new Exception($"Could not calculate route from {origin.Lat},{origin.Lng} to {destination.Lat},{destination.Lng}.");
        }

        public async Task<List<TravelTimeInfo>> CalculateDistanceMatrix(List<LatLng> origins, List<LatLng> destinations)
        {
            var originStrings = origins.Select(o => $"{o.Lat},{o.Lng}");
            var destinationStrings = destinations.Select(d => $"{d.Lat},{d.Lng}");

            var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/distancematrix/json?origins={string.Join("|", originStrings)}&destinations={string.Join("|", destinationStrings)}&key={_distanceMatrixApiKey}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var travelTimes = new List<TravelTimeInfo>();
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("rows", out JsonElement rows))
                {
                    for (int i = 0; i < rows.GetArrayLength(); i++)
                    {
                        JsonElement row = rows[i];
                        if (row.TryGetProperty("elements", out JsonElement elements))
                        {
                            for (int j = 0; j < elements.GetArrayLength(); j++)
                            {
                                JsonElement element = elements[j];
                                if (element.TryGetProperty("status", out JsonElement status) && status.GetString() == "OK")
                                {
                                    travelTimes.Add(new TravelTimeInfo
                                    {
                                        Address = destinations[j].ToString(), // Placeholder, ideally would be the original address string
                                        DurationText = element.GetProperty("duration").GetProperty("text").GetString(),
                                        DurationSeconds = element.GetProperty("duration").GetProperty("value").GetInt32(),
                                        DistanceText = element.GetProperty("distance").GetProperty("text").GetString(),
                                        DistanceMeters = element.GetProperty("distance").GetProperty("value").GetInt32()
                                    });
                                }
                                else
                                {
                                    // Handle cases where status is not OK (e.g., ZERO_RESULTS, NOT_FOUND)
                                    travelTimes.Add(new TravelTimeInfo
                                    {
                                        Address = destinations[j].ToString(),
                                        DurationText = "N/A",
                                        DurationSeconds = 0,
                                        DistanceText = "N/A",
                                        DistanceMeters = 0
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return travelTimes;
        }
    }
}