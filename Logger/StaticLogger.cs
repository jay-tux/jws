using System;
using System.Linq;
using System.Collections.Generic;

namespace Jay.IO.Logging
{
	/// <summary>
	/// A simple static logger.
	/// </summary>
	public static class StaticLogger
	{
#region Constants
		/// <summary>
		/// Maximal length for the logging type. Logging type will be shortened to this length (in characters), then padded with spaces (left or right). [Default=10]
		/// </summary>
		public static int TypeLength = 10;
		/// <summary>
		/// Maximal length for the module name. Module name will be shortened to this length (in characters), then padded with spaces (left or right). [Default=10]
		/// </summary>
		public static int ModLength = 9;
        /// <summary>
        /// Color for Messages.
        /// </summary>
        public static LogColor MessageColor = LogColor.Cyan;
        /// <summary>
        /// Color for Warnings.
        /// </summary>
        public static LogColor WarningColor = LogColor.Yellow;
        /// <summary>
        /// Color for Errors.
        /// </summary>
        public static LogColor ErrorColor = LogColor.Red;
        /// <summary>
        /// Color for Debug Messages.
        /// </summary>
        public static LogColor DebugColor = LogColor.White;
#endregion

#region Fields
        private static uint Messages = 0;
        private static uint Warnings = 0;
        private static uint Errors = 0;
        private static uint Debugs = 0;
#endregion

#region Helpers
		private static string ToLength(this string todo, int maxlen, bool padLeft)
			=> todo.Length > maxlen ? (todo.Substring(0, maxlen - 3) + "...") : (padLeft ? todo.PadLeft(maxlen) : todo.PadRight(maxlen));

