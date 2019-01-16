using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KS.Foundation;

namespace KS.Brain
{	
	public class Bag<T> where T : class
	{
		private T[] m_Data;
		public int Count { get; private set; }

		public Bag()
		{
			m_Data = new T[31];
		}

		public Bag(int capacity)
		{
			if (capacity < 31)
				capacity = 31;

			m_Data = new T[capacity];
		}

		// *** Properties ***

		public int Capacity
		{
			get
			{
				return m_Data.Length;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return Count == 0;
			}
		}

		public bool Contains(T value)
		{
			if (value == null)
				return false;

			for (int i = 0; Count > i; i++)
			{				
				if (ReferenceEquals(value, m_Data[i]))
				//if (value.Equals(m_Data[i]))
				{
					return true;
				}
			}
			return false;
		}

		// *** Get / Set ***

		public T Get(int index)
		{
			try
			{
				return m_Data[index];
			}
			catch (Exception e)
			{
				e.LogError ();
				return null;
			}
		}

		public void Set(int index, T value)
		{
			if (index >= m_Data.Length-1)
				Grow(index * 2);

			if (index >= Count)
				Count = index + 1;

			m_Data[index] = value;
		}

		public T this[int index]
		{
			get
			{
				return Get(index);
			}
			set
			{
				Set(index, value);
			}
		}

		// *** Add / Remove ***

		public void Add(T value)
		{
			if (Count >= m_Data.Length - 1)
				Grow();

			m_Data[Count++] = value;
		}

		public void AddRange(Bag<T> items)
		{
			for (int i = 0, j = items.Count; j > i; i++)
				Add(items.Get(i));
		}

		public T Remove(int index)
		{
			T o = m_Data[index];
			m_Data[index] = m_Data[--Count];
			m_Data[Count] = null;
			return o;
		}
			
		public T RemoveLast()
		{
			if (Count > 0)
			{
				T value = m_Data[--Count];
				m_Data[Count] = null;
				return value;
			}

			return default(T);
		}
			
		public bool Remove(T value)
		{			
			for (int i = 0; i < Count; i++)
			{								
				if (ReferenceEquals(value, m_Data[i]))
				//if (value.Equals(m_Data[i]))
				{
					m_Data[i] = m_Data[--Count];
					m_Data[Count] = null;
					return true;
				}
			}

			return false;
		}			
			
		public bool RemoveAll(Bag<T> bag)
		{
			bool modified = false;

			for (int i = 0, bagSize = bag.Count; i < bagSize; i++)
			{
				T obj = bag.Get(i);
				for (int j = 0; j < Count; j++)
				{					
					if (ReferenceEquals(obj, m_Data[j]))
					//if (obj.Equals(m_Data[i]))
					{
						Remove(j);
						j--;
						modified = true;
						break;
					}
				}
			}

			return modified;
		}			
			
		public void Clear()
		{			
			for (int i = 0; i < m_Data.Length; i++)
				m_Data[i] = null;
			Count = 0;
		}			

		// private helpers

		private void Grow()
		{			
			Grow((m_Data.Length * 3) / 2 + 1);
		}

		private void Grow(int capacity)
		{
			T[] prevData = m_Data;
			m_Data = new T[capacity];
			Array.Copy(prevData, 0, m_Data, 0, prevData.Length);
		}
	}
}

