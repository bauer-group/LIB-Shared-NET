using BAUERGROUP.Shared.Core.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BAUERGROUP.Shared.Desktop.Logging
{
    /// <summary>
    /// Observable list of log records that applies a <see cref="BGLogViewUpdate"/> as one batch.
    /// </summary>
    /// <remarks>
    /// WPF's <c>CollectionView</c> rejects multi-item range actions in <c>CollectionChanged</c>, so a Reset is
    /// the only batched notification available - and a Reset tells the <c>ItemsControl</c> that everything
    /// changed, which discards the realized containers and re-validates the whole view. Large batches still
    /// take it, because the alternative is worse: every per-item removal from the head of a list with
    /// thousands of entries costs a full memmove. The live-tailing steady state - a handful of records per
    /// tick - stays per item, so the list is updated incrementally instead of rebuilt.
    /// Selection survives either path: WPF's <c>Selector</c> re-validates its selection against the new
    /// contents and keeps every selected item that is still present.
    /// </remarks>
    internal sealed class LogRecordCollection : ObservableCollection<BGLogRecord>
    {
        private const int ResetThreshold = 64;

        private static readonly PropertyChangedEventArgs CountChanged = new PropertyChangedEventArgs("Count");
        private static readonly PropertyChangedEventArgs IndexerChanged = new PropertyChangedEventArgs("Item[]");
        private static readonly NotifyCollectionChangedEventArgs CollectionReset =
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        /// <summary>Applies one batch of changes: reset, then remove from the head, then append.</summary>
        internal void Apply(BGLogViewUpdate update)
        {
            if (update.IsEmpty)
                return;

            var added = update.Added;
            var removeFromStart = Math.Min(update.RemoveFromStart, Count);

            if (update.IsReset || removeFromStart + added.Count > ResetThreshold)
            {
                var items = (List<BGLogRecord>)Items;

                if (update.IsReset)
                    items.Clear();
                else if (removeFromStart > 0)
                    items.RemoveRange(0, removeFromStart);

                if (added.Count > 0)
                    items.AddRange(added);

                OnPropertyChanged(CountChanged);
                OnPropertyChanged(IndexerChanged);
                OnCollectionChanged(CollectionReset);
                return;
            }

            for (var i = 0; i < removeFromStart; i++)
                RemoveAt(0);

            for (var i = 0; i < added.Count; i++)
                Add(added[i]);
        }
    }
}
