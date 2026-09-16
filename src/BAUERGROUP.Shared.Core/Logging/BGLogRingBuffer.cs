using System;
using System.Collections.Generic;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// Fixed-capacity, thread-safe, drop-oldest FIFO of <see cref="BGLogRecord"/>.
    /// Producers call <see cref="Add"/> (O(1), short lock, never waits on a UI thread);
    /// the view thread calls <see cref="DrainTo"/>.
    /// </summary>
    internal sealed class BGLogRingBuffer
    {
        private readonly object _sync = new object();

        private BGLogRecord?[] _items;
        private int _head;
        private int _count;
        private long _dropped;

        /// <summary>Creates a ring buffer holding at most <paramref name="capacity"/> records.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
        internal BGLogRingBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "The capacity must be at least 1.");

            _items = new BGLogRecord?[capacity];
        }

        /// <summary>Number of records the buffer can hold before the oldest one is dropped.</summary>
        internal int Capacity
        {
            get { lock (_sync) { return _items.Length; } }
        }

        /// <summary>Number of records waiting to be drained.</summary>
        internal int Count
        {
            get { lock (_sync) { return _count; } }
        }

        /// <summary>
        /// Records discarded before they were drained: overwritten because the buffer was full, or dropped
        /// by a <see cref="Resize"/> to a smaller capacity. Never reset by <see cref="Clear"/>.
        /// </summary>
        internal long DroppedCount
        {
            get { lock (_sync) { return _dropped; } }
        }

        /// <summary>Appends a record, overwriting the oldest one when the buffer is full.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is null.</exception>
        internal void Add(BGLogRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            lock (_sync)
            {
                var capacity = _items.Length;
                var tail = _head + _count;
                if (tail >= capacity)
                    tail -= capacity;

                _items[tail] = record;

                if (_count == capacity)
                {
                    _head = _head + 1 == capacity ? 0 : _head + 1;
                    _dropped++;
                }
                else
                {
                    _count++;
                }
            }
        }

        /// <summary>
        /// Moves every buffered record into <paramref name="destination"/>, oldest first, and empties the
        /// buffer. Drained slots are nulled so the records become collectable.
        /// </summary>
        /// <returns>The number of records appended to <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
        internal int DrainTo(List<BGLogRecord> destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            lock (_sync)
            {
                var capacity = _items.Length;
                var count = _count;
                var index = _head;

                for (var i = 0; i < count; i++)
                {
                    // Every slot in [_head, _head + _count) holds a record by construction: Add never stores
                    // null, and DrainTo, Resize and Clear null slots only outside that window.
                    destination.Add(_items[index]!);
                    _items[index] = null;

                    index = index + 1 == capacity ? 0 : index + 1;
                }

                _head = 0;
                _count = 0;

                return count;
            }
        }

        /// <summary>Changes the capacity, keeping the newest <c>min(Count, capacity)</c> records.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
        internal void Resize(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "The capacity must be at least 1.");

            lock (_sync)
            {
                var currentCapacity = _items.Length;
                if (capacity == currentCapacity)
                    return;

                var items = new BGLogRecord?[capacity];
                var keep = Math.Min(_count, capacity);
                var skip = _count - keep;
                var index = _head + skip;
                if (index >= currentCapacity)
                    index -= currentCapacity;

                for (var i = 0; i < keep; i++)
                {
                    items[i] = _items[index];
                    index = index + 1 == currentCapacity ? 0 : index + 1;
                }

                _items = items;
                _head = 0;
                _count = keep;
                _dropped += skip;
            }
        }

        /// <summary>Empties the buffer. <see cref="DroppedCount"/> is deliberately NOT reset.</summary>
        internal void Clear()
        {
            lock (_sync)
            {
                Array.Clear(_items, 0, _items.Length);
                _head = 0;
                _count = 0;
            }
        }
    }
}
