using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFDevelopers.Charts.Models
{
    public class PieBase : PieSerise
    {
        /// <summary>
        /// 扇形角度
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// 圆弧
        /// </summary>
        public ArcSegment ArcSegment { get; set; }

        public LineSegment LineSegmentStar { get; set; }

        public LineSegment LineSegmentEnd { get; set; }

        /// <summary>
        /// 圆弧起点
        /// </summary>
        public Point StarPoint { get; set; }

        /// <summary>
        /// 圆弧终点
        /// </summary>
        public Point EndPoint { get; set; }

        /// <summary>
        /// 折线
        /// </summary>
        public Polyline Line { get; set; }

        /// <summary>
        /// 折线终点
        /// </summary>
        public Point PolylineEndPoint { get; set; }

        /// <summary>
        /// 文字
        /// </summary>
        public Path TextPath { get; set; }
    }
}
