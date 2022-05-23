using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JudgePlatform.Services;
using System.Xml;

namespace JudgePlatform.Views
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window, INotifyPropertyChanged
    {
        public ConfigWindow()
        {
            DataContext = this;
            InitializeComponent();
            TimeLimit = (LocalService.FindConfigEntry("TimeLimit"));
            MemoryLimit = (LocalService.FindConfigEntry("MemoryLimit"));
            CompileArg = LocalService.FindConfigEntry("CppCompileArguments");
        }

        private string compileArg;

        public string CompileArg
        {
            get
            {
                return compileArg;
            }

            set
            {
                if (value.Contains("<SOURCE>") && value.Contains("<OUTPUT>"))
                {
                    compileArg = value;
                    RaisePropertyChanged(nameof(CompileArg));
                    TextBox1.Foreground = Brushes.Black;
                }
                else
                {
                    TextBox1.Foreground = Brushes.Red;
                }
            }
        }

        private short timeLimit;

        public string TimeLimit
        {
            get
            {
                return $"{timeLimit}";
            }

            set
            {
                try
                {
                    timeLimit = Convert.ToInt16(value);
                    RaisePropertyChanged(nameof(TimeLimit));
                    TextBox2.Foreground = Brushes.Black;
                }
                catch
                {
                    TextBox2.Foreground = Brushes.Red;
                }
            }
        }

        private int memoryLimit;

        public string MemoryLimit
        {
            get
            {
                return $"{memoryLimit}";
            }

            set
            {
                try
                {
                    memoryLimit = Convert.ToInt32(value);
                    RaisePropertyChanged(nameof(MemoryLimit));
                    TextBox3.Foreground = Brushes.Black;
                }
                catch
                {
                    TextBox3.Foreground = Brushes.Red;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var defaultXml = LocalService.DefaultConfig;
            foreach (XmlNode xmlNode in defaultXml.ChildNodes[0].ChildNodes)
            {
                if (xmlNode.Name == "CppCompileArguments")
                {
                    CompileArg = xmlNode.InnerText;
                }
                else if (xmlNode.Name == "MemoryLimit")
                {
                    MemoryLimit = xmlNode.InnerText;
                }
                else if (xmlNode.Name == "TimeLimit")
                {
                    TimeLimit = xmlNode.InnerText;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var defaultXml = LocalService.DefaultConfig;
            foreach (XmlNode xmlNode in defaultXml.ChildNodes[0].ChildNodes)
            {
                if (xmlNode.Name == "CppCompileArguments")
                {
                    xmlNode.InnerText = CompileArg;
                }
                else if (xmlNode.Name == "MemoryLimit")
                {
                    xmlNode.InnerText = MemoryLimit;
                }
                else if (xmlNode.Name == "TimeLimit")
                {
                    xmlNode.InnerText = TimeLimit;
                }
            }
            defaultXml.Save(LocalService.UserConfigPath);
            this.Close();
        }
    }
}
