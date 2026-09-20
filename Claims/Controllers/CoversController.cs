using Asp.Versioning;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Filters;
using Claims.Services.CoversServices;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// Controller for managing insurance covers and computing premiums.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class CoversController(ILogger<CoversController> logger, ICoversService coversService) : ControllerBase
{
    private readonly ILogger<CoversController> _logger = logger;
    private readonly ICoversService _coversService = coversService;

    /// <summary>
    /// Computes the insurance premium for a specified period and cover type.
    /// </summary>
    /// <param name="startDate">Start date of the cover.</param>
    /// <param name="endDate">End date of the cover.</param>
    /// <param name="coverType">Type of cover.</param>
    /// <returns>The computed premium amount.</returns>
    [HttpPost("compute")]
    [ServiceFilter(typeof(ValidationFilter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType)
    {
        var premium = _coversService.ComputePremium(startDate, endDate, coverType);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Computed premium: {Premium} for CoverType: {CoverType}, StartDate: {StartDate}, EndDate: {EndDate}", premium, coverType, startDate, endDate);
        }
        return Ok(premium);
    }

    /// <summary>
    /// Retrieves all covers.
    /// </summary>
    /// <returns>A list of covers.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all covers.");
        var results = await _coversService.GetCoversAsync(cancellationToken);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            var count = results is ICollection<Cover> coll ? coll.Count : results.Count();
            _logger.LogInformation("Fetched {Count} covers.", count);
        }
        return Ok(results);
    }

    /// <summary>
    /// Retrieves a cover by its identifier.
    /// </summary>
    /// <param name="id">The cover identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cover if found, or 404 if not found.</returns>
    [HttpGet("{id}", Name = "GetCoverById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Cover>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await _coversService.GetCoverByIdAsync(id, cancellationToken);
        if (cover is null)
        {
            return NotFound();
        }

        return Ok(cover);
    }

    /// <summary>
    /// Creates a new cover and calculates its premium.
    /// </summary>
    /// <param name="cover">The cover details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created cover with computed premium.</returns>
    [HttpPost]
    [ServiceFilter(typeof(ValidationFilter))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Cover>> CreateAsync([FromBody] Cover cover, CancellationToken cancellationToken)
    {
        var created = await _coversService.CreateCoverAsync(cover, cancellationToken);
        return CreatedAtRoute("GetCoverById", new { id = created.Id, version = "1.0" }, created);
    }

    /// <summary>
    /// Deletes a cover by its identifier.
    /// </summary>
    /// <param name="id">The cover identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 NoContent if deleted, or 404 NotFound if not found.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await _coversService.DeleteCoverAsync(id, cancellationToken);

        if (!deleted)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Cover with ID: {Id} not found for deletion.", id);
            }
            return NotFound();
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Cover with ID: {Id} deleted successfully.", id);
        }
        return NoContent();
    }
}
