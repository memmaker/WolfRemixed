using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;

namespace XNAHelper
{
    public struct IconInfo
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    public class HardwareCursor
    {
        private readonly Game game;
        private readonly Control gameform;
        private Cursor currentCursor;

        public Cursor Current
        {
            get { return currentCursor; }
            set
            {
                currentCursor = value;
                gameform.Cursor = currentCursor;
            }
        }

        public bool IsVisible
        {
            get { return game.IsMouseVisible; }
            set { game.IsMouseVisible = value; }
        }

        public HardwareCursor(Game game)
        {
            this.game = game;
            gameform = Control.FromHandle(this.game.Window.Handle);
            currentCursor = gameform.Cursor;
        }


        public static Cursor FromFile(string curfile)
        {
            IntPtr colorCursorHandle = NativeMethods.LoadCursorFromFile(curfile);
            return new Cursor(colorCursorHandle);
        }

        public static Cursor FromBMPFile(string bmp, int xHotSpot, int yHotSpot)
        {
            var bitmap = new Bitmap(bmp);
            Color alpha = Color.FromArgb(128, 255, 0);
            bitmap.MakeTransparent(alpha);
            IntPtr ptr = bitmap.GetHicon();
            var tmp = new IconInfo();
            NativeMethods.GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = NativeMethods.CreateIconIndirect(ref tmp);
            bitmap.Dispose();
            return new Cursor(ptr);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string fileName);
    }
}