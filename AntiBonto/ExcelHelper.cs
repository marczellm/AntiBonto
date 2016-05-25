using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiBonto
{
    public class ExcelHelper
    {
        /// <summary>
        /// Háttérben megnyitja az Excelt és kiolvassa a résztvevők adatait.
        /// </summary>
        /// <returns>a beolvasott emberek listáját</returns>
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
                var r = new Random();
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
                    Range col4 = range.Columns[4];
                    int i = 0;
                    foreach (var s in col4.Value)
                    {
                        if (i >= ppl.Count)
                            break;
                        int x=0;
                        if (s is string)
                            Int32.TryParse(s, out x);
                        else if (s is double || s is int)
                            x = (int)s;
                        ppl[i++].Type = (PersonType)x;
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
    }
}
