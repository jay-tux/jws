using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jay.Ext;

namespace Jay.Config
{
    public class Jcf
    {
        private Dictionary<string, Jcf> _subs;
        private Dictionary<string, string> _values;
        private Dictionary<string, List<Jcf>> _lists;
        public Jcf Parent { get; private set; }

        public object this[string key]
        {
            get => GetKey(key);
            set => SetKey(key, value);
        }

        private object GetKey(string key)
        {
            var res = Route(key);
            if(res.Item4 == JcfType.Value) return res.Item1.Lookup(res.Item3);
            return res.Item2[res.Item3];
        }

        private void SetKey(string key, object val)
        {
            var res = Route(key);
            switch(res.Item4)
            {
                case JcfType.Value:
                    if(val is string str) res.Item2[res.Item3] = str;
                    else throw new ArgumentException("Expecting a string, not " + val.GetType() + ".", "val");
                    break;
                case JcfType.Jcf:
                    if(val is Jcf jcf) res.Item2[res.Item3] = jcf;
                    else throw new ArgumentException("Expecting a Jcf, not " + val.GetType() + ".", "val");
                    break;
                case JcfType.List:
                    if(val is List<Jcf> list) res.Item2[res.Item3] = list;
                    else throw new ArgumentException("Expecting a List<Jcf>, not " + val.GetType() + ".", "val");
                    break;
            }
        }

        public void SetValue(string key, string value) => _values[key] = value;
        public void SetSub(string key, Jcf sub) => _subs[key] = sub;
        public void SetList(string key, List<Jcf> list) => _lists[key] = list;

        protected (Jcf, Dictionary, string, JcfType) Route(string key)
        {
            int split = key.IndexOf(".");
            if(split == -1)
            {
                if(_values.ContainsKey(key)) return (this, _values, key, JcfType.Value);
                else if(_subs.ContainsKey(key)) return (this, _subs, key, JcfType.Jcf);
                else if(_lists.ContainsKey(key)) return (this, _lists, key, JcfType.List);
                else throw new ArgumentException("Invalid key: " + key, "key");
            }
            else
            {
                string pre = key.Substring(0, split);
                string post = key.Substring(split + 1);
                if(_subs.ContainsKey(pre))
                {
                    try { return _subs[pre].Route(post); }
                    catch(ArgumentException e) { throw new ArgumentException(e.Message + " in " + pre, "key", e); }
                }
                throw new ArgumentException("Invalid partial key: " + pre, "key");
            }
        }

        public Jcf()
        {
            _subs = new Dictionary<string, Jcf>();
            _values = new Dictionary<string, string>();
            _lists = new Dictionary<string, List<Jcf>>();
            Parent = null;
        }

        public Jcf(Jcf parent)
        {
            _subs = new Dictionary<string, Jcf>();
            _values = new Dictionary<string, string>();
            _lists = new Dictionary<string, List<Jcf>>();
            Parent = parent;
        }

        public string Translate(string todo)
        {
            string res = "";
            string key = "";
            bool inkey = false;
            todo.ForEach(chr => {
                if(chr == '$')
                {
                    if(inkey) { inkey = false; res += Lookup(key); key = ""; }
                    else { inkey = true; }
                }
                else
                {
                    if(inkey) { key += chr; }
                    else { res += chr; }
                }
            });
            return res;
        }

        private string Lookup(string todo)
        {
            try
            {
                return this[todo];
            }
            catch(Exception)
            {
                if(Parent != null) { return Parent[todo]; }
                throw new NullReferenceException("Can't find " + todo);
            }
        }

        public override string ToString() => ToString(0);

        protected virtual string ToString(int depth)
        {
            string res = "";
            foreach(var v in _values)
            {
                res += Spacing(depth) + "[value] " + v.Key + " = " + v.Value + "\n";
            }
            foreach(var v in _subs)
            {
                res += Spacing(depth) + "[ sub ] " + v.Key + " = \n" + v.Value.ToString(depth + 2);
            }
            foreach(var v in _lists)
            {
                res += Spacing(depth) + "[list ] " + v.Key + " = [" +
                    (v.Value.Count == 0 ? "" : ("\n" + string.Join("\n", v.Value.Select(x => x.ToString(depth + 4))) + "")) +
                    (v.Value.Count == 0 ? "" : Spacing(depth)) + "]\n";
            }
            return res;
        }

        protected string Spacing(int depth)
        {
            string res = "";
            for(int i = 0; i < depth; i++) res += " ";
            return res;
        }

