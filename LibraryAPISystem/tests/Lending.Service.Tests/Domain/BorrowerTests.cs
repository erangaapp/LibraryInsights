using Lending.Service.Domain.Entities;

namespace Lending.Service.Tests.Domain;

public class BorrowerTests
{
    [Fact]
    public void FullName_ReturnsFirstNameAndLastName()
    {
        var borrower = new Borrower()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        Assert.Equal("John Doe", borrower.FullName);
    }
}
