using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;

namespace SQLite.Library
{
    public static class Utilities
    {
        /// <summary>
        /// Checks whether given records exist within a list of strings.
        /// <para>Matching is accomplished through ordinal sort rules, ignoring case, however, an exact match for each provided record is expected.</para>
        /// </summary>
        /// <param name="list">The list to check through.</param>
        /// <param name="records">The records to check with.</param>
        /// <returns>Returns true, if all provided records were found within the list (matching exactly, regardless to case).</returns>
        public static bool ListContains(List<string> list, params string[] records)
        {
            if (list == null || records == null) throw new ArgumentNullException("Input parameters were null.");
            if (list.Count < 1) throw new ArgumentOutOfRangeException("Provided list was empty.");

            foreach (var record in records.Where(w => !string.IsNullOrWhiteSpace(w)))
                if (!ListContains(list, record))
                    return false;

            return true;
        }

        /// <summary>
        /// Check whether a specific string exists within a given list of strings.
        /// </summary>
        /// <param name="list">The list of strings to check.</param>
        /// <param name="input">The string to match against.</param>
        /// <returns>Returns true if the input string exists in the given list.</returns>
        public static bool ListContains(List<string> list, string input)
        {
            if (list == null || input == null) throw new ArgumentNullException("Input parameters were null.");
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Provided input was empty.");
            if (list.Count < 1) throw new ArgumentOutOfRangeException("Provided list was empty.");

            return list.Any(x => x.Equals(input, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Auto populates an SQLParameter object with value, type, and provides a unqiue name.
        /// <para>Will throw an exception if the provided object type isn't covered by this extension.</para>
        /// <para>Does not support nullable types.</para>
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="o">The object to use for the parameter.</param>
        /// <param name="name">Optional override for name.</param>
        /// <returns>Returns an SqlParameter that can be used within a query.</returns>
        public static SQLiteParameter ToSqliteParameter<T>(this T o, string paramName = "")
        {
            if (o == null)
            {
                return null;
            }
            var type = typeof(T);
            if (Nullable.GetUnderlyingType(type) != null) return null;  // nullable type

            SQLiteParameter sql;

            var name = paramName;
            if (string.IsNullOrWhiteSpace(paramName))
            {
                var id = Guid.NewGuid().ToString();
                name = $"{typeof(T).Name}{id.Substring(id.Length - 4, 4)}";
            }

            if (name[0] != '@')
            {
                name = $"@{name}";
            }

            if (o is int num)
            {
                sql = new SQLiteParameter
                {
                    Value = num,
                    DbType = DbType.Int32
                };
            }
            else if (o is string str)
            {
                sql = new SQLiteParameter
                {
                    Value = str.Trim(),
                    DbType = DbType.String
                };
            }
            else if (o is bool b)
            {
                sql = new SQLiteParameter
                {
                    Value = b,
                    DbType = DbType.Boolean
                };
            }
            else if (o is DateTime dt)
            {
                sql = new SQLiteParameter
                {
                    Value = dt.ToString(),
                    DbType = DbType.String
                };
            }
            else if (o is decimal d)
            {
                sql = new SQLiteParameter
                {
                    Value = d,
                    DbType = DbType.Decimal
                };
            }
            else if (o is float f)
            {
                sql = new SQLiteParameter
                {
                    Value = f,
                    DbType = DbType.Double
                };
            }
            else if (o is double dd)
            {
                sql = new SQLiteParameter
                {
                    Value = dd,
                    DbType = DbType.Double
                };
            }
            else
            {
                // Add more statements as needed, to cover further types.
                throw new Exception("Unsupported Type.");
            }

            sql.ParameterName = name;
            return sql;
        }

        public static DateTime GetStringDateTime(this SQLiteDataReader reader, int index)
        {
            if (reader == null || index < 0) return (DateTime)SqlDateTime.MinValue;

            if (!reader.IsDBNull(index) && DateTime.TryParse(reader.GetString(index), out DateTime result))
            {
                return result;
            }

            return (DateTime)SqlDateTime.MinValue;
        }

        private static readonly Regex WhiteSpace = new Regex(@"\s+");

        public static string RemoveSpaces(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : WhiteSpace.Replace(input.Trim(), string.Empty);
        }
    }
}
