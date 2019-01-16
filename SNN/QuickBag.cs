using System;
using System.Collections.Generic;

namespace KS.Brain
{
	public class QuickBag<T> where T : class
	{
		private T[] items;
		public int Count { get; private set; }

		public QuickBag() : this(31) {}
		public QuickBag (int capacity)
		{
			if (capacity < 31)
				capacity = 31;
			items = new T[capacity];
		}

		~QuickBag()
		{
			Clear ();
		}

		public int Capacity
		{
			get
			{
				return items.Length;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return Count == 0;
			}
		}

		public void ClearFast()
		{
			Count = 0;
		}

		public void Clear()
		{
			for (int i = 0; i < Count; i++)
				items [i] = null;
			Count = 0;
		}

		public void Add(T item)
		{
			if (Count >= items.Length - 1)
				Grow();
			items[Count++] = item;
		}

		public void AddRange(IEnumerable<T> source)
		{
			foreach (T item in source)
				Add (item);
		}

		private void Grow()
		{			
			Grow((items.Length * 3) / 2 + 1);
		}

		private void Grow(int capacity)
		{
			T[] oldItems = items;
			items = new T[capacity];
			Array.Copy(oldItems, 0, items, 0, oldItems.Length);
		}			

		public T this[int index]
		{
			get
			{
				if (index >= Count)
					return null;
				return items[index];
			}		
		}
	}
}

