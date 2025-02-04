using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lineri.ESS.Core.Interfaces;


namespace Lineri.ESS.Core
{
    public class ListAudio<TAudio> : ICollection, IEnumerable<TAudio>
    where TAudio : class, IAudio
    {
        public int Count => _array.Length;
        public bool IsSynchronized => _array.IsSynchronized;
        public object SyncRoot => _array.SyncRoot;
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;

        private int _capacity = 64;
        
        public int Capacity
        {
            get => _capacity;
            set
            {
                _capacity = value;
                RecreateArray();
            }
        }

        private TAudio[] _array;
        private int _lastIndex = 0;

        #region Constructors
        public ListAudio() 
        { 
            _array = new TAudio[_capacity];
        }

        public ListAudio(int capacity)
        {
            _array = new TAudio[capacity];
        }

        public ListAudio(Audio[] array)
        {
            _array = (TAudio[])array.Clone();
            Capacity = array.Length < Capacity ? Capacity : array.Length;
        }
        #endregion  

        public TAudio[] ToArray()
        {
            return (TAudio[])_array.Clone();
        }

        public void CopyTo(Array array, int index)
        {
            _array.CopyTo(array, index);
        }

        #region Enumerators
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        public IEnumerator<TAudio> GetEnumerator()
        {
            int length = _array.Length;
            for (int i = 0; i < length; i++)
            {
                if (_array[i] != null) yield return _array[i];
            }
        }
        #endregion

        public TAudio this[int index]
        {
            get
            {
                return _array[index];
            }
            set
            {
                _array[index] = value;
            }
        }
        
        public void Remove(int index)
        {
            _array[index] = null;
        }

        public void Clear()
        {
            for (int i = _array.Length - 1; i >= 0; i--)
            {
                _array[i] = null;
            }
        }

        public bool Contains(int index)
        {
            if (index < 0 || index >= _array.Length) return false;
            return _array[index] != null;
        }

        #region IndexOf
        public int IndexOf(Audio value)
        {
            for (int i = _array.Length - 1; i >= 0; i--)
            {
                if (_array[i] == value) return i;
            }

            return -1;
        }

        public int IndexOf(Predicate<TAudio> predicate)
        {
            for (int i = _array.Length - 1; i >= 0; i--)
            {
                if (predicate(_array[i])) return i;
            }

            return -1;
        }

        public int IndexOf(IAudioClip value)
        {
            for (int i = _array.Length - 1; i >= 0; i--)
            {
                if (_array[i]?.Clip == value) return i;
            }

            return -1;
        }
        #endregion
        
        public int Add(TAudio audio)
        {
            int index = GetFreeIndex();
            _array[index] = audio;
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public int GetFreeIndex(bool reset = true)
        {
            int length = _array.Length;

            if (_lastIndex >= length - 1)
            {
                if (reset)
                {
                    ResetIndex();
                }
                else
                {
                    Capacity *= 2;
                }
            }

            if (_array[_lastIndex] == null)
            {
                return _lastIndex;
            }
            else
            {
                _lastIndex++;

                if (_array[_lastIndex] == null)
                {
                    return _lastIndex;
                }
            }

            for (int i = _lastIndex; i < length; i++)
            {
                if (_array[i] != null)
                {
                    _lastIndex++;
                }
                else
                {
                    return i;
                }
            }

            return GetFreeIndex(false);
        }

        public int GetCountNoNull()
        {
            int result = 0;
            for (int i = _array.Length - 1; i >= 0; i--)
            {
                if (_array[i] != null) result++;
            }

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetIndex()
        {
            _lastIndex = 0;
        }

        
        private void RecreateArray()
        {
            TAudio[] result = new TAudio[_capacity];

            for (int i = _array.Length - 1; i >= 0; i--)
            {
                result[i] = _array[i];
            }

            _array = result;
        }
    }
}