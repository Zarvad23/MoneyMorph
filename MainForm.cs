using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MoneyMorph
{
    public class MainForm : Form
    {
        private readonly CurrencyConverter _converter;
        private readonly Timer _autoUpdateTimer;

        private ComboBox _fromBox = null!;
        private ComboBox _toBox = null!;
        private TextBox _amountBox = null!;
        private NumericUpDown _decimalsBox = null!;
        private Label _answerLabel = null!;
        private DataGridView _ratesGrid = null!;
        private Label _connectionLabel = null!;
        private Label _lastUpdateLabel = null!;
        private Button _updateRatesButton = null!;
        private ListBox _historyList = null!;

        private bool _isUpdatingRates;
        private bool _connectionOnline;
        private bool _isDarkMode;

        public MainForm()
        {
            _converter = new CurrencyConverter();
            _autoUpdateTimer = new Timer
            {
                Interval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds
            };

            BuildLayout();
            ApplyTheme();
            LoadCurrencies();
            RefreshRatesTable();

            Load += MainForm_Load;
            FormClosed += MainForm_FormClosed;
            _autoUpdateTimer.Tick += AutoUpdateTimer_Tick;
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Text = "MoneyMorph";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 540);

            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(18)
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(rootLayout);

            var leftColumn = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            leftColumn.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rootLayout.Controls.Add(leftColumn, 0, 0);

            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 0, 12, 0)
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));
            leftColumn.Controls.Add(inputLayout, 0, 0);

            var helpLabel = new Label
            {
                Text = "Введите сумму и выберите направление обмена",
                AutoSize = true
            };
            inputLayout.Controls.Add(helpLabel, 0, 0);
            inputLayout.SetColumnSpan(helpLabel, 2);

            var fromLabel = new Label
            {
                Text = "Исходная валюта:",
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 6)
            };
            inputLayout.Controls.Add(fromLabel, 0, 1);

            _fromBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputLayout.Controls.Add(_fromBox, 1, 1);

            var toLabel = new Label
            {
                Text = "Целевая валюта:",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };
            inputLayout.Controls.Add(toLabel, 0, 2);

            _toBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            inputLayout.Controls.Add(_toBox, 1, 2);

            var amountLabel = new Label
            {
                Text = "Сумма:",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };
            inputLayout.Controls.Add(amountLabel, 0, 3);

            _amountBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Например, 125,50"
            };
            inputLayout.Controls.Add(_amountBox, 1, 3);

            var decimalsLabel = new Label
            {
                Text = "Знаков после запятой:",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };
            inputLayout.Controls.Add(decimalsLabel, 0, 4);

            _decimalsBox = new NumericUpDown
            {
                Dock = DockStyle.Left,
                Minimum = 0,
                Maximum = 6,
                Value = 2,
                Width = 90
            };
            inputLayout.Controls.Add(_decimalsBox, 1, 4);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            inputLayout.Controls.Add(buttonPanel, 0, 5);
            inputLayout.SetColumnSpan(buttonPanel, 2);

            var convertButton = new Button
            {
                Text = "Посчитать",
                AutoSize = true,
                Padding = new Padding(14, 6, 14, 6)
            };
            convertButton.Click += ConvertButton_Click;
            buttonPanel.Controls.Add(convertButton);

            var swapButton = new Button
            {
                Text = "Поменять",
                AutoSize = true,
                Padding = new Padding(14, 6, 14, 6)
            };
            swapButton.Click += SwapButton_Click;
            buttonPanel.Controls.Add(swapButton);

            _updateRatesButton = new Button
            {
                Text = "Обновить курсы",
                AutoSize = true,
                Padding = new Padding(14, 6, 14, 6)
            };
            _updateRatesButton.Click += UpdateRatesButton_Click;
            buttonPanel.Controls.Add(_updateRatesButton);

            var themeButton = new Button
            {
                Text = "Смена темы",
                AutoSize = true,
                Padding = new Padding(14, 6, 14, 6)
            };
            themeButton.Click += ThemeButton_Click;
            buttonPanel.Controls.Add(themeButton);

            _connectionLabel = new Label
            {
                Text = "Связь: нет данных",
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };
            inputLayout.Controls.Add(_connectionLabel, 0, 6);
            inputLayout.SetColumnSpan(_connectionLabel, 2);

            _lastUpdateLabel = new Label
            {
                Text = "Последнее обновление: нет",
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 0)
            };
            inputLayout.Controls.Add(_lastUpdateLabel, 0, 7);
            inputLayout.SetColumnSpan(_lastUpdateLabel, 2);

            var historyGroup = new GroupBox
            {
                Text = "История операций",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 16, 12, 12)
            };
            leftColumn.Controls.Add(historyGroup, 0, 1);

            _historyList = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };
            historyGroup.Controls.Add(_historyList);

            var ratesGroup = new GroupBox
            {
                Text = "Курсы валют",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 16, 12, 12)
            };
            rootLayout.Controls.Add(ratesGroup, 1, 0);

            _ratesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None
            };
            _ratesGrid.Columns.Add("Code", "Код");
            _ratesGrid.Columns.Add("Usd", "Цена в USD");
            ratesGroup.Controls.Add(_ratesGrid);

            var resultPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            rootLayout.Controls.Add(resultPanel, 0, 1);
            rootLayout.SetColumnSpan(resultPanel, 2);

            _answerLabel = new Label
            {
                Text = "Результат появится после расчёта",
                AutoSize = true
            };
            resultPanel.Controls.Add(_answerLabel);

            ResumeLayout(true);
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(false);
            _autoUpdateTimer.Start();
        }

        private void LoadCurrencies()
        {
            string[] codes = _converter.GetCurrencyCodes();
            _fromBox.Items.AddRange(codes);
            _toBox.Items.AddRange(codes);

            if (_fromBox.Items.Count > 0)
            {
                _fromBox.SelectedIndex = 0;
            }

            if (_toBox.Items.Count > 1)
            {
                _toBox.SelectedIndex = 1;
            }
        }

        private void ConvertButton_Click(object? sender, EventArgs e)
        {
            if (_fromBox.SelectedItem is not string fromCode || _toBox.SelectedItem is not string toCode)
            {
                MessageBox.Show(this, "Выберите обе валюты для пересчёта.", "MoneyMorph", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.Equals(fromCode, toCode, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "Для обмена выберите разные валюты.", "MoneyMorph", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!decimal.TryParse(_amountBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount) || amount < 0m)
            {
                MessageBox.Show(this, "Введите корректную положительную сумму.", "MoneyMorph", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int decimals = (int)_decimalsBox.Value;
            decimal result = _converter.Convert(fromCode, toCode, amount, decimals);

            _answerLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0} {1} = {2} {3}",
                amount.ToString("N2", CultureInfo.CurrentCulture),
                fromCode,
                result.ToString("N" + decimals, CultureInfo.CurrentCulture),
                toCode);

            AddHistoryItem(fromCode, toCode, amount, decimals, result);
        }

        private void RefreshRatesTable()
        {
            _ratesGrid.Rows.Clear();
            foreach (CurrencyInfo info in _converter.GetAllCurrencies())
            {
                _ratesGrid.Rows.Add(info.Code, info.PriceInUsd.ToString("F4", CultureInfo.InvariantCulture));
            }
        }

        private void SwapButton_Click(object? sender, EventArgs e)
        {
            object? from = _fromBox.SelectedItem;
            _fromBox.SelectedItem = _toBox.SelectedItem;
            _toBox.SelectedItem = from;
        }

        private async void UpdateRatesButton_Click(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(true);
        }

        private async void AutoUpdateTimer_Tick(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(false);
        }

        private async Task TryUpdateRatesAsync(bool showMessages)
        {
            if (_isUpdatingRates)
            {
                return;
            }

            _isUpdatingRates = true;
            _updateRatesButton.Enabled = false;
            UseWaitCursor = true;

            try
            {
                bool success = await _converter.UpdateRatesFromInternetAsync();
                if (success)
                {
                    _connectionOnline = true;
                    RefreshRatesTable();
                    UpdateConnectionTexts();
                    if (showMessages)
                    {
                        _answerLabel.Text = "Курсы успешно обновлены.";
                    }
                }
                else
                {
                    _connectionOnline = false;
                    UpdateConnectionTexts();
                    if (showMessages)
                    {
                        MessageBox.Show(this, "Не удалось обновить курсы. Используются сохранённые значения.", "MoneyMorph", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            finally
            {
                UseWaitCursor = false;
                _updateRatesButton.Enabled = true;
                _isUpdatingRates = false;
            }
        }

        private void UpdateConnectionTexts()
        {
            if (_connectionOnline)
            {
                _connectionLabel.Text = string.Format(CultureInfo.CurrentCulture, "Связь: есть (обновлено {0:HH:mm:ss})", _converter.LastSuccessfulUpdate);
                _connectionLabel.ForeColor = _isDarkMode ? Color.LightGreen : Color.ForestGreen;
            }
            else
            {
                if (_converter.LastSuccessfulUpdate == default)
                {
                    _connectionLabel.Text = "Связь: нет (используются базовые курсы)";
                }
                else
                {
                    _connectionLabel.Text = string.Format(CultureInfo.CurrentCulture, "Связь: нет (последние данные {0:HH:mm:ss})", _converter.LastSuccessfulUpdate);
                }

                _connectionLabel.ForeColor = _isDarkMode ? Color.Salmon : Color.DarkRed;
            }

            if (_converter.LastSuccessfulUpdate == default)
            {
                _lastUpdateLabel.Text = "Последнее обновление: нет";
            }
            else
            {
                _lastUpdateLabel.Text = string.Format(CultureInfo.CurrentCulture, "Последнее обновление: {0:dd.MM.yyyy HH:mm:ss}", _converter.LastSuccessfulUpdate);
            }
        }

        private void ThemeButton_Click(object? sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            Color backColor = _isDarkMode ? Color.FromArgb(28, 30, 38) : Color.White;
            Color textColor = _isDarkMode ? Color.WhiteSmoke : Color.Black;

            BackColor = backColor;
            ForeColor = textColor;

            ApplyThemeRecursive(this, backColor, textColor);

            _historyList.BackColor = _isDarkMode ? Color.FromArgb(22, 24, 30) : Color.White;
            _historyList.ForeColor = textColor;

            _ratesGrid.BackgroundColor = backColor;
            _ratesGrid.DefaultCellStyle.BackColor = backColor;
            _ratesGrid.DefaultCellStyle.ForeColor = textColor;
            _ratesGrid.DefaultCellStyle.SelectionBackColor = _isDarkMode ? Color.FromArgb(70, 110, 160) : Color.FromArgb(210, 230, 255);
            _ratesGrid.DefaultCellStyle.SelectionForeColor = textColor;
            _ratesGrid.ColumnHeadersDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(40, 42, 52) : Color.FromArgb(240, 240, 240);
            _ratesGrid.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
            _ratesGrid.EnableHeadersVisualStyles = false;

            _answerLabel.ForeColor = _isDarkMode ? Color.PaleGreen : Color.DarkGreen;

            UpdateConnectionTexts();
        }

        private void ApplyThemeRecursive(Control parent, Color backColor, Color textColor)
        {
            foreach (Control child in parent.Controls)
            {
                switch (child)
                {
                    case Button button:
                        button.BackColor = _isDarkMode ? Color.FromArgb(62, 66, 84) : Color.FromArgb(230, 232, 240);
                        button.ForeColor = textColor;
                        break;
                    case GroupBox:
                    case Panel:
                        child.BackColor = backColor;
                        child.ForeColor = textColor;
                        break;
                    default:
                        child.BackColor = backColor;
                        child.ForeColor = textColor;
                        break;
                }

                if (child.HasChildren && child is not DataGridView)
                {
                    ApplyThemeRecursive(child, backColor, textColor);
                }
            }
        }

        private void AddHistoryItem(string fromCode, string toCode, decimal amount, int decimals, decimal result)
        {
            const int historyLimit = 15;
            string entry = string.Format(CultureInfo.CurrentCulture,
                "{0:HH:mm:ss} • {1} {2} → {3} {4}",
                DateTime.Now,
                amount.ToString("N2", CultureInfo.CurrentCulture),
                fromCode,
                result.ToString("N" + decimals, CultureInfo.CurrentCulture),
                toCode);

            _historyList.BeginUpdate();
            _historyList.Items.Insert(0, entry);
            while (_historyList.Items.Count > historyLimit)
            {
                _historyList.Items.RemoveAt(_historyList.Items.Count - 1);
            }

            _historyList.EndUpdate();
            _historyList.SelectedIndex = -1;
            UpdateInsights();
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _autoUpdateTimer.Stop();
        }

        private void UpdateInsights()
        {
            if (_historyList.Items.Count == 0)
            {
                Text = "MoneyMorph";
                return;
            }

            if (_historyList.Items[0] is not string latestEntry || string.IsNullOrWhiteSpace(latestEntry))
            {
                Text = "MoneyMorph";
                return;
            }

            int bulletIndex = latestEntry.IndexOf('•');
            string summary = bulletIndex >= 0 && bulletIndex + 1 < latestEntry.Length
                ? latestEntry[(bulletIndex + 1)..].Trim()
                : latestEntry.Trim();

            Text = string.IsNullOrEmpty(summary)
                ? "MoneyMorph"
                : $"MoneyMorph — {summary}";
        }
    }
}