		private static void LogFormat(string type, string mod, string message, LogColor color)
			=> Console.WriteLine($"{LogColor.Prefix}{color.ColorCode}[{type.ToLength(TypeLength, false)} in {mod.ToLength(ModLength, true)} - {DateTime.Now.ToString("MM-dd-yy hh:mm:ss tt")}]: {LogColor.Prefix}{LogColor.Reset.ColorCode}" +
					$"{message.Replace("\n", "\n\t")}");
#endregion

#region Logging as Message
		/// <summary>
		/// Logs "[{datetime}: MESSAGE in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the message.</param>
		/// <param name="msg">The message to log.</param>
		/// </summary>
		public static void LogMessage(string mod, string msg) { LogFormat("MESSAGE", mod, msg, MessageColor); Messages++; }
		/// <summary>
		/// Logs "[{datetime}: MESSAGE in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The module which generated the message; this object is converted to its type name.</param>
		/// <param name="msg">The message to log.</param>
		/// </summary>
		public static void LogMessage(object mod, string msg) { LogFormat("MESSAGE", mod.GetType().Name, msg, MessageColor); Messages++; }
		/// <summary>
		/// Logs "[{datetime}: MESSAGE in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the message.</param>
		/// <param name="msg">The message to log; this object is converted to a string using its ToString() method.</param>
		/// </summary>
		public static void LogMessage(string mod, object msg) { LogFormat("MESSAGE", mod, msg.ToString(), MessageColor); Messages++; }
		/// <summary>
		/// Logs "[{datetime}: MESSAGE in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the message; this object is converted to its type name.</param>
		/// <param name="msg">The message to log; this object is converted to a string using tis ToString() method.</param>
		/// </summary>
		public static void LogMessage(object mod, object msg) { LogFormat("MESSAGE", mod.GetType().Name, msg.ToString(), MessageColor); Messages++; }
#endregion

#region Logging as Warning
		/// <summary>
		/// Logs "[{datetime}: WARNING in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the warning.</param>
		/// <param name="msg">The warning to log.</param>
		/// </summary>
		public static void LogWarning(string mod, string msg) { LogFormat("WARNING", mod, msg, WarningColor); Warnings++; }
		/// <summary>
		/// Logs "[{datetime}: WARNING in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The module which generated the warning; this object is converted to its type name.</param>
		/// <param name="msg">The warning to log.</param>
		/// </summary>
		public static void LogWarning(object mod, string msg) { LogFormat("WARNING", mod.GetType().Name, msg, WarningColor); Warnings++; }
		/// <summary>
		/// Logs "[{datetime}: WARNING in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the warning.</param>
		/// <param name="msg">The warning to log; this object is converted to a string using its ToString() method.</param>
		/// </summary>
		public static void LogWarning(string mod, object msg) { LogFormat("WARNING", mod, msg.ToString(), WarningColor); Warnings++; }
		/// <summary>
		/// Logs "[{datetime}: WARNING in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the warning; this object is converted to its type name.</param>
		/// <param name="msg">The warning to log; this object is converted to a string using tis ToString() method.</param>
		/// </summary>
		public static void LogWarning(object mod, object msg) { LogFormat("WARNING", mod.GetType().Name, msg.ToString(), WarningColor); Warnings++; }
#endregion

#region Logging as Error
		/// <summary>
		/// Logs "[{datetime}: ERROR in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the error.</param>
		/// <param name="msg">The error to log.</param>
		/// </summary>
		public static void LogError(string mod, string msg) { LogFormat("ERROR", mod, msg, ErrorColor); Errors++; }
		/// <summary>
		/// Logs "[{datetime}: ERROR in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The module which generated the error; this object is converted to its type name.</param>
		/// <param name="msg">The error to log.</param>
		/// </summary>
		public static void LogError(object mod, string msg) { LogFormat("ERROR", mod.GetType().Name, msg, ErrorColor); Errors++; }
		/// <summary>
		/// Logs "[{datetime}: ERROR in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the error.</param>
		/// <param name="msg">The error to log; this object is converted to a string using its ToString() method.</param>
		/// </summary>
		public static void LogError(string mod, object msg) { LogFormat("ERROR", mod, msg.ToString(), ErrorColor); Errors++; }
		/// <summary>
		/// Logs "[{datetime}: ERROR in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the error; this object is converted to its type name.</param>
		/// <param name="msg">The error to log; this object is converted to a string using tis ToString() method.</param>
		/// </summary>
		public static void LogError(object mod, object msg) { LogFormat("ERROR", mod.GetType().Name, msg.ToString(), ErrorColor); Errors++; }
#endregion

#region Logging as Debug
		/// <summary>
		/// Logs "[{datetime}: DEBUG in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the debug info.</param>
		/// <param name="msg">The debug info to log.</param>
		/// </summary>
		public static void LogDebug(string mod, string msg) { LogFormat("DEBUG", mod, msg, DebugColor); Debugs++; }
		/// <summary>
		/// Logs "[{datetime}: DEBUG in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The module which generated the debug info; this object is converted to its type name.</param>
		/// <param name="msg">The debug info to log.</param>
		/// </summary>
		public static void LogDebug(object mod, string msg) { LogFormat("DEBUG", mod.GetType().Name, msg, DebugColor); Debugs++; }
		/// <summary>
		/// Logs "[{datetime}: DEBUG in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the debug info.</param>
		/// <param name="msg">The debug info to log; this object is converted to a string using its ToString() method.</param>
		/// </summary>
		public static void LogDebug(string mod, object msg) { LogFormat("DEBUG", mod, msg.ToString(), DebugColor); Debugs++; }
		/// <summary>
		/// Logs "[{datetime}: DEBUG in {module name}]: {msg}]" to the Console.
		/// <param name="mod">The name of the module which generated the debug info; this object is converted to its type name.</param>
		/// <param name="msg">The debug info to log; this object is converted to a string using tis ToString() method.</param>
		/// </summary>
		public static void LogDebug(object mod, object msg) { LogFormat("DEBUG", mod.GetType().Name, msg.ToString(), DebugColor); Debugs++; }
#endregion

#region Stats
        /// <summary>
        /// Logs the current amount of MESSAGEs, WARNINGs, ERRORs and DEBUG messages to the console (using the STATISTICS log type).
        /// </summary>
        public static void LogStats() => LogFormat("STATISTICS", "__LOGGER__",
			$"Overview of log types:\nMESSAGES  : {Messages}\nWARNINGS  : {Warnings}\nERRORS    : {Errors}\nDEBUGGING : {Debugs}", LogColor.Magenta);
#endregion

#region Formatters
        /// <summary>
        /// Quick wrapper for the string.Join(", ", list) method to format a list.
        /// <param name="toFormat">The list to format.</param>
        /// <returns>A string containing all elements in the list, concatenated by ', '.</returns>
        /// </summary>
		public static string FormatList(List<string> toFormat)
			=> string.Join(", ", toFormat);

