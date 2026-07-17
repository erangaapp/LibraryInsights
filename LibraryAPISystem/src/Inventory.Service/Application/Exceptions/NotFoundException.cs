namespace Inventory.Service.Application.Exceptions;

public class NotFoundException(string message) 
    : Exception(message);
