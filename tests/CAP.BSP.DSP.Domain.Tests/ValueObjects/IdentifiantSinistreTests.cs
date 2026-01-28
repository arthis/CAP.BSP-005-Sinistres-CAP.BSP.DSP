using CAP.BSP.DSP.Domain.ValueObjects;

namespace CAP.BSP.DSP.Domain.Tests.ValueObjects;

public class IdentifiantSinistreTests
{
    [Theory]
    [InlineData("SIN-2026-000001")]
    [InlineData("SIN-2025-999999")]
    [InlineData("SIN-1999-123456")]
    public void Create_WithValidFormat_ShouldSucceed(string value)
    {
        // Act
        var identifiant = IdentifiantSinistre.Create(value);

        // Assert
        Assert.NotNull(identifiant);
        Assert.Equal(value, identifiant.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => IdentifiantSinistre.Create(value));
        Assert.Contains("ne peut pas être vide", exception.Message);
    }

    [Theory]
    [InlineData("SIN-26-000001")] // Année trop courte
    [InlineData("SIN-2026-00001")] // Numéro trop court
    [InlineData("SIN-2026-0000001")] // Numéro trop long
    [InlineData("CLM-2026-000001")] // Préfixe incorrect
    [InlineData("SIN2026-000001")] // Tiret manquant
    [InlineData("SIN-2026")] // Numéro manquant
    [InlineData("INVALID")]
    public void Create_WithInvalidFormat_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => IdentifiantSinistre.Create(value));
        Assert.Contains("ne correspond pas au format attendu", exception.Message);
        Assert.Contains("SIN-YYYY-NNNNNN", exception.Message);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var identifiant = IdentifiantSinistre.Create("SIN-2026-000001");

        // Act
        string value = identifiant;

        // Assert
        Assert.Equal("SIN-2026-000001", value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var identifiant = IdentifiantSinistre.Create("SIN-2026-000001");

        // Act
        var result = identifiant.ToString();

        // Assert
        Assert.Equal("SIN-2026-000001", result);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var identifiant1 = IdentifiantSinistre.Create("SIN-2026-000001");
        var identifiant2 = IdentifiantSinistre.Create("SIN-2026-000001");

        // Assert
        Assert.Equal(identifiant1, identifiant2);
        Assert.True(identifiant1 == identifiant2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var identifiant1 = IdentifiantSinistre.Create("SIN-2026-000001");
        var identifiant2 = IdentifiantSinistre.Create("SIN-2026-000002");

        // Assert
        Assert.NotEqual(identifiant1, identifiant2);
        Assert.True(identifiant1 != identifiant2);
    }
}
