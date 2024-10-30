namespace GymTracker.Tests.Services;

public class ExampleService
{
    public string ExampleMethod()
    {
        return "Hello World!";
    }
}

public class ExampleServiceTest
{
    [Fact]
    public void ExampleTest()
    {
        // Arrange
        var service = new ExampleService();

        // Act
        var result = service.ExampleMethod();

        // Assert
        Assert.Equal("Hello World!", result);
    }
}
