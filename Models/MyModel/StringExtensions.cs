using System;
using System.Globalization;
using RahalWeb.Extensions;
namespace RahalWeb.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string representation of a monetary value to a decimal.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The decimal representation of the monetary value.</returns>
        public static decimal ToMoney(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.", nameof(input));
            }

            if (decimal.TryParse(input, NumberStyles.Currency, CultureInfo.CurrentCulture, out var result))
            {
                return result;
            }

            throw new FormatException("Input string is not in a valid monetary format.");
        }
    }
}


// Ensure the namespace for the extension method is included at the top of the file.
