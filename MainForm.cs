using System;
using System.Drawing;
using System.Windows.Forms;

namespace MoneyMorph
{
    // Эта форма отвечает за отображение простого графического интерфейса
    public class MainForm : Form
    {
        private readonly CurrencyConverter _converter;
        private ComboBox _fromCombo;
        private ComboBox _toCombo;
        private TextBox _amountBox;
        private Label _resultLabel;

        // Этот конструктор настраивает элементы и подготавливает данные
        public MainForm()
        {
            _converter = new CurrencyConverter();
            InitializeComponents();
            FillCurrencyBoxes();
        }

        // Эта функция создаёт и размещает элементы формы
        private void InitializeComponents()
        {
            Text = "MoneyMorph - Учебный конвертер валют";
            Width = 420;
            Height = 320;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label titleLabel = new Label
            {
                Text = "Простой конвертер валют",
                Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(80, 20)
            };
            Controls.Add(titleLabel);

            Label fromLabel = new Label
            {
                Text = "Из какой валюты",
                AutoSize = true,
                Location = new Point(40, 70)
            };
            Controls.Add(fromLabel);

            _fromCombo = new ComboBox
            {
                Location = new Point(200, 66),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160
            };
            Controls.Add(_fromCombo);

            Label toLabel = new Label
            {
                Text = "В какую валюту",
                AutoSize = true,
                Location = new Point(40, 110)
            };
            Controls.Add(toLabel);

            _toCombo = new ComboBox
            {
                Location = new Point(200, 106),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160
            };
            Controls.Add(_toCombo);

            Label amountLabel = new Label
            {
                Text = "Сумма",
                AutoSize = true,
                Location = new Point(40, 150)
            };
            Controls.Add(amountLabel);

            _amountBox = new TextBox
            {
                Location = new Point(200, 146),
                Width = 160
            };
            Controls.Add(_amountBox);

            Button convertButton = new Button
            {
                Text = "Конвертировать",
                Location = new Point(140, 190),
                Width = 140
            };
            convertButton.Click += OnConvertClick;
            Controls.Add(convertButton);

            _resultLabel = new Label
            {
                Text = "Результат появится здесь",
                AutoSize = true,
                Location = new Point(40, 240),
                ForeColor = Color.DarkBlue
            };
            Controls.Add(_resultLabel);
        }

        // Эта функция заполняет списки валют кодами из конвертера
        private void FillCurrencyBoxes()
        {
            var codes = _converter.GetSupportedCodes();
            foreach (var code in codes)
            {
                _fromCombo.Items.Add(code);
                _toCombo.Items.Add(code);
            }

            if (_fromCombo.Items.Count > 0)
            {
                _fromCombo.SelectedIndex = 0;
            }
            if (_toCombo.Items.Count > 1)
            {
                _toCombo.SelectedIndex = 1;
            }
        }

        // Эта функция выполняет конвертацию после нажатия кнопки
        private void OnConvertClick(object? sender, EventArgs e)
        {
            if (_fromCombo.SelectedItem is not string fromCode)
            {
                MessageBox.Show("Пожалуйста, выберите исходную валюту.", "Предупреждение");
                return;
            }

            if (_toCombo.SelectedItem is not string toCode)
            {
                MessageBox.Show("Пожалуйста, выберите целевую валюту.", "Предупреждение");
                return;
            }

            if (string.Equals(fromCode, toCode, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Выберите разные валюты для конвертации.", "Подсказка");
                return;
            }

            if (!decimal.TryParse(_amountBox.Text, out decimal amount) || amount < 0)
            {
                MessageBox.Show("Введите положительное число без лишних символов.", "Ошибка ввода");
                return;
            }

            decimal result = _converter.Convert(fromCode, toCode, amount);
            _resultLabel.Text = $"{amount:0.##} {fromCode} = {result:0.##} {toCode}";
        }
    }
}
