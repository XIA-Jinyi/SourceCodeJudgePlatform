using System.Collections.Generic;
using System.Collections.Specialized;

namespace JudgePlatform.Models
{
	/// <summary>
	/// 定义绑定到<see cref="System.Windows.Controls.DataGrid"/>数据源的方法。
	/// </summary>
	/// <typeparam name="T"></typeparam>
	interface IDataCollection<T> : ICollection<T>, INotifyCollectionChanged
	{
		/// <summary>
		/// 当集合更改时向侦听器通知动态更改。
		/// </summary>
		void RaiseCollectionChanged();
	}
}
