using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using AutoCont.Barcodes;
using System.Globalization;

namespace WinTestApp
{
   public partial class Form1 : Form
   {
      public Form1()
      {
         InitializeComponent();
      }

      private void button1_Click(object sender, EventArgs e)
      {
         if (comboBox1.Text == "Code128")
         {
            Code128 bc = new Code128();
            int i;
            bc.BarcodeSubtype = (Code128Subtype)Enum.Parse(typeof(Code128Subtype), comboBox2.Text);
            int.TryParse(textBox3.Text, out i); bc.Height = i;
            int.TryParse(textBox2.Text, out i); bc.ModuleWidth = i;
            bc.TextAbove = checkBox2.Checked;
            bc.DisplayText = checkBox1.Checked;
            int.TryParse(textBox4.Text, out i); bc.RotationAngle = i;
            try
            {
               if (checkBox3.Checked)
               {
                  try
                  {
                     string line;

                     System.IO.StreamReader file = new System.IO.StreamReader(textBox5.Text);
                     while ((line = file.ReadLine()) != null)
                     {
                        bc.SaveBarcodeToFile(line, String.Format("{0}\\{1}.{2}", Path.GetDirectoryName(textBox5.Text), line, "gif"));
                     }

                     file.Close();
                  }
                  catch (Exception ex)
                  {
                     MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                  }
               }
               else
               {
                  pictureBox1.Image = bc.GetBarcodeImage(textBox1.Text);
                  bc.SaveBarcodeToFile(textBox1.Text, @"C:\Temp\BC.gif");
               }
            }
            catch (Exception) { }
         };
         if (comboBox1.Text == "Code39")
         {
            Code39 bc = new Code39();
            int i;
            int.TryParse(textBox3.Text, out i); bc.Height = i;
            int.TryParse(textBox2.Text, out i); bc.ModuleWidth = i;
            bc.TextAbove = checkBox2.Checked;
            bc.DisplayText = checkBox1.Checked;
            int.TryParse(textBox4.Text, out i); bc.RotationAngle = i;
            pictureBox1.Image = bc.GetBarcodeImage(textBox1.Text);
            bc.SaveBarcodeToFile(textBox1.Text, @"C:\Temp\BC.gif");
         }

         if (comboBox1.Text == "PDF417")
         {
            PDF417 bc = new PDF417();
            int i;
            int.TryParse(textBox2.Text, out i); bc.ModuleWidth = i;
            int.TryParse(textBox4.Text, out i); bc.RotationAngle = i;
            pictureBox1.Image = bc.GetBarcodeImage(textBox1.Text);
            bc.SaveBarcodeToFile(textBox1.Text, @"C:\Temp\BC.gif");
         }

         if (comboBox1.Text == "QRCode")
         {
            QRCode bc = new QRCode();
            int i;
            int.TryParse(textBox2.Text, out i); bc.ModuleWidth = i;
            //int.TryParse(textBox4.Text, out i); bc.RotationAngle = i;
            int.TryParse(textBox4.Text, out i); bc.ErrorCorrectionLevel = i;
            pictureBox1.Image = bc.GetBarcodeImage(textBox1.Text);
            bc.SaveBarcodeToFile(textBox1.Text, @"C:\Temp\BC.gif");
         }

         if (comboBox1.Text == "DataMatrix")
         {
            DataMatrix bc = new DataMatrix();
            int i;
            int.TryParse(textBox2.Text, out i); bc.ModuleWidth = i;
            //int.TryParse(textBox4.Text, out i); bc.RotationAngle = i;
            //int.TryParse(textBox4.Text, out i); bc.ErrorCorrectionLevel = i;
            pictureBox1.Image = bc.GetBarcodeImage(textBox1.Text);
            bc.SaveBarcodeToFile(textBox1.Text, @"C:\Temp\BC.gif");
         }

         //BarcodeGenerator BCG = new BarcodeGenerator();
         //BCG.BarcodeType = BarcodeType.Code39;
         //pictureBox1.Image = BCG.GetBarcodeImage(textBox1.Text);
         //textBox2.Text = BCG.TextWidth.ToString();

         //MessageBox.Show(NumberFormatInfo.CurrentInfo.PerMilleSymbol);
         //MessageBox.Show(NumberFormatInfo.CurrentInfo.NegativeInfinitySymbol);
         //MessageBox.Show((-1).ToString("x"));

      }

      private void button3_Click(object sender, EventArgs e)
      {
         using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
         {
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
               textBox5.Text = openFileDialog1.FileName;
            }
         }
      }

   }
}
