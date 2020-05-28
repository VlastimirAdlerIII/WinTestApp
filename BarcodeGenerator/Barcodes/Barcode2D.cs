using System;
using System.Drawing;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// abstraktní třída pro 2D barcodes
    /// </summary>
    public abstract class Barcode2D : Barcode
    {
        int _moduleHeightToWidthRatio;
        /// <summary>
        /// spočítá šířku horizontálních ochranných zón
        /// </summary>
        protected int HorizontalQuietZone
        {
            get
            {
                return ModuleWidth * 10;
            }
        }

        /// <summary>
        /// spočítá výšku vertikálních ochranných zón
        /// </summary>
        protected int VerticalQuietZone
        {
            get
            {
                return ModuleWidth * 10;
            }
        }

        protected int ModuleHeightToWidthRatio
        {
            get
            {
                return _moduleHeightToWidthRatio;
            }
            set
            {
                _moduleHeightToWidthRatio = value;
            }
        }
        /// <summary>
        /// vrací bitmapu s vygenerovaným BC
        /// </summary>
        /// <param name="Binary"></param>
        /// <returns></returns>
        public override Bitmap GetBarcodeBitmap(string Binary)
        {
            try
            {
                string[] binaryArray = Binary.Split('|');

                Bitmap bitmap = new Bitmap(binaryArray[0].Length * ModuleWidth + 2 * HorizontalQuietZone, (binaryArray.Length - 1) * ModuleWidth * _moduleHeightToWidthRatio + 2 * VerticalQuietZone);
                int posX = HorizontalQuietZone;
                int posY = VerticalQuietZone;
                using (Graphics g = Graphics.FromImage(bitmap))
                {

                    Pen black = new Pen(new SolidBrush(Color.FromArgb(0, 0, 0)), base.ModuleWidth);
                    g.Clear(Color.White);

                    //namaluju čáry
                    for (int y = 0; y < binaryArray.Length - 1; y++)
                    {
                        posX = HorizontalQuietZone;
                        for (int x = 0; x < binaryArray[y].Length; x++)
                        {
                            if (binaryArray[y].Substring(x, 1) == "1")
                            {
                                g.DrawLine(black, posX, posY, posX, posY + ModuleWidth * _moduleHeightToWidthRatio);
                            }
                            posX += ModuleWidth;
                        }
                        posY += ModuleWidth * _moduleHeightToWidthRatio;
                    }
                    return bitmap;
                }
            }
            catch (Exception e)
            {
                ExceptionUserMessage += "Chyba při vytváření bitmapy\n";
                throw e;
            }
        }

        /// <summary>
        /// zkontroluje vstupní text, vyhodí nepovolené znaky
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override String CheckBarcodeText(String BarcodeText)
        {
            //TODO: dodat implementaci
            if (BarcodeText == "") return "EMPTY";
            return BarcodeText;
        }
    }
}
