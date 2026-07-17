using Bogus;
using Lending.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lending.Service.Infrastructure.Seeding;

public static class LendingDataSeeder
{
    // Book ids reference Inventory's seed (identity 1..30, insertion order).
    private const int BookCount = 30;

    // Planted patterns — the demo depends on these:
    private static readonly int[] PopularBookIds = [1, 2, 5];      // clear most-borrowed winners
    private static readonly int[] ClusterBookIds = [1, 7, 12, 19]; // "readers of 1 also read 7/12/19"
    private const int MissingBookId = 31;                          // exists in no catalog → "Unknown title" demo

    public static async Task SeedAsync(LendingDbContext db, CancellationToken ct = default)
    {
        if (await db.Borrowers.AnyAsync(ct))
            return;

        Randomizer.Seed = new Random(20260715);
        var faker = new Faker();

        //########### AI GENERATED SEEDING LOGIC FOR TESTING PURPOSES ###########
        
        // ---- Borrowers (50, unique emails) ----
        var usedEmails = new HashSet<string>();
        var borrowers = Enumerable.Range(1, 50).Select(_ =>
        {
            var firstName = faker.Name.FirstName();
            var lastName = faker.Name.LastName();
            string email;
            do { email = faker.Internet.Email(firstName, lastName); } while (!usedEmails.Add(email));
            return new Borrower { FirstName = firstName, LastName = lastName, Email = email };
        }).ToList();

        db.Borrowers.AddRange(borrowers);
        await db.SaveChangesAsync(ct); // materialize ids 1..50

        // ---- Loans ----
        var loans = new List<Loan>();
        var window = (From: DateTime.UtcNow.AddMonths(-6), To: DateTime.UtcNow);

        DateTime RandomBorrowDate() =>
            faker.Date.Between(window.From, window.To.AddDays(-30));

        Loan MakeLoan(int bookId, int borrowerId, bool returned, int? exactDays = null)
        {
            var borrowedAt = RandomBorrowDate();
            return new Loan
            {
                BookId = bookId,
                BorrowerId = borrowerId,
                BorrowedAt = borrowedAt,
                ReturnedAt = returned
                    ? borrowedAt.AddDays(exactDays ?? faker.Random.Int(1, 28))
                    : null,
            };
        }

        // 1. Popularity: planted winners get many loans (25/20/15), spread across borrowers
        int[] popularCounts = [25, 20, 15];
        foreach (var (bookId, count) in PopularBookIds.Zip(popularCounts))
            for (var i = 0; i < count; i++)
                loans.Add(MakeLoan(bookId, faker.Random.Int(1, 50), returned: faker.Random.Bool(0.8f)));

        // 2. Cluster: borrowers 1-8 each borrow book 1 plus 2-3 others from the cluster
        for (var borrowerId = 1; borrowerId <= 8; borrowerId++)
        {
            loans.Add(MakeLoan(ClusterBookIds[0], borrowerId, returned: true));
            foreach (var bookId in faker.PickRandom(ClusterBookIds[1..], faker.Random.Int(2, 3)))
                loans.Add(MakeLoan(bookId, borrowerId, returned: true));
        }

        // 3. Power readers: borrowers 9-11 get 12-15 loans each (top-borrowers winners),
        //    all returned with plausible durations → clean reading-pace data
        for (var borrowerId = 9; borrowerId <= 11; borrowerId++)
            for (var i = 0; i < faker.Random.Int(12, 15); i++)
                loans.Add(MakeLoan(faker.Random.Int(1, BookCount), borrowerId, returned: true));

        // 4. Edge cases, deliberately:
        loans.Add(MakeLoan(3, 12, returned: true, exactDays: 0));   // same-day return → clamp demo
        loans.Add(MakeLoan(MissingBookId, 13, returned: true));     // unknown book → "Unknown title"
        loans.Add(MakeLoan(4, 14, returned: false));                // borrower with ONLY open loans → empty pace
        // Borrower 15: no loans at all → pace returns empty, not error

        // 5. Background noise: ~200 random loans, 75% returned, borrowers 16-50
        for (var i = 0; i < 200; i++)
            loans.Add(MakeLoan(
                faker.Random.Int(1, BookCount),
                faker.Random.Int(16, 50),
                returned: faker.Random.Bool(0.75f)));

        db.Loans.AddRange(loans);
        await db.SaveChangesAsync(ct);

        //########### END AI GENERATED SEEDING LOGIC ###########
    }
}
