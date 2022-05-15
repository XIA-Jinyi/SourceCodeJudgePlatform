using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JudgePlatform.Models;
using JudgePlatform.Services;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace JudgePlatform.ViewModels
{
	internal class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			LocalService.InitializeLocalService();
			Codes = new DataCollection<ISourceCode>();
			SelectFolderCommand = new RelayCommand(() =>
			{
				var openFileDialog = new CommonOpenFileDialog
				{
					IsFolderPicker = true,
					Multiselect = false,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					EnsurePathExists = true,
					EnsureFileExists = true
				};
				if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					SelectedFolderPath = openFileDialog.FileName;
				}
				openFileDialog.Dispose();
			});

			TestAllCodesCommand = new RelayCommand(() =>
			{
				isButtonEnabled = false;
				Status = "评测中";
				LocalService.DoEvents();
				foreach (CppSourceCode code in Codes)
				{
					code.Test();
				}
				isButtonEnabled = true;
				Status = "就绪";
			});

			TestNewCodesCommand = new RelayCommand(() =>
			{
				foreach (CppSourceCode code in Codes)
				{
					if (code.JudgeStatus == JudgeStatus.Pending)
					{
						Thread thread = new Thread(() => { code.Test(); });
						thread.Start();
					}
				}
			});

			AddCommand = new RelayCommand(() =>
			{
				var openFileDialog = new OpenFileDialog
				{
					Multiselect = true,
					InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					CheckFileExists = true,
					CheckPathExists = true,
					Filter = "C/C++ 源文件|*.c;*.cpp;*.cc;*.C;*.cxx;*.c++;*.cp|所有文件|*.*",
				};
				if (openFileDialog.ShowDialog() ?? false)
				{
					foreach (var fileName in openFileDialog.FileNames)
					{
						var code = new CppSourceCode(fileName);
						Codes.Add(code);
						RaisePropertyChanged(nameof(Codes));
						code.RaisePropertyChanged("FilePath");
						code.RaisePropertyChanged("StatusMessage");
					}
				}
				RaisePropertyChanged(nameof(Codes));
				Codes.RaiseCollectionChanged();
			});
		}

		public RelayCommand SelectFolderCommand { get; set; }

		public RelayCommand TestAllCodesCommand { get; set; }

		public RelayCommand TestNewCodesCommand { get; set; }

		public RelayCommand AddCommand { get; set; }

		private bool isButtonEnabled = true;

		public bool IsButtonEnabled
		{
			get => isButtonEnabled;
			set
			{
				isButtonEnabled = value;
				RaisePropertyChanged(nameof(IsButtonEnabled));
			}
		}

		private string status = "就绪";

		public string Status
		{
			get => status;
			set
			{
				status = value;
				RaisePropertyChanged(nameof(Status));
			}
		}

		public DataCollection<ISourceCode> Codes { get; set; }

		public string SelectedFolderPath
		{
			get => LocalService.TestDataFolder ?? "未选择";

			set
			{
				LocalService.TestDataFolder = value;
				RaisePropertyChanged(nameof(SelectedFolderPath));
			}
		}

		//public void DoEvents()
		//{
		//	DispatcherFrame frame = new DispatcherFrame();
		//	Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
		//			new DispatcherOperationCallback(delegate (object f)
		//			{
		//				((DispatcherFrame)f).Continue = false;
		//				return null;
		//			}
		//		), frame);
		//	Dispatcher.PushFrame(frame);
		//}
	}
}
