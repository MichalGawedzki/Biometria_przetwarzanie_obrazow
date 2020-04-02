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
		int[] rgb;

		public HistogramWindow(Bitmap sourceImage) {
			InitializeComponent();
		}

		public void fillRgbArrays(Bitmap bitmap) {

			r = new int[256];
			g = new int[256];
			b = new int[256];
			for (int i = 0; i < bitmap.Width; i++) {
				for (int j = 0; j < bitmap.Height; j++) {
					System.Drawing.Color color = bitmap.GetPixel(i, j);
					r[color.R]++;
					g[color.G]++;
					b[color.B]++;
				}
			}
		}

		public void fillRgbArray(Bitmap bitmap) {
			rgb = new int[256];
			int sum;
			for (int i = 0; i < bitmap.Width; i++) {
				for (int j = 0; j < bitmap.Height; j++) {
					System.Drawing.Color color = bitmap.GetPixel(i, j);
					sum = (color.R + color.G + color.B) / 3;
					rgb[sum]++;
				}
			}
		}

		public void separatedHistograms(Bitmap sourceImage) {
			Bitmap img = sourceImage;

			fillRgbArrays(img);

			SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Title = "Red",
					Values = new ChartValues<int> (r.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Red,
				},
				new LineSeries
				{
					Title = "Green",
					Values = new ChartValues<int> (g.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Green
				},
				new LineSeries
				{
					Title = "Blue",
					Values = new ChartValues<int> (b.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Blue
				}
			};
			var stringList = new List<String>();
			for (int i = 0; i < 256; i++) {
				stringList.Add(i.ToString());
			}
			Labels = stringList.ToArray();
			YFormatter = value => value.ToString("C");
			DataContext = this;
		}

		public void overallHistogram(Bitmap sourceImage) {
			Bitmap img = sourceImage;

			fillRgbArray(img);

			SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Title = "RGB histogram",
					Values = new ChartValues<int> (rgb.ToArray()),
					PointGeometry = null,
				}
			};
			var stringList = new List<String>();
			for (int i = 0; i < 256; i++) {
				stringList.Add(i.ToString());
			}
			Labels = stringList.ToArray();
			YFormatter = value => value.ToString("C");
			DataContext = this;
		}

		private void stretchButton_Click(object sender, RoutedEventArgs e) {

		}
	}
}
