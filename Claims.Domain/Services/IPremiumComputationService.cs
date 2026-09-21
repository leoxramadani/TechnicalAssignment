using Claims.Domain.Enums;

namespace Claims.Domain.Services;

public interface IPremiumComputationService
{
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType);
}
