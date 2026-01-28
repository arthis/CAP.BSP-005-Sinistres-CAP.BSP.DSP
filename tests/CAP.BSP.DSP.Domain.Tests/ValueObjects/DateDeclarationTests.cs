using CAP.BSP.DSP.Domain.ValueObjects;

namespace CAP.BSP.DSP.Domain.Tests.ValueObjects;

public class DateDeclarationTests
{
    [Fact]
    public void Now_ShouldCreateWithCurrentUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var dateDeclaration = DateDeclaration.Now();
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.NotNull(dateDeclaration);
        Assert.True(dateDeclaration.Value >= beforeCreation);
        Assert.True(dateDeclaration.Value <= afterCreation);
    }

    [Fact]
    public void Create_WithSpecificDateTime_ShouldSucceed()
    {
        // Arrange
        var specificDate = new DateTime(2026, 1, 28, 10, 30, 45, DateTimeKind.Utc);

        // Act
        var dateDeclaration = DateDeclaration.Create(specificDate);

        // Assert
        Assert.Equal(specificDate, dateDeclaration.Value);
    }

    [Fact]
    public void ImplicitConversion_ToDateTime_ShouldReturnValue()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var dateDeclaration = DateDeclaration.Create(date);

        // Act
        DateTime value = dateDeclaration;

        // Assert
        Assert.Equal(date, value);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedDateTime()
    {
        // Arrange
        var date = new DateTime(2026, 1, 28, 10, 30, 45, DateTimeKind.Utc);
        var dateDeclaration = DateDeclaration.Create(date);

        // Act
        var result = dateDeclaration.ToString();

        // Assert
        Assert.StartsWith("2026-01-28T10:30:45", result);
        Assert.EndsWith("Z", result);
    }

    [Fact]
    public void Equality_WithSameDateTime_ShouldBeEqual()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var dateDeclaration1 = DateDeclaration.Create(date);
        var dateDeclaration2 = DateDeclaration.Create(date);

        // Assert
        Assert.Equal(dateDeclaration1, dateDeclaration2);
        Assert.True(dateDeclaration1 == dateDeclaration2);
    }

    [Fact]
    public void Equality_WithDifferentDateTime_ShouldNotBeEqual()
    {
        // Arrange
        var date1 = DateTime.UtcNow;
        var date2 = DateTime.UtcNow.AddMinutes(1);
        var dateDeclaration1 = DateDeclaration.Create(date1);
        var dateDeclaration2 = DateDeclaration.Create(date2);

        // Assert
        Assert.NotEqual(dateDeclaration1, dateDeclaration2);
        Assert.True(dateDeclaration1 != dateDeclaration2);
    }

    [Fact]
    public void Now_CalledMultipleTimes_ShouldReturnDifferentValues()
    {
        // Act
        var dateDeclaration1 = DateDeclaration.Now();
        System.Threading.Thread.Sleep(10); // Pause pour garantir une différence
        var dateDeclaration2 = DateDeclaration.Now();

        // Assert
        Assert.NotEqual(dateDeclaration1, dateDeclaration2);
        Assert.True(dateDeclaration2.Value > dateDeclaration1.Value);
    }
}
