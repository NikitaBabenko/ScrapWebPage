using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Services;
using Services.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;
using Services.Enums;

namespace ScrapWebPage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string correctTimeFrame = "D";

        private readonly ParseWebsiteService parseWebsiteService;
        private readonly DbService dbService;
        private readonly FileService fileService;

        private readonly List<string> files = new();

        public MainWindow()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration Configuration = builder.Build();
            var connectionString = Configuration.GetConnectionString("AppDb");

            parseWebsiteService = new ParseWebsiteService();
            dbService = new DbService(connectionString);
            fileService = new FileService();

            InitializeComponent();

            SourceComboBox.ItemsSource = new List<string> { "Голубые фишки", "МосБиржа Акции и ПИФы" };
            SourceComboBox.SelectedIndex = 0;

            ConflictComboBox.ItemsSource = new List<string> { "Останавливать при дублировании", "Игнорировать при дублировании", "Обновлять при дублировании" };
            ConflictComboBox.SelectedIndex = 0;

            YearFilterTextBox.Text = DateTime.Now.Year.ToString();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SourceType sourceType = (SourceType)SourceComboBox.SelectedIndex;
            DateTime? fromDate = !FromCalendar.SelectedDate.HasValue || FromCalendar.SelectedDate.Value.Date == DateTime.Today ? null : FromCalendar.SelectedDate;
            DateTime? toDate = !ToCalendar.SelectedDate.HasValue || ToCalendar.SelectedDate.Value.Date == DateTime.Today ? null : ToCalendar.SelectedDate;

            textBlock.Text = "Начало выгрузки.\n";
            if(toDate.HasValue)
            {
                fromDate ??= DateTime.Now;
                //if (fromDate.Value.Date == toDate.Value.Date)
                //{
                //    Load(sourceType, fromDate);
                //}
                //else
                if(fromDate.Value.Date > toDate.Value.Date)
                {
                    textBlock.Text += "Дата начала должна быть меньше даты окончания периода.";
                }
                else
                {
                    //TimeSpan period = toDate.Value.Date - fromDate.Value.Date;
                    DateTime date = fromDate.Value.Date;
                    while (date <= toDate.Value.Date)
                    {
                        await Load(sourceType, date);
                        date = date.AddDays(1);
                    }
                }
            }
            else
            {
                await Load(sourceType, fromDate);
            }
            textBlock.Text += "\r\nГотово!";

        }

        private async Task Load(SourceType sourceType, DateTime? date)
        {
            string firstLine = date.HasValue ? $"\nВыгрузка за число {date:dd MM yyyy}" : "Выгрузка за сегодняшнюю дату";
            textBlock.Text += $@"{firstLine}
Загрузка с веб страницы {ParseWebsiteService.GetUrl(sourceType, date)}
";
            List<Price> prices = new List<Price>();
            try
            {
                prices = await parseWebsiteService.GetPrices(sourceType, date);
            }
            catch (Exception ex)
            {
                textBlock.Text += $@"Ошибка!
{ex.Message}
Inner exception
{ex.InnerException?.Message}
{ex.InnerException?.InnerException?.Message}
{ex.InnerException?.InnerException?.InnerException?.Message}

StackTrace:
{ex.StackTrace}";
                return;
            }
            textBlock.Text += @$"Загрузка успешно завершена, количество записей - {prices.Count()}
";

            textBlock.Text += @$"Выгрузка в базу данных...
";
            try
            {
                await dbService.SavePrices(prices);
            }
            catch (Exception ex)
            {
                textBlock.Text += $@"Ошибка!
{ex.Message}
Inner exception
{ex.InnerException?.Message}
{ex.InnerException?.InnerException?.Message}
{ex.InnerException?.InnerException?.InnerException?.Message}

StackTrace:
{ex.StackTrace}";
                return;
            }
            textBlock.Text += $"Выгрузка успешно завершена.\n";
        }


        


        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new() 
            { 
                Multiselect= true,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var added = openFileDialog.FileNames;
                files.AddRange(added);
                textBlock.Text += "\nДобавлены файлы:\n";
                textBlock.Text += string.Join("; ", added);
            }
        }

        private void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var added = fileService.GetFiles(fbd.SelectedPath);
                    files.AddRange(added);
                    textBlock.Text += "\nДобавлены файлы:\n";
                    textBlock.Text += string.Join("; ", added);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            files.Clear();
        }

        void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl)
            {
                textBlock.Text = string.Empty;
            }
        }

        const int chunk = 10000;

        private async void StartLoadFilesButton_Click(object sender, RoutedEventArgs e)
        {
            textBlock.Text += "\nНачало выгрузки\n";

            if(!files.Any())
            {
                textBlock.Text += "\nНужно выбрать файлы!\n";
                return;
            }

            foreach (var file in files)
            {


                try
                {
                    await LoadFile(file);
                }
                catch (Exception ex)
                {
                    textBlock.Text += $@"Ошибка!
{ex.Message}
Inner exception
{ex.InnerException?.Message}
{ex.InnerException?.InnerException?.Message}
{ex.InnerException?.InnerException?.InnerException?.Message}

StackTrace:
{ex.StackTrace}";



                    if (System.Windows.MessageBox.Show($"При загрузке файла {file} произошла ошибка\n{ex.Message}\nПродолжить загрузку других файлов?", "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        return;
                    }
                }


            }
            files.Clear();
        }

        private async Task LoadFile(string filePath)
        {
            textBlock.Text += '\n';
            textBlock.Text += $"Выгрузка файла {filePath}\n";
            var fileLength = fileService.GetFileLength(filePath);
            long linesLeft = fileLength - 1;
            textBlock.Text += $"Записей в фале: {fileLength - 1}\n";
            int count = 0;
            int dbCount = 0;
            long totalCount = 0;
            List<Price> pricies = new List<Price>();
            foreach (var price in fileService.GetPrices(filePath))
            {
                pricies.Add(price);

                count++;
                if (count >= chunk || count >= linesLeft)
                {
                    totalCount += count;
                    linesLeft -= count;

                    var pricesToSave = pricies
                        .Where(p => p != null && p.TimeFrame == correctTimeFrame)
                        .Where(p => !YearFilterCheckBox.IsChecked.HasValue || !YearFilterCheckBox.IsChecked.Value || int.Parse(YearFilterTextBox.Text) <= p.Date.Year)
                        .Distinct();
                    
                    dbCount += pricesToSave.Count();

                    await dbService.SavePrices(pricesToSave, (ConflictResolveType)ConflictComboBox.SelectedIndex);
                    var percent = (double)totalCount / (double)(fileLength-1) * 100;
                    textBlock.Text += $"Загружено {string.Format("{0:0.##}", percent)}%\n";
                    count = 0;
                    pricies.Clear();
                }
            }

            textBlock.Text += $"Записей сохранено в БД {dbCount}\nФайл успешно загружен\n";
        }

        private void CleanTextButton_Click(object sender, RoutedEventArgs e)
        {
            textBlock.Text = string.Empty;
        }



        private void MaskNumericInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextIsNum(e.Text);
        }

        private void MaskNumericPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string input = (string)e.DataObject.GetData(typeof(string));
                if (!TextIsNum(input) ) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool TextIsNum(string input)
        {
            return input.All(c => Char.IsDigit(c) || Char.IsControl(c));
        }
    }
}
