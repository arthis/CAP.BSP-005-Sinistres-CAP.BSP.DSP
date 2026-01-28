using CAP.BSP.DSP.Domain.ValueObjects;

namespace CAP.BSP.DSP.Domain.Tests.ValueObjects;

public class IdentifiantContratTests
{
    [Theory]
    [InlineData("POL-20260128-00001")]
    [InlineData("POL-20250101-99999")]
    [InlineData("POL-19991231-12345")]
    public void Create_WithValidFormat_ShouldSucceed(string value)
    {
        // Act
        var identifiant = IdentifiantContrat.Create(value);

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
        var exception = Assert.Throws<ArgumentException>(() => IdentifiantContrat.Create(value));
        Assert.Contains("ne peut pas être vide", exception.Message);
    }

    [Theory]
    [InlineData("POL-2026-00001")] // Année trop courte
    [InlineData("POL-20260128-0001")] // Numéro trop court
    [InlineData("POL-20260128-000001")] // Numéro trop long
    [InlineData("CTR-20260128-00001")] // Préfixe incorrect
    [InlineData("POL20260128-00001")] // Tiret manquant
    [InlineData("POL-20260128")] // Numéro manquant
    [InlineData("INVALID")]
    public void Create_WithInvalidFormat_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => IdentifiantContrat.Create(value));
        Assert.Contains("ne correspond pas au format attendu", exception.Message);
        Assert.Contains("POL-YYYYMMDD-XXXXX", exception.Message);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var identifiant = IdentifiantContrat.Create("POL-20260128-00001");

        // Act
        string value = identifiant;

        // Assert
        Assert.Equal("POL-20260128-00001", value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var identifiant = IdentifiantContrat.Create("POL-20260128-00001");

        // Act
        var result = identifiant.ToString();

        // Assert
        Assert.Equal("POL-20260128-00001", result);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var identifiant1 = IdentifiantContrat.Create("POL-20260128-00001");
        var identifiant2 = IdentifiantContrat.Create("POL-20260128-00001");

        // Assert
        Assert.Equal(identifiant1, identifiant2);
        Assert.True(identifiant1 == identifiant2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var identifiant1 = IdentifiantContrat.Create("POL-20260128-00001");
        var identifiant2 = IdentifiantContrat.Create("POL-20260128-00002");

        // Assert
        Assert.NotEqual(identifiant1, identifiant2);
        Assert.True(identifiant1 != identifiant2);
    }
}
