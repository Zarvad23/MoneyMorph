using System; // позволяет использовать базовые классы .NET
using System.Drawing; // позволяет работать с графикой и цветами
using System.Drawing.Drawing2D; // позволяет рисовать сложные градиенты и эффекты
using System.Linq; // позволяет удобно работать с коллекциями
using System.Threading.Tasks; // позволяет использовать асинхронное программирование
using System.Windows.Forms; // позволяет создавать Windows Forms приложения

namespace MoneyMorph
{
    // Главная форма приложения для конвертации валют
    public class MainForm : Form
    {
        private readonly CurrencyConverter _converter; // Конвертер валют
        private ComboBox _fromBox = null!; // Выпадающий список исходной валюты null! используется чтобы подавить предупреждение о возможном null
        private ComboBox _toBox = null!; // Выпадающий список целевой валюты
        private TextBox _amountBox = null!; // Поле ввода суммы
        private Label _answerLabel = null!; // Метка для отображения результата конвертации
        private NumericUpDown _decimalsBox = null!; // Счётчик для выбора количества знаков после запятой
        private DataGridView _ratesGrid = null!; // Таблица с курсами валют
        private Label _connectionLabel = null!; // Индикатор состояния подключения
        private Label _lastUpdateLabel = null!; // Метка времени последнего обновления
        private Button _updateRatesButton = null!; // Кнопка обновления курсов
        private System.Windows.Forms.Timer _autoUpdateTimer = null!; // Таймер автоматического обновления
        private bool _isUpdatingRates; // Флаг процесса обновления курсов
        private bool _connectionOnline; // Флаг наличия подключения
        private bool _isDarkMode; // Флаг тёмной темы оформления
        private NeonPanel _headerPanel = null!; // Неоновая панель с заголовком
        private ListBox _historyList = null!; // История конверсий
        private ListView _insightsView = null!; // Таблица с инсайтами по валютам
        private ListView _quickBurstView = null!; // Таблица с быстрыми конверсиями
        private FlowLayoutPanel _vibePanel = null!; // Панель с динамичными подсказками
        private Label _vibeLabel = null!; // Метка с мотивирующим текстом
        private System.Windows.Forms.Timer _neonTimer = null!; // Таймер анимации неоновой панели
        private readonly Random _random = new Random(); // Генератор случайностей для подсказок
        private readonly string[] _vibeLibrary = // Набор вдохновляющих сообщений
        {
            "Зарядите кошелёк новыми курсами!",
            "Каждый клик — как удар по басу валютного трека.",
            "Команда MoneyMorph держит космический ритм рынка.",
            "Хватайте лучший курс, пока он сияет!",
            "Играем на опережение — валютный диджей доволен.",
            "Чем больше конверсий, тем ярче неон на панели.",
            "Включаем режим сверхскорости: курс уже готов.",
            "Пусть числа танцуют — вы управляете битом."
        };

        // Инициализирует форму и все её компоненты
        public MainForm() // Конструктор главной формы
        {
            _converter = new CurrencyConverter(); // Создаёт экземпляр конвертера
            BuildLayout(); // Создаёт пользовательский интерфейс
            LoadCurrencies(); // Загружает список валют
            RefreshRatesTable(); // Заполняет таблицу курсов
            ApplyTheme(); // Применяет тему оформления
            UpdateVibeStatus("🎚 Добро пожаловать в MoneyMorph Neon Lab!"); // Устанавливает стартовый слоган
            Load += MainForm_Load; // Подписывается на событие загрузки формы
            FormClosed += MainForm_FormClosed; // Останавливает таймеры при закрытии окна
        }

        // Создаёт и настраивает все элементы управления на форме


        private void BuildLayout()
        {
            Text = "MoneyMorph Neon Lab"; // Новый яркий заголовок окна
            StartPosition = FormStartPosition.CenterScreen; // Центрирует окно на экране
            MinimumSize = new Size(940, 580); // Увеличивает минимальный размер для комфортного размещения элементов

            TableLayoutPanel rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всю форму
                ColumnCount = 1, // Одна колонка
                RowCount = 3, // Три строки: шапка, содержимое и нижняя панель
                Padding = new Padding(0) // Без дополнительных внешних отступов
            };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Высота шапки подстраивается под содержимое
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Основное содержимое занимает всё оставшееся место
            rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Нижняя панель авторазмера
            Controls.Add(rootLayout); // Добавляет контейнер на форму

