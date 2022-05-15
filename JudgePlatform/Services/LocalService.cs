using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;

namespace JudgePlatform.Services
{
	internal static class LocalService
	{
		public static void InitializeLocalService()
		{
			TestDataFolder = null;
			CheckAndInitializeUserFolder();
		}

		public static void DeleteTempFolder()
		{
			if (Directory.Exists($@"{UserDataPath}\temp"))
			{
				Directory.Delete($@"{UserDataPath}\temp", true);
			}
		}

		public static string StandardOutputFilePath => $@"{UserDataPath}\temp\stdout.txt";

		public static string TargetOutputFilePath => $@"{UserDataPath}\temp\out.txt";

		public static string CompiledExecutableFilePath => $@"{UserDataPath}\temp\program.exe";

		public static string UserDataPath => $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\JudgePlatform";

		public static string UserConfigPath => $@"{UserDataPath}\Config.xml";

		public static void CheckAndInitializeUserFolder()
		{
			if (!Directory.Exists(UserDataPath))
			{
				Directory.CreateDirectory(UserDataPath);
			}
			if (!File.Exists(UserConfigPath))
			{
				File.Copy(@".\DefaultConfig.xml", UserConfigPath);
			}
			Directory.CreateDirectory($@"{UserDataPath}\temp");
		}

		public static string FindConfigEntry(string key)
		{
			CheckAndInitializeUserFolder();
			XmlDocument configXml = new XmlDocument();
			configXml.Load(UserConfigPath);
			foreach (XmlNode xmlNode in configXml.ChildNodes[0].ChildNodes)
			{
				if (xmlNode.Name == key)
				{
					return xmlNode.InnerText;
				}
			}
			return null;
		}

		public static string TestDataFolder { get; set; }

		public static List<Tuple<string, string>> TestDataList
		{
			get
			{
				var result = new List<Tuple<string, string>>();
				if (!Directory.Exists(TestDataFolder))
				{
					return result;
				}
				DirectoryInfo directoryInfo = new DirectoryInfo(TestDataFolder);
				List<FileInfo> inputFiles = new List<FileInfo>(), outputFiles = new List<FileInfo>();
				foreach(FileInfo fileInfo in directoryInfo.GetFiles())
				{
					if (Regex.IsMatch(fileInfo.Name, @"\.in$"))
					{
						inputFiles.Add(fileInfo);
					}
					if (Regex.IsMatch(fileInfo.Name, @"\.out$"))
					{
						outputFiles.Add(fileInfo);
					}
				}
				foreach (FileInfo inputFileInfo in inputFiles)
				{
					string name = inputFileInfo.Name.Replace(".in", ".out");
					foreach (FileInfo outputFileInfo in outputFiles)
					{
						if (outputFileInfo.Name == name)
						{
							using (StreamReader input = new StreamReader(inputFileInfo.FullName), output = new StreamReader(outputFileInfo.FullName))
							{
								result.Add(new Tuple<string, string>(input.ReadToEnd(), output.ReadToEnd()));
							}
							outputFiles.Remove(outputFileInfo);
							break;
						}
					}
				}
				return result;
			}
		}

		public static string ParseString(string info, params Tuple<string, string>[] attributes)
		{
			using (Process process = new Process())
			{
				// 进程启动信息。
				process.StartInfo.FileName = @".\StringParse.exe";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardInput = true;
				// 运行并获取结果。
				process.Start();
				process.StandardInput.WriteLine(info);
				process.StandardInput.WriteLine(attributes.Length);
				foreach (var attribute in attributes)
				{
					process.StandardInput.WriteLine($"<{attribute.Item1}> \"{attribute.Item2}\"");
				}
				process.StandardInput.Flush();
				process.WaitForExit();
				switch (process.ExitCode)
				{
					case 0:
						return process.StandardOutput.ReadLine();

					case 1:
						throw new FormatException(process.StandardError.ReadToEnd());

					default:
						throw new Exception();
				}
			}
		}

		public static void DoEvents()
		{
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback(delegate (object f)
				{
					((DispatcherFrame)f).Continue = false;
					return null;
				}
			), frame);
			Dispatcher.PushFrame(frame);
		}
	}
}
