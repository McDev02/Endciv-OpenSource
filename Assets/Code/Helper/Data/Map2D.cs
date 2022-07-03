using System.Collections.Generic;
using System;

namespace Endciv
{
    public class Map2D<T>
    {
        public T[,] arr;

        public T this[int i, int j]
        {
            get
            {
                return arr[i, j];
            }
            set
            {
				if(!EqualityComparer<T>.Default.Equals(arr[i,j], value))
                {
                    arr[i, j] = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(new Vector2i(i, j), value);
                    }
                }

            }
        }

        public Action<Vector2i, T> PropertyChanged;

        public Map2D(int lengthA, int lengthB)
        {
            arr = new T[lengthA, lengthB];
        }

        public Map2D(T[,] arr)
        {
            this.arr = arr;
        }

        public T[,] ToArray()
        {
            return arr;
        }

        public override string ToString()
        {
            return arr.ToString();
        }
    }
}