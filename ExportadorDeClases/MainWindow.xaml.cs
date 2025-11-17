using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExportadorDeClases
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            var selected = ClassSelector.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            var asm = Assembly.LoadFrom(txtPathAssambly.Text);
            var type = asm.GetType(selected); // ← Esto funciona si el ensamblado está cargado
            if (type == null)
            {
                StatusBlock.Text = "No se pudo cargar el tipo. ¿Está referenciado el ensamblado?";
                return;
            }
            var format = ((ComboBoxItem)FormatBox.SelectedItem).Content.ToString();


            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{type.Name}.{format.ToLower()}");
          
            switch (format)
            {
                case "JSON": ClassExporter.ExportToJson(type, path); break;
                case "CSV": ClassExporter.ExportToCsv(type, path); break;
                case "XML": ClassExporter.ExportToXml(type, path); break;
            }


            //var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{type.Name}.json");
            //ClassExporter.ExportToJson(type, path);
            StatusBlock.Text = $"Exportado a: {path}";



        }



        private async void OnLoadClassesClick(object sender, RoutedEventArgs e)
        {
            var projectPath = txtPathProyecto.Text;

            //var projectPath = @"C:\U96\DAL\DAL.csproj"; // ← ajustá esto
            var classes = await ProjectAnalyzer.GetClassNamesAsync(projectPath);

            ClassSelector.ItemsSource = classes;
            StatusBlock.Text = $"Clases encontradas: {classes.Count}";
        }

        private void BtnAbrirCarpeta_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Filter = "Project File(*.csproj)|*.csproj|All Files(*.*)|*.*";
            openFile.CheckFileExists = true;
            openFile.Multiselect = false;
            txtPathProyecto.Text = openFile.ShowDialog() == true ? openFile.FileName : string.Empty;

        }

        private void BtnAbrirAssambly_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Filter = "Assembly File(*.dll)|*.dll|All Files(*.*)|*.*";
            openFile.CheckFileExists = true;
            openFile.Multiselect = false;
            txtPathAssambly.Text = openFile.ShowDialog() == true ? openFile.FileName : string.Empty;
        }
    }
}