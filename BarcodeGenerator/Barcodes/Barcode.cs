using System;
using System.Drawing;
using System.IO;
using SDI = System.Drawing.Imaging;


namespace AutoCont.Barcodes
{
    //TODO: ošetření chyb ve všech vlastnostech a metodách, vlastnost pro chybovou hlášku
    //TODO: změna velikosti hotového obrázku
    //TODO: jiná formáty než gif, průhlednost


    /// <summary>
    /// abstraktní třída, nese společné vlastnosti a metody pro všechny třídy BC
    /// </summary>
    public abstract class Barcode
    {
        protected const int _maxModuleWidth = 10;
        protected const int _maxLinearBarcodeTextLength = 25;
        String _barcodeText = String.Empty;
        int _moduleWidth = 2; //default hodnota pro minimální šířku čáry
        RotateFlipType _rotateFlipType = RotateFlipType.RotateNoneFlipNone;
        Exception _exception;
        String _exceptionUserMessage = String.Empty;

        /// <summary>
        /// text, který se zakóduje do BC
        /// </summary>
        protected string BarcodeText
        {
            set
            {
                _barcodeText = CheckBarcodeText(value);
            }
            get
            {
                return _barcodeText;
            }
        }

        /// <summary>
        /// urči minimální šířku čáry (v pixelech) použité v barcode
        /// pokud není zadáno, použije se výchozí hodnota
        /// </summary>
        public int ModuleWidth
        {
            set
            {
                if (value > 0 && value <= _maxModuleWidth) _moduleWidth = value;
            }
            protected get
            {
                return _moduleWidth;
            }
        }

        /// <summary>
        /// úhel, o který se má barcode otočit - 90°, 180° nebo 270°
        /// </summary>
        public int RotationAngle
        {
            set
            {
                if (value >= 90 && value <= 270)
                {
                    _rotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), "Rotate" + ((int)(value / 90) * 90) + "FlipNone");
                }
            }
        }

        /// <summary>
        /// vrátí informace o chybě ve chvíli, kdy se cokoli pokazí a některá metoda vrátí null
        /// </summary>
        public Exception Exception
        {
            get
            {
                if (_exception == null) _exception = new Exception();
                _exception.Data["UserMessage"] = _exceptionUserMessage;
                return _exception;
            }
            protected set
            {
                _exception = value;
            }
        }

        /// <summary>
        /// poskytne uživateli lidsky čitelný popis chyby
        /// </summary>
        protected String ExceptionUserMessage
        {
            get
            {
                return _exceptionUserMessage;
            }
            set
            {
                _exceptionUserMessage = value;
            }
        }

        /// <summary>
        /// abstraktní metoda, implementovaná pro každý (typ) BC, vrací System.Drawing.Bitpam na základě binární reprezentace barcode
        /// </summary>
        /// <param name="Binary"></param>
        /// <returns></returns>
        public abstract Bitmap GetBarcodeBitmap(String Binary);

        /// <summary>
        /// abstraktní metoda, implementovaná pro každý (typ) BC, vrací binární reprezentaci BC na základě zadaného textu
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected abstract String GetBinary(String BarcodeText);

        /// <summary>
        /// abstraktní metoda, implemetovaná pro každý typ BC, zkontroluje, zda je zadaný typ schopen zakódovat zadaný text. Vrací text s očesanými nepovolenými zkaky
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected abstract String CheckBarcodeText(String BarcodeText);

        /// <summary>
        /// vrací barcode jako System.IO.MemoryStream
        /// </summary>
        /// <param name="BarcodeText">text, který se zakóduje do BC</param>
        /// <returns></returns>
        public MemoryStream GetBarcodeMemoryStream(String BarcodeText)
        {
            this.BarcodeText = BarcodeText;
            try
            {
                using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
                {
                    Bitmap b = GetBarcodeBitmap(GetBinary(this.BarcodeText));
                    b.RotateFlip(_rotateFlipType);
                    b.Save(outStream, SDI.ImageFormat.Gif); //TODO: jiné formáty
                    return outStream;
                }
            }
            catch (Exception e)
            {
                _exception = e;
                _exceptionUserMessage += "Chyba při generování MemoryStream";
                return null;
            }
        }

        /// <summary>
        /// vrací barcode jako pole bajtů
        /// </summary>
        /// <param name="BarcodeText">text, který se zakóduje do BC</param>
        /// <returns></returns>
        public Byte[] GetBarcodeByteArray(String BarcodeText)
        {
            this.BarcodeText = BarcodeText;
            try
            {
                return GetBarcodeMemoryStream(this.BarcodeText).ToArray();
            }
            catch (Exception e)
            {
                _exception = e;
                _exceptionUserMessage += "Chyba při generování ByteArray";
                return null;
            }
        }

        /// <summary>
        /// vrací barcode jako System.Drawing.Image
        /// </summary>
        /// <param name="BarcodeText">text, který se zakóduje do BC</param>
        /// <returns></returns>
        public Image GetBarcodeImage(String BarcodeText)
        {
            try
            {
                this.BarcodeText = BarcodeText;
                Byte[] ByteArray = GetBarcodeByteArray(this.BarcodeText);
                MemoryStream imageStream;
                if (ByteArray.GetUpperBound(0) > 0)
                {
                    using (imageStream = new MemoryStream(ByteArray))
                    {
                        return Image.FromStream(imageStream);
                    }
                }
                else
                {
                    _exceptionUserMessage += "Chyba při generování Image";
                    return null;
                }
            }
            catch (Exception e)
            {
                _exception = e;
                _exceptionUserMessage += "Chyba při generování Image";
                return null;
            }
        }

        /// <summary>
        /// uloží barcode do souboru
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <param name="FileName"></param>
        public void SaveBarcodeToFile(String BarcodeText, String FileName)
        {
            this.BarcodeText = BarcodeText;
            try
            {
                //GetBarcodeBitmap(GetBinary(this.BarcodeText)).Save(FileName, SDI.ImageFormat.Gif); //TODO: jiné formáty
                using (Bitmap b = ExtensionMethods.BitmapTo1Bpp(GetBarcodeBitmap(GetBinary(this.BarcodeText))))
                {
                    //b.MakeTransparent(Color.White);
                    //b = ExtensionMethods.BitmapTo1Bpp(b);
                    b.Save(FileName, SDI.ImageFormat.Gif);
                }
            }
            catch (Exception e)
            {
                _exception = e;
                _exceptionUserMessage += "Chyba při ukládání souboru";
            }
        }
    }
}
