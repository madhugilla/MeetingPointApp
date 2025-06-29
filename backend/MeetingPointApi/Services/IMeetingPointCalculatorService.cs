using MeetingPointApi.Models;

namespace MeetingPointApi.Services
{
    public interface IMeetingPointCalculatorService
    {
        Task<MeetingPointResponse> CalculateMeetingPoint(MeetingPointRequest request);
    }
}