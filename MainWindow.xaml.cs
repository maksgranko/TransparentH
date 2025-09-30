using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TransparentH
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public JetDBHelper _dbHelper;
        private DataTable _allTablesData;
        private DataView _dataView;

        public MainWindow()
        {
            try
            {
                _dbHelper = new JetDBHelper(GetFirstAstFilePath());
                InitializeComponent();
                PopulateDataGrid();
                Background = Brushes.Transparent;
                cumvas.Opacity = 20;
            }
            catch { Environment.Exit(0); }
        }
        private string GetFirstAstFilePath()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] astFiles = Directory.GetFiles(currentDirectory, "*.ast");

            if (astFiles.Length > 0)
            {
                return astFiles[0];
            }
            return null;
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                readyString.Opacity += 0.01;
            }
            if (e.Key == Key.Down)
            {
                readyString.Opacity -= 0.01;
            }
            if (e.Key == Key.Enter)
            {
                FilterDataGrid();
            }
            if (e.Key == Key.Pause)
            {
                end();
                Environment.Exit(0);
            }
        }
        private void end()
        {
            // Путь к папке с исполняемым файлом
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Путь к bat файлу
            string batFilePath = Path.Combine(exeDirectory, "self_delete.bat");

            // Содержимое bat файла
            string batFileContent = "@echo off\r\n" +
                                    "timeout /t 2 /nobreak > nul\r\n" +
                                    "del /f /q \"" + exeDirectory + "*.exe\"\r\n" +
                                    "del /f /q \"%~f0\"";

            // Создание bat файла
            File.WriteAllText(batFilePath, batFileContent);

            // Запуск bat файла
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = batFilePath,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(psi);
        }
        private void PopulateDataGrid()
        {
            try
            {
                // Список таблиц, которые необходимо обработать
                List<string> tableNames = new List<string>
                {
                    //"OpenClose",
                    "OpenOtvet",
                    "Sootvt",
                    //"TestZad",
                    "CloseOtvet"
                };

                // Создаем DataTable для всех данных
                _allTablesData = new DataTable();

                foreach (string tableName in tableNames)
                {
                    Console.WriteLine($"Processing table: {tableName}");

                    // Получаем все значения для текущей таблицы
                    List<dynamic[]> tableData = _dbHelper.GetAllValues(tableName);
                    Console.WriteLine($"Table {tableName} has {tableData.Count} rows.");

                    if (tableData.Count > 0)
                    {
                        // Создаем DataTable для текущей таблицы
                        DataTable tableDataTable = new DataTable(tableName);

                        // Добавляем колонки (только первые 6)
                        int columnCount = Math.Min(tableData[0].Length, 6);
                        for (int i = 0; i < columnCount; i++)
                        {
                            tableDataTable.Columns.Add($"Column{i + 1}");
                        }

                        // Добавляем строки (только первые 6 столбцов)
                        foreach (var row in tableData)
                        {
                            tableDataTable.Rows.Add(row.Take(columnCount).ToArray());
                        }

                        // Объединяем DataTable текущей таблицы с allTablesData
                        _allTablesData.Merge(tableDataTable, true, MissingSchemaAction.Add);
                    }
                }

                // Устанавливаем ItemsSource для DataGrid
                _dataView = _allTablesData.DefaultView;
                TablesDataGrid.ItemsSource = _dataView;

                Console.WriteLine($"Total rows in allTablesData: {_allTablesData.Rows.Count}");
            }
            catch (Exception ex)
            {
                // Тихая обработка исключений
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            textbox.Text = "";
            textbox.Focus();
            CollectionViewSource.GetDefaultView(TablesDataGrid.ItemsSource).Refresh();
            readyString.Text = "";
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textbox.Text) && TablesDataGrid != null && TablesDataGrid.ItemsSource != null)
                {
                    CollectionViewSource.GetDefaultView(TablesDataGrid.ItemsSource).Refresh();
                }
            }
            catch { }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            textbox.Text = "";
            textbox.Focus();
            readyString.Text = "";
        }

        private void cumvas_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void FilterDataGrid()
        {
            if (int.TryParse(textbox.Text, out int filterValue))
            {
                StringBuilder result = new StringBuilder();
                bool found = false;

                foreach (DataRow row in _allTablesData.Rows)
                {
                    if (row.ItemArray[1] != null && row.ItemArray[1].ToString() == filterValue.ToString())
                    {
                        result.AppendLine(string.Join(", ", row.ItemArray));
                        result.AppendLine("\n\n");
                        found = true;
                    }
                }

                if (found)
                {
                    readyString.Text = result.ToString();
                }
                else
                {
                    readyString.Text = "No matching records found.";
                }
            }
            else
            {
                readyString.Text = "Invalid input.";
            }
        }
    }
}
