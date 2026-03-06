namespace VirtualWallet.Domain.Exceptions
{
    public static class DomainErrors
    {
        public static class User
        {
            public const string EmailAlreadyInUse = "Email already in use.";
            public const string InvalidCredentials = "Invalid email or password.";
            public const string InvalidToken = "Invalid token.";
        }

        public static class Account
        {
            public const string InsufficientFunds = "Insufficient funds.";
            public const string AccountNotFound = "Account not found.";
            public const string FromAccountNotFound = "Account not found.";
            public const string ToAccountNotFound = "Account not found.";
        }

        public static class Transaction
        {
            public const string InvalidAmount = "Amount must be greater than zero.";
            public const string SameAccountTransfer = "Cannot transfer to the same account.";
            public const string InsufficientFunds = "Insufficient funds.";
        }
    }
}