            // Создаёт сияющую неоновую панель с заголовком
            _headerPanel = new NeonPanel
            {
                Dock = DockStyle.Fill, // Заполняет всю ширину
                Padding = new Padding(28, 20, 28, 20), // Добавляет внутренние отступы
                Margin = new Padding(0) // Без внешних отступов
            };

            TableLayoutPanel headerContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет панель
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            headerContent.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerContent.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerContent.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titleLabel = new Label
            {
                Text = "MoneyMorph Neon Lab", // Основной заголовок
                AutoSize = true,
                Font = new Font("Segoe UI", 22f, FontStyle.Bold), // Использует современный шрифт
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            headerContent.Controls.Add(titleLabel, 0, 0);

            Label subtitleLabel = new Label
            {
                Text = "Конвертер, который играет по правилам неона", // Подзаголовок
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0)
            };
            headerContent.Controls.Add(subtitleLabel, 0, 1);

            FlowLayoutPanel badgePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 12, 0, 0)
            };
            badgePanel.Controls.Add(CreateBadge("⚡ Живые курсы в пару кликов"));
            badgePanel.Controls.Add(CreateBadge("🎛 История обменов под вашим контролем"));
            badgePanel.Controls.Add(CreateBadge("🌌 Аналитика, которая светится идеями"));
            headerContent.Controls.Add(badgePanel, 0, 2);

            _headerPanel.Controls.Add(headerContent);
            rootLayout.Controls.Add(_headerPanel, 0, 0);

            // Основной блок с вводом данных и аналитикой
            TableLayoutPanel contentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(24, 22, 24, 12)
            };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47f));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53f));
            rootLayout.Controls.Add(contentLayout, 0, 1);

            // Левая колонка: ввод данных и история
            TableLayoutPanel leftColumn = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 0, 16, 0)
            };
            leftColumn.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            contentLayout.Controls.Add(leftColumn, 0, 0);

            TableLayoutPanel inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));
            for (int i = 0; i < 8; i++)
            {
                inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            leftColumn.Controls.Add(inputLayout, 0, 0);

            Label helpLabel = new Label
            {
                Text = "Введите сумму и настройте направление обмена", // Подсказка
                AutoSize = true
            };
            inputLayout.Controls.Add(helpLabel, 0, 0);
            inputLayout.SetColumnSpan(helpLabel, 2);

            Label fromLabel = new Label
            {
                Text = "Исходная валюта:",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
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

            Label amountLabel = new Label
            {
                Text = "Сумма:",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 6)
            };
            inputLayout.Controls.Add(amountLabel, 0, 3);

            _amountBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Например, 125.50" // Добавляет подсказку в поле
            };
            inputLayout.Controls.Add(_amountBox, 1, 3);

            Label decimalsLabel = new Label
            {
                Text = "Округление (знаков):",
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

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            inputLayout.Controls.Add(buttonPanel, 0, 5);
            inputLayout.SetColumnSpan(buttonPanel, 2);

            Button convertButton = new Button
            {
                Text = "Посчитать",
                AutoSize = true,
                Padding = new Padding(14, 6, 14, 6)
            };
            convertButton.Click += ConvertButton_Click;
            buttonPanel.Controls.Add(convertButton);

            Button swapButton = new Button
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

            Button themeButton = new Button
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

            GroupBox historyGroup = new GroupBox
            {
                Text = "История неоновых обменов",
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 16, 14, 14)
            };
            leftColumn.Controls.Add(historyGroup, 0, 1);

            _historyList = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 34
            };
            _historyList.DrawItem += HistoryList_DrawItem;
            historyGroup.Controls.Add(_historyList);

            // Правая колонка: таблица курсов и аналитика
            TableLayoutPanel rightColumn = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            rightColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
            rightColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
            contentLayout.Controls.Add(rightColumn, 1, 0);

            GroupBox ratesGroup = new GroupBox
            {
                Text = "Текущие курсы",
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 16, 14, 14)
            };
            rightColumn.Controls.Add(ratesGroup, 0, 0);

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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None
            };
            _ratesGrid.Columns.Add("Code", "Код валюты");
            _ratesGrid.Columns.Add("Usd", "Цена за 1 единицу (USD)");
            ratesGroup.Controls.Add(_ratesGrid);

            TableLayoutPanel analyticsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            analyticsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            rightColumn.Controls.Add(analyticsLayout, 0, 1);

            GroupBox insightsGroup = new GroupBox
            {
                Text = "Галактические инсайты",
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 16, 14, 14)
            };
            analyticsLayout.Controls.Add(insightsGroup, 0, 0);

            _insightsView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                ShowGroups = true
            };
            _insightsView.Columns.Add("Валюта", 120);
            _insightsView.Columns.Add("Цена (USD)", 110);
            _insightsView.Columns.Add("Энергия", 160);
            _insightsView.SizeChanged += (_, _) => AdjustListViewColumns(_insightsView, 0.36f, 0.26f, 0.38f);
            insightsGroup.Controls.Add(_insightsView);

            GroupBox quickGroup = new GroupBox
            {
                Text = "Неоновый экспресс-конверт",
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 16, 14, 14)
            };
            analyticsLayout.Controls.Add(quickGroup, 0, 1);

            _quickBurstView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                ShowGroups = true
            };
            _quickBurstView.Columns.Add("Валюта", 110);
            _quickBurstView.Columns.Add("Результат", 140);
            _quickBurstView.Columns.Add("Вибрации", 150);
            _quickBurstView.SizeChanged += (_, _) => AdjustListViewColumns(_quickBurstView, 0.32f, 0.38f, 0.30f);
            quickGroup.Controls.Add(_quickBurstView);

            // Нижняя панель с вибами и результатом
            _vibePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(28, 10, 28, 16)
            };
            rootLayout.Controls.Add(_vibePanel, 0, 2);

            _answerLabel = new Label
            {
                Text = "Результат появится после конвертации",
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };
            _vibePanel.Controls.Add(_answerLabel);

            _vibeLabel = new Label
            {
                Text = "Готовы зажечь валютный танцпол.",
                AutoSize = true,
                Margin = new Padding(18, 4, 0, 0),
                Font = new Font("Segoe UI", 10f, FontStyle.Italic)
            };
            _vibePanel.Controls.Add(_vibeLabel);

            AdjustListViewColumns(_insightsView, 0.36f, 0.26f, 0.38f); // Настраивает ширину колонок после создания
            AdjustListViewColumns(_quickBurstView, 0.32f, 0.38f, 0.30f); // Настраивает ширину колонок в таблице экспресса

            // Таймеры приложения
            _autoUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 4000
            };
            _autoUpdateTimer.Tick += AutoUpdateTimer_Tick;

            _neonTimer = new System.Windows.Forms.Timer
            {
                Interval = 120
            };
            _neonTimer.Tick += (_, _) => _headerPanel.AdvancePhase(0.018f);
            _neonTimer.Start();
        }
        // Обрабатывает событие загрузки формы
        private async void MainForm_Load(object? sender, EventArgs e) // sender это сама форма, e это аргументы события
        {
            await TryUpdateRatesAsync(false); // Выполняет первоначальное обновление курсов fslse означает без показа сообщения
            _autoUpdateTimer.Start(); // Запускает таймер автоматического обновления
        }

        // Заполняет выпадающие списки доступными валютами
        private void LoadCurrencies()
        {
            string[] codes = _converter.GetCurrencyCodes(); // Получает список кодов валют
            foreach (string code in codes) // Перебирает все коды
            {
                _fromBox.Items.Add(code); // Добавляет код в список исходной валюты
                _toBox.Items.Add(code); // Добавляет код в список целевой валюты
            }

            if (_fromBox.Items.Count > 0) // Проверяет наличие элементов в списке
            {
                _fromBox.SelectedIndex = 0; // Выбирает первый элемент
            }
            // Это сделано чтобы по умолчанию были выбраны разные валюты
            if (_toBox.Items.Count > 1) // Проверяет наличие хотя бы двух элементов
            {
                _toBox.SelectedIndex = 1; // Выбирает второй элемент
            }
        }

        // Обрабатывает нажатие кнопки конвертации
        private void ConvertButton_Click(object? sender, EventArgs e) // sender это кнопка, e это аргументы события
        {
            if (_fromBox.SelectedItem is not string fromCode) // Проверяет выбор исходной валюты
            {
                MessageBox.Show("Пожалуйста, выберите валюту, с которой начинается обмен."); // Выводит сообщение об ошибке
                return; // Прерывает выполнение метода
            }

            if (_toBox.SelectedItem is not string toCode) // Проверяет выбор целевой валюты
            {
                MessageBox.Show("Пожалуйста, выберите валюту, которую хотите получить."); // Выводит сообщение об ошибке
                return; // Прерывает выполнение метода
            }

            if (string.Equals(fromCode, toCode, StringComparison.OrdinalIgnoreCase)) // Проверяет совпадение валют
            {
                MessageBox.Show("Для обмена выберите две разные валюты."); // Выводит сообщение об ошибке
                return; // Прерывает выполнение метода
            }

            if (!decimal.TryParse(_amountBox.Text, out decimal amount) || amount < 0) // Проверяет корректность введённой суммы
            {
                MessageBox.Show("Введите положительное число без лишних символов."); // Выводит сообщение об ошибке
                return; // Прерывает выполнение метода
            }

            int decimals = (int)_decimalsBox.Value; // Получает количество знаков после запятой
            decimal result = _converter.Convert(fromCode, toCode, amount, decimals); // Выполняет конвертацию
            string amountText = amount.ToString("F2"); // Форматирует исходную сумму
            string resultText = result.ToString("F" + decimals); // Форматирует результат
            _answerLabel.Text = $"{amountText} {fromCode} = {resultText} {toCode}"; // Выводит результат в метку
            AddHistoryItem(fromCode, toCode, amount, decimals, result); // Добавляет запись в историю
            UpdateQuickBurst(fromCode, amount, decimals); // Показывает результаты экспресс-конвертаций
            UpdateVibeStatus($"⚡ {fromCode} → {toCode} — энергия пересчёта доставлена!"); // Обновляет вдохновляющий текст
        }

        // Обновляет таблицу с текущими курсами валют
        private void RefreshRatesTable()
        {
            _ratesGrid.Rows.Clear(); // Очищает все строки таблицы
            CurrencyInfo[] all = _converter.GetAllCurrencies(); // Получает массив всех валют именно копию чтобы избежать проблем с изменением данных
            foreach (CurrencyInfo info in all) // Перебирает все валюты
            {
                _ratesGrid.Rows.Add(info.Code, info.PriceInUsd.ToString("F4")); // Добавляет строку с кодом и курсом а F4 означает 4 знака после запятой
            }
            UpdateInsights(); // Обновляет аналитический блок после перерисовки таблицы
        }

        // Обрабатывает нажатие кнопки обмена валют местами
        private void SwapButton_Click(object? sender, EventArgs e)
        {
            object? temp = _fromBox.SelectedItem; // Сохраняет текущий выбор исходной валюты
            _fromBox.SelectedItem = _toBox.SelectedItem; // Присваивает исходной валюте значение целевой
            _toBox.SelectedItem = temp; // Присваивает целевой валюте сохранённое значение
        }

        // Обрабатывает нажатие кнопки ручного обновления курсов
        private async void UpdateRatesButton_Click(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(true); // Запускает обновление курсов с показом сообщения
        }

        // Обрабатывает событие таймера для автоматического обновления
        private async void AutoUpdateTimer_Tick(object? sender, EventArgs e)
        {
            await TryUpdateRatesAsync(false); // Запускает обновление курсов без показа сообщения
        }

        // Выполняет асинхронное обновление курсов валют
        private async Task TryUpdateRatesAsync(bool showMessage)
        {
            if (_isUpdatingRates) // Проверяет наличие активного процесса обновления
            {
                return;
            }

            _isUpdatingRates = true; // Устанавливает флаг активного обновления
            _updateRatesButton.Enabled = false; // Отключает кнопку обновления

            try
            {
                bool success = await _converter.UpdateRatesFromInternetAsync(); // Запускает асинхронное обновление курсов

                if (success) // Проверяет успешность обновления
                {
                    RefreshRatesTable(); // Обновляет таблицу с новыми курсами
                    _connectionOnline = true; // Устанавливает флаг наличия подключения
                    UpdateConnectionTexts(); // Обновляет текст индикаторов
                    if (_fromBox.SelectedItem is string quickFrom && decimal.TryParse(_amountBox.Text, out decimal currentAmount) && currentAmount >= 0) // Проверяет возможность обновить экспресс панель
                    {
                        int decimals = (int)_decimalsBox.Value; // Берёт текущее значение округления
                        UpdateQuickBurst(quickFrom, currentAmount, decimals); // Обновляет экспресс-конвертации с новыми курсами
                    }
                    UpdateVibeStatus("📡 Свежие курсы на палубе — проверяйте инсайты!"); // Сообщает об успешном обновлении

                    if (showMessage) // Проверяет необходимость показа сообщения
                    {
                        _answerLabel.Text = "Курсы обновлены по данным сервера"; // Выводит сообщение об успехе
                    }
                }
                else
                {
                    _connectionOnline = false; // Сбрасывает флаг наличия подключения
                    UpdateConnectionTexts(); // Обновляет текст индикаторов
                    UpdateVibeStatus("🚧 Не удалось связаться с сервером — играем по старым нотам."); // Показывает сообщение о проблеме

                    if (showMessage) // Проверяет необходимость показа сообщения
                    {
                        MessageBox.Show("Не получилось получить данные. Показываются сохранённые курсы."); // Выводит сообщение об ошибке
                    }
                }
            }
            finally
            {
                _updateRatesButton.Enabled = true; // Включает кнопку обновления
                _isUpdatingRates = false; // Сбрасывает флаг активного обновления
            }
        }

        // Обновляет текст индикаторов состояния подключения
        private void UpdateConnectionTexts()
        {
            if (_connectionOnline) // Проверяет наличие подключения
            {
                _connectionLabel.Text = $"Связь: есть (обновлено {_converter.LastSuccessfulUpdate:HH:mm:ss})"; // Устанавливает текст с временем обновления
                _connectionLabel.ForeColor = Color.ForestGreen; // Устанавливает зелёный цвет текста
            }
            else
            {
                if (_converter.LastSuccessfulUpdate == default) // Проверяет отсутствие обновлений, если их нет то обычно значение по умолчанию это 01.01.0001 00:00:00
                {
                    _connectionLabel.Text = "Связь: нет (показываются курсы по умолчанию)"; // Устанавливает текст для режима по умолчанию
                }
                else
                {
                    _connectionLabel.Text = $"Связь: нет (последние данные {_converter.LastSuccessfulUpdate:HH:mm:ss})"; // Устанавливает текст с временем последних данных
                }

                _connectionLabel.ForeColor = Color.DarkRed; // Устанавливает красный цвет текста
            }

            if (_converter.LastSuccessfulUpdate == default) // Проверяет отсутствие обновлений
            {
                _lastUpdateLabel.Text = "Последнее обновление: нет"; // Устанавливает текст при отсутствии обновлений
            }
            else
            {
                _lastUpdateLabel.Text = $"Последнее обновление: {_converter.LastSuccessfulUpdate:dd.MM.yyyy HH:mm:ss}"; // Устанавливает текст с полной датой и временем
            }
        }

        // Обрабатывает нажатие кнопки смены темы оформления
        private void ThemeButton_Click(object? sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode; // Инвертирует флаг тёмной темы
            ApplyTheme(); // Применяет новую тему
        }

        // Применяет выбранную тему оформления ко всем элементам
        private void ApplyTheme()
        {
            Color backColor = _isDarkMode ? Color.FromArgb(34, 34, 34) : Color.White; // Определяет цвет фона в зависимости от темы
            Color textColor = _isDarkMode ? Color.WhiteSmoke : Color.Black; // Определяет цвет текста в зависимости от темы

            BackColor = backColor; // Устанавливает цвет фона формы
            ForeColor = textColor; // Устанавливает цвет текста формы

            ApplyThemeRecursive(this, backColor, textColor); // Применяет тему рекурсивно ко всем дочерним элементам

            Color analyticsBack = _isDarkMode ? Color.FromArgb(32, 34, 48) : Color.FromArgb(244, 246, 255); // Цвет фоновых панелей
            Color historyBack = _isDarkMode ? Color.FromArgb(24, 25, 33) : Color.FromArgb(250, 250, 255); // Цвет списка истории
            _historyList.BackColor = historyBack; // Устанавливает фон истории
            _historyList.ForeColor = textColor; // Устанавливает цвет текста истории
            _historyList.Invalidate(); // Перерисовывает элементы для применения цвета

            StyleListView(_insightsView, analyticsBack, textColor); // Применяет цвета к таблице инсайтов
            StyleListView(_quickBurstView, analyticsBack, textColor); // Применяет цвета к таблице экспресс-конвертаций

            _vibePanel.BackColor = _isDarkMode ? Color.FromArgb(18, 20, 28) : Color.FromArgb(255, 255, 255); // Подкрашивает нижнюю панель
            _vibeLabel.ForeColor = _isDarkMode ? Color.FromArgb(218, 222, 240) : Color.FromArgb(64, 64, 78); // Настраивает цвет текста вибов

            // Настраивает цветовую схему таблицы курсов
            _ratesGrid.BackgroundColor = backColor; // Устанавливает цвет фона таблицы
            _ratesGrid.DefaultCellStyle.BackColor = backColor; // Устанавливает цвет фона ячеек
            _ratesGrid.DefaultCellStyle.ForeColor = textColor; // Устанавливает цвет текста ячеек
            _ratesGrid.DefaultCellStyle.SelectionBackColor = _isDarkMode ? Color.FromArgb(85, 85, 85) : Color.LightBlue; // Устанавливает цвет выделения
            _ratesGrid.DefaultCellStyle.SelectionForeColor = textColor; // Устанавливает цвет текста выделенной ячейки
            _ratesGrid.RowsDefaultCellStyle.BackColor = backColor; // Устанавливает цвет фона строк
            _ratesGrid.RowsDefaultCellStyle.ForeColor = textColor; // Устанавливает цвет текста строк
            _ratesGrid.ColumnHeadersDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(48, 48, 48) : Color.WhiteSmoke; // Устанавливает цвет фона заголовков
            _ratesGrid.ColumnHeadersDefaultCellStyle.ForeColor = textColor; // Устанавливает цвет текста заголовков
            _ratesGrid.EnableHeadersVisualStyles = false; // Отключает стандартные визуальные стили заголовков
            _ratesGrid.AlternatingRowsDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(45, 45, 45) : Color.FromArgb(245, 245, 245); // Устанавливает цвет чередующихся строк

            _answerLabel.ForeColor = _isDarkMode ? Color.LightGreen : Color.DarkGreen; // Устанавливает цвет текста метки результата
            UpdateConnectionTexts(); // Обновляет цвета индикаторов подключения
        }

        // Рекурсивно применяет тему ко всем дочерним элементам
        private void ApplyThemeRecursive(Control parent, Color backColor, Color textColor)
        {
            foreach (Control control in parent.Controls) // Перебирает все дочерние элементы
            {
                if (control is NeonPanel) // Сохраняет неоновую панель без изменений
                {
                    continue;
                }

                control.ForeColor = textColor; // Устанавливает цвет текста элемента

                switch (control) // Проверяет тип элемента управления
                {
                    case Button button: // Обрабатывает кнопки
                        button.BackColor = _isDarkMode ? Color.FromArgb(64, 64, 64) : Color.LightGray; // Устанавливает цвет фона кнопки
                        button.ForeColor = textColor; // Устанавливает цвет текста кнопки
                        break;
                    case ListView listView: // Обрабатывает списки
                        listView.BackColor = backColor; // Устанавливает цвет фона
                        listView.ForeColor = textColor; // Устанавливает цвет текста
                        break;
                    case ListBox listBox: // Обрабатывает списки истории
                        listBox.BackColor = backColor; // Устанавливает цвет фона
                        listBox.ForeColor = textColor; // Устанавливает цвет текста
                        break;
                    case Panel or GroupBox: // Обрабатывает панели и группы
                        control.BackColor = backColor; // Устанавливает цвет фона
                        break;
                    case DataGridView: // Обрабатывает таблицы
                        control.BackColor = backColor; // Устанавливает цвет фона
                        break;
                    default: // Обрабатывает остальные элементы
                        control.BackColor = backColor; // Устанавливает цвет фона
                        break;
                }

                if (control.HasChildren) // Проверяет наличие дочерних элементов
                {
                    ApplyThemeRecursive(control, backColor, textColor); // Рекурсивно применяет тему к дочерним элементам
                }
            }
        }


        private Control CreateBadge(string text)
        {
            Label badge = new Label
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(14, 6, 14, 6),
                Margin = new Padding(0, 0, 12, 8),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(96, 0, 0, 0),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Regular),
                UseMnemonic = false,
            };

            void ApplyRoundedShape()
            {
                if (badge.Width <= 0 || badge.Height <= 0)
                {
                    return;
                }

                using GraphicsPath path = new GraphicsPath();
                Rectangle rect = new Rectangle(Point.Empty, new Size(badge.Width - 1, badge.Height - 1));
                int radius = 18;
                int diameter = radius * 2;
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                badge.Region = new Region(path);
            }

            badge.Resize += (_, _) => ApplyRoundedShape();
            badge.CreateControl();
            ApplyRoundedShape();
            return badge;
        }

        private void StyleListView(ListView view, Color backColor, Color textColor)
        {
            view.BackColor = backColor;
            view.ForeColor = textColor;
            view.BorderStyle = BorderStyle.None;
            view.FullRowSelect = true;
            view.HideSelection = false;
            foreach (ListViewItem item in view.Items)
            {
                item.BackColor = backColor;
                item.ForeColor = textColor;
            }
        }

        private void HistoryList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= _historyList.Items.Count)
            {
                return;
            }

            string text = _historyList.Items[e.Index]?.ToString() ?? string.Empty;
            Rectangle bounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, Math.Max(0, e.Bounds.Width - 8), Math.Max(0, e.Bounds.Height - 8));

            Color start = _isDarkMode ? Color.FromArgb(160, 78, 205, 196) : Color.FromArgb(170, 140, 220, 255);
            Color end = _isDarkMode ? Color.FromArgb(160, 120, 160, 255) : Color.FromArgb(170, 255, 180, 215);
            if (e.State.HasFlag(DrawItemState.Selected))
            {
                start = Color.FromArgb(200, start);
                end = Color.FromArgb(200, end);
            }

            using LinearGradientBrush brush = new LinearGradientBrush(bounds, start, end, LinearGradientMode.Horizontal);
            using GraphicsPath pathShape = new GraphicsPath();
            pathShape.AddArc(bounds.X, bounds.Y, 24, 24, 180, 90);
            pathShape.AddArc(bounds.Right - 24, bounds.Y, 24, 24, 270, 90);
            pathShape.AddArc(bounds.Right - 24, bounds.Bottom - 24, 24, 24, 0, 90);
            pathShape.AddArc(bounds.X, bounds.Bottom - 24, 24, 24, 90, 90);
            pathShape.CloseFigure();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(brush, pathShape);

            Color borderColor = _isDarkMode ? Color.FromArgb(120, 0, 0, 0) : Color.FromArgb(90, 255, 255, 255);
            using Pen borderPen = new Pen(borderColor, 1.2f);
            e.Graphics.DrawPath(borderPen, pathShape);

            string prefix = e.Index switch
            {
                0 => "🔥",
                1 => "🚀",
                2 => "🎯",
                _ => "🎧"
            };

            Rectangle textBounds = new Rectangle(bounds.X + 16, bounds.Y + 7, Math.Max(0, bounds.Width - 24), Math.Max(0, bounds.Height - 14));
            using Font textFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            TextRenderer.DrawText(
                e.Graphics,
                $"{prefix} {text}",
                textFont,
                textBounds,
                _isDarkMode ? Color.WhiteSmoke : Color.FromArgb(32, 32, 32),
                TextFormatFlags.EndEllipsis);

            e.DrawFocusRectangle();
        }

        private void AddHistoryItem(string fromCode, string toCode, decimal amount, int decimals, decimal result)
        {
            if (_historyList == null)
            {
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string amountText = amount.ToString("F2");
            string resultText = result.ToString("F" + Math.Clamp(decimals, 0, 6));
            string entry = $"{timestamp} • {amountText} {fromCode} → {resultText} {toCode}";

            _historyList.BeginUpdate();
            _historyList.Items.Insert(0, entry);
            while (_historyList.Items.Count > 10)
            {
                _historyList.Items.RemoveAt(_historyList.Items.Count - 1);
            }
            _historyList.EndUpdate();
            _historyList.SelectedIndex = -1;
            _historyList.Invalidate();
        }

        private void UpdateInsights()
        {
            if (_insightsView == null)
            {
                return;
            }

            CurrencyInfo[] currencies = _converter.GetAllCurrencies();
            _insightsView.BeginUpdate();
            _insightsView.Items.Clear();
            _insightsView.Groups.Clear();

            if (currencies.Length == 0)
            {
                _insightsView.EndUpdate();
                return;
            }

            ListViewGroup topGroup = new ListViewGroup("Легенды курса", HorizontalAlignment.Left);
            ListViewGroup chillGroup = new ListViewGroup("Дружелюбные цены", HorizontalAlignment.Left);
            _insightsView.Groups.Add(topGroup);
            _insightsView.Groups.Add(chillGroup);

            CurrencyInfo[] top = currencies.OrderByDescending(c => c.PriceInUsd).Take(3).ToArray();
            CurrencyInfo[] low = currencies.OrderBy(c => c.PriceInUsd).Take(3).ToArray();
            decimal topAnchor = top.Length > 0 ? top[0].PriceInUsd : 1m;
            decimal lowAnchor = low.Length > 0 ? low[0].PriceInUsd : 1m;

            foreach (CurrencyInfo info in top)
            {
                ListViewItem item = new ListViewItem(info.Code, topGroup);
                item.SubItems.Add(info.PriceInUsd.ToString("F4"));
                item.SubItems.Add(BuildEnergyBar(info.PriceInUsd, topAnchor, false, "⚡"));
                _insightsView.Items.Add(item);
            }

            foreach (CurrencyInfo info in low)
            {
                ListViewItem item = new ListViewItem(info.Code, chillGroup);
                item.SubItems.Add(info.PriceInUsd.ToString("F4"));
                item.SubItems.Add(BuildEnergyBar(info.PriceInUsd, lowAnchor, true, "💧"));
                _insightsView.Items.Add(item);
            }

            _insightsView.EndUpdate();
            AdjustListViewColumns(_insightsView, 0.36f, 0.26f, 0.38f);
        }

        private void UpdateQuickBurst(string fromCode, decimal amount, int decimals)
        {
            if (_quickBurstView == null)
            {
                return;
            }

            CurrencyInfo[] highlights = _converter
                .GetAllCurrencies()
                .Where(info => !string.Equals(info.Code, fromCode, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(info => info.PriceInUsd)
                .Take(4)
                .ToArray();

            _quickBurstView.BeginUpdate();
            _quickBurstView.Items.Clear();
            _quickBurstView.Groups.Clear();

            if (highlights.Length == 0)
            {
                _quickBurstView.EndUpdate();
                return;
            }

            ListViewGroup group = new ListViewGroup("Неоновый залп", HorizontalAlignment.Left);
            _quickBurstView.Groups.Add(group);
            int safeDecimals = Math.Clamp(decimals, 0, 6);
            decimal anchor = highlights[0].PriceInUsd;

            foreach (CurrencyInfo target in highlights)
            {
                decimal converted = _converter.Convert(fromCode, target.Code, amount, safeDecimals);
                string formatted = converted.ToString("F" + safeDecimals);
                ListViewItem item = new ListViewItem(target.Code, group);
                item.SubItems.Add(formatted);
                item.SubItems.Add(BuildEnergyBar(target.PriceInUsd, anchor, false, "✨"));
                _quickBurstView.Items.Add(item);
            }

            _quickBurstView.EndUpdate();
            AdjustListViewColumns(_quickBurstView, 0.32f, 0.38f, 0.30f);
        }

        private void AdjustListViewColumns(ListView view, params float[] ratios)
        {
            if (view.Columns.Count != ratios.Length)
            {
                return;
            }

            int width = view.ClientSize.Width;
            if (width <= 0)
            {
                width = view.Width;
            }

            for (int i = 0; i < ratios.Length; i++)
            {
                float ratio = Math.Clamp(ratios[i], 0f, 1f);
                int columnWidth = Math.Max(60, (int)(width * ratio));
                view.Columns[i].Width = columnWidth;
            }
        }

        private string RepeatSymbol(string symbol, int count)
        {
            if (count <= 0)
            {
                return symbol;
            }

            return string.Concat(Enumerable.Repeat(symbol, count));
        }

        private string BuildEnergyBar(decimal value, decimal reference, bool invert, string symbol)
        {
            if (reference <= 0)
            {
                reference = 1m;
            }

            decimal ratio = invert ? reference / Math.Max(value, 0.000001m) : value / reference;
            ratio = Math.Clamp(ratio, 0m, 4m);
            int pulses = Math.Clamp((int)Math.Round((double)(ratio * 2.5m) + 1), 1, 7);
            return RepeatSymbol(symbol, pulses);
        }

        private void UpdateVibeStatus(string leadMessage)
        {
            if (_vibeLabel == null)
            {
                return;
            }

            string vibe = string.IsNullOrWhiteSpace(leadMessage) ? string.Empty : leadMessage.Trim();
            string tail = _vibeLibrary.Length > 0 ? _vibeLibrary[_random.Next(_vibeLibrary.Length)] : string.Empty;
            if (!string.IsNullOrEmpty(tail))
            {
                vibe = string.IsNullOrEmpty(vibe) ? tail : $"{vibe} • {tail}";
            }

            _vibeLabel.Text = vibe;
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _autoUpdateTimer.Stop();
            _neonTimer.Stop();
        }
    }
}
