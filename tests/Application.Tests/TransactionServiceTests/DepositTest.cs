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

public class DepositTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<TransferRequestDto>> _transferValidatorMock;
    private readonly Mock<IValidator<DepositRequestDto>> _depositValidatorMock;
    private readonly Mock<IValidator<WithdrawalRequestDto>> _withdrawalValidatorMock;
    private readonly TransactionService _transactionService;

    public DepositTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _transferValidatorMock = new Mock<IValidator<TransferRequestDto>>();
        _depositValidatorMock = new Mock<IValidator<DepositRequestDto>>();
        _withdrawalValidatorMock = new Mock<IValidator<WithdrawalRequestDto>>();

        _depositValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DepositRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _transactionService = new TransactionService(
            _accountRepoMock.Object,
            _transactionRepoMock.Object,
            _unitOfWorkMock.Object,
            _transferValidatorMock.Object,
            _depositValidatorMock.Object,
            _withdrawalValidatorMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task DepositAsync_ShouldIncreaseBalanceAndCommit_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var initialBalance = 1000m;
        var depositAmount = 500m;

        var mockAccount = new Account
        {
            Id = accountId,
            UserId = userId,
            Balance = initialBalance
        };

        var request = new DepositRequestDto
        {
            Amount = depositAmount,
            Reference = "Salary deposit"
        };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(mockAccount);

        await _transactionService.DepositAsync(request);

        Assert.Equal(1500m, mockAccount.Balance);

        _accountRepoMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => a.Balance == 1500m)), Times.Once);

        _transactionRepoMock.Verify(x => x.AddAsync(It.Is<Transaction>(t =>
            t.ToAccountId == accountId &&
            t.Amount == depositAmount &&
            t.Type == TransactionType.Deposit &&
            t.Status == TransactionStatus.Completed &&
            t.Reference == "Salary deposit"
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task DepositAsync_ShouldThrowBadRequestException_WhenAccountIsNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new DepositRequestDto { Amount = 500m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((Account?)null);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _transactionService.DepositAsync(request));

        Assert.Equal(DomainErrors.Account.AccountNotFound, exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task DepositAsync_ShouldRollbackAndRethrow_WhenDatabaseExceptionOccurs()
    {
        var userId = Guid.NewGuid();
        var mockAccount = new Account { Id = Guid.NewGuid(), UserId = userId, Balance = 1000m };
        var request = new DepositRequestDto { Amount = 500m };

        _currentUserServiceMock.Setup(x => x.GetUserId()).Returns(userId);
        _accountRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(mockAccount);

        _transactionRepoMock.Setup(x => x.AddAsync(It.IsAny<Transaction>()))
                            .ThrowsAsync(new Exception("Database connection lost"));

        // Atrapamos la excepción genérica que explotó
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _transactionService.DepositAsync(request));

        Assert.Equal("Database connection lost", exception.Message);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
}