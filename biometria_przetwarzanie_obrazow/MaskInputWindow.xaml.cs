using System;
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
using System.Windows.Shapes;

namespace biometria_przetwarzanie_obrazow {
	/// <summary>
	/// Interaction logic for MaskInputWindow.xaml
	/// </summary>
	public partial class MaskInputWindow : Window {

		int[,] mask;
		MainWindow mainWindow;

		public MaskInputWindow(MainWindow mainWindow) {
			InitializeComponent();
			this.mainWindow = mainWindow;
		}

		private void saveMask_Click(object sender, RoutedEventArgs e) {
			//int size = Int32.Parse(matrixSize.Text);
			mask = new int[3, 3];

			//if (size == 3) {
				mask[0, 0] = Int32.Parse(mask11.Text);
				mask[0, 1] = Int32.Parse(mask21.Text);
				mask[0, 2] = Int32.Parse(mask31.Text);

				mask[1, 0] = Int32.Parse(mask12.Text);
				mask[1, 1] = Int32.Parse(mask22.Text);
				mask[1, 2] = Int32.Parse(mask32.Text);

				mask[2, 0] = Int32.Parse(mask13.Text);
				mask[2, 1] = Int32.Parse(mask23.Text);
				mask[2, 2] = Int32.Parse(mask33.Text);
			//}
			//else if (size == 5) {
			//	mask[0, 0] = Int32.Parse(mask11.Text);
			//	mask[1, 0] = Int32.Parse(mask21.Text);
			//	mask[2, 0] = Int32.Parse(mask31.Text);
			//	//mask[3, 0] = Int32.Parse(mask41.Text);
			//	//mask[4, 0] = Int32.Parse(mask51.Text);

			//	mask[0, 1] = Int32.Parse(mask12.Text);
			//	mask[1, 1] = Int32.Parse(mask22.Text);
			//	mask[2, 1] = Int32.Parse(mask32.Text);
			//	//mask[3, 1] = Int32.Parse(mask42.Text);
			//	//mask[4, 1] = Int32.Parse(mask52.Text);

			//	mask[0, 2] = Int32.Parse(mask13.Text);
			//	mask[1, 2] = Int32.Parse(mask23.Text);
			//	mask[2, 2] = Int32.Parse(mask33.Text);
			//	//mask[3, 2] = Int32.Parse(mask43.Text);
			//	//mask[4, 2] = Int32.Parse(mask53.Text);

			//	//mask[0, 3] = Int32.Parse(mask14.Text);
			//	//mask[1, 3] = Int32.Parse(mask24.Text);
			//	//mask[2, 3] = Int32.Parse(mask34.Text);
			//	//mask[3, 3] = Int32.Parse(mask44.Text);
			//	//mask[4, 3] = Int32.Parse(mask54.Text);
			
			//	//mask[0, 4] = Int32.Parse(mask15.Text);
			//	//mask[1, 4] = Int32.Parse(mask25.Text);
			//	//mask[2, 4] = Int32.Parse(mask35.Text);
			//	//mask[3, 4] = Int32.Parse(mask45.Text);
			//	//mask[4, 4] = Int32.Parse(mask55.Text);
			//}

			mainWindow.mask = mask;
			this.Close();
		}

	}

}
