using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dyd.BaseService.TaskManager.Node
{
    public static class WinApi
    {


        [DllImport("user32.dll")]
       public   static extern int SetWindowText(IntPtr hWnd, string text);
    }
}
