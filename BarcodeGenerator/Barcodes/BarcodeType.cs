namespace AutoCont.Barcodes
{
    /// <summary>
    /// číselník možných typů barcode
    /// </summary>
    public enum BarcodeType
    {
        Code128 = 1,
        Code39 = 2,
        PDF417 = 4,
        QRCode = 8,
        DataMatrix = 16
    }
}
