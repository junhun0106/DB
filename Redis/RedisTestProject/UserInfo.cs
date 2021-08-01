using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RedisTestProject
{
    public class UserInfo
    {
        public readonly ConcurrentDictionary<string, object> UpdateData = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);

        private string _token;
        private int _value;

        public string Token {
            get => _token;
            set {
                if (_token == value) {
                    return;
                }
                _token = value;
                AddOrUpdate(newValue: value);
            }
        }

        public int Value {
            get => _value;
            set {
                if (_value == value) {
                    return;
                }
                _value = value;
                AddOrUpdate(newValue: value);
            }
        }

        public UserInfo() { }

        public UserInfo(Dictionary<string, object> dic)
        {
            if (dic?.Count > 0) {
                foreach (var kv in dic) {
                    Set(kv.Key, kv.Value);
                }
            }
        }

        private void Set(string key, object value)
        {
            switch (key) {
                case nameof(Token): _token = (string)value; break;
                case nameof(Value): _value = Convert.ToInt32(value); break;
            }
        }

        private void AddOrUpdate([CallerMemberName] string name = null, object newValue = null)
        {
            if (string.IsNullOrEmpty(name) || newValue == null) {
                return;
            }

            if (UpdateData.TryGetValue(name, out var oldValue)) {
                UpdateData.TryUpdate(name, newValue, oldValue);
            } else {
                UpdateData.TryAdd(name, newValue);
            }
        }
    }
}
