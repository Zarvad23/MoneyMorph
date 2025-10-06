using System;
using System.Windows.Forms;

namespace MoneyMorph
{
    internal static class Program
    {
        // Эта функция запускает главное окно приложения
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
