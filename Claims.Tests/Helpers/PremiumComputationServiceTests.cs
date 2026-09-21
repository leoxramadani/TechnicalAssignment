using Claims.Domain.Enums;
using Claims.Domain.Services;
using Xunit;

namespace Claims.Tests.Helpers
{
    public class PremiumComputationServiceTests
    {
        private readonly PremiumComputationService _service;

        public PremiumComputationServiceTests()
        {
            _service = new PremiumComputationService();

        }


        //yacht first tier
        [Fact]
        public void ComputePremium_YachtFor30Days_ShouldUseFirstTierRate()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(30);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.Yacht);

            // Assert
            Assert.Equal(41_250m, premium);
        }

        //yacht 150 days
        [Fact]
        public void ComputePremium_YachtFor150Days_ShouldApplySecondTierDiscount()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(150);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.Yacht);

            // Assert
            var dailyRate = 1250m * 1.1m;

            var expected =
                30 * dailyRate +
                120 * dailyRate * 0.95m;

            Assert.Equal(expected, premium);
        }

        //yacht mothe than 180 days
        [Fact]
        public void ComputePremium_YachtFor200Days_ShouldApplyAllThreeTiers()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(200);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.Yacht);

            // Assert
            var dailyRate = 1250m * 1.1m;

            var expected =
                30 * dailyRate +
                150 * dailyRate * 0.95m +
                20 * dailyRate * 0.92m;

            Assert.Equal(expected, premium);
        }

        //Tanker - 50% multiplier
        [Fact]
        public void ComputePremium_TankerFor30Days_ShouldApplyTankerMultiplier()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(30);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.Tanker);

            // Assert
            var expected = 30 * 1250m * 1.5m;

            Assert.Equal(expected, premium);
        }

        //PassengerShip - second tier discount

        [Fact]
        public void ComputePremium_PassengerShipFor150Days_ShouldApplySecondTierDiscount()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(150);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.PassengerShip);

            // Assert
            var dailyRate = 1250m * 1.2m;

            var expected =
                30 * dailyRate +
                120 * dailyRate * 0.98m;

            Assert.Equal(expected, premium);
        }

        [Fact]
        public void ComputePremium_YachtForExactly180Days_ShouldNotApplyThirdTier()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(180);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.Yacht);

            // Assert
            var dailyRate = 1250m * 1.1m;

            var expected =
                30 * dailyRate +
                150 * dailyRate * 0.95m;

            Assert.Equal(expected, premium);
        }

        [Fact]
        public void ComputePremium_OtherCoverTypeFor30Days_ShouldApplyDefaultMultiplier()
        {
            // Arrange
            var startDate = new DateTime(2026, 1, 1);
            var endDate = startDate.AddDays(30);

            // Act
            var premium = _service.ComputePremium(
                startDate,
                endDate,
                CoverTypeEnum.BulkCarrier);

            // Assert
            var expected = 30 * 1250m * 1.3m;

            Assert.Equal(expected, premium);
        }
    }
}
