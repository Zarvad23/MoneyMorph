using System;
using System.Drawing;
using System.Windows.Forms;

namespace MoneyMorph
{
    // Эта форма показывает пользователю кнопки и поля для конвертации
    public class MainForm : Form
    {
        private readonly CurrencyConverter _converter;
        private ComboBox _fromBox = null!;
        private ComboBox _toBox = null!;
        private TextBox _amountBox = null!;
        private Label _answerLabel = null!;
        private NumericUpDown _decimalsBox = null!;
        private DataGridView _ratesGrid = null!;
        private bool _isDarkMode;

        // Этот конструктор создаёт форму и настраивает все элементы
        public MainForm()
        {
            _converter = new CurrencyConverter();
            BuildLayout();
            LoadCurrencies();
            RefreshRatesTable();
            ApplyTheme();
        }

        // Эта функция вручную добавляет на форму все элементы управления
        private void BuildLayout()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "MoneyMorph - учебный пример";
            Width = 820;
            Height = 460;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Panel inputPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(320, 320)
            };
            Controls.Add(inputPanel);

            Label helpLabel = new Label
            {
                Text = "Введите сумму и выберите направление обмена",
                AutoSize = true,
                Location = new Point(0, 0)
            };
            inputPanel.Controls.Add(helpLabel);

            Label fromLabel = new Label
            {
                Text = "Исходная валюта:",
                AutoSize = true,
                Location = new Point(0, 50)
            };
            inputPanel.Controls.Add(fromLabel);

