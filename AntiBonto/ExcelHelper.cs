using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AntiBonto
{
    public class ExcelHelper
    {
        /// <summary>
        /// Opens Excel in the background and reads available data about participants.
        /// 
        /// Does not use OpenXML SDK because this way we can support the old binary formats too.
        /// Also the amount of code needed is vastly smaller this way.
        /// </summary>
        /// <returns>a list of people</returns>
        public static List<Person> LoadXLS(string filename)
        {
            var excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook file = excel.Workbooks.Open(filename);
            try
            {
                bool isHVKezelo = file.Worksheets.Cast<Worksheet>().Any(s => s.Name == "Alapadatok");
                Worksheet sheet = isHVKezelo ? file.Worksheets["Alapadatok"] : file.Worksheets[1];
                sheet.Unprotect();
                Range range = sheet.UsedRange,
                 col1 = range.Columns[1],
                 col2 = range.Columns[2];
                List<Person> ppl = new List<Person>();
                foreach (string val in col1.Value)
                    ppl.Add(new Person { Name = val });
                if (col1.Count == col2.Count)
                {
                    int i = 0;
                    foreach (string n in col2.Value)
                        ppl[i++].Name += " " + n;
                }
                ppl.RemoveAll(s => String.IsNullOrWhiteSpace(s.Name));
                Person fiuvezeto = null, lanyvezeto = null;
                if (isHVKezelo || MessageBox.Show("Hétvége kezelő formátum?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    int i = 0;
                    foreach (var s in range.Columns[3].Value)
                    {// Nicknames
                        if (i >= ppl.Count)
                            break;
                        ppl[i++].Nickname = s;
                    }
                    i = 0;
                    foreach (var s in range.Columns[4].Value)
                    {// Type of participant
                        if (i >= ppl.Count)
                            break;
                        int x = 0;
                        if (s is string)
                            Int32.TryParse(s, out x);
                        else if (s is double || s is int)
                            x = (int)s;
                        if (Enum.IsDefined(typeof(PersonType), x))
                        {
                            var type = (PersonType)x;
                            ppl[i].Type = type;
                            if (type == PersonType.Lanyvezeto)
                            {
                                ppl[i].Nem = Nem.Lany;
                                lanyvezeto = ppl[i];
                            }
                            else if (type == PersonType.Fiuvezeto)
                            {
                                ppl[i].Nem = Nem.Fiu;
                                fiuvezeto = ppl[i];
                            }
                        }
                        i++;
                    }
                    i = 0;
                    foreach (var s in range.Columns[5].Value)
                    {// Sharing group
                        if (i >= ppl.Count)
                            break;
                        int x = 0;
                        if (s is string)
                            Int32.TryParse(s, out x);
                        else if (s is double || s is int)
                            x = (int)s;
                        if (x != 0)
                            ppl[i].Kiscsoport = x - 1;
                        i++;
                    }
                    i = 0;
                    foreach (var s in range.Columns[6].Value)
                    {// Sharing group leader?
                        if (i >= ppl.Count)
                            break;
                        if (s != null && s.ToString() != "")
                            ppl[i].Kiscsoportvezeto = true;
                        i++;
                    }
                    i = 0;
                    foreach (var s in range.Columns[7].Value)
                    {// Sleeping group
                        if (i >= ppl.Count)
                            break;
                        int x = -1;
                        if (s != null && s is string)
                            x = Encoding.ASCII.GetBytes(s)[0] - 65;
                        if (x != -1)
                            ppl[i].Alvocsoport = x;
                        i++;
                    }
                    i = 0;
                    foreach (var s in range.Columns[8].Value)
                    {// Sleeping group leader?
                        if (i >= ppl.Count)
                            break;
                        if (s != null && s.ToString() != "")
                            ppl[i].Alvocsoportvezeto = true;
                        i++;
                    }
                }
                if (ppl[0].Name.Contains("név"))
                    ppl.RemoveAt(0);
                if (fiuvezeto != null && fiuvezeto.Alvocsoport != -1)
                    foreach (var p in ppl.Where(q => q.Alvocsoport == fiuvezeto.Alvocsoport))
                        p.Nem = Nem.Fiu;
                if (lanyvezeto != null && lanyvezeto.Alvocsoport != -1)
                    foreach (var p in ppl.Where(q => q.Alvocsoport == lanyvezeto.Alvocsoport))
                        p.Nem = Nem.Lany;
                return ppl;
            }
            finally
            {
                file.Close(false);
                excel.Quit();
            }
        }

        public static void SaveXLS(string filename, ViewModel.MainWindow data)
        {
            Uri uri = new Uri("/Resources/hetvegekezelo.xlsm", UriKind.Relative);

            using (var stream = System.Windows.Application.GetResourceStream(uri).Stream)
            using (var f = File.Create(filename))
            {
                stream.CopyTo(f);
            }
            var excel = new Microsoft.Office.Interop.Excel.Application { Visible = true };
            Workbook file = excel.Workbooks.Open(filename);
            try
            {
                Worksheet sheet = file.Worksheets["Vezérlő adatok"];
                sheet.Cells[2, 2] = ViewModel.MainWindow.WeekendNumber;

                sheet = file.Worksheets["Alvócsoport címek"];
                var m = data.Kiscsoportvezetok.Count();
                for (int j = 1; j <= m; j++)
                    sheet.Cells[j + 1, 1] = ((char)(j + 64)).ToString();

                sheet = file.Worksheets["Alapadatok"];
                sheet.Activate();
                sheet.Unprotect();
                Range c = sheet.Cells;
                int i = 2;
                foreach (Person p in data.People)
                {
                    c[i, 1].Activate();
                    string[] nev = p.Name.Split(new Char[] { ' ' }, 2);
                    c[i, 1] = nev[0];
                    c[i, 2] = nev[1];
                    c[i, 3] = p.Nickname;
                    if (p.Type != PersonType.Teamtag)
                        c[i, 4] = (int) p.Type;
                    if (p.Type != PersonType.Egyeb)
                    {
                        c[i, 5] = p.Kiscsoport + 1;
                        if (p.Kiscsoportvezeto)
                            c[i, 6] = p.Kiscsoport + 1;

                        c[i, 7] = ((char)(p.Alvocsoport + 65)).ToString();
                        if (p.Alvocsoportvezeto)
                            c[i, 8] = ((char)(p.Alvocsoport + 65)).ToString();
                    }

                    if (ViewModel.MainWindow.WeekendNumber == 20)
                    {
                        if (data.Szentendre.Contains(p))
                            c[i, 9] = "Szentendre";
                        if (data.MutuallyExclusiveGroups[0].Contains(p))
                            c[i, 9] = "Zugliget";
                    }
                    i++;
                }
            }
            finally
            {
                file.Save();
            }
        }
    }
}

