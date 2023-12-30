using AntiBonto.ViewModel;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Microsoft.Office.Interop.Excel.Range range = sheet.UsedRange,
                 col1 = range.Columns[1],
                 col2 = range.Columns[2];
                List<Person> ppl = new();
                foreach (string val in col1.Value)
                    ppl.Add(new Person { Name = val });
                if (col1.Count == col2.Count)
                {
                    int i = 0;
                    foreach (string n in col2.Value)
                        ppl[i++].Name += " " + n;
                }
                ppl.RemoveAll(s => String.IsNullOrWhiteSpace(s.Name));
                Person boyLeader = null, girlLeader = null;
                if (isHVKezelo || MessageBox.Show("Hétvége kezelő formátum?", "AntiBonto", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                            if (type == PersonType.GirlLeader)
                            {
                                ppl[i].Sex = Sex.Girl;
                                girlLeader = ppl[i];
                            }
                            else if (type == PersonType.BoyLeader)
                            {
                                ppl[i].Sex = Sex.Boy;
                                boyLeader = ppl[i];
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
                            ppl[i].SharingGroup = x - 1;
                        i++;
                    }
                    i = 0;
                    foreach (var s in range.Columns[6].Value)
                    {// Sharing group leader?
                        if (i >= ppl.Count)
                            break;
                        if (s != null && s.ToString() != "")
                            ppl[i].SharingGroupLeader = true;
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
                            ppl[i].SleepingGroup = x;
                        i++;
                    }
                    i = 0;
                    foreach (var s in range.Columns[8].Value)
                    {// Sleeping group leader?
                        if (i >= ppl.Count)
                            break;
                        if (s != null && s.ToString() != "")
                            ppl[i].SleepingGroupLeader = true;
                        i++;
                    }
                }
                if (ppl[0].Name.Contains("név"))
                    ppl.RemoveAt(0);
                if (boyLeader != null && boyLeader.SleepingGroup != -1)
                    foreach (var p in ppl.Where(q => q.SleepingGroup == boyLeader.SleepingGroup))
                        p.Sex = Sex.Boy;
                if (girlLeader != null && girlLeader.SleepingGroup != -1)
                    foreach (var p in ppl.Where(q => q.SleepingGroup == girlLeader.SleepingGroup))
                        p.Sex = Sex.Girl;
                return ppl;
            }
            finally
            {
                file.Close(false);
                excel.Quit();
            }
        }

        public static async Task SaveXLS(string filename, ViewModel.MainWindow data)
        {
            Uri uri = new("/Resources/hetvegekezelo.xlsm", UriKind.Relative);
            var acsn = data.SleepingGroupLeaders.Count();
            var sleepingGroupTitles = data.SleepingGroups.Select(group => ((TitledCollectionView)group).Title).ToList();
            var people = data.People.ToList();

            await Task.Run(() =>
            {
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
                    for (int j = 1; j <= acsn; j++)
                    {
                        sheet.Cells[j + 1, 1] = ((char)(j + 64)).ToString();
                        sheet.Cells[j + 1, 2] = sleepingGroupTitles[j - 1];
                    }

                    sheet = file.Worksheets["Alapadatok"];
                    sheet.Activate();
                    sheet.Unprotect();
                    Microsoft.Office.Interop.Excel.Range c = sheet.Cells;
                    int i = 2;
                    foreach (Person p in people)
                    {
                        c[i, 1].Activate();
                        string[] nev = p.Name.Split(new Char[] { ' ' }, 2);
                        c[i, 1] = nev[0];
                        c[i, 2] = nev[1];
                        c[i, 3] = p.Nickname;
                        if (p.Type != PersonType.Team)
                            c[i, 4] = (int)p.Type;
                        if (p.Type != PersonType.Others)
                        {
                            c[i, 5] = p.SharingGroup + 1;
                            if (p.SharingGroupLeader)
                                c[i, 6] = p.SharingGroup + 1;

                            c[i, 7] = ((char)(p.SleepingGroup + 65)).ToString();
                            if (p.SleepingGroupLeader)
                                c[i, 8] = ((char)(p.SleepingGroup + 65)).ToString();
                        }
                        i++;
                    }
                }
                finally
                {
                    file.Save();
                }
            });
        }
    }
}

