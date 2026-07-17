using BookDelegates = Lending.Service.Endpoints.Delegates.BookDelegates;
using BorrowedDelegates = Lending.Service.Endpoints.Delegates.BorrowedDelegates;
using LoanDelegates = Lending.Service.Endpoints.Delegates.LoanDelegates;

namespace Lending.Service.Endpoints;

public static class LendingEndpoints
{
    public static void MapLendingEndpoints(this WebApplication app)
    {
        var lending = app.MapGroup("/api/lending");

        #region Books
        var books = lending.MapGroup("/books");

        books.MapGet("/most-borrowed", BookDelegates.GetMostBorrowedBooksAsync);
        books.MapGet("/{bookId:int}/also-borrowed", BookDelegates.GetAlsoBorrowedBooksAsync);
        #endregion

        #region Borrowers
        var borrowers = lending.MapGroup("/borrowers");

        borrowers.MapPost("", BorrowedDelegates.CreateBorrowerAsync);
        borrowers.MapGet("/top", BorrowedDelegates.GetTopBorrowersAsync);
        borrowers.MapGet("/{borrowerId:int}/reading-pace", BorrowedDelegates.GetReadingPaceAsync);
        #endregion

        #region Loans
        var loans = lending.MapGroup("/loans");

        loans.MapPost("", LoanDelegates.BorrowBookAsync);
        loans.MapPost("/{loanId:int}/return", LoanDelegates.ReturnLoanAsync);
        #endregion
    }
}