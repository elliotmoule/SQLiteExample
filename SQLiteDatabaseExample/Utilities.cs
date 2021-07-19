using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteDatabaseExample
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
    }
}
