using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IntegrationTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private static readonly Dictionary<int, (int Remaining, int Taken)> _employees = new()
    {
        { 1, (21, 0) },  
        { 2, (21, 0) }   
    };

    [HttpGet(Name = "GetWeather")]
    public IActionResult GetWeather(int userId, int cityId)
    {
        switch (cityId)
        {
            case 1:
                return Ok(new { UserId = userId, City = "New York", Temperature = 25 });
            case 2:
                return Ok(new { UserId = userId, City = "Cairo", Temperature = 40 });
            default:
                return NotFound(new { UserId = userId, Message = "City not found" });
        }
    }

    [HttpPost("requests")]
    public IActionResult CreateVacationRequest([FromBody] VacationRequestDto request)
    {
        if (!_employees.ContainsKey(request.UserId))
            return NotFound(new { Message = "Employee not found" });

        var daysRequested = (request.EndDate - request.StartDate).Days + 1;

        if (daysRequested > _employees[request.UserId].Remaining)
            return BadRequest(new { Message = "Not enough vacation days remaining" });

        _employees[request.UserId] = (
            _employees[request.UserId].Remaining - daysRequested,
            _employees[request.UserId].Taken + daysRequested
        );

        return CreatedAtAction(nameof(GetVacationBalance),
            new { userId = request.UserId },
            new
            {
                Message = "Request created successfully",
                DaysRequested = daysRequested
            });
    }

    [HttpGet("balance/{userId}")]
    public IActionResult GetVacationBalance(int userId)
    {
        if (!_employees.TryGetValue(userId, out var balance))
            return NotFound(new { Message = "Employee not found" });

        return Ok(new VacationBalanceDto(
            UserId: userId,
            TotalAnnualDays: 21,
            UsedDays: balance.Taken,
            PendingDays: 0,
            RemainingDays: balance.Remaining
        ));
    }
}

public record VacationRequestDto(
    [Required] int UserId,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required][StringLength(200)] string Reason,
    string? Notes = null);

public record VacationBalanceDto(
    int UserId,
    int TotalAnnualDays,
    int UsedDays,
    int PendingDays,
    int RemainingDays);