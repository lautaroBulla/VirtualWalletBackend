using FluentValidation;
using FluentValidation.Results;
using Moq;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Application.Services;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Exceptions;

namespace Application.Tests.TransactionServiceTests;

public class MakeTransferTests
{
    [Fact]
    public async Task MakeTransferAsync_Exception_InsufficientFunds()
    {
        var accountRepoMock = new Mock<IAccountRepository>();
        var transactionRepoMock = new Mock<ITransactionRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var validatorMock = new Mock<IValidator<TransferRequestDto>>();

        var miUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myFakeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = miUserId,
            Balance = 50
        };

        var fakeDestinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            AccountNumber = "123456789"
        };

        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(miUserId);

        accountRepoMock.Setup(x => x.GetByUserIdAsync(miUserId)).ReturnsAsync(myFakeAccount);
        accountRepoMock.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(fakeDestinationAccount);

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransferRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var transactionService = new TransactionService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            unitOfWorkMock.Object,
            validatorMock.Object,
            currentUserServiceMock.Object
        );

        var request = new TransferRequestDto
        {
            ToAccountNumber = "123456789",
            Amount = 100,
            Reference = "Prueba de fallo"
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            transactionService.MakeTransferAsync(request));

        Assert.Equal(DomainErrors.Transaction.InsufficientFunds, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_Exception_SameAccountTransfer()
    {
        var accountRepoMock = new Mock<IAccountRepository>();
        var transactionRepoMock = new Mock<ITransactionRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var validatorMock = new Mock<IValidator<TransferRequestDto>>();

        var miUserId = Guid.NewGuid();

        var myFakeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = miUserId,
            AccountNumber = "123456789",
            Balance = 50
        };

        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(miUserId);

        accountRepoMock.Setup(x => x.GetByUserIdAsync(miUserId)).ReturnsAsync(myFakeAccount);
        accountRepoMock.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(myFakeAccount);

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransferRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var transactionService = new TransactionService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            unitOfWorkMock.Object,
            validatorMock.Object,
            currentUserServiceMock.Object
        );

        var request = new TransferRequestDto
        {
            ToAccountNumber = "123456789",
            Amount = 100,
            Reference = "Prueba de fallo"
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            transactionService.MakeTransferAsync(request));

        Assert.Equal(DomainErrors.Transaction.SameAccountTransfer, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_Exception_FromAccountNotFound()
    {
        var accountRepoMock = new Mock<IAccountRepository>();
        var transactionRepoMock = new Mock<ITransactionRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var validatorMock = new Mock<IValidator<TransferRequestDto>>();

        var miUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myFakeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = miUserId,
            Balance = 50
        };

        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(otherUserId);

        accountRepoMock.Setup(x => x.GetByUserIdAsync(otherUserId)).ReturnsAsync((Account?)null);

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransferRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var transactionService = new TransactionService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            unitOfWorkMock.Object,
            validatorMock.Object,
            currentUserServiceMock.Object
        );

        var request = new TransferRequestDto
        {
            ToAccountNumber = "123456781",
            Amount = 50,
            Reference = "Prueba de fallo"
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            transactionService.MakeTransferAsync(request));

        Assert.Equal(DomainErrors.Account.FromAccountNotFound, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_Exception_ToAccountNotFound()
    {
        var accountRepoMock = new Mock<IAccountRepository>();
        var transactionRepoMock = new Mock<ITransactionRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var validatorMock = new Mock<IValidator<TransferRequestDto>>();

        var miUserId = Guid.NewGuid();

        var myFakeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = miUserId,
            Balance = 50
        };

        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(miUserId);

        accountRepoMock.Setup(x => x.GetByUserIdAsync(miUserId)).ReturnsAsync(myFakeAccount);
        accountRepoMock.Setup(x => x.GetByAccountNumberAsync("123456781")).ReturnsAsync((Account?)null);

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransferRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var transactionService = new TransactionService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            unitOfWorkMock.Object,
            validatorMock.Object,
            currentUserServiceMock.Object
        );

        var request = new TransferRequestDto
        {
            ToAccountNumber = "123456781",
            Amount = 50,
            Reference = "Prueba de fallo"
        };

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            transactionService.MakeTransferAsync(request));

        Assert.Equal(DomainErrors.Account.ToAccountNotFound, exception.Message);
    }

    [Fact]
    public async Task MakeTransferAsync_Success()
    {
        var accountRepoMock = new Mock<IAccountRepository>();
        var transactionRepoMock = new Mock<ITransactionRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var validatorMock = new Mock<IValidator<TransferRequestDto>>();

        var miUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myFakeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = miUserId,
            Balance = 50
        };

        var fakeDestinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            AccountNumber = "123456789"
        };

        currentUserServiceMock.Setup(x => x.GetUserId()).Returns(miUserId);

        accountRepoMock.Setup(x => x.GetByUserIdAsync(miUserId)).ReturnsAsync(myFakeAccount);
        accountRepoMock.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(fakeDestinationAccount);

        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TransferRequestDto>(), default))
            .ReturnsAsync(new ValidationResult());

        var transactionService = new TransactionService(
            accountRepoMock.Object,
            transactionRepoMock.Object,
            unitOfWorkMock.Object,
            validatorMock.Object,
            currentUserServiceMock.Object
        );

        var request = new TransferRequestDto
        {
            ToAccountNumber = "123456789",
            Amount = 50,
            Reference = "Prueba de fallo"
        };

        await transactionService.MakeTransferAsync(request);

        Assert.Equal(0, myFakeAccount.Balance);
        Assert.Equal(50, fakeDestinationAccount.Balance);

        unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }
}