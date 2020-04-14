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
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;

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

		// 0 for separated histograms
		// 1 for overall histogram
		int mode;

		Bitmap img;
		Bitmap sourceImage;
		MainWindow mainWindow;

		public HistogramWindow(MainWindow mainWindow) {
			InitializeComponent();
			this.mainWindow = mainWindow;
			this.sourceImage = mainWindow.img;
			this.img = this.sourceImage;
			fillRgbArrays(this.img);
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
			rgb = new int[256];

			for (int i = 0; i < img.Width; i++) {
				for (int j = 0; j < img.Height; j++) {
					currentPixel = i * img.Width + j;

					newR = (double)(rDistribution[rPixel[currentPixel]] - rDistribution[0]) / (double)(img.Width * img.Height) * 255;
					newG = (double)(gDistribution[gPixel[currentPixel]] - gDistribution[0]) / (double)(img.Width * img.Height) * 255;
					newB = (double)(bDistribution[bPixel[currentPixel]] - bDistribution[0]) / (double)(img.Width * img.Height) * 255;

					newR = Math.Round((double)newR);
					newG = Math.Round((double)newG);
					newB = Math.Round((double)newB);

					color = System.Drawing.Color.FromArgb((int)newR, (int)newG, (int)newB);

					r[(int)newR]++;
					g[(int)newG]++;
					b[(int)newB]++;
					rgb[(int)((newR + newG + newB) / 3)]++;

					img.SetPixel(i, j, color);
				}
				mainWindow.image.Source = MainWindow.BitmapToImageSource(img);
			}

			//for (int i = 1; i < 256; i++) {
			//	if (r[i] == 0) r[i] = r[i - 1];
			//	if (g[i] == 0) g[i] = g[i - 1];
			//	if (b[i] == 0) b[i] = b[i - 1];
			//}

			if (mode == 0) separatedHistograms();
			else if (mode == 1) overallHistogram();
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

			SeriesCollection.Clear();
			SeriesCollection.Add(
						new LineSeries() {
							Title = "Summed RGB values",
							Values = new ChartValues<int>(rgb.ToArray()),
							PointGeometry = null
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

		private void stretchButton_Click(object sender, RoutedEventArgs e) {

			if (String.IsNullOrEmpty(stretchA.Text)) {
				stretchA.BorderBrush = Brushes.Red;
				return;
			}
			else stretchA.BorderBrush = Brushes.Black;
			if (String.IsNullOrEmpty(stretchB.Text)) {
				stretchB.BorderBrush = Brushes.Red;
				return;
			}
			else stretchA.BorderBrush = Brushes.Black;

			int A = Int32.Parse(stretchA.Text);
			int B = Int32.Parse(stretchB.Text);
			int min = 0;
			int max = 255;

			if (A < 0 || A > 255) {
				stretchA.Foreground = Brushes.Red;
				return;
			}
			else stretchA.Foreground = Brushes.Black;

			if (B < 0 || B > 255) {
				stretchB.Foreground = Brushes.Red;
				return;
			}
			else stretchB.Foreground = Brushes.Black;

			int[] rLut = getLut(rPixel, A, B);
			int[] gLut = getLut(gPixel, A, B);
			int[] bLut = getLut(bPixel, A, B);
			r = new int[256];
			g = new int[256];
			b = new int[256];
			//r = rLut;
			//g = gLut;
			//b = bLut;

			for (int x = 0; x < img.Width; x++) {
				for (int y = 0; y < img.Height; y++) {
					Color pixel = img.GetPixel(x, y);
					Color newPixel = Color.FromArgb(rLut[pixel.R], gLut[pixel.G], bLut[pixel.B]);
					r[newPixel.R]++;
					g[newPixel.G]++;
					b[newPixel.B]++;
					img.SetPixel(x, y, newPixel);
				}
			}
			mainWindow.image.Source = MainWindow.BitmapToImageSource(img);

			if (mode == 0) separatedHistograms();
			else overallHistogram();

		}

		private int[] getLut(int[] arr, int A, int B) {

			int[] resultArr = new int[256];
			int index;

			//for (int i = 0; i < 256; i++) {
			//	index = ((i * (B - A)) / 256) + A;
			//	resultArr[i] = arr[index];
			//}
			//return resultArr;

			for (int i = 0; i < 256; i++) {
				resultArr[i] = (255 / (B - A)) * (i - A);
				if (resultArr[i] > 255) resultArr[i] = 255;
				else if (resultArr[i] < 0) resultArr[i] = 0;
			}
			return resultArr;
		}

		public void firstSeparatedHistogram() {
			mode = 0;
			SeriesCollection = new SeriesCollection {
				new LineSeries {
					Title = "Red",
					Values = new ChartValues<int>(r.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Red
				},
				new LineSeries {
					Title = "Green",
					Values = new ChartValues<int>(g.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Green
				},
				new LineSeries {
					Title = "Blue",
					Values = new ChartValues<int>(b.ToArray()),
					PointGeometry = null,
					Stroke = System.Windows.Media.Brushes.Blue
				}

			};

			//also adding values updates and animates the chart automatically
			YFormatter = value => value.ToString("N");
			var stringList = new List<String>();
			for (int i = 0; i < 256; i++) {
				stringList.Add(i.ToString());
			}
			Labels = stringList.ToArray();

			DataContext = this;
		}

		public void firstOverallHistogram() {
			mode = 1;
			SeriesCollection = new SeriesCollection
			{
				new LineSeries
				{
					Title = "Summed RGB values",
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

		private void lightenButton_Click(object sender, RoutedEventArgs e) {

			r = new int[256];
			g = new int[256];
			b = new int[256];
			int newR, newG, newB;

			for (int x = 0; x < img.Width; x++) {
				for (int y = 0; y < img.Height; y++) {

					Color pixel = img.GetPixel(x, y);

					newR = (int)Math.Pow(pixel.R, 1.05);
					newG = (int)Math.Pow(pixel.G, 1.05);
					newB = (int)Math.Pow(pixel.B, 1.05);

					if (newR > 255) newR = 255;
					if (newR < 0) newR = 0;
					if (newG > 255) newG = 255;
					if (newG < 0) newG = 0;
					if (newB > 255) newB = 255;
					if (newB < 0) newB = 0;

					Color newPixel = Color.FromArgb(newR, newG, newB);

					r[newPixel.R]++;
					g[newPixel.G]++;
					b[newPixel.B]++;

					img.SetPixel(x, y, newPixel);
				}
			}
			mainWindow.image.Source = MainWindow.BitmapToImageSource(img);

			if (mode == 0) separatedHistograms();
			else overallHistogram();
		}

		private void darkenButton_Click(object sender, RoutedEventArgs e) {
			r = new int[256];
			g = new int[256];
			b = new int[256];
			int newR, newG, newB;

			for (int x = 0; x < img.Width; x++) {
				for (int y = 0; y < img.Height; y++) {
					Color pixel = img.GetPixel(x, y);
					newR = (int)Math.Pow(pixel.R, 0.95);
					newG = (int)Math.Pow(pixel.G, 0.95);
					newB = (int)Math.Pow(pixel.B, 0.95);

					if (newR > 255) newR = 255;
					if (newR < 0) newR = 0;
					if (newG > 255) newG = 255;
					if (newG < 0) newG = 0;
					if (newB > 255) newB = 255;
					if (newB < 0) newB = 0;

					Color newPixel = Color.FromArgb(newR, newG, newB);
					r[newPixel.R]++;
					g[newPixel.G]++;
					b[newPixel.B]++;
					img.SetPixel(x, y, newPixel);
				}
			}
			mainWindow.image.Source = MainWindow.BitmapToImageSource(img);

			if (mode == 0) separatedHistograms();
			else overallHistogram();
		}
	}
}
