using System;
using System.Collections.Generic;
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
using JudgePlatform.Models;
using JudgePlatform.ViewModels;
using JudgePlatform.Services;

namespace JudgePlatform.Views
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.DataContext = viewModel;
			InitializeComponent();
		}

		private MainWindowViewModel viewModel = new MainWindowViewModel();

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			LocalService.DeleteTempFolder();
		}

        private void License_Click(object sender, RoutedEventArgs e)
        {
			string license = @"
MIT License

Copyright (c) 2022 夏锦熠(Xia Jinyi), 刘苏锐(Liu Surui), 黄凯博(Huang Kaibo), 刘航宇(Liu Hangyu), 李旭桓(Li Xuhuan) & 周琦翔(Zhou Qixiang)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
";
			MessageBox.Show(license, "开源许可");
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
			foreach(CppSourceCode item in this.dataGrid.SelectedItems)
            {
				viewModel.Codes.Remove(item);
            }
			viewModel.Codes.RaiseCollectionChanged();
			viewModel.RaisePropertyChanged(nameof(viewModel.Codes));
        }

        private async void TestSelected_Click(object sender, RoutedEventArgs e)
        {
			viewModel.IsButtonEnabled = false;
			viewModel.Status = "评测选中代码中";
			foreach (CppSourceCode item in this.dataGrid.SelectedItems)
			{
				await item.Test();
			}
			viewModel.IsButtonEnabled = true;
			viewModel.Status = "就绪";
		}

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
			viewModel.Status = "编辑配置选项中";
			var window = new ConfigWindow();
			window.WindowState = WindowState.Normal;
			//window.ShowInTaskbar = false;
			window.ShowDialog();
			viewModel.Status = "就绪";
		}
    }
}
