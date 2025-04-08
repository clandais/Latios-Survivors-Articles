using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace Survivors.Utilities
{
    /// <summary>
    ///     Gemini 2.5 pro - generated :-/
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TComparer"></typeparam>
    public struct NativeMinHeap<TElement, TComparer> : IDisposable
        where TElement : unmanaged // Element type must be unmanaged
        where TComparer : struct, IComparer<TElement>
    {
        NativeList<TElement> m_Data;
        TComparer            m_Comparer; // Store the comparer instance

        public bool IsCreated => m_Data.IsCreated;
        public int Count => m_Data.Length;
        public bool IsEmpty => m_Data.Length == 0;

        public NativeMinHeap(int initialCapacity,
            Allocator allocator,
            TComparer comparer = default)
        {
            m_Data     = new NativeList<TElement>(initialCapacity, allocator);
            m_Comparer = comparer; // Initialize the comparer
        }

        // Add an element and maintain heap property
        public void Enqueue(TElement element)
        {
            m_Data.Add(element);
            SiftUp(m_Data.Length - 1);
        }

        // Remove and return the smallest element
        public TElement Dequeue()
        {
            if (m_Data.Length == 0) throw new InvalidOperationException("Heap is empty");

            var minElement = m_Data[0];
            var lastIndex = m_Data.Length - 1;

            // Move the last element to the root
            m_Data[0] = m_Data[lastIndex];
            m_Data.RemoveAt(lastIndex); // Remove the last element

            // Restore heap property from the root
            if (m_Data.Length > 0) SiftDown(0);

            return minElement;
        }

        public bool TryDequeue(out TElement element)
        {
            if (IsEmpty)
            {
                element = default;

                return false;
            }


            element = Dequeue();

            return true;
        }

        public TElement Peek()
        {
            if (m_Data.Length == 0) throw new InvalidOperationException("Heap is empty");

            return m_Data[0];
        }


        public bool TryPeek(out TElement element)
        {
            if (IsEmpty)
            {
                element = default;

                return false;
            }


            element = m_Data[0];

            return true;
        }

        public void Clear()
        {
            m_Data.Clear();
        }

        // Move element up the heap
        void SiftUp(int index)
        {
            if (index <= 0) return;

            var parentIndex = (index - 1) / 2;

            // Use the comparer: if element at index is smaller than its parent
            if (m_Comparer.Compare(m_Data[index], m_Data[parentIndex]) < 0)
            {
                // Swap
                (m_Data[index], m_Data[parentIndex]) = (m_Data[parentIndex], m_Data[index]);

                // Continue sifting up from the parent index
                SiftUp(parentIndex);
            }
        }

        // Move element down the heap
        void SiftDown(int index)
        {
            var leftChildIndex = 2 * index + 1;
            var rightChildIndex = 2 * index + 2;
            var smallestIndex = index; // Assume current node is smallest

            // Compare with left child (using comparer)
            if (leftChildIndex < m_Data.Length && m_Comparer.Compare(m_Data[leftChildIndex], m_Data[smallestIndex]) < 0) smallestIndex = leftChildIndex;

            // Compare with right child (using comparer)
            if (rightChildIndex < m_Data.Length && m_Comparer.Compare(m_Data[rightChildIndex], m_Data[smallestIndex]) < 0) smallestIndex = rightChildIndex;

            // If the smallest is not the current node, swap and continue sifting down
            if (smallestIndex != index)
            {
                (m_Data[index], m_Data[smallestIndex]) = (m_Data[smallestIndex], m_Data[index]);

                SiftDown(smallestIndex);
            }
        }

        public void Dispose()
        {
            if (m_Data.IsCreated) m_Data.Dispose();
        }

        // Optional: Dispose with JobHandle
        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (m_Data.IsCreated) return m_Data.Dispose(inputDeps);

            return inputDeps;
        }
    }
}