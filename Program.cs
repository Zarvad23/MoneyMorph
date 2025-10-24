using System; // позволяет использовать базовые классы .NET
using System.Windows.Forms; // позволяет создавать Windows Forms приложения

namespace MoneyMorph // Пространство имён для приложения MoneyMorph
{
    internal static class Program // Статический класс Program для управления запуском приложения
    {
        // Точка входа приложения
        [STAThread] // Указываем что приложекние использует однопоточную модель, для Windows Forms
        private static void Main() // Главный метод, с которого начинается выполнение программы
        {
            Application.EnableVisualStyles(); // Включает визуальные стили для приложения
            Application.SetCompatibleTextRenderingDefault(false); // Использует GDI+ для рендеринга текста, было бы true был бы просто GDI
            Application.Run(new MainForm()); // Запускает главную форму приложения MainForm
        }
    } 
}