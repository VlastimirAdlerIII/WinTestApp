using System;
using System.Drawing;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// abstraktní třída, nese společné vlastnosti a metody pro 1D barcody
    /// </summary>
    public abstract class BarcodeLinear : Barcode
    {
        private int _width = 0;
        private int _height = 50;
        private int _moduleWidth = 2;
        private bool _displayText = true;
        private bool _textAbove = false;

        /// <summary>
        /// určí, zda se má zobrazit i text zakódovaný v barcode
        /// výchozí hodnota true
        /// </summary>
        public bool DisplayText
        {
            set
            {
                _displayText = value;
            }
        }

        /// <summary>
        /// určí, za má být text zobrazen nad barcode
        /// </summary>
        public bool TextAbove
        {
            set
            {
                _textAbove = value;
            }
        }

        /// <summary>
        /// určí výšku výsledného barcode
        /// pokud není zadáno, použije se výchozí hodnota 50
        /// </summary>
        public int Height
        {
            set
            {
                if (value >= 50 && value <= 1000) _height = value;
            }
        }

        /// <summary>
        /// spočítá šířku horizontálních ochranných zón
        /// </summary>
        protected int HorizontalQuietZone
        {
            get
            {
                return _moduleWidth * 10;
            }
        }

        /// <summary>
        /// spočítá výšku vertikální ochranných zóny nad
        /// </summary>
        protected int VerticalQuietZoneTop
        {
            get
            {
                int verticalQuietZoneTop = _height / 20;
                if (verticalQuietZoneTop < 5) verticalQuietZoneTop = 5;
                if (verticalQuietZoneTop > 50) verticalQuietZoneTop = 50;
                if (_displayText && _textAbove) verticalQuietZoneTop += TextHeight;
                return verticalQuietZoneTop;
            }
        }

        /// <summary>
        /// spočítá výšku vertikální ochranné zóny pod
        /// </summary>
        protected int VerticalQuietZoneBottom
        {
            get
            {
                int verticalQuietZoneBottom = _height / 20;
                if (verticalQuietZoneBottom < 5) verticalQuietZoneBottom = 5;
                if (verticalQuietZoneBottom > 50) verticalQuietZoneBottom = 50;
                if (_displayText && !_textAbove) verticalQuietZoneBottom += TextHeight;
                return verticalQuietZoneBottom;
            }
        }

        /// <summary>
        /// spočítá výšku výsledné čáry - po odečtení ochranných zón
        /// </summary>
        protected int BarHeight
        {
            get
            {
                return _height - VerticalQuietZoneTop - VerticalQuietZoneBottom;
            }
        }

        /// <summary>
        /// spočítá výchozí bod pro umístění textu do BC
        /// </summary>
        protected PointF TextOriginPoint
        {
            get
            {

                if (!_textAbove)
                {
                    float x = (_width / 2 - TextWidth / 2) * 1F;
                    float y = ((float)VerticalQuietZoneTop * 1.5F + (float)BarHeight);
                    return new PointF(x, y);
                }
                else
                {
                    float x = (_width / 2 - TextWidth / 2) * 1F;
                    float y = (VerticalQuietZoneBottom / 2) * 1F;
                    return new PointF(x, y);
                }
            }
        }



        /// <summary>
        /// určí výslednou šířku barcode
        /// pokud není zadáno, šířka se bude řídit minimální šířkou čáry
        /// </summary>
        public int Width
        {
            set
            {
                _width = value;
            }
        }

        /// <summary>
        /// spočítá výšku textu
        /// zatím jednoduše podíl z výšky celého barcode
        /// </summary>
        protected int TextHeight
        {
            get
            {
                int textHeight = _height / 5;
                if (textHeight < 12) textHeight = 12;
                if (textHeight > 50) textHeight = 50;
                return textHeight;
            }
        }

        /// <summary>
        /// spočítá šířku textu
        /// </summary>
        protected int TextWidth
        {
            get
            {
                //TODO: vzít v úvahu i výslednou šířku BC
                using (Font f = new Font("Courier New", TextHeight, GraphicsUnit.Pixel))
                    return (int)Graphics.FromImage(new Bitmap(1, 1)).MeasureString(this.BarcodeText, f).Width;
            }
        }

        /// <summary>
        /// implementace pro 1D barcodes; na základě binární reprezentace vytvoří bitmapu
        /// </summary>
        /// <param name="Binary"></param>
        /// <returns></returns>
        public override Bitmap GetBarcodeBitmap(string Binary)
        {
            try
            {
                _width = Binary.Length * base.ModuleWidth + 2 * HorizontalQuietZone;
                Bitmap bitmap = new Bitmap(_width, _height);
                int pos = HorizontalQuietZone;
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    Pen black = new Pen(new SolidBrush(Color.FromArgb(0, 0, 0)), base.ModuleWidth);
                    g.Clear(Color.White);

                    //namaluju čáry
                    for (int i = 0; i < Binary.Length; i++)
                    {
                        if (Binary.Substring(i, 1) == "1")
                        {
                            g.DrawLine(black, pos, VerticalQuietZoneTop, pos, VerticalQuietZoneTop + BarHeight);
                        }
                        pos += base.ModuleWidth;
                    }

                    // zobrazím případný text
                    if (_displayText)
                    {
                        g.DrawString(this.BarcodeText, new Font("Courier New", TextHeight, FontStyle.Bold, GraphicsUnit.Pixel), new SolidBrush(Color.FromArgb(0, 0, 0)), TextOriginPoint);
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
        /// výchozí kontrola zadaného textu
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override String CheckBarcodeText(String BarcodeText)
        {
            if (BarcodeText.Length > _maxLinearBarcodeTextLength) BarcodeText = BarcodeText.Substring(0, _maxLinearBarcodeTextLength);
            return BarcodeText;
        }
    }
}
