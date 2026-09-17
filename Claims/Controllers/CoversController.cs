using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Services.CoversServices;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// Controller for managing insurance covers and computing premiums.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CoversController : ControllerBase
{
    private readonly ILogger<CoversController> _logger;
    private readonly ICoversService _coversService;
    private readonly IValidator<Cover> _validator;

    public CoversController(ILogger<CoversController> logger, ICoversService coversService, IValidator<Cover> validator)
    {
        _logger = logger;
        _coversService = coversService;
        _validator = validator;
    }

    /// <summary>
    /// Computes the insurance premium for a specified period and cover type.
    /// </summary>
    /// <param name="startDate">Start date of the cover.</param>
    /// <param name="endDate">End date of the cover.</param>
    /// <param name="coverType">Type of cover.</param>
    /// <returns>The computed premium amount.</returns>
    [HttpPost("compute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType)
    {
        _logger.LogInformation("Computing premium for CoverType: {CoverType}, StartDate: {StartDate}, EndDate: {EndDate}", coverType, startDate, endDate);
        var premium = _coversService.ComputePremium(startDate, endDate, coverType);
        _logger.LogInformation("Computed premium: {Premium} for CoverType: {CoverType}, StartDate: {StartDate}, EndDate: {EndDate}", premium, coverType, startDate, endDate);
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
        _logger.LogInformation("Fetched {Count} covers.", results.Count());
        return Ok(results);
    }

    /// <summary>
    /// Retrieves a cover by its identifier.
    /// </summary>
    /// <param name="id">The cover identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cover if found, or 404 if not found.</returns>
    [HttpGet("{id}")]
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Cover>> CreateAsync([FromBody] Cover cover, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(cover, cancellationToken);
        _logger.LogInformation("Attempting to create a new cover of type {CoverType} from {StartDate} to {EndDate}.", cover.Type, cover.StartDate, cover.EndDate);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed for cover creation: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        var created = await _coversService.CreateCoverAsync(cover, cancellationToken);
        _logger.LogInformation("Cover created successfully with ID: {Id}", created.Id);
        return Created("/covers",created);
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
        _logger.LogInformation("Attempting to delete cover with ID: {Id}", id);
        if (!deleted)
        {
            _logger.LogWarning("Cover with ID: {Id} not found for deletion.", id);
            return NotFound();
        }
        
        _logger.LogInformation("Cover with ID: {Id} deleted successfully.", id);
        return NoContent();
    }
}
