using AutoCont.Barcodes.Codec;
using System;
using System.Text;
using QRCodeUtility = AutoCont.Barcodes.Codec.Util;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// specializovaná třída pro QRCode
    /// </summary>
    public class QRCode : Barcode2D
    {
        QRCodeEncoder.ERROR_CORRECTION _errorCorrectionLevel;
        public QRCode()
        {
            ModuleHeightToWidthRatio = 1;
            _errorCorrectionLevel = QRCodeEncoder.ERROR_CORRECTION.Q;
        }

        /// <summary>
        /// úroveň odolnosti výsledného BC proti chybám
        /// </summary>
        public int ErrorCorrectionLevel
        {
            set
            {
                if (value >= 0 && value <= 2) _errorCorrectionLevel = QRCodeEncoder.ERROR_CORRECTION.L;
                if (value >= 3 && value <= 4) _errorCorrectionLevel = QRCodeEncoder.ERROR_CORRECTION.M;
                if (value >= 5 && value <= 6) _errorCorrectionLevel = QRCodeEncoder.ERROR_CORRECTION.Q;
                if (value >= 7 && value <= 8) _errorCorrectionLevel = QRCodeEncoder.ERROR_CORRECTION.H;
            }
        }

        /// <summary>
        /// vrátí binární reprezentaci barcode, řádky jsou odděleny znakem "|"
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override string GetBinary(string BarcodeText)
        {
            try
            {
                byte[] byteArray;
                if (QRCodeUtility.QRCodeUtility.IsUniCode(BarcodeText))
                {
                    byteArray = Encoding.Unicode.GetBytes(BarcodeText);
                }
                else
                {
                    //byteArray = Encoding.ASCII.GetBytes(BarcodeText);
                    byteArray = Encoding.GetEncoding(1250).GetBytes(BarcodeText);
                }

                QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                //qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qrCodeEncoder.QRCodeScale = ModuleWidth;
                qrCodeEncoder.QRCodeVersion = 0;
                qrCodeEncoder.QRCodeErrorCorrect = _errorCorrectionLevel;
                return qrCodeEncoder.calQRCodeBinary(byteArray);

            }
            catch (Exception e)
            {
                Exception = e;
                ExceptionUserMessage += "Chyba při převodu na binární reprezentaci\n";
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
            return BarcodeText.RemoveDiacritics();
        }
    }
}
