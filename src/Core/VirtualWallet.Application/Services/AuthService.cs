using FluentValidation;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Exceptions;

namespace VirtualWallet.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccountNumberGenerator _accountNumberGenerator;
        private readonly IValidator<RegisterUserDto> _validator;

        public AuthService(
            IUserRepository userRepository,
            IAccountRepository accountRepository,
            IPasswordHasher passwordHasher,
            IAccountNumberGenerator accountNumberGenerator,
            IValidator<RegisterUserDto> validator)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
            _accountNumberGenerator = accountNumberGenerator;
            _validator = validator;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var emailExists = await _userRepository.EmailExistsAsync(dto.Email);
            if (emailExists)
            {
                throw new BadRequestException(DomainErrors.User.EmailAlreadyInUse);
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
            };
            await _userRepository.AddAsync(user);

            var uniqueAccountNumber = await _accountNumberGenerator.GenerateUniqueAccountNumberAsync();

            var account = new Account
            {
                UserId = user.Id,
                AccountNumber = uniqueAccountNumber,
                Balance = 0m,
            };
            await _accountRepository.AddAsync(account);

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                AccountNumber = account.AccountNumber
            };
        }
    }
}
