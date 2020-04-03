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

		int[] rOriginal;
		int[] gOriginal;
		int[] bOriginal;

		int[] r;
		int[] g;
		int[] b;
		int[] rgb;

		int[] rPixel;
		int[] gPixel;
		int[] bPixel;

		int[] rDistribution;
		int[] gDistribution;
		int[] bDistribution;

		Bitmap img;
		Bitmap sourceImage;
		MainWindow mainWindow;

		public HistogramWindow(MainWindow mainWindow) {
			InitializeComponent();
			this.mainWindow = mainWindow;
			this.sourceImage = mainWindow.img;
			this.img = this.sourceImage;
			fillRgbArrays(sourceImage);
			initializeChart();
		}

		private void equalizeButton_Click(object sender, RoutedEventArgs e) {
			setCumulativeDistribution();
			double newR, newG, newB;
			int currentPixel;
			System.Drawing.Color color;

			r = new int[256];
			g = new int[256];
			b = new int[256];

			for(int i = 0; i < img.Width; i++) {
				for(int j = 0; j < img.Height; j++) {
					currentPixel = i * img.Width + j;
					newR = rPixel[currentPixel];
					newG = gPixel[currentPixel];
					newB = bPixel[currentPixel];

					newR = (double)(rDistribution[rPixel[currentPixel]] - rDistribution[0]) / (double)(img.Width * img.Height) * 255;
					newG = (double)(gDistribution[gPixel[currentPixel]] - gDistribution[0]) / (double)(img.Width * img.Height) * 255;
					newB = (double)(bDistribution[bPixel[currentPixel]] - bDistribution[0]) / (double)(img.Width * img.Height) * 255;

					color = System.Drawing.Color.FromArgb((int)newR, (int)newG, (int)newB);

					r[(int)newR]++;
					g[(int)newG]++;
					b[(int)newB]++;
					
					img.SetPixel(i, j, color);
				}
				mainWindow.image.Source = MainWindow.BitmapToImageSource(img);
			}

			separatedHistograms();

		}

		public void setCumulativeDistribution() {
			int a = img.Width * img.Height;
			rDistribution = new int[256];
			gDistribution = new int[256];
			bDistribution = new int[256];
			rDistribution[0] = r[0];
			gDistribution[0] = g[0];
			bDistribution[0] = b[0];
			for (int i = 1; i < 256; i++) {
				rDistribution[i] = rDistribution[i - 1] + r[i];
				gDistribution[i] = gDistribution[i - 1] + g[i];
				bDistribution[i] = bDistribution[i - 1] + b[i];
			}
		}

		public void fillRgbArrays(Bitmap bitmap) {
			rOriginal = new int[256];
			gOriginal = new int[256];
			bOriginal = new int[256];

			rgb = new int[256];
			r = new int[256];
			g = new int[256];
			b = new int[256];

			rPixel = new int[bitmap.Width * bitmap.Height];
			gPixel = new int[bitmap.Width * bitmap.Height];
			bPixel = new int[bitmap.Width * bitmap.Height];
			int sum;
			for (int i = 0; i < bitmap.Width; i++) {
				for (int j = 0; j < bitmap.Height; j++) {
					System.Drawing.Color color = bitmap.GetPixel(i, j);
					r[color.R]++;
					g[color.G]++;
					b[color.B]++;

					rOriginal[color.R]++;
					gOriginal[color.R]++;
					bOriginal[color.R]++;

					rPixel[i * bitmap.Width + j] = color.R;
					gPixel[i * bitmap.Width + j] = color.G;
					bPixel[i * bitmap.Width + j] = color.B;

					sum = (color.R + color.G + color.B) / 3;
					rgb[sum]++;
				}
			}
		}

		public void initializeChart() {
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

		public void separatedHistograms() {

			SeriesCollection.Clear();
			SeriesCollection.Add(
				new LineSeries() {
					Title = "Red",
					Values = new ChartValues<int>(r.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Red
				});

			SeriesCollection.Add(
				new LineSeries() {
					Title = "Green",
					Values = new ChartValues<int>(g.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Green
				});
			SeriesCollection.Add(
				new LineSeries() {
					Title = "Blue",
					Values = new ChartValues<int>(b.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Blue
				});
			//also adding values updates and animates the chart automatically
			YFormatter = value => value.ToString("N");
			var stringList = new List<String>();
			for (int i = 0; i < 256; i++) {
				stringList.Add(i.ToString());
			}
			Labels = stringList.ToArray();

			DataContext = this;
		}

		public void overallHistogram() {

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
			int A = Int32.Parse(stretchA.Text);
			int B = Int32.Parse(stretchB.Text);
			int min = 0;
			int max = 255;

			int[] rLut = getLut(rPixel, A, B);
			int[] gLut = getLut(gPixel, A, B);
			int[] bLut = getLut(bPixel, A, B);
			r = rLut;
			g = gLut;
			b = bLut;

			SeriesCollection.Clear();
			SeriesCollection.Add(
				new LineSeries() {
					Title = "Red",
					Values = new ChartValues<int>(r.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Red
				});

			SeriesCollection.Add(
				new LineSeries() {
					Title = "Green",
					Values = new ChartValues<int>(g.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Green
				});
			SeriesCollection.Add(
				new LineSeries() {
					Title = "Blue",
					Values = new ChartValues<int>(b.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Blue
				});
			//also adding values updates and animates the chart automatically
			YFormatter = value => value.ToString("N");
			var stringList = new List<String>();
			for (int i = 0; i < 256; i++) {
				stringList.Add(i.ToString());
			}
			Labels = stringList.ToArray();

			DataContext = this;

		}

		private int[] getLut(int[] arr, int A, int B) {

			int[] resultArr = new int[256];
			int index;

			for (int i = 0; i < 256; i++) {
				index = ((i * (B - A)) / 256) + A;
				resultArr[i] = arr[index];
			}
			return resultArr;
		}

	}
}
