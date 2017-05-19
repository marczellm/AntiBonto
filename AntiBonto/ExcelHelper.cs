using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AntiBonto
{
    public class ExcelHelper
    {
        /// <summary>
        /// Opens Excel in the background and reads available data about participants.
        /// 
        /// Does not use OpenXML SDK because this way we can support the old binary formats too.
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
                if (isHVKezelo)
                {
                    int i = 0;
                    foreach (var s in range.Columns[3].Value)
                    {
                        if (i >= ppl.Count)
                            break;
                        ppl[i++].Nickname = s;
                    }
                    i = 0;
                    foreach (var s in range.Columns[4].Value)
                    {
                        if (i >= ppl.Count)
                            break;
                        int x = 0;
                        if (s is string)
                            Int32.TryParse(s, out x);
                        else if (s is double || s is int)
                            x = (int)s;
                        if (Enum.IsDefined(typeof(PersonType), x))
                            ppl[i++].Type = (PersonType)x;
                        else
                            i++;
                    }                    
                }
                if (ppl[0].Name.Contains("név"))
                    ppl.RemoveAt(0);
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

