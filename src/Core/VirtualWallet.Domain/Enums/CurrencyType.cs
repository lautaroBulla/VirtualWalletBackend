using System.ComponentModel;
using System.Reflection;

namespace VirtualWallet.Domain.Enums
{
    public enum CurrencyType
    {
        UYU,
        USD
    }

    // Por si lo necesito utilizar en algún momento, aunque por ahora no lo uso
    public static class CurrencyTypeExtensions
    {
        public static string GetDescription(this CurrencyType currency) => currency switch
        {
            CurrencyType.UYU => "UYU",
            CurrencyType.USD => "USD",
            _ => currency.ToString() 
        };
    }
}
