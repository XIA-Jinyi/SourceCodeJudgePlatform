using System;
using System.Collections.Generic;
using System.IO;
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
			Codes = new DataCollection<CppSourceCode>();
			SelectFolderCommand = new RelayCommand(() =>
			{
				var openFileDialog = new CommonOpenFileDialog
				{
					IsFolderPicker = true,
					Multiselect = false,
					//InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					EnsurePathExists = true,
					EnsureFileExists = true
				};
				if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
				{
					SelectedFolderPath = openFileDialog.FileName;
				}
				openFileDialog.Dispose();
			});

			TestAllCodesCommand = new RelayCommand(async () =>
			{
				IsButtonEnabled = false;
				Status = "评测全部代码中";
				foreach (CppSourceCode code in Codes)
                {
					code.JudgeStatus = JudgeStatus.Pending;
					code.RaisePropertyChanged(nameof(code.StatusMessage));
				}
				RaisePropertyChanged(nameof(Codes));
				LocalService.DoEvents();
				foreach (CppSourceCode code in Codes)
				{
					await code.Test();
				}
				await Task.Run(() =>
				{
					while (true)
					{
						bool isFinished = true;
						foreach (ISourceCode code in Codes)
						{
							if (code.JudgeStatus == JudgeStatus.Running || code.JudgeStatus == JudgeStatus.Pending || code.JudgeStatus == JudgeStatus.Compiling)
							{
								isFinished = false;
							}
						}
						if (isFinished)
                        {
							break;
                        }
					}
				});
				IsButtonEnabled = true;
				Status = "就绪";
			});

			TestNewCodesCommand = new RelayCommand(async () =>
			{
				IsButtonEnabled = false;
				Status = "评测未评测代码中";
				foreach (CppSourceCode code in Codes)
				{
					if (code.JudgeStatus == JudgeStatus.Pending)
					{
                        await code.Test();
					}
				}
				await Task.Run(() =>
				{
					while (true)
					{
						bool isFinished = true;
						foreach (ISourceCode code in Codes)
						{
							if (code.JudgeStatus == JudgeStatus.Running || code.JudgeStatus == JudgeStatus.Pending || code.JudgeStatus == JudgeStatus.Compiling)
							{
								isFinished = false;
							}
						}
						if (isFinished)
						{
							break;
						}
					}
				});
				IsButtonEnabled = true;
				Status = "就绪";
			});

			AddCommand = new RelayCommand(() =>
			{
				var openFileDialog = new OpenFileDialog
				{
					Multiselect = true,
					//InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
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

			ExportMdCommand = new RelayCommand(() =>
			{
				var saveFileDialog = new SaveFileDialog
				{
					Filter = "MarkDown文件|*.md|所有文件|*.*",
					AddExtension = true,
					DefaultExt = "md",
					CheckPathExists = true,
					FileName = $"{DateTime.Now:yyyy-MM-dd.HH.mm.ss}",
				};
				if (saveFileDialog.ShowDialog() ?? false)
				{
					using (StreamWriter output = new StreamWriter(saveFileDialog.FileName, false, Encoding.Default))
					{
						output.WriteLine("# 评测报告\n");
						output.WriteLine($"{DateTime.Now:yyyy年M月d日}\n");
						output.WriteLine("## 摘要\n");
						output.WriteLine("|路径|状态|得分|");
						output.WriteLine("| :--- | :---: | ---: |");
						foreach (CppSourceCode code in Codes)
						{
							output.Write($"|{code.FilePath}|{code.StatusMessage}|");
							if (code.StatusList.Count == 0)
							{
								output.WriteLine("0|");
							}
							else
							{
								int countAll = code.StatusList.Count;
								int countPassed = 0;
								foreach (var status in code.StatusList)
								{
									if (status.status == JudgeStatus.Accepted)
									{
										countPassed++;
									}
								}
								output.WriteLine($"{100 * (double)countPassed / countAll:0.}|");
							}
						}
						output.WriteLine();
						output.WriteLine("## 详细信息\n");
						foreach (CppSourceCode code in Codes)
						{
							output.WriteLine($"### {code.FilePath}\n");
							output.WriteLine($"__评测结果：__ {code.StatusMessage}\n");
							double score = 0;
							if (code.StatusList.Count != 0)
							{
								int countAll = code.StatusList.Count;
								int countPassed = 0;
								foreach (var status in code.StatusList)
								{
									if (status.status == JudgeStatus.Accepted)
									{
										countPassed++;
									}
								}
								score = 100.0 * countPassed / countAll;
							}
							output.WriteLine($"__得分：__ {score:0.}\n");
							if (code.StatusDetail != string.Empty)
                            {
								output.WriteLine($"__编译器输出信息：__ \n");
								output.WriteLine("```");
								output.Write(code.StatusDetail);
								output.WriteLine("```\n");
							}
							if (code.StatusList.Count != 0)
                            {
								int index = 1;
								output.WriteLine($"__数据点详情：__ \n");
								output.WriteLine("|序号|耗时|内存|状态|");
								output.WriteLine("| ---: | :--- | :--- | :---: |");
								foreach (var status in code.StatusList)
								{
									output.WriteLine($"|{index++}|{status.timeConsumption:0.} ms|{status.memoryConsumption:0.} KB|{LocalService.InterpretJudgeStatus(status.status)}|");
								}
								output.WriteLine();
							}
						}
					}
				}
			});

			ExportCsvCommand = new RelayCommand(() =>
            {
				var saveFileDialog = new SaveFileDialog
				{
					Filter = "CSV文件|*.csv|所有文件|*.*",
					AddExtension = true,
					DefaultExt = "csv",
					CheckPathExists = true,
					FileName = $"{DateTime.Now:yyyy-MM-dd.HH.mm.ss}",
				};
				if (saveFileDialog.ShowDialog() ?? false)
                {
					using (StreamWriter output = new StreamWriter(saveFileDialog.FileName, false, Encoding.Default))
                    {
						output.WriteLine("路径,状态,得分");
						foreach (CppSourceCode code in Codes)
                        {
							output.Write($"\"{code.FilePath}\",{code.StatusMessage},");
							if (code.StatusList.Count == 0)
                            {
								output.WriteLine("0,");
                            }
							else
                            {
								int countAll = code.StatusList.Count;
								int countPassed = 0;
								foreach (var status in code.StatusList)
                                {
									if (status.status == JudgeStatus.Accepted)
                                    {
										countPassed++;
                                    }
                                }
                                output.WriteLine($"{100 * (double)countPassed / countAll:0.}");
                            }
                        }
                    }
                }
			});
		}

		public RelayCommand SelectFolderCommand { get; set; }

		public RelayCommand TestAllCodesCommand { get; set; }

		public RelayCommand TestNewCodesCommand { get; set; }

		public RelayCommand AddCommand { get; set; }

		public RelayCommand ExportMdCommand { get; set; }

		public RelayCommand ExportCsvCommand { get; set; }

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

		public DataCollection<CppSourceCode> Codes { get; set; }

		public string SelectedFolderPath
		{
			get => LocalService.TestDataFolder ?? "未选择";

			set
			{
				LocalService.TestDataFolder = value;
				RaisePropertyChanged(nameof(SelectedFolderPath));
			}
		}
	}
}
