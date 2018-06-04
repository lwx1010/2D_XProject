using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace AL
{
	/// <summary>
	/// 提供受限的增删接口，并提供增删通知事件的字典容器
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class ObservableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		private Dictionary<TKey, TValue> m_data = new Dictionary<TKey, TValue>();

		public event EventHandler<EventArgs<KeyValuePair<TKey, TValue>>> ItemAdd;
		public event EventHandler<EventArgs<KeyValuePair<TKey, TValue>>> ItemRemove;

		public TValue this[TKey key]
		{
			get
			{
				TValue value;
				if (m_data.TryGetValue(key, out value))
					return value;
				return default(TValue);
			}
			set
			{
				var has = m_data.ContainsKey(key);
				m_data[key] = value;
				if (has == false)
					OnItemAdd(key, value);
			}
		}

        public Dictionary<TKey, TValue>.KeyCollection Keys 
        {
            get
            {
                return m_data.Keys;
            }
        }
        public Dictionary<TKey, TValue>.ValueCollection Values 
        {
            get
            {
                return m_data.Values;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_data.TryGetValue(key, out value);
        }

		public void Clear()
		{
			var last = m_data;
			m_data = new Dictionary<TKey, TValue>();
			foreach (var pair in last)
				OnItemRemove(pair.Key, pair.Value);
		}

		public bool ContainsKey(TKey key)
		{
			return m_data.ContainsKey(key);
		}

		public int Count { get { return m_data.Count; } }

		public bool Remove(TKey key)
		{
			var has = m_data.ContainsKey(key);
			if (has == false)
				return false;
			var value = m_data[key];
			m_data.Remove(key);
			OnItemRemove(key, value);
			return true;
		}

		private void OnItemAdd(TKey key, TValue value)
		{
			if (ItemAdd != null)
				ItemAdd(this, new EventArgs<KeyValuePair<TKey, TValue>>() { Data = new KeyValuePair<TKey, TValue>(key, value) });
		}

		private void OnItemRemove(TKey key, TValue value)
		{
			if (ItemRemove != null)
				ItemRemove(this, new EventArgs<KeyValuePair<TKey, TValue>>() { Data = new KeyValuePair<TKey, TValue>(key, value) });
		}

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.m_data.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}