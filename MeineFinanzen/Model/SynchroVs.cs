using MeineFinanzen.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace MeineFinanzen.Model {
    public class SynchroVs : ObservableCollection<SynchroV> {
        public event EventHandler<SynchroVergleichChangedEventArgs> Changed;
        protected override void InsertItem(int index, SynchroV newItem) {
            base.InsertItem(index, newItem);
            Changed?.Invoke(this, new SynchroVergleichChangedEventArgs(
                    ChangeType.Added, newItem, null));
        }
        protected override void SetItem(int index, SynchroV newItem) {
            SynchroV replaced = Items[index];
            base.SetItem(index, newItem);

            Changed?.Invoke(this, new SynchroVergleichChangedEventArgs(
                    ChangeType.Replaced, replaced, newItem));
        }
        protected override void RemoveItem(int index) {
            SynchroV removedItem = Items[index];
            base.RemoveItem(index);
            Changed?.Invoke(this, new SynchroVergleichChangedEventArgs(
                    ChangeType.Removed, removedItem, null));
        }
        protected override void ClearItems() {
            base.ClearItems();

            Changed?.Invoke(this, new SynchroVergleichChangedEventArgs(
                    ChangeType.Cleared, null, null));
        }
    }
    public static class HierWirdSortiert {
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector, bool isAZ) {
            if (isAZ) {
                List<TSource> sortedList = source.OrderBy(keySelector).ToList();
                source.Clear();
                foreach (var sortedItem in sortedList) {
                    source.Add(sortedItem);
                }
            } else {
                List<TSource> sortedList = source.OrderByDescending(keySelector).ToList();
                source.Clear();
                foreach (var sortedItem in sortedList) {
                    source.Add(sortedItem);
                }
            }
        }
    }
}