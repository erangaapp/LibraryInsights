using Lending.Service.Application.Commands.BorrowBook;
using MediatR;

namespace Lending.Service.Application.Commands.ReturnLoan;

public record ReturnLoanCommand(int LoanId) 
    : IRequest<LoanDto>;
