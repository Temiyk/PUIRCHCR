using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Quizes2.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Quizes2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TestData testData;
        private string selectedTestFile;
        private Dictionary<string, List<string>> themesAndTests;
        public MainWindow()
        {
            InitializeComponent();
            LoadThemesAndTests();
        }
        private void LoadThemesAndTests() 
        {
            themesAndTests = new Dictionary<string, List<string>>();

            // Путь к папке проекта - используем относительный путь от исполняемого файла
            // Исполняемый файл находится в bin\Debug\net8.0-windows\ (или подобное)
            // Нужно подняться на 3 уровня вверх
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectPath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));

            string testsFolder = Path.Combine(projectPath, "tests");

            if (!Directory.Exists(testsFolder))
            {
                MessageBox.Show($"Папка с тестами не найдена! Путь: {testsFolder}\n" +
                               "Создайте папку 'tests' с подпапками-темами в корне проекта.",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var themeFolders = Directory.GetDirectories(testsFolder);
            if (themeFolders.Length == 0)
            {
                MessageBox.Show("В папке 'tests' не найдено подпапок с темами.",
                               "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var themeFolder in themeFolders)
            {
                string themeName = Path.GetFileName(themeFolder);
                var testFiles = Directory.GetFiles(themeFolder, "*.txt")
                                        .Select(Path.GetFileName)
                                        .ToList();

                themesAndTests[themeName] = testFiles;
            }

            ThemesListBox.ItemsSource = themesAndTests.Keys.OrderBy(x => x);
        }
        private string GetTestsFolderPath()
        {
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string projectDirectory = Directory.GetParent(assemblyLocation).Parent.Parent.FullName;
            return Path.Combine(projectDirectory, "tests");
        }
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            new TestWindow(testData).ShowDialog();
        }

        private TestData ParseTestFile(string filePath)
        {
            var lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);
            var data = new TestData();

            if (lines.Length == 0)
                throw new Exception("Файл теста пустой.");

            int i = 0;

            // Заголовок теста (первой строкой)
            data.Title = lines[i++].Trim();

            // Читаем вопросы
            while (i < lines.Length)
            {
                // Пропускаем пустые строки
                while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
                    i++;

                if (i >= lines.Length) break;

                // Раздел результатов
                if (lines[i].Trim() == "---RESULTS---")
                {
                    i++; // переходим к разделу результатов
                    break;
                }

                // Вопрос
                var q = new Question
                {
                    Text = lines[i++].Trim()
                };

                // Варианты ответа: до пустой строки или до разделителя
                while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                {
                    var parts = lines[i].Split(';');
                    if (parts.Length >= 2)
                    {
                        string answerText = parts[0].Trim();
                        string pointsStr = parts[1].Trim();

                        if (!int.TryParse(pointsStr, out int points))
                        {
                            // Если не удалось распарсить баллы — выбрасываем понятную ошибку
                            throw new FormatException($"Неверный формат баллов в строке {i + 1}: \"{lines[i]}\".");
                        }

                        q.Answers.Add(new Answer
                        {
                            Text = answerText,
                            Points = points
                        });
                    }
                    else
                    {
                        // Можно решить иначе: пропустить или выбросить; я выбрасываю, чтобы заметить ошибку в файле
                        throw new FormatException($"Ожидалось минимум 2 поля через ';' в строке {i + 1}: \"{lines[i]}\".");
                    }

                    i++;
                }

                data.Questions.Add(q);
            }

            // Читаем результаты после ---RESULTS---
            while (i < lines.Length)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    i++;
                    continue;
                }

                var parts = lines[i].Split(';');
                if (parts.Length >= 3)
                {
                    if (!int.TryParse(parts[0].Trim(), out int minScore))
                        throw new FormatException($"Неверный MinScore в строке {i + 1}: \"{lines[i]}\".");

                    if (!int.TryParse(parts[1].Trim(), out int maxScore))
                        throw new FormatException($"Неверный MaxScore в строке {i + 1}: \"{lines[i]}\".");

                    var text = parts[2].Trim();

                    data.Results.Add(new TestResult
                    {
                        MinScore = minScore,
                        MaxScore = maxScore,
                        Text = text
                    });
                }
                else
                {
                    throw new FormatException($"Ожидалось 3 поля через ';' в строке результата {i + 1}: \"{lines[i]}\".");
                }

                i++;
            }

            return data;
        }

        private void ThemesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemesListBox.SelectedItem != null)
            {
                string selectedTheme = ThemesListBox.SelectedItem.ToString();
                var testTitles = new List<string>();

                foreach (var testFile in themesAndTests[selectedTheme])
                {
                    try
                    {
                        string testsFolder = GetTestsFolderPath();
                        string filePath = Path.Combine(testsFolder, selectedTheme, testFile);

                        string firstLine = File.ReadLines(filePath, Encoding.UTF8).FirstOrDefault()?.Trim();
                        testTitles.Add(string.IsNullOrEmpty(firstLine) ? testFile : firstLine);
                    }
                    catch
                    {
                        testTitles.Add(testFile);
                    }
                }

                TestsListBox.ItemsSource = testTitles;
                TestsListBox.SelectedItem = null;
                StartBtn.IsEnabled = false;
                testData = null;
            }
        }

        private void TestsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string theme = ThemesListBox.SelectedItem.ToString();
            int selectedIndex = TestsListBox.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < themesAndTests[theme].Count)
            {
                string testFile = themesAndTests[theme][selectedIndex];

                string testsFolder = GetTestsFolderPath();
                selectedTestFile = Path.Combine(testsFolder, theme, testFile);

                try
                {
                    testData = ParseTestFile(selectedTestFile);
                    StartBtn.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки теста: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StartBtn.IsEnabled = false;
                    testData = null;
                }
            }
        }

        private void StartBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (testData != null)
            {
                // Передаем также название темы и файла для отображения в заголовке
                var testWindow = new TestWindow(testData);
                testWindow.Title = $"{ThemesListBox.SelectedItem} - {TestsListBox.SelectedItem}";
                testWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите тест для начала.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
       

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}