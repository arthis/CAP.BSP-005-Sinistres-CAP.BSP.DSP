using CAP.BSP.DSP.Domain.Exceptions;

namespace CAP.BSP.DSP.Domain.Tests.Exceptions;

public class DomainExceptionsTests
{
    [Fact]
    public void IdentifiantContratManquantException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new IdentifiantContratManquantException();

        // Assert
        Assert.Contains("IdentifiantContrat obligatoire", exception.Message);
    }

    [Fact]
    public void IdentifiantContratManquantException_CanBeThrown()
    {
        // Act & Assert
        Action action = () => throw new IdentifiantContratManquantException();
        var exception = Assert.Throws<IdentifiantContratManquantException>(action);

        Assert.NotNull(exception);
    }

    [Fact]
    public void IdentifiantContratManquantException_ShouldBeInstanceOfException()
    {
        // Arrange
        var exception = new IdentifiantContratManquantException();

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void DateSurvenanceFutureException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new DateSurvenanceFutureException();

        // Assert
        Assert.Contains("la date de survenance ne peut pas être dans le futur", exception.Message);
    }

    [Fact]
    public void DateSurvenanceFutureException_CanBeThrown()
    {
        // Act & Assert
        Action action = () => throw new DateSurvenanceFutureException();
        var exception = Assert.Throws<DateSurvenanceFutureException>(action);

        Assert.NotNull(exception);
    }

    [Fact]
    public void DateSurvenanceFutureException_ShouldBeInstanceOfException()
    {
        // Arrange
        var exception = new DateSurvenanceFutureException();

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }
}
