using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Homebrew;
using OlxPhoneParser.Component;

namespace OlxPhoneParser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Default settings
        public MainWindow()
        {
            InitializeComponent();

        }

        private void ProgramWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ExitProgram_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void TelegramButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://t.me/iKolesDev");
        }

        private void Developer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TelegramButton_MouseDown(sender, e);
        }
        #endregion

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!LinkField.Text.Equals("")&&!BestProxies.Text.Equals(""))
            {
                Proxies.bestProxyKey = BestProxies.Text;
                Olx olx = new Olx(LinkField.Text, DebugBox);
                Thread thread = new Thread(() =>
                {
                    olx.StartParsing();
                });
                thread.IsBackground = true;
                thread.Start();
            }
            if (BestProxies.Text.Equals(""))
            {
                DebugBox.WriteLine("Вы не ввели прокси-ключ!");
            }
        }
    }
}
