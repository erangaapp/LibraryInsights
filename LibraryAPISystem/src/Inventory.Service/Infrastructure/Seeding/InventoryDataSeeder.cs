using Bogus;
using Inventory.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Infrastructure.Seeding;

public static class InventoryDataSeeder
{
    /// <summary>
    /// A static array of tuples representing a catalog of books, where each tuple contains the title, author, and number of pages of a book.
    /// Seeding purpose only, not for production use. The data is hardcoded and should be replaced with a proper data source in a real application.
    /// </summary>
    private static readonly (string Title, string Author, int Pages)[] Catalog =
    [
        ("The Hobbit", "J.R.R. Tolkien", 310),
        ("1984", "George Orwell", 328),
        ("Pride and Prejudice", "Jane Austen", 432),
        ("To Kill a Mockingbird", "Harper Lee", 281),
        ("The Great Gatsby", "F. Scott Fitzgerald", 180),
        ("Moby-Dick", "Herman Melville", 635),
        ("War and Peace", "Leo Tolstoy", 1225),
        ("Crime and Punishment", "Fyodor Dostoevsky", 671),
        ("The Catcher in the Rye", "J.D. Salinger", 234),
        ("Brave New World", "Aldous Huxley", 311),
        ("The Alchemist", "Paulo Coelho", 208),
        ("One Hundred Years of Solitude", "Gabriel García Márquez", 417),
        ("The Kite Runner", "Khaled Hosseini", 371),
        ("Life of Pi", "Yann Martel", 319),
        ("The Old Man and the Sea", "Ernest Hemingway", 127),
        ("Frankenstein", "Mary Shelley", 280),
        ("Dracula", "Bram Stoker", 418),
        ("Jane Eyre", "Charlotte Brontë", 507),
        ("Wuthering Heights", "Emily Brontë", 342),
        ("Great Expectations", "Charles Dickens", 505),
        ("The Adventures of Huckleberry Finn", "Mark Twain", 366),
        ("Anna Karenina", "Leo Tolstoy", 864),
        ("Don Quixote", "Miguel de Cervantes", 863),
        ("The Odyssey", "Homer", 541),
        ("Fahrenheit 451", "Ray Bradbury", 194),
        ("Animal Farm", "George Orwell", 112),
        ("Lord of the Flies", "William Golding", 224),
        ("The Grapes of Wrath", "John Steinbeck", 464),
        ("Slaughterhouse-Five", "Kurt Vonnegut", 275),
        ("Catch-22", "Joseph Heller", 453),
    ];

    public static async Task SeedAsync(InventoryDbContext db, CancellationToken ct = default)
    {
        if (await db.Books.AnyAsync(ct))
            return; // Guard for idempotent

        Randomizer.Seed = new Random(20260715); // deterministic
        var faker = new Faker();
        var usedIsbns = new HashSet<string>();

        var books = Catalog.Select(c => new Book
        (
            title: c.Title,
            author: c.Author,
            isbn: GenerateUniqueIsbn(faker, usedIsbns),
            pages: c.Pages,
            dateReceived: faker.Date.PastDateOnly(5),
            totalCopies: faker.Random.Int(1, 5)
        )).ToList();
        
        books[0].UpdateInventory(10);
        books[1].UpdateInventory(8);
        books[4].UpdateInventory(8);

        books[29].Discontinue(faker.Date.PastDateOnly(1));

        db.Books.AddRange(books);
        await db.SaveChangesAsync(ct);
    }

    private static string GenerateUniqueIsbn(Faker faker, HashSet<string> used)
    {
        string isbn;
        do { isbn = faker.Random.ReplaceNumbers("978##########"); }
        while (!used.Add(isbn)); // guards the unique index
        return isbn;
    }
}
