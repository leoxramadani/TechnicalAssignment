using Claims.Domain.Enums;

namespace Claims.Domain.Services;

public class PremiumComputationService : IPremiumComputationService
{
    private const decimal BaseDayRate = 1250m;
    private const int FirstTierLengthDays = 30;
    private const int SecondTierLengthDays = 150;
    private const int SecondTierEndDay = FirstTierLengthDays + SecondTierLengthDays;

    /// <summary>
    /// Computes the insurance premium based on the start and end dates, as well as the cover type.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="coverType"></param>
    /// <returns></returns>
    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverTypeEnum coverType)
    {
        var dailyRate = BaseDayRate * GetCoverTypeMultiplier(coverType);
        var insuranceLengthDays = (int)(endDate - startDate).TotalDays;

        var daysInFirstTier = Math.Min(insuranceLengthDays, FirstTierLengthDays);
        var daysInSecondTier = Math.Clamp(insuranceLengthDays - FirstTierLengthDays, 0, SecondTierLengthDays);
        var daysInThirdTier = Math.Max(insuranceLengthDays - SecondTierEndDay, 0);

        var secondTierDiscount = GetSecondTierDiscount(coverType);
        var thirdTierDiscount = GetThirdTierDiscount(coverType);

        return daysInFirstTier * dailyRate
             + daysInSecondTier * dailyRate * (1 - secondTierDiscount)
             + daysInThirdTier * dailyRate * (1 - thirdTierDiscount);
    }

    private static decimal GetCoverTypeMultiplier(CoverTypeEnum coverType) => coverType switch
    {
        CoverTypeEnum.Yacht => 1.1m,
        CoverTypeEnum.PassengerShip => 1.2m,
        CoverTypeEnum.Tanker => 1.5m,
        _ => 1.3m
    };

    // "Following 150 days" discount
    private static decimal GetSecondTierDiscount(CoverTypeEnum coverType) =>
        coverType == CoverTypeEnum.Yacht ? 0.05m : 0.02m;

    // Second-tier discount plus the additional discount for the remaining days
    private static decimal GetThirdTierDiscount(CoverTypeEnum coverType) =>
        coverType == CoverTypeEnum.Yacht ? 0.08m : 0.03m;
}
