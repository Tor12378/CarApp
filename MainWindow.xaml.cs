using App.Metrics.Concurrency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace CarApp
{
    public partial class MainWindow : Window
    {
        private AppDbContext dbContext;
        private ObservableCollection<Car> cars;
        private DispatcherTimer timer;
        private Random random;

        public MainWindow()
        {
            InitializeComponent();
            dbContext = new AppDbContext();
            dbContext.Database.Migrate();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            random = new Random();

            RefreshGrid();
            StartTimer();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateSpeedAsync();
        }

        private async void UpdateSpeedAsync()
        {
            await Task.Run(() =>
            {
                foreach (var car in cars)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lock (car)
                        {
                            car.Speed = random.Next(0, 200);
                        }
                    });
                }
            });
        }

        private void StartTimer()
        {
            timer.Start();
        }

        private void StopTimer()
        {
            timer.Stop();
        }

        private void RefreshGrid()
        {
            cars = new ObservableCollection<Car>(dbContext.Cars);
            datagrid.ItemsSource = cars;

            CollectionViewSource.GetDefaultView(datagrid.ItemsSource).Refresh();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var car = new Car
            {
                Number = numberTextBox.Text,
                Brand = brandTextBox.Text,
                Color = colorTextBox.Text,
                Mileage = int.Parse(mileageTextBox.Text),
                Speed = random.Next(0, 200)
            };

            dbContext.Cars.Add(car);
            dbContext.SaveChanges();
            RefreshGrid();
            numberTextBox.Text = string.Empty;
            brandTextBox.Text = string.Empty;
            colorTextBox.Text = string.Empty;
            mileageTextBox.Text = string.Empty;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem is Car selectedCar)
            {
                dbContext.Cars.Remove(selectedCar);
                dbContext.SaveChanges();
                RefreshGrid();
            }
        }
    }
}