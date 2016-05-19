using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AntiBonto.Tests
{
    [TestClass()]
    public class ExcelHelperTests
    {
        [TestMethod()]
        public void LoadXLSTest()
        {
            foreach(string file in Directory.GetFiles(@"D:\Marci\Programozás\AntiBonto\tests\")) 
            {
                ExcelHelper.LoadXLS(file);
            }
        }
    }
}