using System;
using System.Runtime.InteropServices;

namespace AntiBonto
{
    class DwmAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int left;      // width of left border that retains its size  
            public int right;     // width of right border that retains its size  
            public int top;      // height of top border that retains its size  
            public int bottom;   // height of bottom border that retains its size  
        }

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

    }
}
