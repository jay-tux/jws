using System;
using System.Collections.Generic;
using System.IO;
using Shub;

namespace Parser
{
    public class CnfParser
    {
        private Dictionary<string, CnfParser> Subs;
        private Dictionary<string, string> Values;

        public CnfParser()
        {
            Subs = new Dictionary<string, CnfParser>();
            Values = new Dictionary<string, string>();
        }

        public void Parse(string file)
        {
	    Console.WriteLine(file);
	    string key = "";
	    string val = "";
	    bool inkey = true;
	    bool inBlock = false;
	    int depth = 0;
            file.ForEach(chr => {
		if(chr == ':' && !inBlock)
		{
		    Console.WriteLine($"Moved from key {key} to its value.");
		    inkey = false;
		}
		else if(chr == '{')
		{
		    inBlock = true;
		    depth++;
		    if(depth != 1) { val += chr; }
		}
		else if(chr == '}')
		{
		    depth--;
		    if(depth == 0) { 
		    	inBlock = false; 
			Subs[key.Trim()] = new CnfParser();
			Console.WriteLine("ADDED SUB: " + key.Trim());
			Subs[key.Trim()].Parse(val);
			key = "";
			val = "";
		    }
		    else { val += chr; }
		}
		else if(inBlock)
		{
			val += chr;
		}
		else if(chr == '\n')
		{
			inkey = true;
			if(key.Trim() != "" && val.Trim() != "") 
			{
			    Console.WriteLine("ADDED KVP: " + key.Trim());
			    Values[key.Trim()] = val.Trim(); 
			}
			else if(key.Trim() == "") { Console.WriteLine($"Skipped empty key ('{key}')."); }
			else { Console.WriteLine($"Skipped empty value ('{val}')."); }
			key = ""; val = "";
		}
		else
		{
			if(inkey) { key += chr; }
			else { val += chr; }
		}
            });
        }

	public override string ToString() => ToString(0);

	public string ToString(int i)
	{
	    string result = "\n";
	    Subs.ForEach(sub => { result += $"{indent(i)}[SUB] '{sub.Key}' = {sub.Value.ToString(i + 2)}\n"; });
	    Values.ForEach(val => { result += $"{indent(i)}[KVP] '{val.Key}' = '{val.Value}'\n"; });
	    return result;
	}

	private static string indent(int am) 
	{
	    string res = "";
	    for(int i = 0; i < am; i++) { res += " "; }
	    return res;
	}
    }
}






