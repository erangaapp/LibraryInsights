namespace Lending.Service.Application.Queries.GetReadingPace;

public record ReadingPaceDto(
    int BorrowerId,
    string BorrowerName,
    double? AveragePagesPerDay,
    IReadOnlyList<ReadingPaceBookDto> Books);

public record ReadingPaceBookDto(int BookId, string Title,
    int Pages, int Days, double PagesPerDay);