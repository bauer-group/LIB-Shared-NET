using System;
using System.Collections.Generic;

namespace BAUERGROUP.Shared.Core.Logging
{
    /// <summary>
    /// One batch of changes for a UI list mirroring a <see cref="BGLogViewBuffer"/>.
    /// Apply in this order: if <see cref="IsReset"/> clear the list; then remove
    /// <see cref="RemoveFromStart"/> items from the head; then append <see cref="Added"/>.
    /// </summary>
    public readonly struct BGLogViewUpdate
    {
        private readonly IReadOnlyList<BGLogRecord>? _added;

        internal BGLogViewUpdate(bool isReset, int removeFromStart, IReadOnlyList<BGLogRecord>? added)
        {
            IsReset = isReset;
            RemoveFromStart = removeFromStart;
            _added = added;
        }

        /// <summary>The mirroring list must be cleared before the update is applied.</summary>
        public bool IsReset { get; }

        /// <summary>Number of items to remove from the head of the mirroring list.</summary>
        public int RemoveFromStart { get; }

        /// <summary>
        /// Records to append, oldest first. A fresh array per update, never reused.
        /// Never null (empty array when there is nothing to append).
        /// </summary>
        public IReadOnlyList<BGLogRecord> Added
        {
            get { return _added ?? Array.Empty<BGLogRecord>(); }
        }

        /// <summary>True when applying this update would not change the mirroring list.</summary>
        public bool IsEmpty
        {
            get { return !IsReset && RemoveFromStart == 0 && Added.Count == 0; }
        }
    }
}
