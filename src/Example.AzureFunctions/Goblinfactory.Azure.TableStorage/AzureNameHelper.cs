using System;
using System.Linq;

namespace Goblinfactory.Azure.TableStorage
{
    public static class Azure
        {
            public static string ValidateTableStorageName(this string name)
            {
                return name
                .MustNotBeEmpty()
                .MustBeAlphanumeric()
                .CannotBeginWithANumber()
                .MustBeBetweenMinAndMaxCharactersLong(3, 63);
            }
        }

    public static class Names
    { 
        public static string MustNotBeEmpty(this string src)
        {
            if (string.IsNullOrWhiteSpace(src)) throw new ArgumentException("must not be empty");
            return src;
        }

        public static string MustBeAlphanumeric(this string src)
        {
            if (src.Any(c =>!char.IsLetterOrDigit(c))) throw new ArgumentException("must be alphanumeric");
            return src;
        }
        public static string CannotBeginWithANumber(this string src)
        {
            if (char.IsNumber(src[0])) throw new ArgumentException("cannot begin with a number");
            return src;
        }

        public static string MustBeBetweenMinAndMaxCharactersLong(this string src, int min, int max)
        {
            int len = src.Length;
            if (len < min) throw new ArgumentOutOfRangeException($"must be between {min} and {max} (inclusive) characters long.");
            if (len > max) throw new ArgumentOutOfRangeException($"must be between {min} and {max} (inclusive) characters long.");
            return src;
        }
    }
}