        /// <summary>
        /// Maps all items in the list to strings, then concatenates all of them using ', '.
        /// <param name="toFormat">The list to format.</param>
        /// <param name="mapper">The mapping string.</param>
        /// <returns>A string representation of the list.</returns>
        /// </summary>
		public static string FormatList<T>(List<T> toFormat, Func<T, string> mapper)
			=> string.Join(", ", toFormat.Select(elem => mapper(elem)));

        /// <summary>
        /// Concatenates all elements in the list using ', ', with a certain maximal
        /// amount of items per line.
        /// <param name="toFormat">The list to format.</param>
        /// <param name="perLine">The amount of elements per line.</param>
        /// <returns>A string representation of the list.</returns>
        /// </summary>
		public static string FormatList(List<string> toFormat, uint perLine)
		{
			List<string> grouped = new List<string>();
			for(int i = 0; i < toFormat.Count; i += (int)perLine)
			{
				List<string> tmp = new List<string>();
				for(int ind = 0; ind < perLine && (i * (int)perLine + ind) < toFormat.Count; ind++) { tmp.Add(toFormat[i * (int)perLine + ind]); }
				grouped.Add(string.Join(", ", tmp) + ", ");
			}
			return string.Join("\n\t", grouped);
		}

		/// <summary>
		/// Maps all items in the list to strings, then concatenates all of them using ', ',
		/// with a certain maximal amount of items per line.
		/// <param name="toFormat">The list to format.</param>
		/// <param name="mapper">The function mapping the items to strings.</param>
		/// <param name="perLine">The amount of items per line.</param>
		/// <returns>A string representation of the list.</returns>
		/// </summary>
		public static string FormatList<T>(List<T> toFormat, Func<T, string> mapper, uint perLine)
			=> FormatList(toFormat.Select(x => mapper(x)).ToList(), perLine);
#endregion

#region BlockLogging
            private static List<string> Block;

            /// <summary>
            /// Adds a string to the current block.
            /// <param name="msg">The message to add to the block.</param>
            /// </summary>
            public static void AddToBlock(string msg)
            {
                if(Block == null) Block = new List<string>();
                Block.Add(msg);
            }

            /// <summary>
            /// Adds an object to the current block. The object is converted to its string representation.
            /// </summary>
            /// <param name="msg">The object to add to the block.</param>
            public static void AddToBlock(object msg) => AddToBlock(msg.ToString());

            private static string PrepareBlock(int perLine, string separator)
            {
                string res = "";
                for(int i = 0; i < Block.Count; i++)
                {
                    res += Block[i] + separator;
                    if(i % perLine == perLine - 1) res += "\n";
                }
                Block = null;
                return res;
            }

            /// <summary>
            /// Logs the current block to the console as an error message.
            /// <param name="header">The header line to be used above the block.</param>
            /// <param name="mod">The module which generated the error block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// <param name="separator">The separator to be used between two elements.</param>
            /// </summary>
            public static void ErrorBlock(string header, string mod, int perLine, string separator) => LogError(mod, header + "\n" + PrepareBlock(perLine, separator));

            /// <summary>
            /// Logs the current block to the console as an error message. An empty line is used as header.
            /// <param name="mod">The module which generated the error block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// <param name="separator">The separator to be used between two elements.</param>
            /// </summary>
            public static void ErrorBlock(string mod, int perLine, string separator) => ErrorBlock("", mod, perLine, separator);

