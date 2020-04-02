using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;


namespace biometria_przetwarzanie_obrazow {
	/// <summary>
	/// Interaction logic for HistogramWindow.xaml
	/// </summary>
	public partial class HistogramWindow : Window {

		public SeriesCollection SeriesCollection { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

        int[] r;
        int[] g;
        int[] b;

		public HistogramWindow(Bitmap sourceImage) {
			InitializeComponent();

            Bitmap img = sourceImage;

            r = getRarray(img);

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<int> (r.ToArray()),
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };
            var stringList = new List<String>();
            for (int i = 0; i < 256; i++) {
                stringList.Add(i.ToString());
            }
            Labels = stringList.ToArray();

            //Labels = new[] {"Jan", "Feb", "Mar", "Apr", "May"};
            YFormatter = value => value.ToString("C");
 
            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> {5, 3, 2, 4},
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = System.Windows.Media.Brushes.Gray
            });
 
            //modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);
 
            DataContext = this;
        }

        public int[] getRarray(Bitmap bitmap) {

            int[] arr = new int[256];
            for(int i = 0; i < bitmap.Width; i++) {
                for(int j = 0; j < bitmap.Height; j++) {
                    System.Drawing.Color color = bitmap.GetPixel(i, j);
                    arr[color.R]++;
                }
            }
            return arr;
        }
	}
}
