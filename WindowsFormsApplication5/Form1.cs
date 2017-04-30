using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

    /* 參考網址 : https://dotblogs.com.tw/huanlin/2008/04/23/3320 */

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        const int WH_KEYBOARD_LL = 13;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static int m_HookHandle = 0;    // Hook handle
        private HookProc m_KbdHookProc;            // 鍵盤掛鉤函式指標

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // 設置掛鉤.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        // 將之前設置的掛鉤移除。記得在應用程式結束前呼叫此函式.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // 呼叫下一個掛鉤處理常式（若不這麼做，會令其他掛鉤處理常式失效）.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_HookHandle == 0)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    m_KbdHookProc = new HookProc(Form1.KeyboardHookProc);

                    m_HookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, m_KbdHookProc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }

                if (m_HookHandle == 0)
                {
                    MessageBox.Show("呼叫 SetWindowsHookEx 失敗!");
                    return;
                }
                button1.Text = "解除鍵盤熱鍵";
                Status_label.Text = "作用中";
                Status_label.ForeColor = Color.Green;
            }
            else
            {
                bool ret = UnhookWindowsHookEx(m_HookHandle);
                if (ret == false)
                {
                    MessageBox.Show("呼叫 UnhookWindowsHookEx 失敗!");
                    return;
                }
                m_HookHandle = 0;
                button1.Text = "設置鍵盤熱鍵";
                Status_label.Text = "待命";
                Status_label.ForeColor = Color.Red;
            }
        }


        public static int KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            // 當按鍵按下及鬆開時都會觸發此函式，這裡只處理鍵盤按下的情形。
            bool isPressed = (lParam.ToInt32() & 0x80000000) == 0;

            if (nCode < 0 || !isPressed)
            {
                return CallNextHookEx(m_HookHandle, nCode, wParam, lParam);
            }

            /* 取得欲攔截之按鍵狀態 */
            //KeyStateInfo ctrlKey = KeyboardInfo.GetKeyState(Keys.ControlKey);
            //KeyStateInfo altKey = KeyboardInfo.GetKeyState(Keys.Alt);
            //KeyStateInfo shiftKey = KeyboardInfo.GetKeyState(Keys.ShiftKey);
            KeyStateInfo f8Key = KeyboardInfo.GetKeyState(Keys.F8);

            /*if (ctrlKey.IsPressed)
            {
                System.Diagnostics.Debug.WriteLine("Ctrl Pressed!");
            }
            if (altKey.IsPressed)
            {
                System.Diagnostics.Debug.WriteLine("Alt Pressed!");
            }*/
            /*if (shiftKey.IsPressed)
            {
                Process notePad = new Process();
                // FileName 是要執行的檔案
                notePad.StartInfo.FileName = "notepad.exe";
                notePad.Start();
            }*/
            if (f8Key.IsPressed)
            {
                Process[] processes = Process.GetProcessesByName("GTA5");

                foreach (Process p in processes)
                {
                    // 關閉目前程序前先等待 100 毫秒
                    //p.WaitForExit(100);
                    //p.CloseMainWindow();
                    p.Kill();
                    //p.Close();
                }
            }

            return CallNextHookEx(m_HookHandle, nCode, wParam, lParam);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }

    public class KeyboardInfo
    {
        private KeyboardInfo() { }

        [DllImport("user32")]
        private static extern short GetKeyState(int vKey);

        public static KeyStateInfo GetKeyState(Keys key)
        {
            int vkey = (int)key;

            if (key == Keys.Alt)
            {
                vkey = 0x12;    // VK_ALT
            }

            short keyState = GetKeyState(vkey);
            int low = Low(keyState);
            int high = High(keyState);
            bool toggled = (low == 1);
            bool pressed = (high == 1);

            return new KeyStateInfo(key, pressed, toggled);
        }




        private static int High(int keyState)
        {
            if (keyState > 0)
            {
                return keyState >> 0x10;
            }
            else
            {
                return (keyState >> 0x10) & 0x1;
            }

        }

        private static int Low(int keyState)
        {
            return keyState & 0xffff;
        }
    }


    public struct KeyStateInfo
    {
        Keys m_Key;
        bool m_IsPressed;
        bool m_IsToggled;

        public KeyStateInfo(Keys key, bool ispressed, bool istoggled)
        {
            m_Key = key;
            m_IsPressed = ispressed;
            m_IsToggled = istoggled;
        }

        public static KeyStateInfo Default
        {
            get
            {
                return new KeyStateInfo(Keys.None, false, false);
            }
        }

        public Keys Key
        {
            get { return m_Key; }
        }

        public bool IsPressed
        {
            get { return m_IsPressed; }
        }

        public bool IsToggled
        {
            get { return m_IsToggled; }
        }
    }


}
