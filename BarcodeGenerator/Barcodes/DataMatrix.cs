using System;
using System.Text;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// specializovaná třída pro DataMatrix
    /// </summary>
    public class DataMatrix : Barcode2D
    {
        /// <summary>
        /// konstruktor, pouze nastaví výchozí hodnoty proměnných
        /// </summary>
        public DataMatrix()
        {
            ModuleHeightToWidthRatio = 1;
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
                DmtxImageEncoder encoder = new DmtxImageEncoder();
                bool[,] rawData = encoder.EncodeRawData(BarcodeText);
                StringBuilder sb = new StringBuilder();
                for (int rowIdx = 0; rowIdx < rawData.GetLength(1); rowIdx++)
                {
                    for (int colIdx = 0; colIdx < rawData.GetLength(0); colIdx++)
                    {
                        sb.Append(rawData[colIdx, rowIdx] ? "1" : "0");
                    }
                    sb.Append("|");
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Exception = e;
                ExceptionUserMessage += "Chyba při převodu na binární reprezentaci\n";
                throw e;
            }
        }
    }
}
