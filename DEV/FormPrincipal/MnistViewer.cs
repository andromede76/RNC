using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

// WinForm program for MSDN Magazine article "Image Recognition with the MNIST Data Set"
// Save as MnistViewer.cs
// You must get, download, and unzip the two MNIST data files below
// To compile, create a new VS project, then add this .cs file to the project
// OR use the C# compiler directly: >csc.exe /target:winexe MnistViewer.cs

namespace MnistViewer
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }

  public partial class Form1 : Form
  {
    // get these files from http://yann.lecun.com/exdb/mnist/
    // they must be unzipped after downloading
    // edit paths as necessary

    private string pixelFile = @"C:\MnistViewer\train-images.idx3-ubyte";
    private string labelFile = @"C:\MnistViewer\train-labels.idx1-ubyte";
    private DigitImage[] trainImages = null;

    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    public Form1()
    {
      InitializeComponent();
      textBox1.Text = pixelFile;
      textBox2.Text = labelFile;
      comboBox1.SelectedItem = "6";  // Magnification
      this.ActiveControl = button1;
    }

    // ==== UI methods

    private void button1_Click(object sender, EventArgs e) // Load data 
    {
      this.pixelFile = textBox1.Text;
      this.labelFile = textBox2.Text;
      this.trainImages = LoadData(pixelFile, labelFile);
      listBox1.Items.Add("MNIST images loaded into memory");
    }

    private void button2_Click(object sender, EventArgs e) // Display 'next' image
    {
      int nextIndex = int.Parse(textBox4.Text);
      DigitImage currImage = trainImages[nextIndex];

      int mag = int.Parse(comboBox1.SelectedItem.ToString()); // magnification
      Bitmap bitMap = MakeBitmap(currImage, mag);
      pictureBox1.Image = bitMap;

      string pixelVals = PixelValues(currImage);
      textBox5.Text = pixelVals;

      textBox3.Text = textBox4.Text; // update curr idx from old next idz
      textBox4.Text = (nextIndex + 1).ToString(); // ++next index

      listBox1.Items.Add("Curr image index = " + textBox3.Text + " label = " + currImage.label);
    } 

    // ==== Code Logic methods =======================================================================

    //public static int ReverseBytes(int v) // 32 bits = 4 bytes
    //{
    //  // bit-manipulation version
    //  return (v & 0x000000FF) << 24 | (v & 0x0000FF00) << 8 |
    //         (v & 0x00FF0000) >> 8 | ((int)(v & 0xFF000000)) >> 24;
    //}

    public static int ReverseBytes(int v)
    {
      byte[] intAsBytes = BitConverter.GetBytes(v);
      Array.Reverse(intAsBytes);
      return BitConverter.ToInt32(intAsBytes, 0);
    }

    public static string IntToBinaryString(int v)
    {
      // to pretty print an int as binary
      string s = Convert.ToString(v, 2); // base 2 but without leading 0s
      string t = s.PadLeft(32, '0'); // add leading 0s
      string res = "";
      for (int i = 0; i < t.Length; ++i)
      {
        if (i > 0 && i % 8 == 0)
          res += " "; // add a space every 8 chars
        res += t[i];
      }
      return res;
    }

    public static DigitImage[] LoadData(string pixelFile, string labelFile)
    {
      // Load MNIST training set of 60,000 images into memory
      // remove static to access listBox1
      int numImages = 60000;
      DigitImage[] result = new DigitImage[numImages];

      byte[][] pixels = new byte[28][];
      for (int i = 0; i < pixels.Length; ++i)
        pixels[i] = new byte[28];

      FileStream ifsPixels = new FileStream(pixelFile, FileMode.Open);
      FileStream ifsLabels = new FileStream(labelFile, FileMode.Open);

      BinaryReader brImages = new BinaryReader(ifsPixels);
      BinaryReader brLabels = new BinaryReader(ifsLabels);

      int magic1 = brImages.ReadInt32(); // stored as Big Endian
      magic1 = ReverseBytes(magic1); // convert to Intel format

      int imageCount = brImages.ReadInt32();
      imageCount = ReverseBytes(imageCount);

      int numRows = brImages.ReadInt32();
      numRows = ReverseBytes(numRows);
      int numCols = brImages.ReadInt32();
      numCols = ReverseBytes(numCols);

      int magic2 = brLabels.ReadInt32();
      magic2 = ReverseBytes(magic2);
      
      int numLabels = brLabels.ReadInt32();
      numLabels = ReverseBytes(numLabels);
 
      // each image
      for (int di = 0; di < numImages; ++di)
      {
        for (int i = 0; i < 28; ++i) // get 28x28 pixel values
        {
          for (int j = 0; j < 28; ++j)
          {
            byte b = brImages.ReadByte();
            pixels[i][j] = b;
          }
        }

        byte lbl = brLabels.ReadByte(); // get the label
        DigitImage dImage = new DigitImage(28, 28, pixels, lbl);
        result[di] = dImage;
      } // each image

      ifsPixels.Close(); brImages.Close();
      ifsLabels.Close(); brLabels.Close();

      return result;
    } // LoadData

    public static Bitmap MakeBitmap(DigitImage dImage, int mag)
    {
      // create a C# Bitmap suitable for display in a PictureBox control
      int width = dImage.width * mag;
      int height = dImage.height * mag;
      Bitmap result = new Bitmap(width, height);
      Graphics gr = Graphics.FromImage(result);
      for (int i = 0; i < dImage.height; ++i)
      {
        for (int j = 0; j < dImage.width; ++j)
        {
          int pixelColor = 255 - dImage.pixels[i][j]; // white background, black digits
          //int pixelColor = dImage.pixels[i][j]; // black background, white digits
          Color c = Color.FromArgb(pixelColor, pixelColor, pixelColor); // gray scale
          //Color c = Color.FromArgb(pixelColor, 0, 0); // red scale
          SolidBrush sb = new SolidBrush(c);
          gr.FillRectangle(sb, j * mag, i * mag, mag, mag); // fills bitmap via Graphics object
        }
      }
      return result;
    }

    public static string PixelValues(DigitImage dImage)
    {
      // create a string, with embedded newlines, suitable 
      // for display in a multi-line TextBox control
      string s = "";
      for (int i = 0; i < dImage.height; ++i)
      {
        for (int j = 0; j < dImage.width; ++j)
        {
          s += dImage.pixels[i][j].ToString("X2") + " ";
        }
        s += Environment.NewLine;
      }
      return s;
    }

    // ===================

    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.TextBox textBox2;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox textBox3;
    private System.Windows.Forms.TextBox textBox4;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox comboBox1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.TextBox textBox5;

    private void InitializeComponent()
    {
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.textBox3 = new System.Windows.Forms.TextBox();
      this.textBox4 = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.button2 = new System.Windows.Forms.Button();
      this.textBox5 = new System.Windows.Forms.TextBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 22);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(308, 20);
      this.textBox1.TabIndex = 0;
      this.textBox1.Text = "C:\\MnistViewer\\train-images.idx3-ubyte";
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(12, 48);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(308, 20);
      this.textBox2.TabIndex = 1;
      this.textBox2.Text = "C:\\MnistViewer\\train-labels.idx1-ubyte";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(326, 32);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(93, 23);
      this.button1.TabIndex = 2;
      this.button1.Text = "Load Images";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // textBox3
      // 
      this.textBox3.Enabled = false;
      this.textBox3.Location = new System.Drawing.Point(12, 91);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(62, 20);
      this.textBox3.TabIndex = 3;
      this.textBox3.Text = "NA";
      // 
      // textBox4
      // 
      this.textBox4.Location = new System.Drawing.Point(80, 91);
      this.textBox4.Name = "textBox4";
      this.textBox4.Size = new System.Drawing.Size(62, 20);
      this.textBox4.TabIndex = 4;
      this.textBox4.Text = "0";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 114);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(55, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Curr Index";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(77, 114);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(58, 13);
      this.label2.TabIndex = 6;
      this.label2.Text = "Next Index";
      // 
      // comboBox1
      // 
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
      this.comboBox1.Location = new System.Drawing.Point(159, 91);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(62, 21);
      this.comboBox1.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(156, 115);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(70, 13);
      this.label3.TabIndex = 8;
      this.label3.Text = "Magnification";
      // 
      // listBox1
      // 
      this.listBox1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.listBox1.FormattingEnabled = true;
      this.listBox1.HorizontalScrollbar = true;
      this.listBox1.ItemHeight = 14;
      this.listBox1.Location = new System.Drawing.Point(12, 443);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(280, 102);
      this.listBox1.TabIndex = 9;
      // 
      // pictureBox1
      // 
      this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlDark;
      this.pictureBox1.Location = new System.Drawing.Point(12, 143);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(280, 280);
      this.pictureBox1.TabIndex = 10;
      this.pictureBox1.TabStop = false;
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(245, 91);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 11;
      this.button2.Text = "Display Next";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // textBox5
      // 
      this.textBox5.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textBox5.Location = new System.Drawing.Point(307, 143);
      this.textBox5.Multiline = true;
      this.textBox5.Name = "textBox5";
      this.textBox5.Size = new System.Drawing.Size(606, 412);
      this.textBox5.TabIndex = 12;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(929, 570);
      this.Controls.Add(this.textBox5);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBox4);
      this.Controls.Add(this.textBox3);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.textBox1);
      this.Name = "Form1";
      this.Text = "MNIST Viewer";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    // ===================


  } // class Form1
 

  public class DigitImage
  {
    // an MNIST image of a '0' thru '9' digit
    public int width; // 28
    public int height; // 28
    public byte[][] pixels; // 0(white) - 255(black)
    public byte label; // '0' - '9'

    public DigitImage(int width, int height, byte[][] pixels, byte label)
    {
      this.width = width; this.height = height;
      this.pixels = new byte[height][];
      for (int i = 0; i < this.pixels.Length; ++i)
        this.pixels[i] = new byte[width];

      for (int i = 0; i < height; ++i)
        for (int j = 0; j < width; ++j)
          this.pixels[i][j] = pixels[i][j];

      this.label = label;
    }
  }

} // ns
