using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;

namespace AntiBonto
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadXLS(object sender, RoutedEventArgs e)
        {
            var excelInstalled = Type.GetTypeFromProgID("Excel.Application");
            if (excelInstalled == null)
            {
                MessageBox.Show("Excel nincs telepítve!");
                return;
            }
            var dialog = new OpenFileDialog
            {
                Filter = "Excel|*.xls;*.xlsx;*.xlsm",
                DereferenceLinks = true,
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (dialog.ShowDialog(this) != true)
                return;
            const int xlLastCell = 11;
            dynamic excel = Activator.CreateInstance(excelInstalled),
                file = excel.Workbooks.Open(dialog.FileName),
                sheet = file.Sheets(1).Range("A1", excel.ActiveCell.SpecialCells(xlLastCell)),
                col1 = sheet.Columns(1),
                col2 = sheet.Columns(2);
            excel.Visible = true;
            List<string> names = new List<string>();
            foreach (string val in col1.Value)
                names.Add(val);
            if (col1.Count == col2.Count)
            {
                int i = 0;
                foreach (string n in col2.Value)
                    names[i++] += " " + n;
            }
            foreach (string name in names)
                Console.WriteLine(name);
            file.Close(false);
            excel.Quit();
        }
    }
}
