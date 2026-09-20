using Asp.Versioning;
using Claims.Domain.Entities;
using Claims.Filters;
using Claims.Services.ClaimsServices;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    /// <summary>
    /// Controller for managing insurance claims.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ClaimsController(ILogger<ClaimsController> logger, IClaimsService claimsService) : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger = logger;
        private readonly IClaimsService _claimsService = claimsService;

        /// <summary>
        /// Retrieves all claims.
        /// </summary>
        /// <returns>A list of claims.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Claim>>> GetAsync(CancellationToken cancellationToken)
        {
            var claims = await _claimsService.GetClaimsAsync(cancellationToken);
            var claimList = claims.ToList();
            if(_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Retrieved {Count} claims successfully.", claimList.Count);
            }
            return Ok(claimList);
        }

        /// <summary>
        /// Retrieves a claim by its identifier.
        /// </summary>
        /// <param name="id">The claim identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The claim if found, or 404 if not found.</returns>
        [HttpGet("{id}", Name = "GetClaimById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Claim>> GetAsync(string id, CancellationToken cancellationToken)
        {
            var claim = await _claimsService.GetClaimByIdAsync(id, cancellationToken);
            if (claim is null)
            {
                _logger.LogWarning("Claim with ID {ClaimId} was not found.", id);
                return NotFound();
            }

            if(_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully retrieved claim with ID: {ClaimId}", id);
            }
            return Ok(claim);
        }


        /// <summary>
        /// Creates a new claim.
        /// </summary>
        /// <param name="claim">The claim details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created claim.</returns>
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilter))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Claim>> CreateAsync([FromBody] Claim claim, CancellationToken cancellationToken)
        {
            var created = await _claimsService.CreateClaimAsync(claim, cancellationToken);
            if(_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully created claim with ID: {ClaimId}", created.Id);
            }
            return CreatedAtRoute("GetClaimById", new { id = created.Id, version = "1.0" }, created);
        }

        /// <summary>
        /// Deletes a claim by its identifier.
        /// </summary>
        /// <param name="id">The claim identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>204 NoContent if deleted, or 404 NotFound if not found.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var deleted = await _claimsService.DeleteClaimAsync(id, cancellationToken);
            if (!deleted)
            {
                _logger.LogWarning("Failed to delete claim: Claim with ID {ClaimId} was not found.", id);
                return NotFound();
            }

            if(_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully deleted claim with ID: {ClaimId}", id);
            }
            return NoContent();
        }
    }
}
