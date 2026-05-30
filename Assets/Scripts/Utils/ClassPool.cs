namespace ZGameFrameWork.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    public interface IPoolable
    {
        void OnRecycled();
    }

    /// <summary>
    /// C#层的对象池，集合类的对象池请使用ListPool和DictionaryPool，专门用来装list和字典的对象池，省的拿list或dic得Clear
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClassPool<T> where T : class, new()
    {
        //使用栈来存储对象池中的对象,因为栈的Push和Pop操作效率较高，并且增加缓存命中率，提高性能
        //得到后需要手动清除旧数据,或者实现IPoolable接口在OnRecycled中清除数据
        public static readonly Stack<T> pool = new Stack<T>();

        public static T Get()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            else
            {
                return new T();
            }
        }

        public static void Recycle(T obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj is IPoolable poolable)
            {
                poolable.OnRecycled();
            }
            pool.Push(obj);
        }

        public static void Clear()
        {
            pool.Clear();
        }
    }

}