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
	}
}
