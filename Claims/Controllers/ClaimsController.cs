using Claims.Domain.Entities;
using Claims.Services.ClaimsServices;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    /// <summary>
    /// Controller for managing insurance claims.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly IClaimsService _claimsService;
        private readonly IValidator<Claim> _validator;

        public ClaimsController(ILogger<ClaimsController> logger, IClaimsService claimsService, IValidator<Claim> validator)
        {
            _logger = logger;
            _claimsService = claimsService;
            _validator = validator;
        }

        /// <summary>
        /// Retrieves all claims.
        /// </summary>
        /// <returns>A list of claims.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Claim>>> GetAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all claims.");
            var claims = await _claimsService.GetClaimsAsync(cancellationToken);
            var claimList = claims.ToList();
            _logger.LogInformation("Retrieved {Count} claims successfully.", claimList.Count);
            return Ok(claimList);
        }

        /// <summary>
        /// Retrieves a claim by its identifier.
        /// </summary>
        /// <param name="id">The claim identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The claim if found, or 404 if not found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Claim>> GetAsync(string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving claim with ID: {ClaimId}", id);
            var claim = await _claimsService.GetClaimByIdAsync(id, cancellationToken);
            if (claim is null)
            {
                _logger.LogWarning("Claim with ID {ClaimId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved claim with ID: {ClaimId}", id);
            return Ok(claim);
        }


        /// <summary>
        /// Creates a new claim.
        /// </summary>
        /// <param name="claim">The claim details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created claim.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Claim>> CreateAsync([FromBody] Claim claim, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to create a new claim for CoverId: {CoverId}, DamageCost: {DamageCost}", claim.CoverId, claim.DamageCost);

            var validationResult = await _validator.ValidateAsync(claim, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Claim validation failed with {Count} errors: {Errors}",
                    validationResult.Errors.Count,
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(validationResult.Errors);
            }

            var created = await _claimsService.CreateClaimAsync(claim, cancellationToken);
            _logger.LogInformation("Successfully created claim with ID: {ClaimId}", created.Id);
            return Created("Claims", created);
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
            _logger.LogInformation("Attempting to delete claim with ID: {ClaimId}", id);
            var deleted = await _claimsService.DeleteClaimAsync(id, cancellationToken);
            if (!deleted)
            {
                _logger.LogWarning("Failed to delete claim: Claim with ID {ClaimId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully deleted claim with ID: {ClaimId}", id);
            return NoContent();
        }
    }
}
