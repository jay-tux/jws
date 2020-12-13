# JWS - Libs/JcfHelper.cs
*This is the documentation about the source file located under `Libs/JcfHelper.cs`; only public/protected fields/methods/classes are described in here.*

## Enum Jay.Config.ResultOptions
*Enum type containing the possible results of a query.*  
 - ``Success``: for a success,  
 - ``TypeWrong``: when the type of the result is different from the type requested,  
 - ``UnknownKey``: the key doesn't exist in the config.

## Class Jay.Config.JcfResult<T>
*Class containing quick-access data for query results.*  
*Constraint on ``T``: ``T : class`` (T should be a reference type).*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JcfResult<T>](./JcfHelper.md).

### Fields
 - ``public T value``: the value (in case of a success) or ``null`` otherwise.  
 - ``public ResultOptions state``: the result of the query.  
 - ``publci ArgumentException exception``: the exception thrown (in case of an ``UnkownKey`` result), or ``null`` otherwise.

### Methods
 - ``public JcfResult<T2> CrossCast<T2>()``: cross-casts this ``JcfResult`` to a new type. *Throws an ArgumentException if this is a ``Success`` result.*  
 *Constraint on ``T2``: ``T2 : class`` (T2 should be a reference type).*

### Cast Operators
 - ``public static implicit operator bool(JcfResult<T> res)``: returns ``true`` if the query given was a success, otherwise ``false``.  
 - ``public static explicit operator T(JcfResult<T> res)``: returns the value contained in the result.  
 - ``public static explicit operator ArgumentException(JcfResult<T> res)``: returns the exception contained in the result.  
 - ``public static explicit operator JcfResult<T>(T res)``: generates a new ``Success`` result from the given value.  
 - ``public static explicit operator JcfResult<T>(ArgumentException e)``: generates a new ``UnknownKey`` result from the given exception.  
 - ``public static explicit operator JcfResult<T>(ResultOptions opt)``: generates a new ``TypeWrong`` result from the given options. *Throws an ArgumentException if ``opt`` is not ``TypeWrong``.*  

Below is a table of possible (legal) casts:

 Result | Boolean result | Cast to ``JcfResult<T>`` | Cast from ``JcfResult<T>`` | Cross-castable?  
 --- | --- | --- | --- | ---  
 Success | ``true`` | ``(JcfResult<T>)value`` | ``(T)result`` | No  
 TypeWrong | ``false`` | ``(JcfResult<T>)ResultOptions.TypeWrong`` | None | Yes  
 UnknownKey | ``false`` | ``(JcfResult<T>)exception`` | ``(ArgumentException)result`` | Yes  

Where ``value`` is of type ``T``, ``result`` is of type ``JcfResult<T>`` and ``exception`` is an ``ArgumentException``.

## Class Jay.Config.JcfExt
*A static class containing extension query methods for the [Jcf](./Conf.md) class.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JcfExt](./JcfHelper.md).

### Extension Methods
 - ``JcfResult<string> Jcf.GetString(string key)``: attempts to get the string at key ``key`` in the given Jcf.  
 - ``JcfResult<Jcf> Jcf.GetBock(string key)``: attempts to get the JCF-block at key ``key`` in the given Jcf.  
 - ``JcfResult<List<Jcf>> Jcf.GetList(string key)``: attempts to get the JCF-list at key ``key`` in the given Jcf.  
 - ``JcfResult<JcfEnumerable> Jcf.GetMappedList(string key, string innerkey)``: attempts to get the JCF-list at key ``key`` in the given Jcf as a string-enumerable.  

## Class Jay.Config.JcfEnumerable
*Enumerable class for enumerating single-key JCF-lists.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JcfEnumerable](./JcfHelper.md).  
Implements: [IEnumerable<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable?view=net-5.0).

### Constructors
 - ``public JcfEnumerable(List<Jcf> source, string inner)``: sets up a new JcfEnumerable from the given list and inner key.  

### Methods
 - ``public IEnumerator<JcfResult<string>> GetEnumerator()``: gets the enumerator.  
 - ``public IEnumerator IEnumerable.GetEnumerator()``: gets the enumerator.

## Class Jay.Config.JcfEnumerator
*Enumerator class for the JcfEnumerable.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JcfEnumerable](./JcfHelper.md).  
Implements: [IEnumerable<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerator?view=net-5.0).

### Constructors
 - ``public JcfEnumerator(List<Jcf> source, string inner)``: sets up the enumerator with the given list and inner key.  

### Properties
 - ``public JcfResult<string> Current``: attempts to extract the specified string value (by the inner key) from the current element.  
 - ``public object IEnumerator.Current``: attempts to extract the specified string value (by the inner key) from the current element.  

### Methods
 - ``public bool MoveNext()``: attempts to move the cursor.  
 - ``public void Reset()``: resets the cursor.  
 - ``public void Dispose()``: does nothing. *Only for inheritance*  
 - ``protected virtual void Dispose(bool disposing)``: does nothing. *Only for inheritance*  
 - ``~JcfEnumerator()``: does nothing. *Only for inheritance*
