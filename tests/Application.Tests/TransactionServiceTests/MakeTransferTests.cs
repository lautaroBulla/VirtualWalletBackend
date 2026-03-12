using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Application.Services;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Exceptions;

namespace Application.Tests.TransactionServiceTests;

public class MakeTransferTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<TransferRequestDto>> _transferValidatorMock;
    private readonly Mock<IValidator<WithdrawalRequestDto>> _withdrawalValidatorMock;
    private readonly TransactionService _transactionService;

    public MakeTransferTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _transferValidatorMock = new Mock<IValidator<TransferRequestDto>>();
        _withdrawalValidatorMock = new Mock<IValidator<WithdrawalRequestDto>>();

        _transferValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransferRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _transactionService = new TransactionService(
            _accountRepoMock.Object,
            _transactionRepoMock.Object,
            _unitOfWorkMock.Object,
            _transferValidatorMock.Object,
            _withdrawalValidatorMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldCompleteTransfer_WhenRequestIsValid()
    {
        var currentUserId = Guid.NewGuid();
        var fromAccount = new Account { Id = Guid.NewGuid(), UserId = currentUserId, Balance = 100m };
        var toAccount = new Account { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), AccountNumber = "123456789", Balance = 50m };
        var request = new TransferRequestDto { ToAccountNumber = "123456789", Amount = 50m, Reference = "Test" };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(currentUserId)).ReturnsAsync(fromAccount);
        _accountRepoMock.Setup(x => x.GetByAccountNumberAsync(request.ToAccountNumber)).ReturnsAsync(toAccount);

        await _transactionService.MakeTransferAsync(request);

        Assert.Equal(50m, fromAccount.Balance);
        Assert.Equal(100m, toAccount.Balance);

        _accountRepoMock.Verify(x => x.UpdateAsync(fromAccount), Times.Once);
        _accountRepoMock.Verify(x => x.UpdateAsync(toAccount), Times.Once);
        _transactionRepoMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Once);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldThrowValidationException_WhenDtoIsInvalid()
    {
        var request = new TransferRequestDto { Amount = -10 }; 
        var validationFailures = new List<ValidationFailure> { new("Amount", "Invalid amount") };
        var invalidResult = new ValidationResult(validationFailures);

        _transferValidatorMock.Setup(v => v.ValidateAsync(request, default)).ReturnsAsync(invalidResult);

        await Assert.ThrowsAsync<ValidationException>(() => _transactionService.MakeTransferAsync(request));

        _accountRepoMock.Verify(x => x.GetByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldThrowBadRequestException_WhenFromAccountNotFound()
    {
        var currentUserId = Guid.NewGuid();
        var request = new TransferRequestDto { ToAccountNumber = "123456781", Amount = 50m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(currentUserId)).ReturnsAsync((Account?)null);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _transactionService.MakeTransferAsync(request));
        Assert.Equal(DomainErrors.Account.FromAccountNotFound, exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldThrowBadRequestException_WhenToAccountNotFound()
    {
        var currentUserId = Guid.NewGuid();
        var fromAccount = new Account { Id = Guid.NewGuid(), UserId = currentUserId, Balance = 50m };
        var request = new TransferRequestDto { ToAccountNumber = "INVALID_ACC", Amount = 50m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(currentUserId)).ReturnsAsync(fromAccount);
        _accountRepoMock.Setup(x => x.GetByAccountNumberAsync(request.ToAccountNumber)).ReturnsAsync((Account?)null);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _transactionService.MakeTransferAsync(request));
        Assert.Equal(DomainErrors.Account.ToAccountNotFound, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldThrowBadRequestException_WhenSameAccountTransfer()
    {
        var currentUserId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var myAccount = new Account { Id = accountId, UserId = currentUserId, AccountNumber = "MY_ACC", Balance = 50m };
        var request = new TransferRequestDto { ToAccountNumber = "MY_ACC", Amount = 10m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(currentUserId)).ReturnsAsync(myAccount);
        _accountRepoMock.Setup(x => x.GetByAccountNumberAsync(request.ToAccountNumber)).ReturnsAsync(myAccount); 

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _transactionService.MakeTransferAsync(request));
        Assert.Equal(DomainErrors.Transaction.SameAccountTransfer, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldThrowBadRequestException_WhenInsufficientFunds()
    {
        var currentUserId = Guid.NewGuid();
        var fromAccount = new Account { Id = Guid.NewGuid(), UserId = currentUserId, Balance = 50m }; 
        var toAccount = new Account { Id = Guid.NewGuid(), AccountNumber = "TARGET_ACC" };
        var request = new TransferRequestDto { ToAccountNumber = "TARGET_ACC", Amount = 100m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(currentUserId)).ReturnsAsync(fromAccount);
        _accountRepoMock.Setup(x => x.GetByAccountNumberAsync(request.ToAccountNumber)).ReturnsAsync(toAccount);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _transactionService.MakeTransferAsync(request));
        Assert.Equal(DomainErrors.Transaction.InsufficientFunds, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_ShouldRollbackAndRethrow_WhenDatabaseExceptionOccurs()
    {
        var currentUserId = Guid.NewGuid();
        var fromAccount = new Account { Id = Guid.NewGuid(), UserId = currentUserId, Balance = 100m };
        var toAccount = new Account { Id = Guid.NewGuid(), AccountNumber = "TARGET_ACC", Balance = 0m };
        var request = new TransferRequestDto { ToAccountNumber = "TARGET_ACC", Amount = 50m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(currentUserId)).ReturnsAsync(fromAccount);
        _accountRepoMock.Setup(x => x.GetByAccountNumberAsync(request.ToAccountNumber)).ReturnsAsync(toAccount);

        _accountRepoMock.Setup(x => x.UpdateAsync(toAccount)).ThrowsAsync(new Exception("Deadlock detected"));

        var exception = await Assert.ThrowsAsync<Exception>(() => _transactionService.MakeTransferAsync(request));
        Assert.Equal("Deadlock detected", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
}