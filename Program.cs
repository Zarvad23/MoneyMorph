using System;
using System.Windows.Forms;

namespace MoneyMorph
{
    internal static class Program
    {
        // Эта функция служит точкой входа и открывает главное окно
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
