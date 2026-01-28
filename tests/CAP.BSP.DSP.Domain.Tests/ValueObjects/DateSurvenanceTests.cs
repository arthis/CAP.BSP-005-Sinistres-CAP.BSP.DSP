using CAP.BSP.DSP.Domain.ValueObjects;

namespace CAP.BSP.DSP.Domain.Tests.ValueObjects;

public class DateSurvenanceTests
{
    [Fact]
    public void Create_WithTodayDate_ShouldSucceed()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var dateSurvenance = DateSurvenance.Create(today);

        // Assert
        Assert.NotNull(dateSurvenance);
        Assert.Equal(today, dateSurvenance.Value);
    }

    [Fact]
    public void Create_WithPastDate_ShouldSucceed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-10).Date;

        // Act
        var dateSurvenance = DateSurvenance.Create(pastDate);

        // Assert
        Assert.NotNull(dateSurvenance);
        Assert.Equal(pastDate, dateSurvenance.Value);
    }

    [Fact]
    public void Create_WithFutureDate_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DateSurvenance.Create(futureDate));
        Assert.Contains("ne peut pas être dans le futur", exception.Message);
        Assert.Contains(futureDate.Date.ToString("yyyy-MM-dd"), exception.Message);
    }

    [Fact]
    public void Create_ShouldNormalizeToDateOnly()
    {
        // Arrange
        var dateTimeWithTime = new DateTime(2026, 1, 15, 14, 30, 45);
        var expectedDate = new DateTime(2026, 1, 15, 0, 0, 0);

        // Act
        var dateSurvenance = DateSurvenance.Create(dateTimeWithTime);

        // Assert
        Assert.Equal(expectedDate, dateSurvenance.Value);
        Assert.Equal(TimeSpan.Zero, dateSurvenance.Value.TimeOfDay);
    }

    [Fact]
    public void ImplicitConversion_ToDateTime_ShouldReturnValue()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var dateSurvenance = DateSurvenance.Create(date);

        // Act
        DateTime value = dateSurvenance;

        // Assert
        Assert.Equal(date, value);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedDate()
    {
        // Arrange
        var date = new DateTime(2026, 1, 28);
        var dateSurvenance = DateSurvenance.Create(date);

        // Act
        var result = dateSurvenance.ToString();

        // Assert
        Assert.Equal("2026-01-28", result);
    }

    [Fact]
    public void Equality_WithSameDate_ShouldBeEqual()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var dateSurvenance1 = DateSurvenance.Create(date);
        var dateSurvenance2 = DateSurvenance.Create(date);

        // Assert
        Assert.Equal(dateSurvenance1, dateSurvenance2);
        Assert.True(dateSurvenance1 == dateSurvenance2);
    }

    [Fact]
    public void Equality_WithDifferentDate_ShouldNotBeEqual()
    {
        // Arrange
        var date1 = DateTime.UtcNow.Date;
        var date2 = DateTime.UtcNow.AddDays(-1).Date;
        var dateSurvenance1 = DateSurvenance.Create(date1);
        var dateSurvenance2 = DateSurvenance.Create(date2);

        // Assert
        Assert.NotEqual(dateSurvenance1, dateSurvenance2);
        Assert.True(dateSurvenance1 != dateSurvenance2);
    }

    [Theory]
    [InlineData(-365)] // 1 an dans le passé
    [InlineData(-30)]  // 1 mois dans le passé
    [InlineData(-7)]   // 1 semaine dans le passé
    [InlineData(-1)]   // Hier
    [InlineData(0)]    // Aujourd'hui
    public void Create_WithValidPastOrTodayDates_ShouldSucceed(int daysOffset)
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(daysOffset).Date;

        // Act
        var dateSurvenance = DateSurvenance.Create(date);

        // Assert
        Assert.Equal(date, dateSurvenance.Value);
    }

    [Theory]
    [InlineData(1)]    // Demain
    [InlineData(7)]    // 1 semaine dans le futur
    [InlineData(30)]   // 1 mois dans le futur
    [InlineData(365)]  // 1 an dans le futur
    public void Create_WithFutureDates_ShouldThrowArgumentException(int daysOffset)
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(daysOffset);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DateSurvenance.Create(futureDate));
        Assert.Contains("ne peut pas être dans le futur", exception.Message);
    }
}
