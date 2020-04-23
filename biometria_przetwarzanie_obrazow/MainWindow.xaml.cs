using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
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
using System.Text.RegularExpressions;
using Brushes = System.Windows.Media.Brushes;
using System.Drawing;

namespace biometria_przetwarzanie_obrazow {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
			InitializeComponent();
		}

		string filePath = string.Empty;
		public Bitmap img { get; set; }
		public Bitmap imgCopy;
		int[,] r;
		int[,] g;
		int[,] b;


		private void loadImageButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog open = new OpenFileDialog();
			open.Title = "Select a picture";
			open.Filter = "All supported graphics|*.jpg;*.png;*.bmp;*.gif;*.tiff|JPG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp|Gif Image|*.gif|TIFF Image|*.tiff|TIFF Image|*.tif";
			if (open.ShowDialog() == true) {

				img = new Bitmap(open.FileName);
				imgCopy = new Bitmap(open.FileName);
				originalImage.Source = BitmapToImageSource(img);
				image.Source = BitmapToImageSource(img);

				//img = this.CreateNonIndexedImage(img);
				MemoryStream MS = new MemoryStream();
				img.Save(MS, System.Drawing.Imaging.ImageFormat.Jpeg);
				img = new Bitmap(MS);
				imgCopy = new Bitmap(MS);

				rTextBox.Text = "0";
				gTextBox.Text = "0";
				bTextBox.Text = "0";
				zoomSlider.Value = 5;

				borderOriginal.BorderBrush = Brushes.Black;
				borderOriginal.BorderThickness = new Thickness(1);

				r = new int[img.Width, img.Height];
				g = new int[img.Width, img.Height];
				b = new int[img.Width, img.Height];
			}
		}

		private void saveImageButton_Click(object sender, RoutedEventArgs e) {
			if (image.Source == null) return;
			SaveFileDialog save = new SaveFileDialog();
			save.FileName = "image";
			save.Filter = "All supported graphics|*.jpg;*.png;*.bmp;*.gif;*.tiff|JPG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp|Gif Image|*.gif|TIFF Image|*.tiff|TIFF Image|*.tif";
			save.Title = "Save and Image File";
			if (save.ShowDialog() == true) {
				var filename = save.FileName;
				JpegBitmapEncoder encoder = new JpegBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create((BitmapImage)image.Source));
				using (var filestream = new FileStream(filename, FileMode.Create))
					encoder.Save(filestream);
			}
		}

		public Bitmap CreateNonIndexedImage(System.Drawing.Image src) {
			Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics gfx = Graphics.FromImage(newBmp)) {
				gfx.DrawImage(src, 0, 0);
			}
			return newBmp;
		}

		public static BitmapImage BitmapToImageSource(Bitmap bitmap) {
			using (MemoryStream memory = new MemoryStream()) {
				bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
				memory.Position = 0;
				BitmapImage bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memory;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();
				return bitmapimage;
			}
		}

		private void image_MouseWheel(object sender, MouseWheelEventArgs e) {
			var st = (ScaleTransform)image.RenderTransform;
			double zoom = e.Delta > 0 ? .1 : -.1;
			st.ScaleX += zoom;
			st.ScaleY += zoom;
		}

		private void image_MouseMove(object sender, MouseEventArgs e) {
			if (readRgbRadioButton.IsChecked == true) {
				var position = Mouse.GetPosition(this);
				var st = (ScaleTransform)image.RenderTransform;

				System.Windows.Point relativePoint = image.TransformToAncestor(Application.Current.MainWindow).Transform(new System.Windows.Point(0, 0));

				double xx = position.X - relativePoint.X;
				double yy = position.Y - relativePoint.Y;

				xx = xx / st.ScaleX;
				yy = yy / st.ScaleY;

				xx = xx * img.Width / image.ActualWidth;
				yy = yy * img.Height / image.ActualHeight;

				System.Drawing.Color color = img.GetPixel((int)xx, (int)yy);

				rTextBox.Text = color.R.ToString();
				gTextBox.Text = color.G.ToString();
				bTextBox.Text = color.B.ToString();
			}
		}

		private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			var slider = sender as Slider;
			var st = (ScaleTransform)image.RenderTransform;
			st.ScaleX = slider.Value / 2;
			st.ScaleY = slider.Value / 2;
		}

		private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			if (setRgbRadioButton.IsChecked == true) {
				var position = Mouse.GetPosition(this);
				var st = (ScaleTransform)image.RenderTransform;

				System.Windows.Point relativePoint = image.TransformToAncestor(Application.Current.MainWindow).Transform(new System.Windows.Point(0, 0));

				double xx = position.X - relativePoint.X;
				double yy = position.Y - relativePoint.Y;
				xx = xx / st.ScaleX;
				yy = yy / st.ScaleY;
				xx = xx * img.Width / image.ActualWidth;
				yy = yy * img.Height / image.ActualHeight;

				if (String.IsNullOrEmpty(rTextBox.Text)) {
					rTextBox.BorderBrush = Brushes.Red;
					return;
				}
				else gTextBox.BorderBrush = Brushes.Black;
				if (String.IsNullOrEmpty(gTextBox.Text)) {
					gTextBox.BorderBrush = Brushes.Red;
					return;
				}
				else bTextBox.BorderBrush = Brushes.Black;
				if (String.IsNullOrEmpty(bTextBox.Text)) {
					bTextBox.BorderBrush = Brushes.Red;
					return;
				}
				else rTextBox.BorderBrush = Brushes.Black;

				int r = int.Parse(rTextBox.Text);
				int g = int.Parse(gTextBox.Text);
				int b = int.Parse(bTextBox.Text);

				if (r < 0 || r > 255) {
					rTextBox.Foreground = Brushes.Red;
					return;
				}
				else rTextBox.Foreground = Brushes.Black;

				if (g < 0 || g > 255) {
					gTextBox.Foreground = Brushes.Red;
					return;
				}
				else gTextBox.Foreground = Brushes.Black;

				if (b < 0 || b > 255) {
					bTextBox.Foreground = Brushes.Red;
					return;
				}
				else bTextBox.Foreground = Brushes.Black;

				img.SetPixel((int)xx, (int)yy, System.Drawing.Color.FromArgb(255, Int32.Parse(rTextBox.Text), Int32.Parse(gTextBox.Text), Int32.Parse(bTextBox.Text)));
				image.Source = BitmapToImageSource(img);
			}
		}

		private void rgbValueValidation(object sender, TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void makeRHistogramButton_Click(object sender, RoutedEventArgs e) {
			HistogramWindow histogramWindow = new HistogramWindow(this);
			histogramWindow.Show();
		}

		private void separatedHistograms_Click(object sender, RoutedEventArgs e) {
			if (img == null) {
				borderOriginal.BorderBrush = Brushes.Red;
				borderOriginal.BorderThickness = new Thickness(2);
				return;
			}
			else {
				borderOriginal.BorderBrush = Brushes.Black;
				borderOriginal.BorderThickness = new Thickness(1);
			}
			HistogramWindow histogramWindow = new HistogramWindow(this);
			histogramWindow.firstSeparatedHistogram();
			histogramWindow.Show();
		}

		private void overallHistogram_Click(object sender, RoutedEventArgs e) {
			if (img == null) {
				borderOriginal.BorderBrush = Brushes.Red;
				borderOriginal.BorderThickness = new Thickness(2);
				return;
			}
			else {
				borderOriginal.BorderBrush = Brushes.Black;
				borderOriginal.BorderThickness = new Thickness(1);
			}
			HistogramWindow histogramWindow = new HistogramWindow(this);
			histogramWindow.firstOverallHistogram();
			histogramWindow.Show();
		}

		private void tresholdButton_Click(object sender, RoutedEventArgs e) {
			if (String.IsNullOrEmpty(thresholdTextBox.Text)) {
				thresholdTextBox.BorderBrush = Brushes.Red;
				return;
			}
			else thresholdTextBox.BorderBrush = Brushes.Black;

			int threshold = Int32.Parse(thresholdTextBox.Text.ToString());
			if (threshold > 255 || threshold < 0) return;
			binarise(threshold);
		}

		public void binarise(int threshold) {
			int r;
			for (int i = 0; i < img.Width; i++) {
				for (int j = 0; j < img.Height; j++) {
					System.Drawing.Color color = img.GetPixel(i, j);
					r = color.R;

					if (r < threshold) r = 0;
					else r = 255;

					img.SetPixel(i, j, System.Drawing.Color.FromArgb(255, r, r, r));

				}
			}
			image.Source = BitmapToImageSource(img);
		}

		private void otsuButton_Click(object sender, RoutedEventArgs e) {
			int r, g, b, colorValue;

			// gray scale
			//for (int i = 0; i < img.Width; i++) {
			//	for (int j = 0; j < img.Height; j++) {
			//		System.Drawing.Color color = img.GetPixel(i, j);
			//		r = color.R;
			//		g = color.G;
			//		b = color.B;

			//		int meanColor = Convert.ToInt32(0.2126 * r + 0.7152 * g + 0.0722 * b);

			//		img.SetPixel(i, j, System.Drawing.Color.FromArgb(255, meanColor, meanColor, meanColor));
			//	}
			//}

			// otsu calculation
			int threshold = calculateOtsuThreshold(img);

			// binarisation
			for (int i = 0; i < img.Width; i++) {
				for (int j = 0; j < img.Height; j++) {
					System.Drawing.Color color = img.GetPixel(i, j);
					colorValue = color.R;

					if (colorValue < threshold) colorValue = 0;
					else colorValue = 255;

					img.SetPixel(i, j, System.Drawing.Color.FromArgb(255, colorValue, colorValue, colorValue));
				}
			}
			thresholdTextBox.Text = threshold.ToString();
			image.Source = BitmapToImageSource(img);
		}

		private int calculateOtsuThreshold(Bitmap bitmapImage) {

			double weightBackground, weightForeground;
			double meanBackground, meanForeground;
			int[] histogram;
			double[] varianceArray = new double[256];
			double maxVariance = 0;
			int threshold = 0;

			histogram = calculateHistogram(bitmapImage);



			for (int i = 0; i < 256; i++) {
				weightBackground = getWeightBackground(histogram, i);
				weightForeground = getWeightForeground(histogram, i);
				meanBackground = getMeanBackground(histogram, i);
				meanForeground = getMeanForeground(histogram, i);
				varianceArray[i] = getVariance(weightBackground, weightForeground, meanBackground, meanForeground);

				if (varianceArray[i] > maxVariance) {
					maxVariance = varianceArray[i];
					threshold = i;
				}
			}

			return threshold;
		}

		private double getVariance(double wb, double wf, double mb, double mf) {


			double variance = wb * wf * Math.Pow(mb - mf, 2);
			return variance;
		}

		private double getMeanBackground(int[] hist, int threshold) {
			int mb = 0;
			int sum = 0;
			for (int i = 0; i < threshold; i++) {
				mb += (i * hist[i]);
				sum += hist[i];
			}

			if(sum > 0) mb /= sum;
			return mb;
		}

		private double getMeanForeground(int[] hist, int threshold) {
			int mf = 0;
			int sum = 0;
			for (int i = threshold; i < 256; i++) {
				mf += (i * hist[i]);
				sum += hist[i];
			}
			if (sum > 0) mf /= sum;
			return mf;
		}

		private double getWeightBackground(int[] hist, int threshold) {
			double wb = 0;
			for (int i = 0; i < threshold; i++) {
				wb += hist[i];
			}
			wb /= (256*256);
			return wb;
		}
		private double getWeightForeground(int[] hist, int threshold) {
			double wb = 0;
			for (int i = threshold; i < 256; i++) {
				wb += hist[i];
			}
			wb /= (256*256);
			return wb;
		}

		private int[] calculateHistogram(Bitmap bitmapImage) {
			int[] arr = new int[256];

			int r, g, b;
			for (int i = 0; i < bitmapImage.Width; i++) {
				for (int j = 0; j < bitmapImage.Height; j++) {
					System.Drawing.Color color = bitmapImage.GetPixel(i, j);
					r = color.R;
					g = color.G;
					b = color.B;
					int meanColor = Convert.ToInt32(0.2126 * r + 0.7152 * g + 0.0722 * b);
					arr[meanColor]++;
				}
			}
			return arr;
		}

		private void grayScaleButton_Click(object sender, RoutedEventArgs e) {
			int r, g, b;
			for (int i = 0; i < img.Width; i++) {
				for (int j = 0; j < img.Height; j++) {
					System.Drawing.Color color = img.GetPixel(i, j);
					r = color.R;
					g = color.G;
					b = color.B;
					int meanColor = Convert.ToInt32(0.2126 * r + 0.7152 * g + 0.0722 * b);
					img.SetPixel(i, j, System.Drawing.Color.FromArgb(255, meanColor, meanColor, meanColor));

				}
			}
			image.Source = BitmapToImageSource(img);
		}

		private void niblackButton_Click(object sender, RoutedEventArgs e) {
			int[,] thresholdArray = new int[img.Width, img.Height];
			double weightBackground, weightForeground;
			double meanBackground, meanForeground;
			int[] histogram;
			double k = -0.5;
			int windowSize = 7;

			for (int i = 0; i < img.Width; i++) {
				for (int j = 0; j < img.Height; j++) {
					System.Drawing.Color color = img.GetPixel(i, j);
					r[i, j] = color.R;
					g[i, j] = color.G;
					b[i, j] = color.B;
				}
			}

			// histogram
			histogram = calculateHistogram(img);

			// calculate niblack thresholds
			for (int i = 0; i < img.Width; i++) {
				for(int j = 0; j < img.Height; j++) {
					thresholdArray[i, j] = calculateNiblackThreshold(i, j, histogram, k, windowSize);
				}
			}

			int colorValue;
			for (int i = 0; i < img.Width; i++) {
				for (int j = 0; j < img.Height; j++) {
					System.Drawing.Color color = img.GetPixel(i, j);
					colorValue = color.R;

					if (colorValue < thresholdArray[i, j]) colorValue = 0;
					else colorValue = 255;

					img.SetPixel(i, j, System.Drawing.Color.FromArgb(255, colorValue, colorValue, colorValue));
				}
			}
			image.Source = BitmapToImageSource(img);
		}

		private int calculateNiblackThreshold(int width, int height, int[] hist, double k, int windowSize) {

			double mean = 0; ;
			double deviation = 0;
			int spanDistance = Convert.ToInt32(windowSize / 2);
			int elements = 0;
			int threshold;

			for (int i = width - spanDistance; i < width + spanDistance; i++) {
				for(int j = height - spanDistance; j < height + spanDistance; j++) {
					if (i < 0) continue;
					if (j < 0) continue;
					if (i >= img.Width) continue;
					if (j >= img.Height) continue;
					mean += r[i, j];
					elements++;
				}
			}
			mean /= elements;

			elements = 0;
			for (int i = width - spanDistance; i < width + spanDistance; i++) {
				for (int j = height - spanDistance; j < height + spanDistance; j++) {
					if (i < 0) continue;
					if (j < 0) continue;
					if (i >= img.Width) continue;
					if (j >= img.Height) continue;
					deviation += Math.Pow((r[i, j] - mean), 2);
					elements++;
				}
			}

			deviation = Math.Sqrt(deviation / elements);
			threshold = (int)Math.Round(mean + k * deviation);

			return threshold;
		}
	}


}
