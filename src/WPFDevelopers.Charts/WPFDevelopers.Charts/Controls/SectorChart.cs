using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using WPFDevelopers.Charts.Models;

namespace WPFDevelopers.Charts.Controls
{
    [TemplatePart(Name = CanvasTemplateName, Type = typeof(Canvas))]
    [TemplatePart(Name = PopupTemplateName, Type = typeof(Popup))]
    public partial class SectorChart : Control
    {
        const string CanvasTemplateName = "PART_Canvas";
        const string PopupTemplateName = "PART_Popup";

        private Canvas _canvas;
        private Popup _popup;
        private double centenrX, centenrY, radius, offsetX, offsetY;
        private Point minPoint;
        private double fontsize = 12;
        private bool flg = false;



        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(SectorChart), new PropertyMetadata(null));



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SectorChart), new PropertyMetadata(null));


        public ObservableCollection<PieSerise> ItemsSource
        {
            get { return (ObservableCollection<PieSerise>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<PieSerise>), typeof(SectorChart), new PropertyMetadata(null, new PropertyChangedCallback(ItemsSourceChanged)));

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as SectorChart;
            if (e.NewValue != null)
                view.DrawArc();
        }

        static SectorChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SectorChart), new FrameworkPropertyMetadata(typeof(SectorChart)));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = GetTemplateChild(CanvasTemplateName) as Canvas;
            _popup = GetTemplateChild(PopupTemplateName) as Popup;
            DrawArc();
        }

        void DrawArc()
        {
            if (ItemsSource is null || !ItemsSource.Any() || _canvas is null)
                return;
            _canvas.Children.Clear();

            var pieWidth = _canvas.ActualWidth > _canvas.ActualHeight ? _canvas.ActualHeight : _canvas.ActualWidth;
            var pieHeight = _canvas.ActualWidth > _canvas.ActualHeight ? _canvas.ActualHeight : _canvas.ActualWidth;
            centenrX = pieWidth / 2;
            centenrY = pieHeight / 2;
            radius = this.ActualWidth > this.ActualHeight ? this.ActualHeight / 2 : this.ActualWidth / 2;
            double angle = 0;
            double prevAngle = 0;

            var sum = ItemsSource.Select(ser => ser.Percentage).Sum();

            foreach (var item in ItemsSource)
            {
                var line1X = radius * Math.Cos(angle * Math.PI / 180) + centenrX;
                var line1Y = radius * Math.Sin(angle * Math.PI / 180) + centenrY;

                angle = item.Percentage / sum * 360 + prevAngle;

                double arcX = 0;
                double arcY = 0;

                if (ItemsSource.Count() == 1 && angle == 360)
                {
                    arcX = centenrX + Math.Cos(359.99999 * Math.PI / 180) * radius;
                    arcY = (radius * Math.Sin(359.99999 * Math.PI / 180)) + centenrY;
                }
                else
                {
                    arcX = centenrX + Math.Cos(angle * Math.PI / 180) * radius;
                    arcY = (radius * Math.Sin(angle * Math.PI / 180)) + centenrY;
                }


                var line1Segment = new LineSegment(new Point(line1X, line1Y), false);

                bool isLargeArc = item.Percentage / sum > 0.5;


                var arcWidth = radius;
                var arcHeight = radius;
                var arcSegment = new ArcSegment();


                arcSegment.Size = new Size(arcWidth, arcHeight);
                arcSegment.Point = new Point(arcX, arcY);
                arcSegment.SweepDirection = SweepDirection.Clockwise;
                arcSegment.IsLargeArc = isLargeArc;



                var line2Segment = new LineSegment(new Point(centenrX, centenrY), false);


                PieBase piebase = new PieBase();
                piebase.Title = item.Title;
                piebase.Percentage = item.Percentage;
                piebase.PieColor = item.PieColor;
                piebase.LineSegmentStar = line1Segment;
                piebase.ArcSegment = arcSegment;
                piebase.LineSegmentEnd = line2Segment;
                piebase.Angle = item.Percentage / sum * 360;
                piebase.StarPoint = new Point(line1X, line1Y);
                piebase.EndPoint = new Point(arcX, arcY);


                var pathFigure = new PathFigure(new Point(centenrX, centenrY), new List<PathSegment>()
                {
                    line1Segment,
                    arcSegment,
                   line2Segment,
                }, true);



                var pathFigures = new List<PathFigure>()
                {
                    pathFigure,
                };
                var pathGeometry = new PathGeometry(pathFigures);
                var path = new Path() { Fill = item.PieColor, Data = pathGeometry, DataContext = piebase };
                _canvas.Children.Add(path);

                prevAngle = angle;

                var line3 = DrawLine(path);
                if (line3 != null)
                    piebase.Line = line3;
                var textPathGeo = DrawText(path);
                var textpath = new Path() { Fill = item.PieColor, Data = textPathGeo };
                piebase.TextPath = textpath;

                _canvas.Children.Add(textpath);
                path.MouseMove += Path_MouseMove1;
                path.MouseLeave += Path_MouseLeave;

                if (ItemsSource.Count() == 1 && angle == 360)
                {
                    _canvas.Children.Add(line3);
                }
                else
                {
                    var outline1 = new Line()
                    {
                        X1 = centenrX,
                        Y1 = centenrY,
                        X2 = line1Segment.Point.X,
                        Y2 = line1Segment.Point.Y,
                        Stroke = Brushes.White,
                        StrokeThickness = 0.8,
                    };
                    var outline2 = new Line()
                    {
                        X1 = centenrX,
                        Y1 = centenrY,
                        X2 = arcSegment.Point.X,
                        Y2 = arcSegment.Point.Y,
                        Stroke = Brushes.White,
                        StrokeThickness = 0.8,
                    };
                    _canvas.Children.Add(outline1);
                    _canvas.Children.Add(outline2);
                    _canvas.Children.Add(line3);
                }

            }
        }
        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            _popup.IsOpen = false;
            var path = sender as Path;
            var dt = path.DataContext as PieBase;

            TranslateTransform ttf = new TranslateTransform();
            ttf.X = 0;
            ttf.Y = 0;
            path.RenderTransform = ttf;
            dt.Line.RenderTransform = new TranslateTransform()
            {
                X = 0,
                Y = 0,
            };

            dt.TextPath.RenderTransform = new TranslateTransform()
            {
                X = 0,
                Y = 0,
            };

            path.Effect = new DropShadowEffect()
            {
                Color = (Color)ColorConverter.ConvertFromString("#FF949494"),
                BlurRadius = 20,
                Opacity = 0,
                ShadowDepth = 0
            };
            flg = false;
        }

        private void Path_MouseMove1(object sender, MouseEventArgs e)
        {
            Path path = sender as Path;
            //动画
            if (!flg)
            {

                BegionOffsetAnimation(path);
            }
            ShowMousePopup(path, e);


        }

        void ShowMousePopup(Path path, MouseEventArgs e)
        {
            var data = path.DataContext as PieBase;
            if (!_popup.IsOpen)
                _popup.IsOpen = true;

            var mousePosition = e.GetPosition((UIElement)_canvas.Parent);

            _popup.HorizontalOffset = mousePosition.X + 20;
            _popup.VerticalOffset = mousePosition.Y + 20;

            Text = (data.Title + " : " + data.Percentage);//显示鼠标当前坐标点
            Fill = data.PieColor;
        }

        void BegionOffsetAnimation(Path path)
        {
            NameScope.SetNameScope(this, new NameScope());
            var pathDataContext = path.DataContext as PieBase;
            var angle = pathDataContext.Angle;

            minPoint = new Point(Math.Round(pathDataContext.StarPoint.X + pathDataContext.EndPoint.X) / 2, Math.Round(pathDataContext.StarPoint.Y + pathDataContext.EndPoint.Y) / 2);


            var v1 = minPoint - new Point(centenrX, centenrY);

            var v2 = new Point(2000, 0) - new Point(0, 0);
            double vAngle = 0;
            if (180 < angle && angle <= 360 && pathDataContext.Percentage / ItemsSource.Select(p => p.Percentage).Sum() >= 0.5)
            {
                vAngle = Math.Round(Vector.AngleBetween(v2, -v1));
            }
            else
            {
                vAngle = Math.Round(Vector.AngleBetween(v2, v1));
            }


            offsetX = 10 * Math.Cos(vAngle * Math.PI / 180);
            offsetY = 10 * Math.Sin(vAngle * Math.PI / 180);

            var line3 = pathDataContext.Line;
            var textPath = pathDataContext.TextPath;

            TranslateTransform LineAnimatedTranslateTransform =
                new TranslateTransform();
            this.RegisterName("LineAnimatedTranslateTransform", LineAnimatedTranslateTransform);
            line3.RenderTransform = LineAnimatedTranslateTransform;


            TranslateTransform animatedTranslateTransform =
                new TranslateTransform();
            this.RegisterName("AnimatedTranslateTransform", animatedTranslateTransform);
            path.RenderTransform = animatedTranslateTransform;

            TranslateTransform TextAnimatedTranslateTransform =
               new TranslateTransform();
            this.RegisterName("TextAnimatedTranslateTransform", animatedTranslateTransform);
            textPath.RenderTransform = animatedTranslateTransform;


            DoubleAnimation daX = new DoubleAnimation();
            Storyboard.SetTargetProperty(daX, new PropertyPath(TranslateTransform.XProperty));
            daX.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            daX.From = 0;
            daX.To = offsetX;


            DoubleAnimation daY = new DoubleAnimation();

            Storyboard.SetTargetName(daY, nameof(animatedTranslateTransform));
            Storyboard.SetTargetProperty(daY, new PropertyPath(TranslateTransform.YProperty));
            daY.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            daY.From = 0;
            daY.To = offsetY;

            path.Effect = new DropShadowEffect()
            {
                Color = (Color)ColorConverter.ConvertFromString("#2E2E2E"),
                BlurRadius = 33,
                Opacity = 0.6,
                ShadowDepth = 0
            };

            animatedTranslateTransform.BeginAnimation(TranslateTransform.XProperty, daX);
            animatedTranslateTransform.BeginAnimation(TranslateTransform.YProperty, daY);
            LineAnimatedTranslateTransform.BeginAnimation(TranslateTransform.XProperty, daX);
            LineAnimatedTranslateTransform.BeginAnimation(TranslateTransform.YProperty, daY);
            TextAnimatedTranslateTransform.BeginAnimation(TranslateTransform.XProperty, daX);
            TextAnimatedTranslateTransform.BeginAnimation(TranslateTransform.YProperty, daY);




            flg = true;
        }
        /// <summary>
        /// 画指示线
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Polyline DrawLine(Path path)
        {
            NameScope.SetNameScope(this, new NameScope());
            var pathDataContext = path.DataContext as PieBase;
            var angle = pathDataContext.Angle;
            pathDataContext.Line = null;
            minPoint = new Point(Math.Round(pathDataContext.StarPoint.X + pathDataContext.EndPoint.X) / 2, Math.Round(pathDataContext.StarPoint.Y + pathDataContext.EndPoint.Y) / 2);

            Vector v1;
            if (angle > 180 && angle < 360)
            {
                v1 = new Point(centenrX, centenrY) - minPoint;
            }
            else if (angle == 180 || angle == 360)
            {
                if (Math.Round(pathDataContext.StarPoint.X) == Math.Round(pathDataContext.EndPoint.X))
                {
                    v1 = new Point(radius * 2, radius) - new Point(centenrX, centenrY);

                }
                else
                {
                    if (Math.Round(pathDataContext.StarPoint.X) - Math.Round(pathDataContext.EndPoint.X) == 2 * radius)
                    {
                        v1 = new Point(radius, 2 * radius) - new Point(centenrX, centenrY);
                    }
                    else
                    {
                        v1 = new Point(radius, 0) - new Point(centenrX, centenrY);
                    }
                }
            }
            else
            {
                v1 = minPoint - new Point(centenrX, centenrY);
            }
            v1.Normalize();
            var Vmin = v1 * radius;
            var RadiusToNodal = Vmin + new Point(centenrX, centenrY);
            var v2 = new Point(2000, 0) - new Point(0, 0);
            double vAngle = 0;
            vAngle = Math.Round(Vector.AngleBetween(v2, v1));

            offsetX = 10 * Math.Cos(vAngle * Math.PI / 180);
            offsetY = 10 * Math.Sin(vAngle * Math.PI / 180);

            var prolongPoint = new Point(RadiusToNodal.X + offsetX * 1, RadiusToNodal.Y + offsetY * 1);

            if (RadiusToNodal.X == double.NaN || RadiusToNodal.Y == double.NaN || prolongPoint.X == double.NaN || prolongPoint.Y == double.NaN)
                return null;


            var point1 = RadiusToNodal;
            var point2 = prolongPoint;
            Point point3;
            if (prolongPoint.X >= radius)
                point3 = new Point(prolongPoint.X + 10, prolongPoint.Y);
            else
                point3 = new Point(prolongPoint.X - 10, prolongPoint.Y);
            PointCollection polygonPoints = new PointCollection();
            polygonPoints.Add(point1);
            polygonPoints.Add(point2);
            polygonPoints.Add(point3);
            var line3 = new Polyline();
            line3.Points = polygonPoints;
            line3.Stroke = pathDataContext.PieColor;
            pathDataContext.PolylineEndPoint = point3;

            return line3;
        }

        PathGeometry DrawText(Path path)
        {
            NameScope.SetNameScope(this, new NameScope());
            var pathDataContext = path.DataContext as PieBase;

            Typeface typeface = new Typeface
                (new FontFamily("Microsoft YaHei"),
                FontStyles.Normal,
                FontWeights.Normal, FontStretches.Normal);

            FormattedText text = new FormattedText(
                pathDataContext.Title,
                new System.Globalization.CultureInfo("zh-cn"),
                FlowDirection.LeftToRight, typeface, fontsize, Brushes.RosyBrown
                );

            var textWidth = text.Width;

            Geometry geo = null;
            if (pathDataContext.PolylineEndPoint.X > radius)
                geo = text.BuildGeometry(new Point(pathDataContext.PolylineEndPoint.X + 4, pathDataContext.PolylineEndPoint.Y - fontsize / 1.8));
            else
                geo = text.BuildGeometry(new Point(pathDataContext.PolylineEndPoint.X - textWidth - 4, pathDataContext.PolylineEndPoint.Y - fontsize / 1.8));
            PathGeometry pathGeometry = geo.GetFlattenedPathGeometry();
            return pathGeometry;

        }
    }
}
