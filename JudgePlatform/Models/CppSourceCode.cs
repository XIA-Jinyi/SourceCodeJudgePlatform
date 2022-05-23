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
			StatusList = new List<StatusRecord>();
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
					case JudgeStatus.MultipleError: return "多种错误";
					case JudgeStatus.InternalException: return "系统异常";
					default: return "未知异常";
				}
			}
		}

		public string StatusDetail { get; set; }

		public struct StatusRecord
        {
			public JudgeStatus status;
			public double timeConsumption;
			public double memoryConsumption;
        }

		public List<StatusRecord> StatusList { get; set; }

		public async Task Test()
		{
			StatusDetail = string.Empty;
			StatusList = new List<StatusRecord>();
			Guid guid = Guid.NewGuid();
			try
			{
				
				var testDataList = LocalService.TestDataList;
				if (!File.Exists(FilePath))
				{
					JudgeStatus = JudgeStatus.FileException;
				}
				else if (testDataList.Count == 0)
				{
					JudgeStatus = JudgeStatus.TestException;
				}
				else
				{
					await Task.Run(() =>
                    {
						string compilerPath = LocalService.FindConfigEntry("CppCompilerPath");
						string compileArgs = LocalService.FindConfigEntry("CppCompileArguments");
						if (compilerPath == null)
						{
							JudgeStatus = JudgeStatus.CompileException;
						}
						JudgeStatus = JudgeStatus.Compiling;
						RaisePropertyChanged(nameof(StatusMessage));
						using (Process compileProcess = new Process())
						{
							compileProcess.StartInfo.FileName = compilerPath;
							compileProcess.StartInfo.Arguments = LocalService.ParseString(
								compileArgs,
								new Tuple<string, string>("SOURCE", FilePath),
								new Tuple<string, string>("OUTPUT", $@"{LocalService.TempFolderPath}\{guid}.exe")
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
									StatusDetail += compileProcess.StandardOutput.ReadToEnd();
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
					});
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
                        await Task.Run(() =>
                        {
                            foreach (var testData in testDataList)
                            {
                                StatusRecord record = new StatusRecord();
								using (Process targetProcess = new Process())
                                {
                                    try
                                    {
                                        targetProcess.StartInfo.FileName = $@"{LocalService.TempFolderPath}\{guid}.exe";
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
											//targetProcess.Refresh();
											double currentMemorySize = targetProcess.PeakPagedMemorySize64 / 1024.0;
                                            memorySize = currentMemorySize > memorySize ? currentMemorySize : memorySize;
                                            if (targetProcess.HasExited)
                                            {
                                                break;
                                            }
                                        }
										record.timeConsumption = (DateTime.Now - startDT).TotalMilliseconds;
										record.memoryConsumption = memorySize;
										if (targetProcess.HasExited)
                                        {
                                            if (targetProcess.ExitCode != 0)
                                            {
                                                record.status = JudgeStatus.RuntimeError;
                                            }
                                            else if (memorySize > Convert.ToDouble(LocalService.FindConfigEntry("MemoryLimit")))
                                            {
												record.status = JudgeStatus.MemoryLimitExceeded;
                                            }
                                            else
                                            {
                                                using (StreamWriter targetOutput = new StreamWriter($@"{LocalService.TempFolderPath}\{guid}-out.txt"), standardOutput = new StreamWriter($@"{LocalService.TempFolderPath}\{guid}-stdout.txt"))
                                                {
                                                    targetOutput.WriteLine(targetProcess.StandardOutput.ReadToEnd());
                                                    targetOutput.Write("\0");
                                                    standardOutput.WriteLine(testData.Item2);
                                                    standardOutput.Write("\0");
                                                    targetOutput.Flush();
                                                    standardOutput.Flush();
                                                }
												using (Process compareProcess = new Process())
                                                {
													compareProcess.StartInfo.FileName = @".\FileComparator.exe";
													compareProcess.StartInfo.UseShellExecute = false;
													compareProcess.StartInfo.CreateNoWindow = true;
													compareProcess.StartInfo.RedirectStandardError = true;
													compareProcess.StartInfo.RedirectStandardOutput = true;
													compareProcess.StartInfo.RedirectStandardInput = true;
													compareProcess.StartInfo.Arguments = $@"""{LocalService.TempFolderPath}\{guid}-out.txt"" ""{LocalService.TempFolderPath}\{guid}-stdout.txt""";
													compareProcess.Start();
													compareProcess.WaitForExit();
													record.status = compareProcess.ExitCode == 0 ? JudgeStatus.Accepted : JudgeStatus.WrongAnswer;
												}
												//record.status = JudgeStatus.Accepted;
                                            }
                                        }
                                        else
                                        {
											record.status = JudgeStatus.TimeLimitExceeded;
                                        }
										StatusList.Add(record);
                                    }
                                    catch
                                    {
                                        JudgeStatus = JudgeStatus.InternalException;
                                        break;
                                    }
                                    finally
                                    {
                                        if (!targetProcess.HasExited)
                                            targetProcess.Kill();
                                    }
                                }
                            }
                        });
                        if (JudgeStatus == JudgeStatus.Running)
                        {
                            foreach (var record in StatusList)
                            {
								if (record.status != JudgeStatus.Accepted)
                                {
									if (JudgeStatus != JudgeStatus.Running && JudgeStatus != record.status)
                                    {
										JudgeStatus = JudgeStatus.MultipleError;
										break;
                                    }
									else
                                    {
										JudgeStatus = record.status;
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
            }
            catch
            {
                JudgeStatus = JudgeStatus.InternalException;
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
