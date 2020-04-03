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
		private void loadImageButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog open = new OpenFileDialog();
			open.Title = "Select a picture";
			open.Filter = "All supported graphics|*.jpg;*.png;*.bmp;*.gif;*.tiff|JPG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp|Gif Image|*.gif|TIFF Image|*.tiff|TIFF Image|*.tif";
			if (open.ShowDialog() == true) {

				img = new Bitmap(open.FileName);
				originalImage.Source = BitmapToImageSource(img);
				image.Source = BitmapToImageSource(img);

				//img = this.CreateNonIndexedImage(img);
				MemoryStream MS = new MemoryStream();
				img.Save(MS, System.Drawing.Imaging.ImageFormat.Jpeg);
				img = new Bitmap(MS);

				rTextBox.Text = "0";
				gTextBox.Text = "0";
				bTextBox.Text = "0";
				zoomSlider.Value = 5;

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
			HistogramWindow histogramWindow = new HistogramWindow(this);
			histogramWindow.separatedHistograms();
			histogramWindow.Show();
		}

		private void overallHistogram_Click(object sender, RoutedEventArgs e) {
			HistogramWindow histogramWindow = new HistogramWindow(this);
			histogramWindow.overallHistogram();
			histogramWindow.Show();
		}
	}


}
