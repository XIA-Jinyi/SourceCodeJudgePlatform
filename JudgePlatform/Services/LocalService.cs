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
using JudgePlatform.Models;

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
            try
            {
                if (Directory.Exists($@"{UserDataPath}\temp"))
                {
                    Directory.Delete($@"{UserDataPath}\temp", true);
                }
            }
            catch { }
        }

        public static XmlDocument DefaultConfig
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement rootElem = xmlDoc.CreateElement("UserConfig");
                xmlDoc.AppendChild(rootElem);
                Action<string, string> append = (name, value) =>
                {
                    var xmlElem = xmlDoc.CreateElement(name);
                    xmlElem.InnerText = value;
                    rootElem.AppendChild(xmlElem);
                };
                append("CppCompilerPath", $@"{Directory.GetCurrentDirectory()}\mingw64\bin\g++.exe");
                append("CppCompileArguments", @"<SOURCE> -o <OUTPUT>");
                append("TimeLimit", "1000");
                append("MemoryLimit", "65535");
                return xmlDoc;
            }
        }

        public static string TempFolderPath => $@"{UserDataPath}\temp";

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
                //File.Copy(@".\DefaultConfig.xml", UserConfigPath);
                DefaultConfig.Save(UserConfigPath);
            }
            Directory.CreateDirectory($@"{UserDataPath}\temp");
        }

        public static string FindConfigEntry(string key)
        {
            CheckAndInitializeUserFolder();
            XmlDocument configXml = new XmlDocument();
            try
            {
                configXml.Load(UserConfigPath);
            }
            catch
            {
                configXml = DefaultConfig;
            }
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
                process.StartInfo.FileName = $@"{Directory.GetCurrentDirectory()}\StringParse.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
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

        public static string TranslateStatus(JudgeStatus status)
        {
            switch (status)
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
}
