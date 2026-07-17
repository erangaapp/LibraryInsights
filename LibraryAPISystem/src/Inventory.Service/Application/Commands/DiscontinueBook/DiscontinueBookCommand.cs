using MediatR;

namespace Inventory.Service.Application.Commands.DiscontinueBook;

public record DiscontinueBookCommand(int BookId) : IRequest;
