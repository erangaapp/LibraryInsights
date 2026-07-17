using Inventory.Service.Application.Queries.GetBooks;
using MediatR;

namespace Inventory.Service.Application.Queries.GetBookById;

public record GetBookByIdQuery(int Id) 
    : IRequest<BookDto?>;
