using System;
using System.Collections;


namespace AutoCont.Barcodes
{
    /// <summary>
    /// specializovaná třída pro Code128
    /// </summary>
    public class Code128 : BarcodeLinear
    {
        private Hashtable _arrA;
        private Hashtable _arrB;
        private Hashtable _arrC;
        private Hashtable _arrEncode;
        private Code128Subtype _barcodeSubtype = Code128Subtype.Auto; //default hodnota pro subtyp

        /// <summary>
        /// konstruktor
        /// </summary>
        public Code128()
        {
            Initialize();
        }

        /// <summary>
        /// subtyp
        /// </summary>
        public Code128Subtype BarcodeSubtype
        {
            set
            {
                //TODO: zohlednit subtyp při generování BC
                _barcodeSubtype = value;
            }
        }

        /// <summary>
        /// zkontroluje text, nepovolené znaky vyhodí
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override String CheckBarcodeText(String BarcodeText)
        {
            BarcodeText = base.CheckBarcodeText(BarcodeText);
            string result = String.Empty;
            try
            {
                for (int i = 0; i < BarcodeText.Length; i++)
                {
                    if (_barcodeSubtype == Code128Subtype.Auto || _barcodeSubtype == Code128Subtype.B)
                    {
                        if (_arrB.Contains(BarcodeText.Substring(i, 1)))
                        {
                            result += BarcodeText.Substring(i, 1);
                        }
                    }
                    if (_barcodeSubtype == Code128Subtype.C)
                    {
                        if (Char.IsNumber((BarcodeText.Substring(i, 1).ToCharArray()[0])))
                        {
                            result += BarcodeText.Substring(i, 1);
                        }
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
                ExceptionUserMessage += "Code128 - chyba při kontrole povolených znaků\n";
                return "ERROR";
            }
        }

        /// <summary>
        /// převeden zadanou hodnotu na binární reprezentaci - pro Code128
        /// </summary>
        /// <param name="BarcodeText"></param>
        /// <returns></returns>
        protected override String GetBinary(string BarcodeText)
        {
            string Encoding = "B";
            int Counter = 1;
            int Pos = 0;
            float CheckSum = 0;
            string Bin = "";

            try
            {
                if (BarcodeText.Length >= 4 && BarcodeText.Substring(0, 4).IsNumeric())
                {
                    Encoding = "C";
                }

                Bin = GetCode128Binary("START " + Encoding, Encoding);
                CheckSum = FindCode(Encoding, "START " + Encoding);
                do
                {
                    if (Encoding == "C")
                    {
                        while (BarcodeText.Length - Pos >= 2 && BarcodeText.Substring(Pos, 2).IsNumeric())
                        {
                            Bin = Bin + GetCode128Binary(BarcodeText.Substring(Pos, 2), "C");
                            CheckSum = CheckSum + FindCode("C", BarcodeText.Substring(Pos, 2)) * Counter;
                            Pos = Pos + 2;
                            Counter++;
                        }
                        if (Pos < BarcodeText.Length)
                        {
                            Bin = Bin + GetCode128Binary("Code B", Encoding);
                            CheckSum = CheckSum + FindCode("C", "Code B") * Counter;
                            Encoding = "B";
                            Counter++;
                        }
                    }

                    if (Pos < BarcodeText.Length)
                    {
                        Bin = Bin + GetCode128Binary(BarcodeText.Substring(Pos, 1), "B");
                        CheckSum = CheckSum + FindCode("B", BarcodeText.Substring(Pos, 1)) * Counter;
                        Pos++;
                        Counter++;
                        //if ((BarcodeText.Length - Pos >= 4) && IsFinite(BarcodeText.Substring(Pos, 4)) && (BarcodeText.Substring(Pos, 4).LastIndexOf(".") == -1) && (BarcodeText.Substring(Pos, 4).LastIndexOf("-") == -1) && (BarcodeText.Substring(Pos, 4).LastIndexOf("+") == -1))
                        if (BarcodeText.Length - Pos >= 4 && BarcodeText.Substring(Pos, 4).IsNumeric())
                        {
                            Bin = Bin + GetCode128Binary("Code C", Encoding);
                            CheckSum = CheckSum + FindCode("B", "Code C") * Counter;
                            Encoding = "C";
                            Counter++;
                        }
                    }
                }
                while (Pos < BarcodeText.Length);

                //přidám CheckSum
                Bin = Bin + _arrEncode[(CheckSum % 103).ToString("00")];

                //přidám Stop + Terminate
                Bin = Bin + "11000111010" + "11";
                return Bin;
            }
            catch (Exception e)
            {
                Exception = e;
                ExceptionUserMessage += "Chyba při převodu na binární reprezentaci\n";
                throw e;
            }
        }

        /// <summary>
        /// vrací binární kód z kódové tabulky pro zadanou hodnotu
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="Encoding"></param>
        /// <returns></returns>
        private string GetCode128Binary(string Code, string Encoding)
        {
            switch (Encoding)
            {
                case "A":
                    {
                        return _arrEncode[_arrA[Code].ToString()].ToString();
                    }
                case "B":
                    {
                        return _arrEncode[_arrB[Code].ToString()].ToString();
                    }
                case "C":
                    {
                        return _arrEncode[_arrC[Code].ToString()].ToString();
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// najde v zadaném kódování zadanou hodnotu
        /// </summary>
        /// <param name="Encoding">kódování</param>
        /// <param name="Value">hledaná hodnota</param>
        /// <returns></returns>
        private int FindCode(string Encoding, string Value)
        {
            switch (Encoding)
            {
                case "A":
                    {
                        return int.Parse(_arrA[Value].ToString());
                    }
                case "B":
                    {
                        return int.Parse(_arrB[Value].ToString());
                    }
                case "C":
                    {
                        return int.Parse(_arrC[Value].ToString());
                    }
            }
            return -1;
        }

        /// <summary>
        /// Inicializace kódové tabulky
        /// </summary>
        private void Initialize()
        {
            _arrA = new Hashtable();
            _arrB = new Hashtable();
            _arrC = new Hashtable();
            _arrEncode = new Hashtable();

            _arrA[" "] = "00"; _arrB[" "] = "00"; _arrC["00"] = "00"; _arrEncode["00"] = "11011001100";
            _arrA["!"] = "01"; _arrB["!"] = "01"; _arrC["01"] = "01"; _arrEncode["01"] = "11001101100";
            _arrA["\""] = "02"; _arrB["\""] = "02"; _arrC["02"] = "02"; _arrEncode["02"] = "11001100110";
            _arrA["#"] = "03"; _arrB["#"] = "03"; _arrC["03"] = "03"; _arrEncode["03"] = "10010011000";
            _arrA["$"] = "04"; _arrB["$"] = "04"; _arrC["04"] = "04"; _arrEncode["04"] = "10010001100";
            _arrA["%"] = "05"; _arrB["%"] = "05"; _arrC["05"] = "05"; _arrEncode["05"] = "10001001100";
            _arrA["&"] = "06"; _arrB["&"] = "06"; _arrC["06"] = "06"; _arrEncode["06"] = "10011001000";
            _arrA["\""] = "07"; _arrB["\""] = "07"; _arrC["07"] = "07"; _arrEncode["07"] = "10011000100";
            _arrA["("] = "08"; _arrB["("] = "08"; _arrC["08"] = "08"; _arrEncode["08"] = "10001100100";
            _arrA[")"] = "09"; _arrB[")"] = "09"; _arrC["09"] = "09"; _arrEncode["09"] = "11001001000";
            _arrA["*"] = "10"; _arrB["*"] = "10"; _arrC["10"] = "10"; _arrEncode["10"] = "11001000100";
            _arrA["+"] = "11"; _arrB["+"] = "11"; _arrC["11"] = "11"; _arrEncode["11"] = "11000100100";
            _arrA[","] = "12"; _arrB[","] = "12"; _arrC["12"] = "12"; _arrEncode["12"] = "10110011100";
            _arrA["-"] = "13"; _arrB["-"] = "13"; _arrC["13"] = "13"; _arrEncode["13"] = "10011011100";
            _arrA["."] = "14"; _arrB["."] = "14"; _arrC["14"] = "14"; _arrEncode["14"] = "10011001110";
            _arrA["/"] = "15"; _arrB["/"] = "15"; _arrC["15"] = "15"; _arrEncode["15"] = "10111001100";
            _arrA["0"] = "16"; _arrB["0"] = "16"; _arrC["16"] = "16"; _arrEncode["16"] = "10011101100";
            _arrA["1"] = "17"; _arrB["1"] = "17"; _arrC["17"] = "17"; _arrEncode["17"] = "10011100110";
            _arrA["2"] = "18"; _arrB["2"] = "18"; _arrC["18"] = "18"; _arrEncode["18"] = "11001110010";
            _arrA["3"] = "19"; _arrB["3"] = "19"; _arrC["19"] = "19"; _arrEncode["19"] = "11001011100";
            _arrA["4"] = "20"; _arrB["4"] = "20"; _arrC["20"] = "20"; _arrEncode["20"] = "11001001110";
            _arrA["5"] = "21"; _arrB["5"] = "21"; _arrC["21"] = "21"; _arrEncode["21"] = "11011100100";
            _arrA["6"] = "22"; _arrB["6"] = "22"; _arrC["22"] = "22"; _arrEncode["22"] = "11001110100";
            _arrA["7"] = "23"; _arrB["7"] = "23"; _arrC["23"] = "23"; _arrEncode["23"] = "11101101110";
            _arrA["8"] = "24"; _arrB["8"] = "24"; _arrC["24"] = "24"; _arrEncode["24"] = "11101001100";
            _arrA["9"] = "25"; _arrB["9"] = "25"; _arrC["25"] = "25"; _arrEncode["25"] = "11100101100";
            _arrA[":"] = "26"; _arrB[":"] = "26"; _arrC["26"] = "26"; _arrEncode["26"] = "11100100110";
            _arrA[";"] = "27"; _arrB[";"] = "27"; _arrC["27"] = "27"; _arrEncode["27"] = "11101100100";
            _arrA["<"] = "28"; _arrB["<"] = "28"; _arrC["28"] = "28"; _arrEncode["28"] = "11100110100";
            _arrA["="] = "29"; _arrB["="] = "29"; _arrC["29"] = "29"; _arrEncode["29"] = "11100110010";
            _arrA[">"] = "30"; _arrB[">"] = "30"; _arrC["30"] = "30"; _arrEncode["30"] = "11011011000";
            _arrA["?"] = "31"; _arrB["?"] = "31"; _arrC["31"] = "31"; _arrEncode["31"] = "11011000110";
            _arrA["@"] = "32"; _arrB["@"] = "32"; _arrC["32"] = "32"; _arrEncode["32"] = "11000110110";
            _arrA["A"] = "33"; _arrB["A"] = "33"; _arrC["33"] = "33"; _arrEncode["33"] = "10100011000";
            _arrA["B"] = "34"; _arrB["B"] = "34"; _arrC["34"] = "34"; _arrEncode["34"] = "10001011000";
            _arrA["C"] = "35"; _arrB["C"] = "35"; _arrC["35"] = "35"; _arrEncode["35"] = "10001000110";
            _arrA["D"] = "36"; _arrB["D"] = "36"; _arrC["36"] = "36"; _arrEncode["36"] = "10110001000";
            _arrA["E"] = "37"; _arrB["E"] = "37"; _arrC["37"] = "37"; _arrEncode["37"] = "10001101000";
            _arrA["F"] = "38"; _arrB["F"] = "38"; _arrC["38"] = "38"; _arrEncode["38"] = "10001100010";
            _arrA["G"] = "39"; _arrB["G"] = "39"; _arrC["39"] = "39"; _arrEncode["39"] = "11010001000";
            _arrA["H"] = "40"; _arrB["H"] = "40"; _arrC["40"] = "40"; _arrEncode["40"] = "11000101000";
            _arrA["I"] = "41"; _arrB["I"] = "41"; _arrC["41"] = "41"; _arrEncode["41"] = "11000100010";
            _arrA["J"] = "42"; _arrB["J"] = "42"; _arrC["42"] = "42"; _arrEncode["42"] = "10110111000";
            _arrA["K"] = "43"; _arrB["K"] = "43"; _arrC["43"] = "43"; _arrEncode["43"] = "10110001110";
            _arrA["L"] = "44"; _arrB["L"] = "44"; _arrC["44"] = "44"; _arrEncode["44"] = "10001101110";
            _arrA["M"] = "45"; _arrB["M"] = "45"; _arrC["45"] = "45"; _arrEncode["45"] = "10111011000";
            _arrA["N"] = "46"; _arrB["N"] = "46"; _arrC["46"] = "46"; _arrEncode["46"] = "10111000110";
            _arrA["O"] = "47"; _arrB["O"] = "47"; _arrC["47"] = "47"; _arrEncode["47"] = "10001110110";
            _arrA["P"] = "48"; _arrB["P"] = "48"; _arrC["48"] = "48"; _arrEncode["48"] = "11101110110";
            _arrA["Q"] = "49"; _arrB["Q"] = "49"; _arrC["49"] = "49"; _arrEncode["49"] = "11010001110";
            _arrA["R"] = "50"; _arrB["R"] = "50"; _arrC["50"] = "50"; _arrEncode["50"] = "11000101110";
            _arrA["S"] = "51"; _arrB["S"] = "51"; _arrC["51"] = "51"; _arrEncode["51"] = "11011101000";
            _arrA["T"] = "52"; _arrB["T"] = "52"; _arrC["52"] = "52"; _arrEncode["52"] = "11011100010";
            _arrA["U"] = "53"; _arrB["U"] = "53"; _arrC["53"] = "53"; _arrEncode["53"] = "11011101110";
            _arrA["V"] = "54"; _arrB["V"] = "54"; _arrC["54"] = "54"; _arrEncode["54"] = "11101011000";
            _arrA["W"] = "55"; _arrB["W"] = "55"; _arrC["55"] = "55"; _arrEncode["55"] = "11101000110";
            _arrA["X"] = "56"; _arrB["X"] = "56"; _arrC["56"] = "56"; _arrEncode["56"] = "11100010110";
            _arrA["Y"] = "57"; _arrB["Y"] = "57"; _arrC["57"] = "57"; _arrEncode["57"] = "11101101000";
            _arrA["Z"] = "58"; _arrB["Z"] = "58"; _arrC["58"] = "58"; _arrEncode["58"] = "11101100010";
            _arrA["["] = "59"; _arrB["["] = "59"; _arrC["59"] = "59"; _arrEncode["59"] = "11100011010";
            _arrA["\\"] = "60"; _arrB["\\"] = "60"; _arrC["60"] = "60"; _arrEncode["60"] = "11101111010";
            _arrA["]"] = "61"; _arrB["]"] = "61"; _arrC["61"] = "61"; _arrEncode["61"] = "11001000010";
            _arrA["^"] = "62"; _arrB["^"] = "62"; _arrC["62"] = "62"; _arrEncode["62"] = "11110001010";
            _arrA["_"] = "63"; _arrB["_"] = "63"; _arrC["63"] = "63"; _arrEncode["63"] = "10100110000";
            _arrA["NUL"] = "64"; _arrB["`"] = "64"; _arrC["64"] = "64"; _arrEncode["64"] = "10100001100";
            _arrA["SOH"] = "65"; _arrB["a"] = "65"; _arrC["65"] = "65"; _arrEncode["65"] = "10010110000";
            _arrA["STX"] = "66"; _arrB["b"] = "66"; _arrC["66"] = "66"; _arrEncode["66"] = "10010000110";
            _arrA["ETX"] = "67"; _arrB["c"] = "67"; _arrC["67"] = "67"; _arrEncode["67"] = "10000101100";
            _arrA["EOT"] = "68"; _arrB["d"] = "68"; _arrC["68"] = "68"; _arrEncode["68"] = "10000100110";
            _arrA["ENQ"] = "69"; _arrB["e"] = "69"; _arrC["69"] = "69"; _arrEncode["69"] = "10110010000";
            _arrA["ACK"] = "70"; _arrB["f"] = "70"; _arrC["70"] = "70"; _arrEncode["70"] = "10110000100";
            _arrA["BEL"] = "71"; _arrB["g"] = "71"; _arrC["71"] = "71"; _arrEncode["71"] = "10011010000";
            _arrA["BS"] = "72"; _arrB["h"] = "72"; _arrC["72"] = "72"; _arrEncode["72"] = "10011000010";
            _arrA["HT"] = "73"; _arrB["i"] = "73"; _arrC["73"] = "73"; _arrEncode["73"] = "10000110100";
            _arrA["LF"] = "74"; _arrB["j"] = "74"; _arrC["74"] = "74"; _arrEncode["74"] = "10000110010";
            _arrA["VT"] = "75"; _arrB["k"] = "75"; _arrC["75"] = "75"; _arrEncode["75"] = "11000010010";
            _arrA["FF"] = "76"; _arrB["l"] = "76"; _arrC["76"] = "76"; _arrEncode["76"] = "11001010000";
            _arrA["CR"] = "77"; _arrB["m"] = "77"; _arrC["77"] = "77"; _arrEncode["77"] = "11110111010";
            _arrA["SO"] = "78"; _arrB["n"] = "78"; _arrC["78"] = "78"; _arrEncode["78"] = "11000010100";
            _arrA["SI"] = "79"; _arrB["o"] = "79"; _arrC["79"] = "79"; _arrEncode["79"] = "10001111010";
            _arrA["DLE"] = "80"; _arrB["p"] = "80"; _arrC["80"] = "80"; _arrEncode["80"] = "10100111100";
            _arrA["DC1"] = "81"; _arrB["q"] = "81"; _arrC["81"] = "81"; _arrEncode["81"] = "10010111100";
            _arrA["DC2"] = "82"; _arrB["r"] = "82"; _arrC["82"] = "82"; _arrEncode["82"] = "10010011110";
            _arrA["DC3"] = "83"; _arrB["s"] = "83"; _arrC["83"] = "83"; _arrEncode["83"] = "10111100100";
            _arrA["DC4"] = "84"; _arrB["t"] = "84"; _arrC["84"] = "84"; _arrEncode["84"] = "10011110100";
            _arrA["NAK"] = "85"; _arrB["u"] = "85"; _arrC["85"] = "85"; _arrEncode["85"] = "10011110010";
            _arrA["SYN"] = "86"; _arrB["v"] = "86"; _arrC["86"] = "86"; _arrEncode["86"] = "11110100100";
            _arrA["ETB"] = "87"; _arrB["w"] = "87"; _arrC["87"] = "87"; _arrEncode["87"] = "11110010100";
            _arrA["CAN"] = "88"; _arrB["x"] = "88"; _arrC["88"] = "88"; _arrEncode["88"] = "11110010010";
            _arrA["EM"] = "89"; _arrB["y"] = "89"; _arrC["89"] = "89"; _arrEncode["89"] = "11011011110";
            _arrA["SUB"] = "90"; _arrB["z"] = "90"; _arrC["90"] = "90"; _arrEncode["90"] = "11011110110";
            _arrA["ESC"] = "91"; _arrB["{"] = "91"; _arrC["91"] = "91"; _arrEncode["91"] = "11110110110";
            _arrA["FS"] = "92"; _arrB["|"] = "92"; _arrC["92"] = "92"; _arrEncode["92"] = "10101111000";
            _arrA["GS"] = "93"; _arrB["}"] = "93"; _arrC["93"] = "93"; _arrEncode["93"] = "10100011110";
            _arrA["RS"] = "94"; _arrB["~"] = "94"; _arrC["94"] = "94"; _arrEncode["94"] = "10001011110";
            _arrA["US"] = "95"; _arrB["DEL"] = "95"; _arrC["95"] = "95"; _arrEncode["95"] = "10111101000";
            _arrA["FNC3"] = "96"; _arrB["FNC3"] = "96"; _arrC["96"] = "96"; _arrEncode["96"] = "10111100010";
            _arrA["FNC2"] = "97"; _arrB["FNC2"] = "97"; _arrC["97"] = "97"; _arrEncode["97"] = "11110101000";
            _arrA["SHIFT"] = "98"; _arrB["SHIFT"] = "98"; _arrC["98"] = "98"; _arrEncode["98"] = "11110100010";
            _arrA["Code C"] = "99"; _arrB["Code C"] = "99"; _arrC["99"] = "99"; _arrEncode["99"] = "10111011110";
            _arrA["Code B"] = "100"; _arrB["FNC4"] = "100"; _arrC["Code B"] = "100"; _arrEncode["100"] = "10111101110";
            _arrA["FNC4"] = "101"; _arrB["Code A"] = "101"; _arrC["Code A"] = "101"; _arrEncode["101"] = "11101011110";
            _arrA["FNC1"] = "102"; _arrB["FNC1"] = "102"; _arrC["FNC1"] = "102"; _arrEncode["102"] = "11110101110";

            _arrA["START A"] = "103"; _arrB["START A"] = "103"; _arrC["START A"] = "103"; _arrEncode["103"] = "11010000100";
            _arrA["START B"] = "104"; _arrB["START B"] = "104"; _arrC["START B"] = "104"; _arrEncode["104"] = "11010010000";
            _arrA["START C"] = "105"; _arrB["START C"] = "105"; _arrC["START C"] = "105"; _arrEncode["105"] = "11010011100";
        }
    }
}