        protected enum JcfType { Value, Jcf, List }
    }

    public static class JcfParser
    {
        public static List<Jcf> ParseList(string list, Jcf parent)
        {
            List<Jcf> res = new List<Jcf>();
            string curr = "";
            Stack<char> openers = new Stack<char>();
            list.ForEach(chr => {
                if(chr == '{')
                {
                    openers.Push('{');
                    if(openers.Count != 1) curr += '{';
                }
                else if(chr == '[')
                {
                    openers.Push('[');
                    if(openers.Count != 1) curr += '[';
                }
                else if(chr == '}')
                {
                    if(openers.Count == 0) throw new JcfException("Cannot close an unopened block.");
                    if(openers.Peek() != '{') throw new JcfException("Encountered }, expected ].");
                    openers.Pop();
                    if(openers.Count != 0) curr += '}';
                    else
                    {
                        res.Add(JcfParser.Parse(curr, parent));
                        curr = "";
                    }
                }
                else if(chr == ']')
                {
                    if(openers.Count == 0) throw new JcfException("Cannot close an unopened list.");
                    if(openers.Peek() != '[') throw new JcfException("Encountered ], expected }.");
                    openers.Pop();
                    if(openers.Count != 0) curr += ']';
                    else throw new JcfException("List can only contain blocks, not other lists.");
                }
                else
                {
                    curr += chr;
                }
            });
            return res;
        }

        public static Jcf Parse(string file, Jcf parent = null)
        {
            Jcf current = parent == null ? new Jcf() : new Jcf(parent);

            string key = ""; string val = ""; bool inkey = true;
            Stack<char> openers = new Stack<char>();

            file.ToList().Enumerate((char chr, int ind) => {
                //Console.WriteLine($"Encountered {chr == '\n' ? "newline" : chr.ToString()}\tKey is '{key}'\tValue is '{val.Replace("\n", "\\n")}'\tKey state is {inkey}\tDepth is {openers.Count}");
                if(chr == '{')
                {
                    if(openers.Count == 0 && (inkey || !string.IsNullOrWhiteSpace(val))) throw new JcfException($"[block] Key: {key}; Value: {val}"); //throw new JcfException("Blocks can only exist as values, at index " + ind + ".");
                    openers.Push('{');
                    if(openers.Count != 1) val += '{';
                }
                else if(chr == '[')
                {
                    if(openers.Count == 0 && (inkey || !string.IsNullOrWhiteSpace(val))) throw new JcfException($"[list] Key: {key}; Value: {val}"); //throw new JcfException("Lists can only exist as values, at index " + ind + ".");
                    openers.Push('[');
                    if(openers.Count != 1) val += '[';
                }
                else if(chr == '}')
                {
                    if(openers.Count == 0) throw new JcfException("Cannot close an unopened block, at index " + ind + ".");
                    if(openers.Peek() != '{') throw new JcfException("Encountered }, expected ], at index " + ind + ".");
                    openers.Pop();
                    if(openers.Count != 0) val += '}';
                    else
                    {
                        current.SetSub(key.Trim(), JcfParser.Parse(val, current));
                        key = ""; val = "";
                    }
                }
                else if(chr == ']')
                {
                    if(openers.Count == 0) throw new JcfException("Cannot close an unopened list, at index " + ind + ".");
                    if(openers.Peek() != '[') throw new JcfException("Encountered ], expected }, at index " + ind + ".");
                    openers.Pop();
                    if(openers.Count != 0) val += ']';
                    else
                    {
                        current.SetList(key.Trim(), JcfParser.ParseList(val, parent));
                        key = ""; val = "";
                    }
                }
                else if(chr == ':' && openers.Count == 0)
                {
                    if(inkey) inkey = false;
                    else val += ':';
                }
                else if(chr == '\n' && openers.Count == 0)
                {
                    if(inkey && !string.IsNullOrWhiteSpace(key)) throw new JcfException("Unexpected newline, expected :, at index " + ind + ".");
                    if(!string.IsNullOrWhiteSpace(key)) current.SetValue(key.Trim(), val.Trim());
                    key = ""; val = "";
                    inkey = true;
                }
                else
                {
                    if(inkey) key += chr;
                    else val += chr;
                }
            });

            return current;
        }

        public static Jcf ParseFile(string filename) => Parse(File.ReadAllText(filename));
    }

    public class JcfException : Exception
    {
        public JcfException(string message) : base(message) {}
        public JcfException(string message, Exception inner) : base(message, inner) {}
    }
}
