using MeetingPointApi.Models;
using MeetingPointApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetingPointApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingPointController : ControllerBase
    {
        private readonly IMeetingPointCalculatorService _meetingPointCalculatorService;

        public MeetingPointController(IMeetingPointCalculatorService meetingPointCalculatorService)
        {
            _meetingPointCalculatorService = meetingPointCalculatorService;
        }

        [HttpPost]
        public async Task<ActionResult<MeetingPointResponse>> CalculateMeetingPoint([FromBody] MeetingPointRequest request)
        {
            if (request == null || !request.StartingAddresses.Any() || string.IsNullOrEmpty(request.DestinationAddress))
            {
                return BadRequest("Invalid request. Please provide starting addresses and a destination address.");
            }

            try
            {
                var response = await _meetingPointCalculatorService.CalculateMeetingPoint(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}