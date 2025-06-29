using MeetingPointApi.Models;

namespace MeetingPointApi.Services
{
    public interface IGoogleMapsService
    {
        Task<LatLng> GeocodeAddress(string address);
        Task<TravelTimeInfo> CalculateRoute(LatLng origin, LatLng destination);
        Task<List<TravelTimeInfo>> CalculateDistanceMatrix(List<LatLng> origins, List<LatLng> destinations);
    }
}