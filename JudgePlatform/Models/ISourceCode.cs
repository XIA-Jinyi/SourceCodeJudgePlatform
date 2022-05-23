using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JudgePlatform.Models
{
	interface ISourceCode : INotifyPropertyChanged
	{
		Task Test();

		JudgeStatus JudgeStatus { get; set; }

		string FilePath { get; }

		string StatusMessage { get; }

		string StatusDetail { get; set; }
	}
}
