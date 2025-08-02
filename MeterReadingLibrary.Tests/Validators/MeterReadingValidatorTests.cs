using FluentAssertions;
using MeterReadingLibrary.DataAccess;
using MeterReadingLibrary.Models;
using MeterReadingLibrary.Validators;
using Moq;

namespace MeterReadingLibrary.Tests.Validators
{
    public class MeterReadingValidatorTests
    {
        private readonly Mock<IStoredProcedureRunner> _mockSpRunner;
        private readonly MeterReadingValidator _validator;

        public MeterReadingValidatorTests()
        {
            _mockSpRunner = new Mock<IStoredProcedureRunner>();
            var accountModel = new AccountModel
            {
                AccountId = 1234,
                FirstName = "Test FirstName",
                LastName = "Test LastName"
            };
            var readingsModel = new List<MeterReadingModel>
            {
                new()
                {
                    MeterReadingId = 1,
                    AccountId = 1234,
                    ReadingDate = new DateTime(2024, 12, 31),
                    ReadingValue = 11111
                },
                new()
                {
                    MeterReadingId = 2,
                    AccountId = 1234,
                    ReadingDate = new DateTime(2025, 01, 01),
                    ReadingValue = 22222
                }
            };

            _mockSpRunner.Setup(x => x.GetAccountByAccountId(1234)).Returns(Task.FromResult(accountModel)!);
            _mockSpRunner.Setup(x => x.GetReadingsByAccountId(1234)).Returns(Task.FromResult(readingsModel)!);
            _mockSpRunner.Setup(x => x.GetAccountByAccountId(1235)).Returns(Task.FromResult<AccountModel?>(null));
            _mockSpRunner.Setup(x => x.GetReadingsByAccountId(1235)).Returns(Task.FromResult(new List<MeterReadingModel>())!);

            _validator = new MeterReadingValidator(_mockSpRunner.Object);
        }

        [Theory]
        [InlineData("1234", "31/12/2024", "11111", "This reading already exists")]
        [InlineData("1234", "31/12/2023", "11111", "This reading is older than an existing read")]
        [InlineData("1235", "01/01/2025", "11111", "Account does not exist")]
        [InlineData("", "01/01/2025", "11111", "Invalid AccountId")]
        [InlineData("123a", "01/01/2025", "11111", "Invalid AccountId")]
        [InlineData("1234", "01/01/202a", "11111", "Invalid ReadingDate")]
        [InlineData("1234", "", "11111", "Invalid ReadingDate")]
        [InlineData("1234", "29/02/2025", "11111", "Invalid ReadingDate")]
        [InlineData("1234", "01/01/2025", "1234a", "Invalid ReadingValue - not an integer")]
        [InlineData("1234", "01/01/2025", "", "Invalid ReadingValue - not an integer")]
        [InlineData("1234", "01/01/2025", "a", "Invalid ReadingValue - not an integer")]
        [InlineData("1234", "01/01/2025", "-1", "Invalid ReadingValue - value is negative")]
        [InlineData("1234", "01/01/2025", "-12345", "Invalid ReadingValue - value is negative")]
        [InlineData("1234", "01/01/2025", "100000", "Invalid ReadingValue - value is over 5 digits long")]
        public async Task ValidateMeterReading_ShouldHaveOneValidationError(string accountId, string readingDate, string readingValue, string expectedError)
        {
            var inputModel = new MeterReadingInputModel
            {
                AccountId = accountId,
                ReadingDate = readingDate,
                ReadingValue = readingValue
            };

            var testOutput = await _validator.ValidateMeterReading(inputModel);

            testOutput.IsValid.Should().BeFalse();
            testOutput.Errors.Count.Should().Be(1);
            testOutput.Errors[0].Should().Be(expectedError);
        }

        [Theory]
        [InlineData("1234", "01/01/2025", "0")]
        [InlineData("1234", "01/01/2025", "1")]
        [InlineData("1234", "01/01/2025", "99999")]
        [InlineData("1234", "January 1 2025 01:23:45.000", "11111")]
        [InlineData("1234", "01/01/2025 12:34", "11111")]
        [InlineData("1234", "2100-12-31", "11111")]
        
        public async Task ValidateMeterReading_ShouldBeValid(string accountId, string readingDate, string readingValue)
        {
            var inputModel = new MeterReadingInputModel
            {
                AccountId = accountId,
                ReadingDate = readingDate,
                ReadingValue = readingValue
            };

            var testOutput = await _validator.ValidateMeterReading(inputModel);

            testOutput.IsValid.Should().BeTrue();
            testOutput.Errors.Count.Should().Be(0);
        }
    }
}