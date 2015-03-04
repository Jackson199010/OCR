#define LEARN

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using MYOCR.Binarization;
using MYOCR.Utils;
using MYOCR.Borders;
using MYOCR.Recognition;


namespace MYOCR
{
   
    public partial class Form1 : Form
    {
        Image image1;
        Stopwatch timer;

        public Form1()
        {
                 
            InitializeComponent();
        }

        private void start_Click(object sender, EventArgs e)
        {
           //  image1 = pictureBox1.Image;
            if (this.image1 == null)
            {
                MessageBox.Show("Сперва нужно загрузить картинку");
                return;
            }

             Bitmap picture = new Bitmap(image1);


             timer = new Stopwatch();
             timer.Start();

            // RecognUtils.CreateLetterDatabase("Times New Roman");

            ABinarization binarize = new BinarizationNo1Quick(picture);
             binarize.Binarize();

             

            LineBorders linbrd = new LineBorders(binarize.GetBinarized());
            List<StringLine> stringbrdrs = linbrd.FindLineBrdrs(2,3);

            LettersBorders Segmentation = new LettersBorders(linbrd);
            Segmentation.ProceedSegmentation(2.875);
            Segmentation.Assemble_Letters(2.5);
            List<Letter> letters = Segmentation.GetLetters();

            NeuralRecognition Neuro = new NeuralRecognition(binarize, letters, linbrd,this.progressBar1);
#if LEARN
/*            
   //         Neuro.CreateAndTrainNN(TrainingSet.Small);

           Neuro.LoadAndTrain(TrainingSet.Small, 0.3, 5000);
           Neuro.SaveNetworks(TrainingSet.Small);
            /*
           Neuro.CreateAndTrainNN(TrainingSet.Big);
           Neuro.SaveNetworks(TrainingSet.Big);
            */
#else
            Neuro.CreateNNFromFile(TrainingSet.Small);
            Neuro.CreateNNFromFile(TrainingSet.Big);
          this.textBox1.Text = Neuro.Recognize();
#endif
            
            PatternRecognition Recognize= new PatternRecognition(binarize, letters, linbrd);
            Recognize.LoadLetterPatterns(FontName.TimesNewRoman);
            this.textBox1.Text = Recognize.Recognize();

            //// нарисовать вспомогательные линии
            foreach (int check in checkedListBox1.CheckedIndices)
            {
                switch (check)
                {
                    case 0: { pictureBox1.Image = Util.DrawStringLines(picture, stringbrdrs); break; }
                    case 1: { pictureBox1.Image = Util.DrawLetters(picture, letters); break; }
                    case 2: { pictureBox1.Image = Util.DrawSubLines(picture, stringbrdrs, Color.Tan); break; }
                }
            }





            timer.Stop();
            Util.PrintToFile<byte>(BordersUtil.IdentifyObjBorders(binarize.GetBinarized()), "borders.txt");
            label2.Text = timer.Elapsed.ToString();

        }

        private void load_Click(object sender, EventArgs e)
        {
            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.Filter = "Файлы изображений|*.bmp;*.jpg";
            if (opendialog.ShowDialog() != DialogResult.OK)
                return;
           pictureBox1.Image = Image.FromFile(opendialog.FileName);
           image1 = Image.FromFile(opendialog.FileName);
            
        }
    }
}
