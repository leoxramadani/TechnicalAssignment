using Claims.Domain.Enums;

namespace Claims.Helpers.PremiumComputation
{
    public interface IPremiumComputationService
    {
        decimal ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType);

    }
}
