using MeetingPointApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingPointApi.Services
{
    public class MeetingPointCalculatorService : IMeetingPointCalculatorService
    {
        private readonly IGoogleMapsService _googleMapsService;

        public MeetingPointCalculatorService(IGoogleMapsService googleMapsService)
        {
            _googleMapsService = googleMapsService;
        }

        public async Task<MeetingPointResponse> CalculateMeetingPoint(MeetingPointRequest request)
        {
            var startPointLatLngs = new List<LatLng>();
            foreach (var address in request.StartingAddresses)
            {
                startPointLatLngs.Add(await _googleMapsService.GeocodeAddress(address));
            }

            var destinationLatLng = await _googleMapsService.GeocodeAddress(request.DestinationAddress);

            // --- Improved Meeting Point Algorithm ---
            // For a more robust solution, we'll consider the centroid as a primary candidate
            // and then use Distance Matrix API to find the optimal point.

            // Calculate centroid of starting points
            double avgLat = startPointLatLngs.Average(p => p.Lat);
            double avgLng = startPointLatLngs.Average(p => p.Lng);
            var centroidLatLng = new LatLng { Lat = avgLat, Lng = avgLng };

            // For a real-world scenario, you might generate more candidate points
            // (e.g., points along the routes, or a grid search around the centroid).
            // For this iteration, we'll use the centroid as our main candidate.
            var candidateMeetingPoints = new List<LatLng> { centroidLatLng };

            LatLng optimalMeetingPoint = null;
            long minTotalTravelTime = long.MaxValue;

            foreach (var candidate in candidateMeetingPoints)
            {
                var travelTimesToCandidate = await _googleMapsService.CalculateDistanceMatrix(startPointLatLngs, new List<LatLng> { candidate });
                long currentTotalTravelTime = travelTimesToCandidate.Sum(t => t.DurationSeconds);

                if (currentTotalTravelTime < minTotalTravelTime)
                {
                    minTotalTravelTime = currentTotalTravelTime;
                    optimalMeetingPoint = candidate;
                }
            }

            if (optimalMeetingPoint == null)
            {
                throw new Exception("Could not determine an optimal meeting point.");
            }

            // Reverse geocode the optimal meeting point to get a human-readable address
            string meetingPointAddress = await _googleMapsService.ReverseGeocode(optimalMeetingPoint);

            // Calculate travel times from each starting point to the optimal meeting point
            var travelTimesFromStartPoints = new List<TravelTimeInfo>();
            var distanceMatrixResults = await _googleMapsService.CalculateDistanceMatrix(startPointLatLngs, new List<LatLng> { optimalMeetingPoint });

            // Populate TravelTimeInfo with original addresses for clarity
            for (int i = 0; i < request.StartingAddresses.Count; i++)
            {
                var info = distanceMatrixResults[i];
                info.Address = request.StartingAddresses[i];
                travelTimesFromStartPoints.Add(info);
            }

            // Calculate travel time from meeting point to destination
            var travelTimeToDestination = await _googleMapsService.CalculateRoute(optimalMeetingPoint, destinationLatLng);
            travelTimeToDestination.Address = request.DestinationAddress; // Set destination address for clarity

            return new MeetingPointResponse
            {
                MeetingPointAddress = meetingPointAddress,
                Latitude = optimalMeetingPoint.Lat,
                Longitude = optimalMeetingPoint.Lng,
                TravelTimesFromStartPoints = travelTimesFromStartPoints,
                TravelTimeToDestination = travelTimeToDestination,
                StartingPointLatLngs = startPointLatLngs,
                DestinationLatLng = destinationLatLng
            };
        }
    }
}