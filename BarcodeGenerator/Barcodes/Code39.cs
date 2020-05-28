using System;
using System.Collections;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// specializovaná třída pro Code39
    /// </summary>
    public class Code39 : BarcodeLinear
    {
        private Hashtable _arr39;

        /// <summary>
        /// kostruktor
        /// </summary>
        public Code39()
        {
            Initialize();
        }

        /// <summary>
        /// zkontroluje vstupní text, provede případné korekce
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override String CheckBarcodeText(String BarcodeText)
        {
            BarcodeText = base.CheckBarcodeText(BarcodeText);
            string result = String.Empty;
            BarcodeText = BarcodeText.ToUpper();
            try
            {
                for (int i = 0; i < BarcodeText.Length; i++)
                {
                    if (_arr39.Contains(BarcodeText.Substring(i, 1)))
                    {
                        result += BarcodeText.Substring(i, 1);
                    }
                }
                if (result.Equals(String.Empty))
                {
                    result = "EMPTY";
                }
                return result;

            }
            catch (Exception e)
            {
                Exception = e;
                ExceptionUserMessage += "Code39 - chyba při kontrole povolených znaků\n";
                return "ERROR";
            }
        }

        /// <summary>
        /// zakóduje zadaný text do Code3of9 - vrací binární reprezentaci
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override string GetBinary(string BarcodeText)
        {
            string binary = String.Empty;
            try
            {
                if (BarcodeText.Substring(0, 1) != "*") BarcodeText = "*" + BarcodeText;
                if (BarcodeText.Substring(BarcodeText.Length - 1, 1) != "*") BarcodeText = BarcodeText + "*";
                for (var i = 0; i < BarcodeText.Length; i++)
                {
                    binary = binary + _arr39[BarcodeText.Substring(i, 1)] + "0";
                }
                return binary.Substring(0, binary.Length - 1);
            }
            catch (Exception e)
            {
                Exception = e;
                ExceptionUserMessage += "Chyba při převodu na binární reprezentaci\n";
                throw e;
            }
        }

        /// <summary>
        /// Inicializace kódové tabulky
        /// </summary>
        private void Initialize()
        {
            _arr39 = new Hashtable();

            _arr39["0"] = "101001101101";
            _arr39["1"] = "110100101011";
            _arr39["2"] = "101100101011";
            _arr39["3"] = "110110010101";
            _arr39["4"] = "101001101011";
            _arr39["5"] = "110100110101";
            _arr39["6"] = "101100110101";
            _arr39["7"] = "101001011011";
            _arr39["8"] = "110100101101";
            _arr39["9"] = "101100101101";
            _arr39["A"] = "110101001011";
            _arr39["B"] = "101101001011";
            _arr39["C"] = "110110100101";
            _arr39["D"] = "101011001011";
            _arr39["E"] = "110101100101";
            _arr39["F"] = "101101100101";
            _arr39["G"] = "101010011011";
            _arr39["H"] = "110101001101";
            _arr39["I"] = "101101001101";
            _arr39["J"] = "101011001101";
            _arr39["K"] = "110101010011";
            _arr39["L"] = "101101010011";
            _arr39["M"] = "110110101001";
            _arr39["N"] = "101011010011";
            _arr39["O"] = "110101101001";
            _arr39["P"] = "101101101001";
            _arr39["Q"] = "101010110011";
            _arr39["R"] = "110101011001";
            _arr39["S"] = "101101011001";
            _arr39["T"] = "101011011001";
            _arr39["U"] = "110010101011";
            _arr39["V"] = "100110101011";
            _arr39["W"] = "110011010101";
            _arr39["X"] = "100101101011";
            _arr39["Y"] = "110010110101";
            _arr39["Z"] = "100110110101";
            _arr39["-"] = "100101011011";
            _arr39["."] = "110010101101";
            _arr39[" "] = "100110101101";
            _arr39["$"] = "100100100101";
            _arr39["/"] = "100100101001";
            _arr39["+"] = "100101001001";
            _arr39["%"] = "101001001001";
            _arr39["*"] = "100101101101";
        }
    }
}
