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

        public static implicit operator bool(JcfResult<T> res)
            => res.state == ResultOptions.Success;
    }

    public static class JcfExt
    {
        public static JcfResult<string> GetString(this Jcf source, string key)
        {
            try
            {
                object s = source[key];
                if(s is string str)
                {
                    return new JcfResult<string> {
                        value = str,
                        state = ResultOptions.Success
                    };
                }
                else {
                    return new JcfResult<string> {
                        value = null,
                        state = ResultOptions.TypeWrong
                    };
                }
            }
            catch(ArgumentException)
            {
                return new JcfResult<string> {
                    value = null,
                    state = ResultOptions.UnknownKey
                };
            }
        }

        public static JcfResult<Jcf> GetBlock(this Jcf source, string key)
        {
            try
            {
                object j = source[key];
                if(j is Jcf jcf)
                {
                    return new JcfResult<Jcf> {
                        value = jcf,
                        state = ResultOptions.Success
                    };
                }
                else {
                    return new JcfResult<Jcf> {
                        value = null,
                        state = ResultOptions.TypeWrong
                    };
                }
            }
            catch(ArgumentException)
            {
                return new JcfResult<Jcf> {
                    value = null,
                    state = ResultOptions.UnknownKey
                };
            }
        }

        public static JcfResult<List<Jcf>> GetList(this Jcf source, string key)
        {
            try
            {
                object l = source[key];
                if(l is List<Jcf> lst)
                {
                    return new JcfResult<List<Jcf>> {
                        value = lst,
                        state = ResultOptions.Success
                    };
                }
                else {
                    return new JcfResult<List<Jcf>> {
                        value = null,
                        state = ResultOptions.TypeWrong
                    };
                }
            }
            catch(ArgumentException)
            {
                return new JcfResult<List<Jcf>> {
                    value = null,
                    state = ResultOptions.UnknownKey
                };
            }
        }

        public static JcfResult<IEnumerable<JcfResult<string>>> GetMappedList<TOut>(this Jcf source,
            string key, string innerkey)
        {
            JcfResult<List<Jcf>> lst = source.GetList(key);
            return (!lst)
                ? (new JcfResult<IEnumerable<JcfResult<string>>> {
                    value = null,
                    state = lst.state
                })
                : (new JcfResult<IEnumerable<JcfResult<string>>> {
                    value = lst.value.Select(jcf => jcf.GetString(innerkey)),
                    state = ResultOptions.Success
                });
        }
    }
}
