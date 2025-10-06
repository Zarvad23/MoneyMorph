using System;
using System.Drawing;
using System.Threading.Tasks;
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
        private Label _connectionLabel = null!;
        private Label _lastUpdateLabel = null!;
        private Button _updateRatesButton = null!;
        private System.Windows.Forms.Timer _autoUpdateTimer = null!;
        private bool _isUpdatingRates;
        private bool _connectionOnline;
        private bool _isDarkMode;

        // Этот конструктор создаёт форму и настраивает все элементы
        public MainForm()
        {
            _converter = new CurrencyConverter();
            BuildLayout();
            LoadCurrencies();
            RefreshRatesTable();
            ApplyTheme();
            Load += MainForm_Load;
        }

        // Эта функция вручную добавляет на форму все элементы управления
        private void BuildLayout()
        {
            Text = "MoneyMorph - учебный пример";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(820, 480);

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(mainLayout);

            TableLayoutPanel inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 10, 0)
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (int i = 0; i < 8; i++)
            {
                inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            mainLayout.Controls.Add(inputLayout, 0, 0);

            Label helpLabel = new Label
            {
                Text = "Введите сумму и выберите направление обмена",
                AutoSize = true
            };
            inputLayout.Controls.Add(helpLabel, 0, 0);
            inputLayout.SetColumnSpan(helpLabel, 2);

            Label fromLabel = new Label
            {
                Text = "Исходная валюта:",
                AutoSize = true
            };
            inputLayout.Controls.Add(fromLabel, 0, 1);

            _fromBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputLayout.Controls.Add(_fromBox, 1, 1);

            Label toLabel = new Label
            {
                Text = "Нужная валюта:",
                AutoSize = true
            };
            inputLayout.Controls.Add(toLabel, 0, 2);

            _toBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputLayout.Controls.Add(_toBox, 1, 2);

            Label amountLabel = new Label
            {
                Text = "Сумма:",
                AutoSize = true
            };
            inputLayout.Controls.Add(amountLabel, 0, 3);

            _amountBox = new TextBox
            {
                Dock = DockStyle.Fill
            };
            inputLayout.Controls.Add(_amountBox, 1, 3);

            Label decimalsLabel = new Label
            {
                Text = "Округление (знаков):",
                AutoSize = true
            };
            inputLayout.Controls.Add(decimalsLabel, 0, 4);

            _decimalsBox = new NumericUpDown
            {
                Dock = DockStyle.Left,
                Minimum = 0,
                Maximum = 6,
                Value = 2,
                Width = 80
            };
            inputLayout.Controls.Add(_decimalsBox, 1, 4);

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };
            inputLayout.Controls.Add(buttonPanel, 0, 5);
            inputLayout.SetColumnSpan(buttonPanel, 2);

            Button convertButton = new Button
            {
                Text = "Посчитать",
                AutoSize = true
            };
            convertButton.Click += ConvertButton_Click;
            buttonPanel.Controls.Add(convertButton);

            Button swapButton = new Button
            {
                Text = "Поменять",
                AutoSize = true
            };
            swapButton.Click += SwapButton_Click;
            buttonPanel.Controls.Add(swapButton);

            _updateRatesButton = new Button
            {
                Text = "Обновить курсы",
                AutoSize = true
            };
            _updateRatesButton.Click += UpdateRatesButton_Click;
            buttonPanel.Controls.Add(_updateRatesButton);

            Button themeButton = new Button
            {
                Text = "Смена темы",
                AutoSize = true
            };
            themeButton.Click += ThemeButton_Click;
            buttonPanel.Controls.Add(themeButton);

            _connectionLabel = new Label
            {
                Text = "Связь: нет данных",
                AutoSize = true
            };
            inputLayout.Controls.Add(_connectionLabel, 0, 6);
            inputLayout.SetColumnSpan(_connectionLabel, 2);

            _lastUpdateLabel = new Label
            {
                Text = "Последнее обновление: нет",
                AutoSize = true
            };
            inputLayout.Controls.Add(_lastUpdateLabel, 0, 7);
            inputLayout.SetColumnSpan(_lastUpdateLabel, 2);

            GroupBox ratesGroup = new GroupBox
            {
                Text = "Текущие курсы",
                Dock = DockStyle.Fill
            };
            mainLayout.Controls.Add(ratesGroup, 1, 0);

            _ratesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
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

            _answerLabel = new Label
            {
                Text = "Результат появится ниже",
                AutoSize = true,
                ForeColor = Color.DarkGreen,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            mainLayout.Controls.Add(_answerLabel, 0, 1);
            mainLayout.SetColumnSpan(_answerLabel, 2);

            // Этот таймер автоматически запрашивает курсы с заданным интервалом
            _autoUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 4000
            };
            _autoUpdateTimer.Tick += AutoUpdateTimer_Tick;
        }

        // Эта функция запускается при загрузке формы и сразу пытается обновить данные из интернета
        private async void MainForm_Load(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(false);
            _autoUpdateTimer.Start();
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

        // Эта функция вручную запрашивает свежие курсы и показывает сообщение пользователю
        private async void UpdateRatesButton_Click(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(true);
        }

        // Эта функция запускает автоматическое обновление по таймеру
        private async void AutoUpdateTimer_Tick(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(false);
        }

        // Эта функция аккуратно обращается к конвертеру и обновляет курсы из сети
        private async Task TryUpdateRatesAsync(bool showMessage)
        {
            if (_isUpdatingRates)
            {
                return;
            }

            _isUpdatingRates = true;
            _updateRatesButton.Enabled = false;

            try
            {
                bool success = await _converter.UpdateRatesFromInternetAsync();

                if (success)
                {
                    RefreshRatesTable();
                    _connectionOnline = true;
                    UpdateConnectionTexts();

                    if (showMessage)
                    {
                        _answerLabel.Text = "Курсы обновлены по данным сервера";
                    }
                }
                else
                {
                    _connectionOnline = false;
                    UpdateConnectionTexts();

                    if (showMessage)
                    {
                        MessageBox.Show("Не получилось получить данные. Показываются сохранённые курсы.");
                    }
                }
            }
            finally
            {
                _updateRatesButton.Enabled = true;
                _isUpdatingRates = false;
            }
        }

        // Эта функция обновляет подписи о связи и времени последнего ответа
        private void UpdateConnectionTexts()
        {
            if (_connectionOnline)
            {
                _connectionLabel.Text = $"Связь: есть (обновлено {_converter.LastSuccessfulUpdate:HH:mm:ss})";
                _connectionLabel.ForeColor = Color.ForestGreen;
            }
            else
            {
                if (_converter.LastSuccessfulUpdate == default)
                {
                    _connectionLabel.Text = "Связь: нет (показываются курсы по умолчанию)";
                }
                else
                {
                    _connectionLabel.Text = $"Связь: нет (последние данные {_converter.LastSuccessfulUpdate:HH:mm:ss})";
                }

                _connectionLabel.ForeColor = Color.DarkRed;
            }

            if (_converter.LastSuccessfulUpdate == default)
            {
                _lastUpdateLabel.Text = "Последнее обновление: нет";
            }
            else
            {
                _lastUpdateLabel.Text = $"Последнее обновление: {_converter.LastSuccessfulUpdate:dd.MM.yyyy HH:mm:ss}";
            }
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
            UpdateConnectionTexts();
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
