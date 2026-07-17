using Lending.Service.Application.Models;
using Library.Contracts.Inventory.Common;

namespace Lending.Service.Extensions;

public static class Mapper
{
    public static CatalogBook ToCatalog(this BookInfo? book)
        => book == null ? CatalogBook.Unknown(0) : new(book.Id, book.Title,
            book.Author, book.Pages);

    public static CatalogBook ToCatalog(this Book? book)
        => book == null ? CatalogBook.Unknown(0) : new(book.Id, book.Title,
            book.Author, book.Pages, book.IsDiscontinued, book.Copies);
}
