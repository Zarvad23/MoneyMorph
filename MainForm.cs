using System; // позволяет использовать базовые классы .NET
using System.Drawing; // позволяет работать с графикой и цветами
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

        // Инициализирует форму и все её компоненты
        public MainForm() // Конструктор главной формы
        {
            _converter = new CurrencyConverter(); // Создаёт экземпляр конвертера
            BuildLayout(); // Создаёт пользовательский интерфейс
            LoadCurrencies(); // Загружает список валют
            RefreshRatesTable(); // Заполняет таблицу курсов
            ApplyTheme(); // Применяет тему оформления
            Load += MainForm_Load; // Подписывается на событие загрузки формы
        }

        // Создаёт и настраивает все элементы управления на форме
        private void BuildLayout()
        {
            Text = "MoneyMorph - учебный пример"; // Устанавливает заголовок окна
            StartPosition = FormStartPosition.CenterScreen; // Центрирует окно на экране
            MinimumSize = new Size(820, 480); // Задаёт минимальный размер окна чтобы элементы не сжимались слишком сильно

            // Создаёт основной контейнер с двумя колонками
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет всю форму
                ColumnCount = 2, // Устанавливает две колонки
                RowCount = 2, // Устанавливает две строки
                Padding = new Padding(15) // Добавляет отступы
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f)); // Левая колонка 45%
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f)); // Правая колонка 55%
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Первая строка занимает всё доступное пространство
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Вторая строка подстраивается под содержимое
            Controls.Add(mainLayout); // Добавляет контейнер на форму

            // Создаёт панель с элементами ввода данных
            TableLayoutPanel inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет свою область
                ColumnCount = 2, // Устанавливает две колонки
                RowCount = 8, // Устанавливает восемь строк
                AutoSize = true, // Включает автоматический размер
                AutoSizeMode = AutoSizeMode.GrowAndShrink, // Режим автоматического изменения размера
                Padding = new Padding(0, 0, 10, 0) // Добавляет отступ справа
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f)); // Левая колонка 50%
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f)); // Правая колонка 50%
            for (int i = 0; i < 8; i++) // Перебирает все строки
            {
                inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Устанавливает автоматическую высоту строки
            }
            mainLayout.Controls.Add(inputLayout, 0, 0); // Добавляет панель в левую верхнюю ячейку

            // Создаёт подсказку для пользователя
            Label helpLabel = new Label
            {
                Text = "Введите сумму и выберите направление обмена", // Устанавливает текст подсказки
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(helpLabel, 0, 0); // Добавляет метку в первую ячейку
            inputLayout.SetColumnSpan(helpLabel, 2); // Объединяет две колонки

            // Создаёт метку для выбора исходной валюты
            Label fromLabel = new Label
            {
                Text = "Исходная валюта:", // Устанавливает текст метки
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(fromLabel, 0, 1); // Добавляет метку во вторую строку

            _fromBox = new ComboBox
            {
                Dock = DockStyle.Fill, // Заполняет ячейку
                DropDownStyle = ComboBoxStyle.DropDownList // Запрещает ввод текста вручную
            };
            inputLayout.Controls.Add(_fromBox, 1, 1); // Добавляет выпадающий список рядом с меткой

            // Создаёт метку для выбора целевой валюты
            Label toLabel = new Label
            {
                Text = "Нужная валюта:", // Устанавливает текст метки
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(toLabel, 0, 2); // Добавляет метку в третью строку

            _toBox = new ComboBox
            {
                Dock = DockStyle.Fill, // Заполняет ячейку
                DropDownStyle = ComboBoxStyle.DropDownList // Запрещает ввод текста вручную
            };
            inputLayout.Controls.Add(_toBox, 1, 2); // Добавляет выпадающий список рядом с меткой

            // Создаёт метку для поля ввода суммы
            Label amountLabel = new Label
            {
                Text = "Сумма:", // Устанавливает текст метки
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(amountLabel, 0, 3); // Добавляет метку в четвёртую строку

            _amountBox = new TextBox
            {
                Dock = DockStyle.Fill // Заполняет ячейку
            };
            inputLayout.Controls.Add(_amountBox, 1, 3); // Добавляет текстовое поле рядом с меткой

            // Создаёт метку для настройки количества знаков после запятой
            Label decimalsLabel = new Label
            {
                Text = "Округление (знаков):", // Устанавливает текст метки
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(decimalsLabel, 0, 4); // Добавляет метку в пятую строку

            _decimalsBox = new NumericUpDown
            {
                Dock = DockStyle.Left, // Выравнивает по левому краю
                Minimum = 0, // Минимальное значение
                Maximum = 6, // Максимальное значение
                Value = 2, // Начальное значение
                Width = 80 // Ширина элемента
            };
            inputLayout.Controls.Add(_decimalsBox, 1, 4); // Добавляет счётчик рядом с меткой

            // Создаёт панель с кнопками управления
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, // Заполняет ячейку
                FlowDirection = FlowDirection.LeftToRight, // Горизонтальное размещение
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(buttonPanel, 0, 5); // Добавляет панель в шестую строку
            inputLayout.SetColumnSpan(buttonPanel, 2); // Объединяет две колонки

            Button convertButton = new Button
            {
                Text = "Посчитать", // Текст кнопки
                AutoSize = true // Включает автоматический размер
            };
            convertButton.Click += ConvertButton_Click; // Подписывается на событие нажатия
            buttonPanel.Controls.Add(convertButton); // Добавляет кнопку на панель

            Button swapButton = new Button
            {
                Text = "Поменять", // Текст кнопки
                AutoSize = true // Включает автоматический размер
            };
            swapButton.Click += SwapButton_Click; // Подписывается на событие нажатия
            buttonPanel.Controls.Add(swapButton); // Добавляет кнопку на панель

            _updateRatesButton = new Button
            {
                Text = "Обновить курсы", // Текст кнопки
                AutoSize = true // Включает автоматический размер
            };
            _updateRatesButton.Click += UpdateRatesButton_Click; // Подписывается на событие нажатия
            buttonPanel.Controls.Add(_updateRatesButton); // Добавляет кнопку на панель

            Button themeButton = new Button
            {
                Text = "Смена темы", // Текст кнопки
                AutoSize = true // Включает автоматический размер
            };
            themeButton.Click += ThemeButton_Click; // Подписывается на событие нажатия
            buttonPanel.Controls.Add(themeButton); // Добавляет кнопку на панель

            // Создаёт индикатор состояния подключения
            _connectionLabel = new Label
            {
                Text = "Связь: нет данных", // Начальный текст индикатора
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(_connectionLabel, 0, 6); // Добавляет метку в седьмую строку
            inputLayout.SetColumnSpan(_connectionLabel, 2); // Объединяет две колонки

            // Создаёт метку времени последнего обновления
            _lastUpdateLabel = new Label
            {
                Text = "Последнее обновление: нет", // Начальный текст метки
                AutoSize = true // Включает автоматический размер
            };
            inputLayout.Controls.Add(_lastUpdateLabel, 0, 7); // Добавляет метку в восьмую строку
            inputLayout.SetColumnSpan(_lastUpdateLabel, 2); // Объединяет две колонки

            // Создаёт группу с таблицей курсов валют
            GroupBox ratesGroup = new GroupBox
            {
                Text = "Текущие курсы", // Заголовок группы
                Dock = DockStyle.Fill // Заполняет свою область
            };
            mainLayout.Controls.Add(ratesGroup, 1, 0); // Добавляет группу в правую верхнюю ячейку

            _ratesGrid = new DataGridView
            {
                Dock = DockStyle.Fill, // Заполняет всю группу
                ReadOnly = true, // Только для чтения
                AllowUserToAddRows = false, // Запрещает добавление строк
                AllowUserToDeleteRows = false, // Запрещает удаление строк
                AllowUserToResizeColumns = false, // Запрещает изменение ширины колонок
                AllowUserToResizeRows = false, // Запрещает изменение высоты строк
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize, // Автоматическая высота заголовков
                RowHeadersVisible = false, // Скрывает заголовки строк
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // Выделение целой строки
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill // Колонки заполняют всю ширину
            };
            _ratesGrid.Columns.Add("Code", "Код валюты"); // Добавляет колонку для кода валюты
            _ratesGrid.Columns.Add("Usd", "Цена за 1 единицу (USD)"); // Добавляет колонку для курса
            ratesGroup.Controls.Add(_ratesGrid); // Добавляет таблицу в группу

            // Создаёт метку для отображения результата конвертации
            _answerLabel = new Label
            {
                Text = "Результат появится ниже", // Начальный текст метки
                AutoSize = true, // Включает автоматический размер
                ForeColor = Color.DarkGreen, // Устанавливает цвет текста
                Dock = DockStyle.Fill, // Заполняет ячейку
                TextAlign = ContentAlignment.MiddleLeft, // Выравнивание текста по левому краю
                Padding = new Padding(0, 10, 0, 0) // Добавляет отступ сверху
            };
            mainLayout.Controls.Add(_answerLabel, 0, 1); // Добавляет метку в нижнюю строку
            mainLayout.SetColumnSpan(_answerLabel, 2); // Объединяет две колонки

            // Создаёт таймер для автоматического обновления курсов
            _autoUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 4000 // Интервал в миллисекундах (4 секунды)
            };
            _autoUpdateTimer.Tick += AutoUpdateTimer_Tick; // Подписывается на событие таймера
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

                    if (showMessage) // Проверяет необходимость показа сообщения
                    {
                        _answerLabel.Text = "Курсы обновлены по данным сервера"; // Выводит сообщение об успехе
                    }
                }
                else
                {
                    _connectionOnline = false; // Сбрасывает флаг наличия подключения
                    UpdateConnectionTexts(); // Обновляет текст индикаторов

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
                control.ForeColor = textColor; // Устанавливает цвет текста элемента

                switch (control) // Проверяет тип элемента управления
                {
                    case Button button: // Обрабатывает кнопки
                        button.BackColor = _isDarkMode ? Color.FromArgb(64, 64, 64) : Color.LightGray; // Устанавливает цвет фона кнопки
                        button.ForeColor = textColor; // Устанавливает цвет текста кнопки
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
    }
}