namespace Lending.Service.Application.Exceptions;

public class ConflictException(string message) 
    : Exception(message);
