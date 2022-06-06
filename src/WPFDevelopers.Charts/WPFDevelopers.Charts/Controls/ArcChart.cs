using Microsoft.Expression.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFDevelopers.Charts.Models;

namespace WPFDevelopers.Charts.Controls
{
    [TemplatePart(Name = GridTemplateName, Type = typeof(Grid))]
    public partial class ArcChart : Control
    {
        const string GridTemplateName = "PART_ChartLayout";

        private Grid _grid;


        /// <summary>
        /// 图表的尺寸
        /// </summary>
        public double ChartSize
        {
            get { return (double)GetValue(ChartSizeProperty); }
            set { SetValue(ChartSizeProperty, value); }
        }

        public static readonly DependencyProperty ChartSizeProperty =
            DependencyProperty.Register("ChartSize", typeof(double), typeof(ArcChart), new PropertyMetadata(300d));


        /// <summary>
        /// 圆弧的宽度
        /// </summary>
        public double ArcThickness
        {
            get { return (double)GetValue(ArcThicknessProperty); }
            set { SetValue(ArcThicknessProperty, value); }
        }

        public static readonly DependencyProperty ArcThicknessProperty =
            DependencyProperty.Register("ArcThickness", typeof(double), typeof(ArcChart), new PropertyMetadata(20d));


