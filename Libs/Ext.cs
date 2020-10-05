using System;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Jay.Ext
{
    /// <summary>
    /// A static class containing multiple extension methods.
    /// </summary>
    public static class Ext
    {
        private static Dictionary<char, byte> HexRev = new Dictionary<char, byte>() {
          ['0'] = 0, ['1'] = 1, ['2'] = 2,  ['3'] = 3,  ['4'] = 4,  ['5'] = 5,  ['6'] = 6,  ['7'] = 7,
          ['8'] = 8, ['9'] = 9, ['A'] = 10, ['B'] = 11, ['C'] = 12, ['D'] = 13, ['E'] = 14, ['F'] = 15
        };

        /// <summary>
        /// Converts a sequence of bytes to their hexadecimal readable representation (as 0xXX with spaces between).
        /// </summary>
        /// <param name="val">The byte sequence</param>
        /// <returns>The string representation</returns>
        public static string ToX(this IEnumerable<byte> val) =>
            string.Join(" ", val.Select(b => $"0x{b:X2}"));

        /// <summary>
        /// Converts a sequence of bytes to their hexadecimal string representation (as XX without spaces between).
        /// This method is the inverse of the HexBytes() method.
        /// </summary>
        /// <param name="val">The byte sequence</param>
        /// <returns>The string sequence</returns>
        public static string ToHex(this byte[] val) => val.ToHex("");

        /// <summary>
        /// Converts a sequence of bytes to their hexadecimal string representation (as XX with a given character between).
        /// </summary>
        /// <param name="val">The byte sequence</param>
        /// <param name="join">The string to put between to bytes</param>
        /// <returns>The string sequence</returns>
        public static string ToHex(this IEnumerable<byte> val, string join) =>
            string.Join(join, val.Select(b => $"{b:X2}"));

        /// <summary>
        /// Converts a byte sequence to the corresponding ASCII string.
        /// </summary>
        /// <param name="val">The byte sequence</param>
        /// <returns>The corresponding ASCII string</returns>
        public static string ToChars(this IEnumerable<byte> val) =>
            string.Join("", val.Select(b => (char)b));

        /// <summary>
        /// Converts a string to its corresponding ASCII byte sequence.
        /// </summary>
        /// <param name="val">The string to convert</param>
        /// <returns>The ASCII byte sequence</returns>
        public static byte[] ToBytes(this string val) => val.Select(c => (byte)c).ToArray();

        /// <summary>
        /// Converts a string of hexadecimals bytes (XX form, no characters between) back to a byte sequence.
        /// This method is the inverse of the ToHex() method.
        /// </summary>
        /// <param name="val">The string to convert</param>
        /// <returns>The original byte sequence</returns>
        /// <exception cref="ArgumentException">The length of the string is not a multiple of two</exception>
        public static byte[] HexBytes(this string val)
        {
            if(val.Length % 2 != 0) { throw new ArgumentException("Invalid length."); }
            byte[] res = new byte[val.Length / 2];
            for(int i = 0; i < val.Length / 2; i++)
            {
                if(!HexRev.ContainsKey(val[2 * i])) throw new ArgumentException($"'{val[2 * i]}' ({(int)(val[2 * i])}) is not a valid hexadecimal character.");
                if(!HexRev.ContainsKey(val[2 * i + 1])) throw new ArgumentException($"'{val[2 * i + 1]}' ({(int)(val[2 * i + 1])}) is not a valid hexadecimal character.");
                res[i] = (byte)(HexRev[val[2 * i]] * 16 + HexRev[val[2 * i + 1]]);
            }
            return res;
        }

        /// <summary>
        /// Enumerates a List Python-style (with element and index).
        /// </summary>
        /// <param name="toEnum">The List to enumerate</param>
        /// <param name="consumer">The method to execute for each (item, index) pair</param>
        /// <typeparam name="T">The type contained in the list</typeparam>
        public static void Enumerate<T>(this List<T> toEnum, Action<T, int> consumer)
        {
            for(int i = 0; i < toEnum.Count; i++) consumer(toEnum[i], i);
        }

        /// <summary>
        /// Enumerates an array Python-style (with element and index).
        /// </summary>
        /// <param name="toEnum">The array to enumerate</param>
        /// <param name="consumer">The method to execute for each (item, index) pair</param>
        /// <typeparam name="T">The type contained in the list</typeparam>
        public static void Enumerate<T>(this T[] toEnum, Action<T, int> consumer)
        {
            for(int i = 0; i < toEnum.Length; i++) consumer(toEnum[i], i);
        }

        /// <summary>
        /// Pops the first element of the List.
        /// </summary>
        /// <param name="lst">The List to pop from</param>
        /// <returns>The popped item</returns>
        public static T Pop<T>(this List<T> lst)
        {
            T item = lst[0];
            lst.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Double-cyclic enumerates two lists, resulting in a third list.
        /// Each item of the first list is matched with the item from the second list at position [index % length],
        /// where index is the index of the item in the first list and length is the length of the second list.
        /// Each resulting item is stored in the third list.
        /// </summary>
        /// <param name="first">The first list (source) to be enumerated</param>
        /// <param name="second">The second list (manipulator) to be used</param>
        /// <param name="enumerator">The translation function (TSource, TManipulator) -> TResult</param>
        /// <typeparam name="TSource">The type contained in the source list</typeparam>
        /// <typeparam name="TManipulator">The type contained in the manipulator list</typeparam>
        /// <typeparam name="TResult">The type contained in the resulting list</typeparam>
        /// <returns>The remapped list</returns>
        public static List<TResult> DcEnumerate<TSource, TManipulator, TResult>(List<TSource> first,
            List<TManipulator> second, Func<TSource, TManipulator, TResult> enumerator)
        {
            var res = new List<TResult>();
            for(int i = 0; i < first.Count; i++) res.Add(enumerator(first[i], second[i % second.Count]));
            return res;
        }

        /// <summary>
        /// Executes an action for each element in a given sequence.
        /// </summary>
        /// <param name="todo">The sequence to enumerate</param>
        /// <param name="consumer">The action to execute</param>
        /// <typeparam name="T">The type contained in the sequence</typeparam>
        public static void ForEach<T>(this IEnumerable<T> todo, Action<T> consumer)
        {
            foreach(T t in todo)
                consumer(t);
        }

        /// <summary>
        /// Prepends a given array with a given value.
        /// </summary>
        /// <param name="arr">The array to prepend to</param>
        /// <param name="newval">The value to prepend to the array</param>
        /// <typeparam name="T">The type contained in the array</typeparam>
        /// <returns>The prepended array</returns>
        public static T[] Prepend<T>(this T[] arr, T newval)
        {
            var res = new T[arr.Length + 1];
            res[0] = newval;
            for(int i = 1; i < arr.Length + 1; i++) res[i] = arr[i - 1];
            return res;
        }

        /// <summary>
        /// Appends a given array with a given other array.
        /// </summary>
        /// <param name="arr">The array to append to</param>
        /// <param name="add">The other array to append to the array</param>
        /// <typeparam name="T">The type contained in the array</typeparam>
        /// <returns>The appended array</returns>
        public static T[] Append<T>(this T[] arr, T[] add)
        {
            T[] res = new T[arr.Length + add.Length];
            for(int i = 0; i < arr.Length; i++) { res[i] = arr[i]; }
            for(int i = 0; i < add.Length; i++) { res[i + arr.Length] = add[i]; }
            return res;
        }

        /// <summary>
        /// Appends a given array with a given value.
        /// </summary>
        /// <param name="arr">The array to append to</param>
        /// <param name="add">The value to append to the array</param>
        /// <typeparam name="T">The type contained in the array</typeparam>
        /// <returns>The appended array</returns>
        public static T[] Append<T>(this T[] arr, T add)
        {
            T[] res = new T[arr.Length + 1];
            for(int i = 0; i < arr.Length; i++) { res[i] = arr[i]; }
            res[arr.Length] = add;
            return res;
        }

        /// <summary>
        /// Checks whether all given keys occur in a given Dictionary.
        /// </summary>
        /// <param name="dict">The Dictionary to check</param>
        /// <param name="args">The keys to check</param>
        /// <typeparam name="TKey">The type of the keys</typeparam>
        /// <typeparam name="TVal">The type of the values</typeparam>
        /// <returns>True if all keys occur in the Dictionary, otherwise false</returns>
        public static bool ContainsKeys<TKey, TVal>(this Dictionary<TKey, TVal> dict, params TKey[] args) => args.All(dict.ContainsKey);

        public static List<string> PerLength(this string toSplit, int len)
        {
            List<string> res = new List<string>();
            int start = 0;
            while(start + len < toSplit.Length)
            {
                res.Add(toSplit.Substring(start, len));
                start += len;
            }
            if(start < toSplit.Length - 1) { res.Add(toSplit.Substring(start, toSplit.Length - start)); }
            return res;
        }

        /// <summary>
        /// Collapses an array of arrays into a single array, using a certain separator between the elements.
        /// </summary>
        /// <param name="src">The array of arrays to collapse</param>
        /// <param name="separator">The separator to use between the deepest-nested array elements</param>
        /// <typeparam name="T">The type contained in the array of arrays</typeparam>
        /// <returns>The collapsed array</returns>
        public static T[] Join<T>(this T[][] src, T separator)
        {
            T[] res = new T[0];
            for(int i = 0; i < src.Length - 1; i++)
            {
                res = res.Append(src[i]);
                res = res.Append(separator);
            }
            res = res.Append(src[src.Length - 1]);
            return res;
        }

        /// <summary>
        /// Gets the local IP Address of the machine.
        /// </summary>
        /// <returns>The IP Address of the machine, in string form</returns>
        public static string GetIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "No IPv4 compatible system adapters.";
        }

        /// <summary>
        /// Turns a list into an IEnumerator of smaller lists, each of up to a given amount of elements.
        /// </summary>
        /// <param name="source">The list to divide</param>
        /// <param name="per">The maximal amount of elements per list</param>
        /// <typeparam name="T">The type contained in the list</typeparam>
        /// <returns>The divided lists, generated as necessary</returns>
        public static IEnumerator<List<T>> Per<T>(this List<T> source, int per)
        {
            int curr = 0;
            while(curr < source.Count)
            {
                List<T> res = new List<T>();
                for(int i = 0; i + curr < source.Count && i < per; i++) res.Add(source[curr + i - 1]);
                yield return res;
                curr += per;
            }
        }

        /// <summary>
        /// Turns an array into an IEnumerator of smaller arrays, each of up to a given amount of elements.
        /// </summary>
        /// <param name="source">The array to divide</param>
        /// <param name="per">The maximal amount of elements per array</param>
        /// <typeparam name="T">The type contained in the array</typeparam>
        /// <returns>The divided arrays, generated as necessary</returns>
        public static IEnumerator<T[]> Per<T>(this T[] source, int per)
        {
            int curr = 0;
            while(curr < source.Length)
            {
                T[] res = new T[(source.Length - curr) > per ? per : (source.Length - curr - 1)];
                for(int i = 0; i < res.Length; i++) res[i] = source[curr + i];
                yield return res;
                curr += per;
            }
        }

        /// <summary>
        /// Prints all elements in a list, a given amount per line, each separated by a certain string.
        /// </summary>
        /// <param name="source">The list to print</param>
        /// <param name="per">The amount of elements per line</param>
        /// <param name="separator">The separator string</param>
        /// <typeparam name="T">The type contained in the list</typeparam>
        public static void PrintPer<T>(this List<T> source, int per, string separator)
        {
            IEnumerator<List<T>> en = source.Per(per);
            while(en.MoveNext())
            {
                if (en.Current != null)
                    foreach (T val in en.Current)
                        Console.Write(val + separator);

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Prints all elements in an array, a given amount per line, each separated by a certain string.
        /// </summary>
        /// <param name="source">The array to print</param>
        /// <param name="per">The amount of elements per line</param>
        /// <param name="separator">The separator string</param>
        /// <typeparam name="T">The type contained in the array</typeparam>
        public static void PrintPer<T>(this T[] source, int per, string separator)
        {
            IEnumerator<T[]> en = source.Per(per);
            while(en.MoveNext())
            {
                if (en.Current != null)
                    foreach (T val in en.Current)
                        Console.Write(val + separator);

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Removes all elements from a given dictionary matching a key-based predicate.
        /// </summary>
        /// <param name="dict">The dictionary to clean</param>
        /// <param name="predicate">The predicate to evaluate</param>
        /// <typeparam name="TKey">The type used as key</typeparam>
        /// <typeparam name="TValue">The type used as value</typeparam>
        public static void RemoveIf<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TKey, bool> predicate)
        {
            foreach (TKey key in dict.Keys.Where(predicate).ToList())
            {
                dict.Remove(key);
            }
        }

        /// <summary>
        /// Removes all elements from a given dictionary matching an entry-based predicate.
        /// </summary>
        /// <param name="dict">The dictionary to clean</param>
        /// <param name="predicate">The predicate to evaluate</param>
        /// <typeparam name="TKey">The type used as key</typeparam>
        /// <typeparam name="TValue">The type used as value</typeparam>
        public static void RemoveIf<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            Func<TKey, TValue, bool> predicate)
        {
            foreach (TKey key in dict.Where(kvp => predicate(kvp.Key, kvp.Value))
                .Select(kvp => kvp.Key).ToList())
            {
                dict.Remove(key);
            }
        }

        public static bool ArrEq<T>(this T[] src, T[] other)
        {
            if(src.Length != other.Length) return false;
            for(int i = 0; i < src.Length; i++) {
                if(!src[i].Equals(other[i])) return false;
            }
            return true;
        }
    }
}
