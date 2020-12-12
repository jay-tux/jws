using System;
using System.Linq;
using System.Collections.Generic;

namespace Jay.Config
{
    public enum ResultOptions { Success, TypeWrong, UnknownKey }

    public class JcfResult<T> where T : class
    {
        public T value;
        public ResultOptions state;
        public ArgumentException exception;

        public static implicit operator bool(JcfResult<T> res)
            => res.state == ResultOptions.Success;

        public static explicit operator T(JcfResult<T> res)
            => res.value;

        public static explicit operator ArgumentException(JcfResult<T> res)
            => res.exception;

        public static explicit operator JcfResult<T>(T res) => new JcfResult<T> {
            value = res,
            state = ResultOptions.Success,
            exception = null
        };

        public static explicit operator JcfResult<T>(ArgumentException e) => new JcfResult<T> {
            value = null,
            state = ResultOptions.UnknownKey,
            exception = e
        };

        public JcfResult<T2> CrossCast<T2>() where T2 : class
        {
            if(this)
                throw new ArgumentException("Result option Success can't be instantiated from throughcast.");
            return new JcfResult<T2> {
                value = null,
                exception = this.exception,
                state = this.state
            };
        }

        public static explicit operator JcfResult<T>(ResultOptions opt)
        {
            if(opt != ResultOptions.TypeWrong)
                throw new ArgumentException("Only result option TypeWrong can be instantiated from cast.");
            return new JcfResult<T> {
                value = null,
                exception = null,
                state = opt
            };
        }
    }

    public static class JcfExt
    {
        public static JcfResult<string> GetString(this Jcf source, string key)
        {
            try
            {
                return (source[key] is string str) ? (JcfResult<string>)str :
                    (JcfResult<string>)ResultOptions.TypeWrong;
            }
            catch(ArgumentException ae)
            {
                return (JcfResult<string>)ae;
            }
        }

        public static JcfResult<Jcf> GetBlock(this Jcf source, string key)
        {
            try
            {
                return (source[key] is Jcf jcf) ? (JcfResult<Jcf>)jcf :
                    (JcfResult<Jcf>)ResultOptions.TypeWrong;
            }
            catch(ArgumentException ae)
            {
                return (JcfResult<Jcf>)ae;
            }
        }

        public static JcfResult<List<Jcf>> GetList(this Jcf source, string key)
        {
            try
            {
                return (source[key] is List<Jcf> jcf) ? (JcfResult<List<Jcf>>)jcf :
                    (JcfResult<List<Jcf>>)ResultOptions.TypeWrong;
            }
            catch(ArgumentException ae)
            {
                return (JcfResult<List<Jcf>>)ae;
            }
        }

        public static JcfResult<IEnumerable<JcfResult<string>>> GetMappedList<TOut>(this Jcf source,
            string key, string innerkey)
        {
            JcfResult<List<Jcf>> lst = source.GetList(key);
            return (!lst) ? lst.CrossCast<IEnumerable<JcfResult<string>>>() :
                (JcfResult<IEnumerable<JcfResult<string>>>)lst.value.Select(jcf => jcf.GetString(innerkey));
        }
    }
}
