using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace JudgePlatform.Models
{
	class DataCollection<T> : Collection<T>, IDataCollection<T>
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void RaiseCollectionChanged()
		{
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}
