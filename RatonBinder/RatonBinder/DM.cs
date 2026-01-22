using DarkModeForms;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Raton.Classes
{
    public class DarkMode
    {
        private static readonly DarkModeCS.DisplayMode lightMode = DarkModeCS.DisplayMode.ClearMode;
        private static readonly DarkModeCS.DisplayMode darkMode = DarkModeCS.DisplayMode.DarkMode;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        private const int DWMWA_BORDER_COLOR = 34;

        public static void Start(Form form)
        {
            bool darkModeBool = true;
            if (darkModeBool == true)
            {
                DarkModeCS meow = new DarkModeCS(form)
                {
                    ColorMode = darkMode,
                    ColorizeIcons = false,
                };
            }
            Color borderColor = Color.FromArgb(174, 127, 226);
            SetBorderColor(form, borderColor);
        }
        private static void SetBorderColor(Form form, Color color)
        {
            int colorValue = color.R | (color.G << 8) | (color.B << 16);
            DwmSetWindowAttribute(form.Handle, DWMWA_BORDER_COLOR, ref colorValue, sizeof(int));
        }
    }
}