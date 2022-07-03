using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public abstract class BaseObjectPool<T>
	{
		protected Stack<T> m_Stack = new Stack<T>();
		protected int m_CreatedInstances;

		public int TotalCount
		{
			get { return m_CreatedInstances; }
		}

		public int ActiveCount
		{
			get { return m_CreatedInstances - m_Stack.Count; }
		}

		public int InactiveCount
		{
			get { return m_Stack.Count; }
		}

		public virtual T Get()
		{
			T value;
			if (m_Stack.Count == 0)
			{
				value = CreateInstance();
				m_CreatedInstances++;
			}
			else
			{
				value = m_Stack.Pop();
			}
			return value;
		}

		public virtual void Recycle(T element)
		{
			if (element == null)
				throw new System.ArgumentNullException();

			if (m_Stack.Contains(element))
				throw new System.ArgumentException("element already recycled!");

			m_Stack.Push(element);
		}

		protected abstract T CreateInstance();
	}

	public class ObjectPool<T> : BaseObjectPool<T>
		where T : class, new()
	{
		protected override T CreateInstance()
		{
			return new T();
		}
	}

	public class ConcurrentObjectPool<T> : ObjectPool<T>
		where T : class, new()
	{
		protected readonly object m_SyncLock = new object();

		public override T Get()
		{
			lock (m_SyncLock)
			{
				return base.Get();
			}
		}

		public override void Recycle(T element)
		{
			lock (m_SyncLock)
			{
				base.Recycle(element);
			}
		}
	}

	public static class GlobalObjectPool<T>
		where T : class, new()
	{
		public static readonly ConcurrentObjectPool<T> Pool;

		public static GlobalObjectPoolDisposer<T> Get()
		{
			return new GlobalObjectPoolDisposer<T>(Pool.Get());
		}

		static GlobalObjectPool()
		{
			if (typeof(IList).IsAssignableFrom(typeof(T)))
			{
				Pool = new ConcurrentObjectListPool<T>();
			}
			else
			{
				Pool = new ConcurrentObjectPool<T>();
			}
		}

		private class ConcurrentObjectListPool<L> : ConcurrentObjectPool<L>
			where L : class, new()
		{
			public override void Recycle(L element)
			{
				((IList)element).Clear();
				base.Recycle(element);
			}
		}
	}

	public struct GlobalObjectPoolDisposer<T> : System.IDisposable
		where T : class, new()
	{
		public T Object;

		public GlobalObjectPoolDisposer(T obj)
		{
			Object = obj;
		}

		public void Dispose()
		{
			GlobalObjectPool<T>.Pool.Recycle(Object);
			Object = null;
		}

		public static implicit operator T(GlobalObjectPoolDisposer<T> obj)
		{
			return obj.Object;
		}
	}

	/// <summary>
	/// Used to store GameObjects. Get does not create new instances.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GlobalGameObjectPool<T>
		where T : Object
	{
		private Stack<T> m_Stack = new Stack<T>();

		public bool HasObjects { get { return m_Stack != null && m_Stack.Count > 0; } }

		public T Get()
		{
			if (m_Stack.Count >= 0)
				return m_Stack.Pop();

			return null;
		}
		public void Recycle(T element)
		{
			if (element == null)
				throw new System.ArgumentNullException();

			if (m_Stack.Contains(element))
				throw new System.ArgumentException("element already recycled!");

			m_Stack.Push(element);
		}
	}

	/// <summary>
	/// Used to store GameObjects. Get does not create new instances.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GlobalGameObjectKeyPool<K, T>
		where T : Object
	{
		private Dictionary<K, Stack<T>> m_Stack = new Dictionary<K, Stack<T>>();
		public T Get(K key)
		{
			if (m_Stack.Count >= 0 && m_Stack.ContainsKey(key))
				return m_Stack[key].Pop();

			return null;
		}
		public bool HasObjects { get { return m_Stack != null && m_Stack.Count > 0; } }
		public bool HasObject(K key)
		{
			return m_Stack.ContainsKey(key);
		}
		public void Recycle(K key, T element)
		{
			if (element == null)
				throw new System.ArgumentNullException();

			Stack<T> stack;
			if (m_Stack.ContainsKey(key))
				stack = m_Stack[key];
			else
				stack = new Stack<T>();

			if (stack.Contains(element))
				throw new System.ArgumentException("element already recycled!");

			stack.Push(element);
			m_Stack[key] = stack;
		}
	}
}