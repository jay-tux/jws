# JWS - Libs/Ext.cs
*This is the documentation about the source file located under `Libs/Ext.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Ext.Ext
*A static class containing multiple extension methods.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Ext](.).

### Static Methods
 - ``static string GetIP()``: gets the first IPv4 currently used of this machine.

### Extension Methods
 - ``string IEnumerable<byte>.ToX()``: converts a sequence of bytes to their hexadecimal readable representation (as 0xXX with spaces between).  
 - ``string byte[].ToHex()``: converts a sequence of bytes to their hexadecimal string representation (as XX without spaces between).  
 - ``string IEnumerable<byte>.ToHex(string join)``: converts a sequence of bytes to their hexadecimal string representation (as XX with a given character between).  
 - ``string IEnumerable<byte>.ToChars()``: converts a byte sequence to the corresponding ASCII string.  
 - ``byte[] string.ToBytes()``: converts a string to its corresponding ASCII byte sequence.  
 - ``byte[] string.HexBytes()``: converts a string of hexadecimal bytes (XX form, no characters between) back to a byte sequence.  
 - ``void List<T>.Enumerate(Action<T, int> consumer)``: enumerates an array Python-style (with element and index).  
 - ``T List<T>.Pop()``: pops the first element of the List.  
 - ``List<TResult> List<TSource>.DcEnumerate(List<TManipulator> second, Func<TSource, TManipulator, TResult> enumerator)``: combines two lists using ``enumerator`` (all elements of the first list are guaranteed to be enumerated. If ``second`` is shorter, indexing is restarted on ``second``).  
 - ``void IEnumerable<T>.ForEach(Action<T> consumer)``: applies ``consumer`` to each element in the given ``IEnumerable<T>``.  
 - ``T[] T[].Prepend(T newval)``: prepends ``newval`` to a copy of the given array.  
 - ``T[] T[].Append(T add)``: appends ``add`` to a copy of the given array.  
 - ``Dictionary<TKey, TVal>.ContainsKeys(params TKey[] args)``: checks whether the given Dictionary contains all keys in ``args``.  
 - ``List<string> string.PerLength(int len)``: converts a string to a list of strings with a length smaller than or equal to ``len``.  
 - ``T[] T[][].Join(T separator)``: collapses an array of arrays into a single array, using a certain separator between the elements.  
 - ``IEnumerator<List<T>> List<T>.Per(int per)``: turns a list into an IEnumerator of smaller lists, each having up to a given amount of elements.  
 - ``IEnumerator<T[]> T[].Per(int per)``: turns an array into an IEnumerator of smaller arrays, each of up to a given amount of elements.  
 - ``void List<T>.PrintPer(int per, string sep)``: prints all elements in a list, a given amount per line, each separated by a certain string.  
 - ``void T[].PrintPer(int per, string sep)``: prints all elements in an array, a given amount per line, each separated by a certain string.  
 - ``void Dictionary<TKey, TValue>.RemoveIf(Func<TKey, bool> predicate)``: removes all elements from a given dictionary matching a key-based predicate.  
 - ``void Dictionary<TKey, TValue>.RemoveIf(Func<TKey, TValue, bool> predicate)``: removes all elements from a given dictionary matching an entry-based predicate.  
 - ``void NameValueCollection.ForEach(Action<string, string> consumer)``: executes an action for each pair in a given NameValueCollection.  
 - ``IEnumerable<T> NameValueCollection.Select(Func<string, string, T> selector)``: maps each key-value pair in this NameValueCollection to another element.  
 - ``IEnumerable<T> T[].Zipper(params T[][] others)``: returns a sequence consisting of an element from the given array, then the corresponding elements of the others, in order (if present), etc.  
 - ``IEnumerable<T> IEnumerable<T>.Zipper(params IEnumerable<T>[] others)``: returns a sequence consisting of an element from the given sequence, then the corresponding elements of the others, in order (if present), etc.  
 - ``IEnumerable<(T, T)> IEnumerable<T>.ToPairs()``: converts a sequence of n elements to a sequence of n/2 pairs. If n is odd, the last element is discarded.  
 - ``IEnumerable<(T, T, T)> IEnumerable<T>.ToTriples()``: converts a sequence of n elements to a sequence of n/3 triples. If n is not divisible by three, the last element (or two elements) are discarded.  
 - ``Action<(T1, T2)> Action<T1, T2>.Curry()``: converts a function with two arguments to a function over tuples.  
 - ``Action<T1, T2> Action<(T1, T2)>.Uncurry()``: converts a function over tuples to a function with two arguments.  
 - ``Func<(T1, T2), TOut> Func<T1, T2, TOut>.Curry()``: converts a function with two arguments and a return value to a function over tuples with a return value.  
 - ``Func<T1, T2, TOut> Func<(T1, T2), TOut>.Uncurry()``: converts a function over tuples with a return value to a function with two arguments and a return value.  
 - ``IEnumerable<string> Dictionary<string, string>.JoinPairs(string delim)``: maps each key-value pair to a string of the form ``<key><delim><value>``.  
 - ``IEnumerable<string> Dictionary<string, string>.JoinPairs(char delim)``: maps each key-value pair to a string of the form ``<key><delim><value>``.  