            _fromBox = new ComboBox
            {
                Location = new Point(160, 46),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputPanel.Controls.Add(_fromBox);

            Label toLabel = new Label
            {
                Text = "Нужная валюта:",
                AutoSize = true,
                Location = new Point(0, 90)
            };
            inputPanel.Controls.Add(toLabel);

            _toBox = new ComboBox
            {
                Location = new Point(160, 86),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputPanel.Controls.Add(_toBox);

            Label amountLabel = new Label
            {
                Text = "Сумма:",
                AutoSize = true,
                Location = new Point(0, 130)
            };
            inputPanel.Controls.Add(amountLabel);

            _amountBox = new TextBox
            {
                Location = new Point(160, 126),
                Width = 180
            };
            inputPanel.Controls.Add(_amountBox);

            Label decimalsLabel = new Label
            {
                Text = "Округление (знаков):",
                AutoSize = true,
                Location = new Point(0, 170)
            };
            inputPanel.Controls.Add(decimalsLabel);

            _decimalsBox = new NumericUpDown
            {
                Location = new Point(160, 166),
                Width = 60,
                Minimum = 0,
                Maximum = 6,
                Value = 2
            };
            inputPanel.Controls.Add(_decimalsBox);

            Button convertButton = new Button
            {
                Text = "Посчитать",
                Location = new Point(0, 210),
                Width = 100
            };
            convertButton.Click += ConvertButton_Click;
            inputPanel.Controls.Add(convertButton);

            Button swapButton = new Button
            {
                Text = "Поменять",
                Location = new Point(120, 210),
                Width = 100
            };
            swapButton.Click += SwapButton_Click;
            inputPanel.Controls.Add(swapButton);

            Button updateRatesButton = new Button
            {
                Text = "Обновить курсы",
                Location = new Point(0, 250),
                Width = 120
            };
            updateRatesButton.Click += UpdateRatesButton_Click;
            inputPanel.Controls.Add(updateRatesButton);

            Button themeButton = new Button
            {
                Text = "Смена темы",
                Location = new Point(140, 250),
                Width = 120
            };
            themeButton.Click += ThemeButton_Click;
            inputPanel.Controls.Add(themeButton);

            _answerLabel = new Label
            {
                Text = "Результат появится ниже",
                AutoSize = true,
                Location = new Point(20, 360),
                ForeColor = Color.DarkGreen
            };
            Controls.Add(_answerLabel);

            GroupBox ratesGroup = new GroupBox
            {
                Text = "Текущие курсы",
                Location = new Point(360, 20),
                Size = new Size(420, 320)
            };
            Controls.Add(ratesGroup);

            _ratesGrid = new DataGridView
            {
                Location = new Point(15, 30),
                Width = 380,
                Height = 250,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _ratesGrid.Columns.Add("Code", "Код валюты");
            _ratesGrid.Columns.Add("Usd", "Цена за 1 единицу (USD)");
            ratesGroup.Controls.Add(_ratesGrid);
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

            int decimals = (int)_decimalsBox.Value;
            decimal result = _converter.Convert(fromCode, toCode, amount, decimals);
            string amountText = amount.ToString("F2");
            string resultText = result.ToString("F" + decimals);
            _answerLabel.Text = $"{amountText} {fromCode} = {resultText} {toCode}";
        }

        // Эта функция обновляет таблицу с курсами на экране
        private void RefreshRatesTable()
        {
            _ratesGrid.Rows.Clear();
            CurrencyInfo[] all = _converter.GetAllCurrencies();
            foreach (CurrencyInfo info in all)
            {
                _ratesGrid.Rows.Add(info.Code, info.PriceInUsd.ToString("F4"));
            }
        }

        // Эта функция меняет местами выбранные валюты, когда нажата кнопка "Поменять"
        private void SwapButton_Click(object? sender, EventArgs e)
        {
            object? temp = _fromBox.SelectedItem;
            _fromBox.SelectedItem = _toBox.SelectedItem;
            _toBox.SelectedItem = temp;
        }

        // Эта функция имитирует обновление курсов и сразу перерисовывает таблицу
        private void UpdateRatesButton_Click(object? sender, EventArgs e)
        {
            _converter.UpdateRatesRandomly();
            RefreshRatesTable();
            _answerLabel.Text = "Курсы обновлены, можно пересчитать сумму";
        }

        // Эта функция по кнопке меняет оформление формы на светлое или тёмное
        private void ThemeButton_Click(object? sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme();
        }

        // Эта функция задаёт цвета элементов в зависимости от выбранной темы
        private void ApplyTheme()
        {
            Color backColor = _isDarkMode ? Color.FromArgb(34, 34, 34) : Color.White;
            Color textColor = _isDarkMode ? Color.WhiteSmoke : Color.Black;

            BackColor = backColor;
            ForeColor = textColor;

            ApplyThemeRecursive(this, backColor, textColor);

            _ratesGrid.BackgroundColor = backColor;
            _ratesGrid.DefaultCellStyle.BackColor = backColor;
            _ratesGrid.DefaultCellStyle.ForeColor = textColor;
            _ratesGrid.DefaultCellStyle.SelectionBackColor = _isDarkMode ? Color.FromArgb(85, 85, 85) : Color.LightBlue;
            _ratesGrid.DefaultCellStyle.SelectionForeColor = textColor;
            _ratesGrid.RowsDefaultCellStyle.BackColor = backColor;
            _ratesGrid.RowsDefaultCellStyle.ForeColor = textColor;
            _ratesGrid.ColumnHeadersDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(48, 48, 48) : Color.WhiteSmoke;
            _ratesGrid.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
            _ratesGrid.EnableHeadersVisualStyles = false;
            _ratesGrid.AlternatingRowsDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(45, 45, 45) : Color.FromArgb(245, 245, 245);

            _answerLabel.ForeColor = _isDarkMode ? Color.LightGreen : Color.DarkGreen;
        }

        // Эта служебная функция красит элементы управления и их потомков под выбранную тему
        private void ApplyThemeRecursive(Control parent, Color backColor, Color textColor)
        {
            foreach (Control control in parent.Controls)
            {
                control.ForeColor = textColor;

                switch (control)
                {
                    case Button button:
                        button.BackColor = _isDarkMode ? Color.FromArgb(64, 64, 64) : Color.LightGray;
                        button.ForeColor = textColor;
                        break;
                    case Panel or GroupBox:
                        control.BackColor = backColor;
                        break;
                    case DataGridView:
                        control.BackColor = backColor;
                        break;
                    default:
                        control.BackColor = backColor;
                        break;
                }

                if (control.HasChildren)
                {
                    ApplyThemeRecursive(control, backColor, textColor);
                }
            }
        }
    }
}
