namespace Lending.Service.Domain.Entities;

public class Borrower
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string FullName { get { return $"{FirstName} {LastName}"; } }
}
