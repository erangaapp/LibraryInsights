namespace Inventory.Service.Domain.Entities;

public class Book
{
    public int Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Author { get; private set; } = null!;
    public string Isbn { get; private set; } = null!;
    public int Pages { get; private set; }
    public int TotalCopies { get; private set; }
    public DateOnly DateReceived { get; private set; }
    public DateOnly? DiscontinuedDate { get; private set; }

    // Private constructor for EF Core
    private Book() { }

    // Book Creation 
    public Book(string title, string author, string isbn, int pages, DateOnly dateReceived, int totalCopies = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        // Validation for author
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty.", nameof(author));

        // Validation for isbn
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN cannot be empty.", nameof(isbn));

        if (pages <= 0)
            throw new ArgumentOutOfRangeException(nameof(pages), "Book must have at least 1 page.");

        if (totalCopies < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCopies), "Total copies cannot be negative.");

        Title = title.Trim();
        Author = author.Trim();
        Isbn = isbn.Trim();
        Pages = pages;
        DateReceived = dateReceived;
        TotalCopies = totalCopies;
    }

    public void UpdateInventory(int newTotalCopies)
    {
        if (newTotalCopies < 0)
            throw new InvalidOperationException("Total copies cannot be negative.");

        TotalCopies = newTotalCopies;
    }

    public void Discontinue(DateOnly discontinueDate)
    {
        if (discontinueDate < DateReceived)
            throw new InvalidOperationException("Cannot discontinue a book before it was received.");

        DiscontinuedDate = discontinueDate;
    }

    public void CorrectDetails(string? title, string? author, string? isbn, int? pages, int? totalCopies)
    {
        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));
            Title = title.Trim();
        }

        if (author is not null)
        {
            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("Author cannot be empty.", nameof(author));
            Author = author.Trim();
        }

        if (isbn is not null)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN cannot be empty.", nameof(isbn));
            Isbn = isbn.Trim();
        }

        if (pages.HasValue)
        {
            if (pages.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(pages), "Book must have at least 1 page.");
            Pages = pages.Value;
        }

        if (totalCopies.HasValue)
        {
            UpdateInventory(totalCopies.Value);
        }
    }
}