            /// <summary>
            /// Logs the current block to the console as an error message. An empty line is used as header and a single space (' ') is used as separator.
            /// <param name="mod">The module which generated the error block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// </summary>
            public static void ErrorBlock(string mod, int perLine) => ErrorBlock("", mod, perLine, " ");

            /// <summary>
            /// Logs the current block to the console as an warning message.
            /// <param name="header">The header line to be used above the block.</param>
            /// <param name="mod">The module which generated the warning block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// <param name="separator">The separator to be used between two elements.</param>
            /// </summary>
            public static void WarningBlock(string header, string mod, int perLine, string separator) => LogWarning(mod, header + "\n" + PrepareBlock(perLine, separator));

            /// <summary>
            /// Logs the current block to the console as an warning message. An empty line is used as header.
            /// <param name="mod">The module which generated the warning block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// <param name="separator">The separator to be used between two elements.</param>
            /// </summary>
            public static void WarningBlock(string mod, int perLine, string separator) => WarningBlock("", mod, perLine, separator);

            /// <summary>
            /// Logs the current block to the console as an warning message. An empty line is used as header and a single space (' ') is used as separator.
            /// <param name="mod">The module which generated the warning block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// </summary>
            public static void WarningBlock(string mod, int perLine) => WarningBlock("", mod, perLine, " ");

            /// <summary>
            /// Logs the current block to the console as a normal message.
            /// <param name="header">The header line to be used above the block.</param>
            /// <param name="mod">The module which generated the block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// <param name="separator">The separator to be used between two elements.</param>
            /// </summary>
            public static void MessageBlock(string header, string mod, int perLine, string separator) => LogMessage(mod, header + "\n" + PrepareBlock(perLine, separator));

            /// <summary>
            /// Logs the current block to the console as a normal message. An empty line is used as header.
            /// <param name="mod">The module which generated the block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// <param name="separator">The separator to be used between two elements.</param>
            /// </summary>
            public static void MessageBlock(string mod, int perLine, string separator) => MessageBlock("", mod, perLine, separator);

            /// <summary>
            /// Logs the current block to the console as a normal message. An empty line is used as header and a single space (' ') is used as separator.
            /// <param name="mod">The module which generated the block.</param>
            /// <param name="perLine">Amount of block elements per line.</param>
            /// </summary>
            public static void MessageBlock(string mod, int perLine) => MessageBlock("", mod, perLine, " ");
#endregion
	}

	/// <summary>
	/// Simple class containing some terminal color codes.
	/// </summary>
	public class LogColor
	{
        /// <summary>
        /// This color's code.
        /// </summary>
        public string ColorCode { get; private set; }

        /// <summary>
        /// General quick prefix \u001b
        /// </summary>
        public const string Prefix = "\u001b";

        private LogColor(){}

        /// <summary>
        /// Signifies the color black ([30m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Black = new LogColor() { ColorCode = "[30m" };
        /// <summary>
        /// Signifies the color red ([31m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Red = new LogColor() { ColorCode = "[31m"} ;
        /// <summary>
        /// Signifies the color green ([32m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Green = new LogColor() { ColorCode = "[32m"} ;
        /// <summary>
        /// Signifies the color yellow ([33m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Yellow = new LogColor() { ColorCode = "[33m"} ;
        /// <summary>
        /// Signifies the color blue ([34m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Blue = new LogColor() { ColorCode = "[34m"} ;
        /// <summary>
        /// Signifies the color magenta ([35m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Magenta = new LogColor() { ColorCode = "[35m"} ;
        /// <summary>
        /// Signifies the color cyan ([36m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Cyan = new LogColor() { ColorCode = "[36m"} ;
        /// <summary>
        /// Signifies the color white ([37m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor White = new LogColor() { ColorCode = "[37m"} ;
        /// <summary>
        /// Signifies the reset color ([0m).
        /// Each color still needs to prefix in order to work.
        /// </summary>
        public static readonly LogColor Reset = new LogColor() { ColorCode = "[0m"} ;
    }
}
