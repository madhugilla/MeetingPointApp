namespace MeetingPointApi.Models
{
    public class MeetingPointRequest
    {
        public List<string> StartingAddresses { get; set; }
        public string DestinationAddress { get; set; }
    }

    public class MeetingPointResponse
    {
        public string MeetingPointAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<TravelTimeInfo> TravelTimesFromStartPoints { get; set; }
        public TravelTimeInfo TravelTimeToDestination { get; set; }
        public List<LatLng> StartingPointLatLngs { get; set; }
        public LatLng DestinationLatLng { get; set; }
    }

    public class TravelTimeInfo
    {
        public string Address { get; set; }
        public string DurationText { get; set; }
        public int DurationSeconds { get; set; }
        public string DistanceText { get; set; }
        public int DistanceMeters { get; set; }
    }
}