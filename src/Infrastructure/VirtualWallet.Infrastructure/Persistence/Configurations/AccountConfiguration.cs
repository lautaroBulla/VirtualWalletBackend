using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Enums;

namespace VirtualWallet.Infrastructure.Persistence.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccountNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(a => a.AccountNumber)
                .IsUnique();

            builder.Property(a => a.Balance)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(a => a.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue(nameof(CurrencyType.UYU));

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
