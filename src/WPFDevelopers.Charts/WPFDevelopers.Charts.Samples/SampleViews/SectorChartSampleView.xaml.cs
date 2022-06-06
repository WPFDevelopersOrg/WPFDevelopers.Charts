﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFDevelopers.Charts.Models;

namespace WPFDevelopers.Charts.Samples.SampleViews
{
    /// <summary>
    /// SectorChartSampleView.xaml 的交互逻辑
    /// </summary>
    public partial class SectorChartSampleView : UserControl
    {

        /// <summary>
        /// SectorChart
        /// </summary>
        public ObservableCollection<PieSerise> ItemsSource
        {
            get { return (ObservableCollection<PieSerise>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<PieSerise>), typeof(SectorChartSampleView), new PropertyMetadata(null));
        public SectorChartSampleView()
        {
            InitializeComponent();
            Loaded += SectorChartSampleView_Loaded;
        }

        private void SectorChartSampleView_Loaded(object sender, RoutedEventArgs e)
        {
            ItemsSource = new ObservableCollection<PieSerise>();
            var collection1 = new ObservableCollection<PieSerise>();
            collection1.Add(new PieSerise
            {
                Title = "2012",
                Percentage = 30,
                PieColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B9BD5")),
            });
            collection1.Add(
                new PieSerise
                {
                    Title = "2013",
                    Percentage = 140,
                    PieColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4472C4")),
                });

            collection1.Add(new PieSerise
            {
                Title = "2014",
                Percentage = 49,
                PieColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007fff")),
            });

            collection1.Add(new PieSerise
            {
                Title = "2015",
                Percentage = 50,
                PieColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED7D31")),
            });
            collection1.Add(new PieSerise
            {
                Title = "2016",
                Percentage = 30,
                PieColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC000")),
            });

            collection1.Add(new PieSerise
            {
                Title = "2017",
                Percentage = 30,
                PieColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff033e")),
            });
            ItemsSource = collection1;
        }
    }
}
