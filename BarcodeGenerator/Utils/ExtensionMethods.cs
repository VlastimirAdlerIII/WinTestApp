using System;
using System.Collections;
using System.IO;
using System.Drawing;
using SDI = System.Drawing.Imaging;
using System.Text;
using AutoCont.Barcodes.Codec;
using System.Runtime.InteropServices;
using QRCodeUtility = AutoCont.Barcodes.Codec.Util;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// rozšiřující funkce
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// zjišťuje, zda je zadaný řetězec celý číselný
        /// zvolený způsob vyloučí znaky jako je desetinná tečka, plus, mínus, mezera ... které nebrání parsování stringu jako čísla
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public static bool IsNumeric(this String Number)
        {
            try
            {
                bool result = true;
                for (int i = 0; i < Number.Length; i++)
                {
                    result &= ((int)Number[i] >= 48 && (int)Number[i] <= 57);
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// odstraní diakritiku
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(this String s)
        {
            // oddělení znaků od modifikátorů (háčků, čárek, atd.)
            s = s.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                // do řetězce přidá všechny znaky kromě modifikátorů
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(s[i]) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[i]);
                }
            }

            // vrátí řetězec bez diakritiky
            return sb.ToString();
        }

        /// <summary>
        /// převede bitmapy na monochromatickou (1bit)
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap BitmapTo1Bpp(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap bmp = new Bitmap(w, h, SDI.PixelFormat.Format1bppIndexed);
            SDI.BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), SDI.ImageLockMode.ReadWrite, SDI.PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                byte[] scan = new byte[(w + 7) / 8];
                for (int x = 0; x < w; x++)
                {
                    Color c = img.GetPixel(x, y);
                    if (c.GetBrightness() >= 0.5) scan[x / 8] |= (byte)(0x80 >> (x % 8));
                }
                Marshal.Copy(scan, 0, (IntPtr)((int)data.Scan0 + data.Stride * y), scan.Length);
            }
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
