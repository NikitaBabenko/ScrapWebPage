using Microsoft.Extensions.Configuration;
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

namespace ScrapWebPage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ParseWebsiteService parseWebsiteService;
        private readonly DbService dbService;

        public MainWindow()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration Configuration = builder.Build();
            var connectionString = Configuration.GetConnectionString("AppDb");

            parseWebsiteService = new ParseWebsiteService();
            dbService = new DbService(connectionString);

            InitializeComponent();

            SourceComboBox.ItemsSource = new List<string> { "Голубые фишки", "МосБиржа Акции и ПИФы" };
            SourceComboBox.SelectedIndex = 0;
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
                await dbService.SaveOrUpdatePrices(prices);
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
    }
}
