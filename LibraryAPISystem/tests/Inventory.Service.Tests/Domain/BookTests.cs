using Inventory.Service.Domain.Entities;

namespace Inventory.Service.Tests.Domain;

public class BookTests
{
    private readonly DateOnly _today = new(2026, 7, 17);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesBookSuccessfully()
    {
        // Act
        var book = new Book("  Clean Code  ", "  Robert Martin  ", "  9780132350884  ", 464, _today, 5);

        // Assert
        Assert.Equal("Clean Code", book.Title);       // Verifies Trim
        Assert.Equal("Robert Martin", book.Author);   // Verifies Trim
        Assert.Equal("9780132350884", book.Isbn);     // Verifies Trim
        Assert.Equal(464, book.Pages);
        Assert.Equal(_today, book.DateReceived);
        Assert.Equal(5, book.TotalCopies);
        Assert.Null(book.DiscontinuedDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTitle_ThrowsArgumentException(string? invalidTitle)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Book(invalidTitle!, "Author", "ISBN", 100, _today));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidAuthor_ThrowsArgumentException(string? invalidAuthor)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Book("Title", invalidAuthor!, "ISBN", 100, _today));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidIsbn_ThrowsArgumentException(string? invalidIsbn)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Book("Title", "Author", invalidIsbn!, 100, _today));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_WithInvalidPages_ThrowsArgumentOutOfRangeException(int invalidPages)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Book("Title", "Author", "ISBN", invalidPages, _today));
    }

    [Fact]
    public void Constructor_WithNegativeTotalCopies_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Book("Title", "Author", "ISBN", 100, _today, totalCopies: -1));
    }

    #endregion

    #region UpdateInventory Tests

    [Fact]
    public void UpdateInventory_WithValidCopies_UpdatesTotalCopies()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", 100, _today, 5);

        // Act
        book.UpdateInventory(12);

        // Assert
        Assert.Equal(12, book.TotalCopies);
    }

    [Fact]
    public void UpdateInventory_WithNegativeCopies_ThrowsInvalidOperationException()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", 100, _today, 5);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => book.UpdateInventory(-3));
    }

    #endregion

    #region Discontinue Tests

    [Fact]
    public void Discontinue_WithValidDate_SetsDiscontinuedDate()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", 100, _today);
        var discontinueDate = _today.AddDays(1);

        // Act
        book.Discontinue(discontinueDate);

        // Assert
        Assert.Equal(discontinueDate, book.DiscontinuedDate);
    }

    [Fact]
    public void Discontinue_WithDateBeforeReceivedDate_ThrowsInvalidOperationException()
    {
        // Arrange
        var book = new Book("Title", "Author", "ISBN", 100, _today);
        var invalidDate = _today.AddDays(-1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => book.Discontinue(invalidDate));
    }

    #endregion

    #region CorrectDetails Tests

    [Fact]
    public void CorrectDetails_WithValidChanges_UpdatesPropertiesAndTrimsStrings()
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today, 1);

        // Act
        book.CorrectDetails(
            title: "  New Title  ",
            author: "  New Author  ",
            isbn: "  New ISBN  ",
            pages: 150,
            totalCopies: 10
        );

        // Assert
        Assert.Equal("New Title", book.Title);
        Assert.Equal("New Author", book.Author);
        Assert.Equal("New ISBN", book.Isbn);
        Assert.Equal(150, book.Pages);
        Assert.Equal(10, book.TotalCopies);
    }

    [Fact]
    public void CorrectDetails_WhenArgumentsAreNull_KeepsOriginalProperties()
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today, 1);

        // Act
        book.CorrectDetails(title: null, author: null, isbn: null, pages: null, totalCopies: null);

        // Assert
        Assert.Equal("Original Title", book.Title);
        Assert.Equal("Original Author", book.Author);
        Assert.Equal("Original ISBN", book.Isbn);
        Assert.Equal(100, book.Pages);
        Assert.Equal(1, book.TotalCopies);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CorrectDetails_WithInvalidTitle_ThrowsArgumentException(string invalidTitle)
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            book.CorrectDetails(title: invalidTitle, author: null, isbn: null, pages: null, totalCopies: null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CorrectDetails_WithInvalidAuthor_ThrowsArgumentException(string invalidAuthor)
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            book.CorrectDetails(title: null, author: invalidAuthor, isbn: null, pages: null, totalCopies: null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CorrectDetails_WithInvalidIsbn_ThrowsArgumentException(string invalidIsbn)
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            book.CorrectDetails(title: null, author: null, isbn: invalidIsbn, pages: null, totalCopies: null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CorrectDetails_WithInvalidPages_ThrowsArgumentOutOfRangeException(int invalidPages)
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            book.CorrectDetails(title: null, author: null, isbn: null, pages: invalidPages, totalCopies: null));
    }

    [Fact]
    public void CorrectDetails_WithNegativeTotalCopies_ThrowsInvalidOperationException()
    {
        // Arrange
        var book = new Book("Original Title", "Original Author", "Original ISBN", 100, _today, 5);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            book.CorrectDetails(title: null, author: null, isbn: null, pages: null, totalCopies: -1));
    }

    #endregion
}