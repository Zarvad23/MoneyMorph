using System;
using System.Drawing;
using System.Windows.Forms;

namespace MoneyMorph
{
    // Эта форма показывает пользователю кнопки и поля для конвертации
    public class MainForm : Form
    {
        private readonly CurrencyConverter _converter;
        private ComboBox _fromBox;
        private ComboBox _toBox;
        private TextBox _amountBox;
        private Label _answerLabel;

        // Этот конструктор создаёт форму и настраивает все элементы
        public MainForm()
        {
            _converter = new CurrencyConverter();
            BuildLayout();
            LoadCurrencies();
        }

        // Эта функция вручную добавляет на форму все элементы управления
        private void BuildLayout()
        {
            Text = "MoneyMorph - учебный пример";
            Width = 440;
            Height = 320;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label helpLabel = new Label
            {
                Text = "Введите сумму и выберите направление обмена",
                AutoSize = true,
                Location = new Point(40, 20)
            };
            Controls.Add(helpLabel);

            Label fromLabel = new Label
            {
                Text = "Исходная валюта:",
                AutoSize = true,
                Location = new Point(40, 70)
            };
            Controls.Add(fromLabel);

            _fromBox = new ComboBox
            {
                Location = new Point(200, 66),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_fromBox);

            Label toLabel = new Label
            {
                Text = "Нужная валюта:",
                AutoSize = true,
                Location = new Point(40, 110)
            };
            Controls.Add(toLabel);

            _toBox = new ComboBox
            {
                Location = new Point(200, 106),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_toBox);

            Label amountLabel = new Label
            {
                Text = "Сумма:",
                AutoSize = true,
                Location = new Point(40, 150)
            };
            Controls.Add(amountLabel);

            _amountBox = new TextBox
            {
                Location = new Point(200, 146),
                Width = 180
            };
            Controls.Add(_amountBox);

            Button convertButton = new Button
            {
                Text = "Посчитать",
                Location = new Point(160, 190),
                Width = 100
            };
            convertButton.Click += ConvertButton_Click;
            Controls.Add(convertButton);

            _answerLabel = new Label
            {
                Text = "Результат появится ниже",
                AutoSize = true,
                Location = new Point(40, 240),
                ForeColor = Color.DarkGreen
            };
            Controls.Add(_answerLabel);
        }

        // Эта функция наполняет выпадающие списки названиями валют
        private void LoadCurrencies()
        {
            string[] codes = _converter.GetCurrencyCodes();
            foreach (string code in codes)
            {
                _fromBox.Items.Add(code);
                _toBox.Items.Add(code);
            }

            if (_fromBox.Items.Count > 0)
            {
                _fromBox.SelectedIndex = 0;
            }

            if (_toBox.Items.Count > 1)
            {
                _toBox.SelectedIndex = 1;
            }
        }

        // Эта функция запускается, когда пользователь нажимает на кнопку подсчёта
        private void ConvertButton_Click(object? sender, EventArgs e)
        {
            if (_fromBox.SelectedItem is not string fromCode)
            {
                MessageBox.Show("Пожалуйста, выберите валюту, с которой начинается обмен.");
                return;
            }

            if (_toBox.SelectedItem is not string toCode)
            {
                MessageBox.Show("Пожалуйста, выберите валюту, которую хотите получить.");
                return;
            }

            if (string.Equals(fromCode, toCode, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Для обмена выберите две разные валюты.");
                return;
            }

            if (!decimal.TryParse(_amountBox.Text, out decimal amount) || amount < 0)
            {
                MessageBox.Show("Введите положительное число без лишних символов.");
                return;
            }

            decimal result = _converter.Convert(fromCode, toCode, amount);
            _answerLabel.Text = $"{amount:0.##} {fromCode} = {result:0.##} {toCode}";
        }
    }
}
