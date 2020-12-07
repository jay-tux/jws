# JWS - Libs/Conf.cs
*This is the documentation about the source file located under `Libs/Conf.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Config.Jcf
*The tree-like data structure for the configuration data.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Jcf](.).

### Fields
 - ``public Jcf Parent``: this Jcf's parent; can be null.

### Properties
 - ``public object this[string key]``: searches for the given key in the data structure. Returns either a string (a single value), another Jcf object (a JCF block), or a ``List<Jcf>`` (a JCF list). *Throws an ArgumentException if fails.*

### Types
 - ``protected enum JcfType``: is either Value (a single value), Jcf (a sub-block) or List (a JCF list).

### Constructors
 - ``public Jcf()``: creates a new, empty Jcf.  
 - ``public Jcf(Jcf parent)``: creates a new Jcf, setting the parent.

### Methods
 - ``public void Override(string key, string val)``: overrides a single value temporarily.
 - ``public void SetValue(string key, string value)``: same as ``this[key] = value``, but works only locally (so doesn't traverse the DS in-depth). Can be used to add values.  
 - ``public void SetSub(string key, Jcf sub)``: same as ``this[key] = sub``, but works only locally (so doesn't traverse the DS in-depth). Can be used to add subs.  
 - ``public void SetList(string key, List<Jcf> list)``: same as ``this[key] = list``, but works only locally (so doesn't traverse the DS in-depth). Can be used to add lists.  
 - ``protected (Jcf, object, string, JcfType) Route(string key)``: attempts to find the object in the DS corresponding to the breadcrumb path given by ``key`` (using ``.`` for path-separation and ``#`` for indexing). The return object is of the form ``(parent, value, sub-key, type of value)``. *Throws an ArgumentException if fails.*
 - ``public void EnumerateKeys(Action<string, string> consumer)``: executes ``consumer(key, value)`` for all single-values in this Jcf.  
 - ``public string Translate(string todo)``: substitutes all variable in a value (using ``$VARNAME$`` to determine what are variables). *Throws an ArgumentException if fails.*  
 - ``public override string ToString()``: converts this Jcf to a human-readable version. Shorthand for ``this.ToString(0)``.  
 - ``public virtual string ToFileString()``: converts this Jcf to a machine-readable version. Shorthand for ``this.SaveString(0)``.  
 - ``protected virtual string ToString(int depth)``: converts this Jcf to a human-readable version. Uses the depth parameter as indent value for recursive calls.  
 - ``protected string Spacing(int depth)``: convenience function which generates a string consisting of ``depth`` spaces (`` ``).  
 - ``protected virtual string SaveString(int depth)``: converts this Jcf to a machine-readable version. Uses the depth parameter as indent value for recursive calls (so the file is also human-readable).  
 - ``public void Save(string filename)``: Saves this JCF to a file. Shorthand for ``System.IO.File.WriteAllText(filenam, SaveString(0))``

## Class Jay.Config.JcfParser
*A static class for parsing JCF files.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JcfParser](.).

### Static Methods
 - ``public static Jcf Parse(string file, Jcf parent = null)``: reads ``string`` into a Jcf object and returns it. ``parent`` is used for recursive calls. *Throws a JcfException if fails.*  
 - ``public static List<Jcf> ParseList(string list, Jcf parent)``: reads ``list`` into a Jcf list object and returns it. ``parent`` is the containing Jcf object. *Throws a JcfException if fails.*  
 - ``public static Jcf ParseFile(string filename)``: parses a given file. Shorthand for ``JcfParser.Parse(System.IO.File.ReadAllText(filename))``. *Throws a JcfException if fails.*

## Class Jay.Config.JcfException
*Exception class for representing parsing errors.*  
Inheritance: Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception?view=net-5.0) -> [JcfException](.).

### Constructors
 - ``public JcfException(string message)``: creates a new JcfException with the given message.  
 - ``public JcfExcpetion(string message, Exception inner)``: creates a new JcfExcpetion with the given message and the Exception causing this JcfException.
