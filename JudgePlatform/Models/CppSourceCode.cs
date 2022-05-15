using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using JudgePlatform.Services;

namespace JudgePlatform.Models
{
	internal class CppSourceCode : ViewModelBase, ISourceCode
	{
		public CppSourceCode(string filePath)
		{
			path = filePath;
			JudgeStatus = JudgeStatus.Pending;
			StatusDetail = string.Empty;
		}
		
		public JudgeStatus JudgeStatus { get; set; }

		protected string path;

		public string FilePath => path;

		public string StatusMessage
		{
			get
			{
				switch (JudgeStatus)
				{
					case JudgeStatus.Accepted: return "程序通过";
					case JudgeStatus.WrongAnswer: return "答案错误";
					case JudgeStatus.PresentationError: return "格式错误";
					case JudgeStatus.CompileError: return "编译失败";
					case JudgeStatus.RuntimeError: return "运行时错误";
					case JudgeStatus.TimeLimitExceeded: return "运行超时";
					case JudgeStatus.MemoryLimitExceeded: return "内存超限";
					case JudgeStatus.OutputLimitExceeded: return "输出超限";
					case JudgeStatus.Pending: return "等待评测";
					case JudgeStatus.Compiling: return "正在编译";
					case JudgeStatus.Running: return "正在运行";
					case JudgeStatus.FileException: return "文件异常";
					case JudgeStatus.CompileException: return "编译异常";
					case JudgeStatus.TestException: return "数据异常";
					default: return "系统异常";
				}
			}
		}

		public string StatusDetail { get; set; }

		public void Test()
		{
			try
			{
				string compilerPath = LocalService.FindConfigEntry("CppCompilerPath");
				string compileArgs = LocalService.FindConfigEntry("CppCompileArguments");
				var testDataList = LocalService.TestDataList;
				if (!File.Exists(FilePath))
				{
					JudgeStatus = JudgeStatus.FileException;
				}
				else if (compilerPath == null)
				{
					JudgeStatus = JudgeStatus.CompileException;
				}
				else if (testDataList.Count == 0)
				{
					JudgeStatus = JudgeStatus.TestException;
				}
				else
				{
					// 编译目标代码。
					JudgeStatus = JudgeStatus.Compiling;
					RaisePropertyChanged(nameof(StatusMessage));
					using (Process compileProcess = new Process())
					{
						compileProcess.StartInfo.FileName = compilerPath;
						compileProcess.StartInfo.Arguments = LocalService.ParseString(
							compileArgs,
							new Tuple<string, string>("SOURCE", FilePath),
							new Tuple<string, string>("OUTPUT", LocalService.CompiledExecutableFilePath)
							);
						compileProcess.StartInfo.UseShellExecute = false;
						compileProcess.StartInfo.CreateNoWindow = true;
						compileProcess.StartInfo.RedirectStandardError = true;
						compileProcess.StartInfo.RedirectStandardOutput = true;
						compileProcess.StartInfo.RedirectStandardInput = true;
						compileProcess.Start();
						compileProcess.WaitForExit();
						switch (compileProcess.ExitCode)
						{
							case 0:
								JudgeStatus = JudgeStatus.Running;
								break;
							case 1:
								JudgeStatus = JudgeStatus.CompileError;
								StatusDetail += compileProcess.StandardError.ReadToEnd();
								break;
							default:
								JudgeStatus = JudgeStatus.CompileException;
								break;
						}
						RaisePropertyChanged(nameof(StatusMessage));
						LocalService.DoEvents();
					}
					if (JudgeStatus != JudgeStatus.Running)
					{
						LocalService.DoEvents();
						return;
					}
					// 测试目标代码。
					if (testDataList.Count == 0)
					{
						JudgeStatus = JudgeStatus.TestException;
					}
					else
					{
						foreach (var testData in testDataList)
						{
							using (Process targetProcess = new Process())
							{
								try
								{
									targetProcess.StartInfo.FileName = LocalService.CompiledExecutableFilePath;
									targetProcess.StartInfo.UseShellExecute = false;
									targetProcess.StartInfo.CreateNoWindow = true;
									targetProcess.StartInfo.RedirectStandardError = true;
									targetProcess.StartInfo.RedirectStandardOutput = true;
									targetProcess.StartInfo.RedirectStandardInput = true;
									targetProcess.Start();
									targetProcess.StandardInput.WriteLine(testData.Item1);
									targetProcess.StandardInput.Flush();
									int timeSpan = Convert.ToInt32(LocalService.FindConfigEntry("TimeLimit"));
									double memorySize = 0.0;
									DateTime startDT = DateTime.Now;
									while ((DateTime.Now - startDT).TotalMilliseconds < timeSpan)
									{
										double currentMemorySize = targetProcess.PeakPagedMemorySize64 / 1024.0;
										memorySize = currentMemorySize > memorySize ? currentMemorySize : memorySize;
										if (targetProcess.HasExited)
										{
											break;
										}
									}
									if (targetProcess.HasExited)
									{
										if (targetProcess.ExitCode != 0)
										{
											JudgeStatus = JudgeStatus.RuntimeError;
											break;
										}
										else if (memorySize > Convert.ToDouble(LocalService.FindConfigEntry("MemoryLimit")))
										{
											JudgeStatus = JudgeStatus.MemoryLimitExceeded;
											break;
										}
										else
										{
											using (StreamWriter targetOutput = new StreamWriter(LocalService.TargetOutputFilePath), standardOutput = new StreamWriter(LocalService.StandardOutputFilePath))
											{
												targetOutput.WriteLine(targetProcess.StandardOutput.ReadToEnd());
												targetOutput.Write("\0");
												standardOutput.WriteLine(testData.Item2);
												standardOutput.Write("\0");
												targetOutput.Flush();
												standardOutput.Flush();
											}
											/*比对输出结果*/
										}
									}
									else
									{
										JudgeStatus = JudgeStatus.TimeLimitExceeded;
										break;
									}
								}
								catch (Exception ex)
								{
									throw ex;
								}
								finally
								{
									if (!targetProcess.HasExited)
										targetProcess.Kill();
								}
							}
						}
						if (JudgeStatus == JudgeStatus.Running)
						{
							JudgeStatus = JudgeStatus.Accepted;
						}
					}
				}
			}
			catch (Exception ex)
			{
				JudgeStatus = JudgeStatus.InternalException;
				MessageBox.Show(ex.Message);
			}
			finally
			{
				RaisePropertyChanged(nameof(StatusMessage));
				RaisePropertyChanged(nameof(StatusDetail));
				RaisePropertyChanged(nameof(JudgeStatus));
				LocalService.DoEvents();
			}
		}
	}
}
