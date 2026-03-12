using FluentValidation;
using FluentValidation.Results;
using Moq;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Application.Services;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Enums;
using VirtualWallet.Domain.Exceptions;

namespace Application.Tests.TransactionServiceTests;

public class WithdrawalTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<TransferRequestDto>> _transferValidatorMock;
    private readonly Mock<IValidator<WithdrawalRequestDto>> _withdrawalValidatorMock;
    private readonly TransactionService _transactionService;

    public WithdrawalTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _transferValidatorMock = new Mock<IValidator<TransferRequestDto>>();
        _withdrawalValidatorMock = new Mock<IValidator<WithdrawalRequestDto>>();

        _withdrawalValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<WithdrawalRequestDto>(), default))
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
    public async Task WithdrawalAsync_ShouldDecreaseBalanceAndCommit_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var initialBalance = 1000m;
        var withdrawalAmount = 300m;

        var mockAccount = new Account
        {
            Id = accountId,
            UserId = userId,
            Balance = initialBalance
        };

        var request = new WithdrawalRequestDto
        {
            Amount = withdrawalAmount,
            Reference = "ATM Withdrawal"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(mockAccount);

        await _transactionService.WithdrawalAsync(request);

        Assert.Equal(700m, mockAccount.Balance);

        _accountRepoMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => a.Balance == 700m)), Times.Once);

        _transactionRepoMock.Verify(x => x.AddAsync(It.Is<Transaction>(t =>
            t.ToAccountId == accountId &&
            t.Amount == withdrawalAmount &&
            t.Type == TransactionType.Withdrawal &&
            t.Status == TransactionStatus.Completed &&
            t.Reference == "ATM Withdrawal"
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task WithdrawalAsync_ShouldThrowBadRequestException_WhenAccountIsNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new WithdrawalRequestDto { Amount = 500m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((Account?)null);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _transactionService.WithdrawalAsync(request));

        Assert.Equal(DomainErrors.Account.AccountNotFound, exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task WithdrawalAsync_ShouldThrowBadRequestException_WhenInsufficientFunds()
    {
        var userId = Guid.NewGuid();

        var mockAccount = new Account { Id = Guid.NewGuid(), UserId = userId, Balance = 50m };

        var request = new WithdrawalRequestDto { Amount = 500m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(mockAccount);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _transactionService.WithdrawalAsync(request));

        Assert.Equal(DomainErrors.Transaction.InsufficientFunds, exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task WithdrawalAsync_ShouldRollbackAndRethrow_WhenDatabaseExceptionOccurs()
    {
        var userId = Guid.NewGuid();
        var mockAccount = new Account { Id = Guid.NewGuid(), UserId = userId, Balance = 1000m };
        var request = new WithdrawalRequestDto { Amount = 500m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(mockAccount);

        _accountRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                        .ThrowsAsync(new Exception("Timeout expired. The timeout period elapsed prior to completion of the operation."));

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _transactionService.WithdrawalAsync(request));

        Assert.Equal("Timeout expired. The timeout period elapsed prior to completion of the operation.", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
}