        /// <summary>
        /// popup的宽度
        /// </summary>
        public double PopupWidth
        {
            get { return (double)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register("PopupWidth", typeof(double), typeof(ArcChart), new PropertyMetadata(100d));

        /// <summary>
        /// popup的高度
        /// </summary>
        public double PopupHeight
        {
            get { return (double)GetValue(PopupHeightProperty); }
            set { SetValue(PopupHeightProperty, value); }
        }

        public static readonly DependencyProperty PopupHeightProperty =
            DependencyProperty.Register("PopupHeight", typeof(double), typeof(ArcChart), new PropertyMetadata(50d));

        /// <summary>
        /// Percentage 总和不能超过100
        /// </summary>
        public ObservableCollection<PieSerise> ItemsSource
        {
            get { return (ObservableCollection<PieSerise>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<PieSerise>), typeof(ArcChart), new PropertyMetadata(null, new PropertyChangedCallback(ItemsSourceChanged)));

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as ArcChart;
            if (view is null) return;
            if (e.NewValue != null)
                view.DrawArc();
        }

        static ArcChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArcChart), new FrameworkPropertyMetadata(typeof(ArcChart)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _grid = GetTemplateChild(GridTemplateName) as Grid;
            _grid.Loaded += _grid_Loaded;
        }

        private void _grid_Loaded(object sender, RoutedEventArgs e)
        {
            _grid.Width = ChartSize;
            _grid.Height = ChartSize;
            DrawArc();
        }

        void DrawArc() 
        {
            if (ItemsSource is null || !ItemsSource.Any() || _grid is null)
                return;
            _grid.Children.Clear();
            /*
            * 圆弧首尾相连 上个圆弧的终点是下个圆弧的起点
            * 每个圆弧都绑定一个popup 用于显示明细
            * popup内包含一个椭圆背景 一个text 一个折线的虚线
            */

            //下一个圆弧的起点
            double globalStart = 0;
            var currentNumber = 0;
            for (int i = 0; i < ItemsSource.Count; i++)
            {
                //圆弧的起始角度
                double startAngle = globalStart;
                //圆弧的终结角度
                double endAngle = startAngle + ItemsSource[i].Percentage * 3.6;
                //圆弧的中间点的角度
                double middleAngle = globalStart + ItemsSource[i].Percentage * 3.6 / 2;

                #region arc
                Arc arc = new Arc()
                {
                    ArcThickness = ArcThickness,
                    Stretch = Stretch.None,
                    Fill = ItemsSource[i].PieColor,
                    StartAngle = startAngle,
                    EndAngle = endAngle
                };
                globalStart = endAngle;
                _grid.Children.Add(arc);
                #endregion

                #region popup
                //显示的明细 椭圆为背景+text 放到一个grid容器里
                var tb = new TextBlock() 
                { 
                    Text = string.Format("{0}%",
                    ItemsSource[i].Percentage.ToString("0.00")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center, 
                    Foreground = Brushes.White 
                };
                var ell = new Ellipse() 
                { 
                    Fill = ItemsSource[i].PieColor 
                };
                //中间点角度小于180 明细靠右显示 否则靠作显示
                var detailGrid = new Grid() 
                { 
                    Width = PopupHeight,
                    HorizontalAlignment = HorizontalAlignment.Right 
                };
                if (middleAngle > 180)
                {
                    detailGrid.HorizontalAlignment = HorizontalAlignment.Left;
                }
                detailGrid.Children.Add(ell);
                detailGrid.Children.Add(tb);

                //标记线
                var pLine = GetPopupPolyline(middleAngle, ItemsSource[i].PieColor);
                //popup布局容器
                var popupLayout = new Grid();
                popupLayout.Children.Add(pLine);
                popupLayout.Children.Add(detailGrid);
                //popup
                var popup = GetPopup(middleAngle);
                popup.Child = popupLayout;
                //将popup的IsOpen绑定到arc的IsMouseOver 也就是鼠标进入arc时 popup就打开
                var binding = new Binding()
                {
                    Source = arc,
                    Path = new PropertyPath(IsMouseOverProperty),
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(popup, Popup.IsOpenProperty, binding);

                _grid.Children.Add(popup);
                #endregion
                currentNumber++;
            }
        }

        /// <summary>
        /// 获取popup内的标记线
        /// </summary>
        /// <param name="middleAngle">圆弧中间点的角度</param>
        /// <returns>Polyline</returns>
        private Polyline GetPopupPolyline(double middleAngle, Brush brush)
        {
            var pLine = new Polyline() { Stroke = brush ,StrokeThickness = 2, StrokeDashArray = new DoubleCollection(new double[] { 5, 2 }) };
            double x1 = 0, y1 = 0;
            double x2 = 0, y2 = 0;
            double x3 = 0, y3 = 0;
            if (middleAngle > 0 && middleAngle <= 90)
            {
                x1 = 0; y1 = PopupHeight;
                x2 = PopupWidth / 2; y2 = PopupHeight;
                x3 = PopupWidth * 3 / 4; y3 = PopupHeight / 2;
            }
            if (middleAngle > 90 && middleAngle <= 180)
            {
                x1 = 0; y1 = 0;
                x2 = PopupWidth / 2; y2 = 0;
                x3 = PopupWidth * 3 / 4; y3 = PopupHeight / 2;
            }
            if (middleAngle > 180 && middleAngle <= 270)
            {
                x1 = PopupWidth; y1 = 0;
                x2 = PopupWidth / 2; y2 = 0;
                x3 = PopupWidth / 4; y3 = PopupHeight / 2;
            }
            if (middleAngle > 270 && middleAngle <= 360)
            {
                x1 = PopupWidth; y1 = PopupHeight;
                x2 = PopupWidth / 2; y2 = PopupHeight;
                x3 = PopupWidth / 4; y3 = PopupHeight / 2;
            }
            pLine.Points.Add(new Point(x1, y1));
            pLine.Points.Add(new Point(x2, y2));
            pLine.Points.Add(new Point(x3, y3));
            return pLine;
        }
        /// <summary>
        /// 获取popup
        /// </summary>
        /// <param name="middleAngle">圆弧中间点的角度</param>
        /// <returns>Popup</returns>
        private Popup GetPopup(double middleAngle)
        {
            /*
             * 生成popup
             * 设置popup的offset 让标记线的起点 对应到圆弧的中间点
             */
            var popup = new Popup() { Width = PopupWidth, Height = PopupHeight, AllowsTransparency = true, IsHitTestVisible = false };
            //直角三角形 a=r*sinA 勾股定理 c^2=a^2+b^2 b=Sqrt(c^2-a^2)
            double r = ChartSize / 2 - ArcThickness / 2;
            double offsetX = 0, offsetY = 0;
            if (middleAngle > 0 && middleAngle <= 90)
            {
                double sinA = Math.Sin(Math.PI * (90 - middleAngle) / 180);
                double a = r * sinA;
                double c = r;
                double b = Math.Sqrt(c * c - a * a);
                offsetX = ChartSize / 2 + b;
                offsetY = -(ChartSize / 2 + PopupHeight + a);
            }
            if (middleAngle > 90 && middleAngle <= 180)
            {
                double sinA = Math.Sin(Math.PI * (180 - middleAngle) / 180);
                double a = r * sinA;
                double c = r;
                double b = Math.Sqrt(c * c - a * a);
                offsetX = ChartSize / 2 + a;
                offsetY = -(ArcThickness / 2 + (r - b));
            }
            if (middleAngle > 180 && middleAngle <= 270)
            {
                double sinA = Math.Sin(Math.PI * (270 - middleAngle) / 180);
                double a = r * sinA;
                double c = r;
                double b = Math.Sqrt(c * c - a * a);
                offsetX = -PopupWidth + (r - b) + ArcThickness / 2;
                offsetY = -(ArcThickness / 2 + (r - a));
            }
            if (middleAngle > 270 && middleAngle <= 360)
            {
                double sinA = Math.Sin(Math.PI * (360 - middleAngle) / 180);
                double a = r * sinA;
                double c = r;
                double b = Math.Sqrt(c * c - a * a);
                offsetX = -PopupWidth + (r - a) + ArcThickness / 2;
                offsetY = -(ChartSize / 2 + PopupHeight + b);
            }
            popup.HorizontalOffset = offsetX;
            popup.VerticalOffset = offsetY;

            return popup;
        }
    }
}
