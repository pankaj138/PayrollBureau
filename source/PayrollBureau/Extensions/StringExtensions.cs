﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PayrollBureau.Extensions
{
    public static class StringExtensions
    {
        public static int? ToNullableInt(this string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string ArrayToString<T>(this T[] obj)
        {
            return string.Join(",", obj.Select(o => o.ToString()));
        }

        public static T[] StringToArray<T>(this string @string)
        {
            return (from piece in @string.Split(',')
                    let trimmed = piece.Trim()
                    where !string.IsNullOrEmpty(trimmed)
                    select trimmed.ToEnum<T>()).ToArray();
        }

        public static string AsString(this IEnumerable<string> source, string separator = "")
        {
            return string.Join(separator, source);
        }

        public static byte[] ToByteArray(this string @string)
        {
            return Encoding.UTF8.GetBytes(@string);
        }
    }
}