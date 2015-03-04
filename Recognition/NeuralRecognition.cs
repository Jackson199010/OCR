//#define Rec8

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using MYOCR.Exceptions;
using MYOCR.Binarization;
using MYOCR.Utils;
using MYOCR.Borders;
using ArtificialNeural.Feedforward;
using ArtificialNeural.Feedforward.Train;
using ArtificialNeural.Feedforward.Train.Backpropagation;
using ArtificialNeural.NMatrix;

namespace MYOCR.Recognition
{

    /// <summary>
    /// Класс распознаваня сегментированных символов нейросетевым методом
    /// </summary>
    public class NeuralRecognition: Recogn
    {
        /// <summary>
        /// Список малых букв которым присвоен уникальный индекс
        /// </summary>
        List<char> GlobSmallIndices;

        /// <summary>
        /// Список больших букв которым присвоен уникальный индекс
        /// </summary>
        List<char> GlobBigIndices;

        /// <summary>
        /// Хранит матрицу буквы и индекс на идеальный массив
        /// </summary>
        struct InputIdealPair
        {
          public  byte[,] letter_matr;
            public int index;
        }

        /// <summary>
        /// Тренировочная пара для НС
        /// </summary>
        struct TrainingPair
        {
            public double[][] input;
            public double[][] ideal;
        }

        
        /// <summary>
        /// Список шаблонов больших букв и их индексов
        /// </summary>
        List<InputIdealPair> Big;

        /// <summary>
        /// Список шаблонов малых букв и их индексов
        /// </summary>
        List<InputIdealPair> Small;

        /// <summary>
        /// НС для распознавания больших букв
        /// </summary>
        FeedforwardNetwork Big_NN;

        /// <summary>
        /// НС для распознавания малых букв
        /// </summary>
        FeedforwardNetwork Small_NN;

        /// <summary>
        /// Были ли сети обучены
        /// </summary>
        TrainedState trained_state; 

        ProgressBar progress_bar;

        public NeuralRecognition(ABinarization binar, List<Letter> letrs, LineBorders lines, ProgressBar progressbarr1)
            : base(binar, letrs, lines)
        {
            this.GlobBigIndices = new List<char>();
            this.GlobSmallIndices = new List<char>();
            this.Big = new List<InputIdealPair>();
            this.Small = new List<InputIdealPair>();

            
#if Rec8 
            this.LoadLetterPatterns(FontName.TimesNewRoman8x8);   /// эксперемент с шаблонами 8х8
#else
           // this.LoadLetterPatterns(FontName.TimesNewRoman);
            this.LoadLetterPatterns(FontName.Arial);
#endif

            this.progress_bar = progressbarr1;
            trained_state=new TrainedState();

                      
        }

        /// <summary>
        /// Создает НС из указанных файлов. Файлы в папке NeuralData. Тут указать только имя!
        /// </summary>
        /// <param name="structure">Какой архитектуры НС строить(кол-во слоев, нейронов). Имя файла</param>
        /// <param name="memory">Память НС, ее веса</param>
        /// <param name="NeuralToCreate">Какую нейронную сеть создавать, для больших или малых букв</param>
        public void CreateNNFromFile(TrainingSet NeuralToCreate)
        {
            string path = this.DataDirPath + @"NeuralData\";
            StreamReader strct;
            StreamReader mem;
            StreamReader ind_ch;
          
            string s;
            double[] Memory; 
            
            switch (NeuralToCreate)
            {
                    //// Создание НС для Больших букв
                case TrainingSet.Big: 
                    {
                        Big_NN = new FeedforwardNetwork();
                        strct=new StreamReader(path+"Struct_Big.txt");
                        mem = new StreamReader(path+"Big.txt");
                        ind_ch = new StreamReader(path + "IdealBig.txt",Encoding.Default);
                        ///// создаем стр-ру НС 
                        try
                        {
                            while ((s = strct.ReadLine()) != null)
                            {
                                this.Big_NN.AddLayer(new FeedforwardLayer(Int32.Parse(s)));

                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new LetterRecognException("Невозможно создать сеть из файла Struct_Big.txt, файл содержит неверные данные");
                        }
                        finally
                        {
                            strct.Close();
                           
                        }

                        /// восстанавливаем память НС
                        List<double> list = new List<double>();
                        try
                        {
                            while ((s = mem.ReadLine()) != null)
                            {
                                list.Add(Double.Parse(s));
                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new LetterRecognException("Невозможно создать сеть из файла Big.txt + , файл содержит неверные данные");
                        }
                        finally
                        {
                            mem.Close();
                        }
                        Memory = new double[list.Count];
                        for (int i = 0; i < list.Count; i++)
                            Memory[i] = list[i];
                        MatrixCODEC.ArrayToNetwork(Memory, this.Big_NN);

                        /// список соответствий
                        this.GlobBigIndices.Clear();
                        while ((s = ind_ch.ReadLine()) != null)
                        {
                            this.GlobBigIndices.Add(Char.Parse(s));
                        }
                        ind_ch.Close();

                        this.trained_state.BigNN = true;

                        break;
                    }

                //// Создание НС для маленьких букв
                case TrainingSet.Small:
                    {
                        Small_NN = new FeedforwardNetwork();
                        strct = new StreamReader(path + "Struct_Small.txt");
                        mem = new StreamReader(path + "Small.txt");
                        ind_ch = new StreamReader(path + "IdealSmall.txt", Encoding.Default);
                        ///// создаем стр-ру НС 
                        try
                        {
                            while ((s = strct.ReadLine()) != null)
                            {
                                this.Small_NN.AddLayer(new FeedforwardLayer(Int32.Parse(s)));

                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new LetterRecognException("Невозможно создать сеть из файла Struct_Small.txt, файл содержит неверные данные");
                        }
                        finally
                        {
                            strct.Close();

                        }

                        /// восстанавливаем память НС
                        List<double> list = new List<double>();
                        try
                        {
                            while ((s = mem.ReadLine()) != null)
                            {
                                list.Add(Double.Parse(s));
                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new LetterRecognException("Невозможно создать сеть из файла Small.txt, файл содержит неверные данные");
                        }
                        finally
                        {
                            mem.Close();
                        }
                        Memory = new double[list.Count];
                        for (int i = 0; i < list.Count; i++)
                            Memory[i] = list[i];
                        MatrixCODEC.ArrayToNetwork(Memory, this.Small_NN);

                        /// список соответствий
                        this.GlobSmallIndices.Clear();
                        while ((s = ind_ch.ReadLine()) != null)
                        {
                            this.GlobSmallIndices.Add(Char.Parse(s));
                        }
                        ind_ch.Close();

                        this.trained_state.SmallNN = true;
                        break;
                    }
            }
        }
        

        /// <summary>
        /// Шрифт добавляется в тренировочный набор для НС
        /// </summary>
        /// <param name="Letter_list">Набор букв</param>
        /// <param name="GLMatrList">Матрици всех букв шрифта</param>
        /// <param name="trainingset">Тип тренировочного набора</param>
         void Add_to_Train(SortedList<char, string> Letter_list, SortedList<char, byte[,]> GLMatrList, TrainingSet trainingset)
        {
            switch (trainingset)
            {
                case TrainingSet.Small:
                    {
                        foreach (var l in Letter_list)
                        {
                            /// заносим букву в глобальный массив но один раз для формирования сталых индексов
                            if (!this.GlobSmallIndices.Contains(l.Key))
                            { this.GlobSmallIndices.Add(l.Key); }
                          InputIdealPair x =new InputIdealPair();
                          x.letter_matr = GLMatrList[l.Key];
                          x.index = this.GlobSmallIndices.IndexOf(l.Key);
                          this.Small.Add(x);
                        }
                        break;
                    }

                case TrainingSet.Big:
                    {
                        foreach (var l in Letter_list)
                        {
                            /// заносим букву в глобальный массив но один раз для формирования сталых индексов
                            if (!this.GlobBigIndices.Contains(l.Key))
                            { this.GlobBigIndices.Add(l.Key); }
                            InputIdealPair x = new InputIdealPair();
                            x.letter_matr = GLMatrList[l.Key];
                            x.index = this.GlobBigIndices.IndexOf(l.Key);
                            this.Big.Add(x);
                        }
                        break;
                    }
                    
            }
        }

        /// <summary>
        /// В тренировочный набор для НС добавляется конкретный символ вместе с его матрицей
        /// </summary>
        /// <param name="ch">Символ который добавляется</param>
        /// <param name="letter_matrix">Матрица символа</param>
        /// <param name="trainingset">Тип тренировочного набора куда попадет символ (Big/Small)</param>
         void Add_to_Train(char ch, byte[,] letter_matrix, TrainingSet trainingset)
         {
             switch (trainingset)
             {
                 case TrainingSet.Small:
                     {
                        
                             /// заносим букву в глобальный массив но один раз для формирования сталых индексов
                             if (!this.GlobSmallIndices.Contains(ch))
                             { this.GlobSmallIndices.Add(ch); }
                             InputIdealPair x = new InputIdealPair();
                             x.letter_matr = letter_matrix;
                             x.index = this.GlobSmallIndices.IndexOf(ch);
                             this.Small.Add(x);
                       
                         break;
                     }

                 case TrainingSet.Big:
                     {
                             /// заносим букву в глобальный массив но один раз для формирования сталых индексов
                             if (!this.GlobBigIndices.Contains(ch))
                             { this.GlobBigIndices.Add(ch); }
                             InputIdealPair x = new InputIdealPair();
                             x.letter_matr = letter_matrix;
                             x.index = this.GlobBigIndices.IndexOf(ch);
                             this.Big.Add(x);
                             break;
                     }

             }
         }

        /// <summary>
        /// Сформировать тренировочную пару для НС
        /// </summary>
        /// <param name="trainingset">Тренировочный набор</param>
        /// <param name="GlobIndices">Индексы для тренировочного набора</param>
        /// <returns></returns>
        TrainingPair MakeTrainingPair(List<InputIdealPair> trainingset, List<char> GlobIndices)
        {
            double[][] input=new double[trainingset.Count][];
            double[][] ideal=new double[trainingset.Count][];
            int inpsize=trainingset[0].letter_matr.GetLength(0)*trainingset[0].letter_matr.GetLength(1);
            for (int i = 0; i < trainingset.Count; i++)
            {
                input[i] = new double[inpsize];
                ideal[i] = new double[GlobIndices.Count];
            }

            /// переписываем из шрифтового набора в массив
            for (int i = 0; i < trainingset.Count; i++)
            {
                int ind = 0;
                for (int x = 0; x < trainingset[i].letter_matr.GetLength(0); x++)
                {
                    for (int y = 0; y < trainingset[i].letter_matr.GetLength(1); y++)
                    {
                        input[i][ind++] = trainingset[i].letter_matr[x, y];
                    }
                }
                ideal[i][trainingset[i].index] = 1;
            }

            TrainingPair result = new TrainingPair();
            result.input = input;
            result.ideal = ideal;
            return result;
        }

       /// <summary>
        /// Создает тренировочные наборы для НС(добавление шаболов поизводится тут)
       /// </summary>
       /// <param name="Big_set">Набор из больших букв и цифр</param>
       /// <param name="Small_set">Набор из маліх букв</param>
       void MakeTrainingSet(out TrainingPair Big_set, out TrainingPair Small_set)
        {
            if (!this.Check_State.Load)
                throw new LetterRecognException("Сперва нужо загрузить шрифты!");
/*
            this.Add_to_Train(this.BigList, this.GLMatrList, TrainingSet.Big);
             this.Add_to_Train(this.DigitList, this.GLMatrList, TrainingSet.Big);
             this.Add_to_Train(this.SpecBigList, this.GLMatrList, TrainingSet.Big);
           */
            this.Add_to_Train(this.SmallList, this.GLMatrList, TrainingSet.Small);
           /*
            this.Add_to_Train('п', this.GLMatrList['п'], TrainingSet.Small);
            this.Add_to_Train('м', this.GLMatrList['м'], TrainingSet.Small);
            this.Add_to_Train('и', this.GLMatrList['и'], TrainingSet.Small);
            this.Add_to_Train('с', this.GLMatrList['с'], TrainingSet.Small);
            this.Add_to_Train('л', this.GLMatrList['л'], TrainingSet.Small);
            this.Add_to_Train('в', this.GLMatrList['в'], TrainingSet.Small);
            this.Add_to_Train('о', this.GLMatrList['о'], TrainingSet.Small);
            this.Add_to_Train('а', this.GLMatrList['а'], TrainingSet.Small);
            this.Add_to_Train('р', this.GLMatrList['р'], TrainingSet.Small);
            this.Add_to_Train('і', this.GLMatrList['і'], TrainingSet.Small);
            this.Add_to_Train('т', this.GLMatrList['т'], TrainingSet.Small);
           */
           this.Add_to_Train('П', this.GLMatrList['П'], TrainingSet.Big);
            this.Add_to_Train('У', this.GLMatrList['У'], TrainingSet.Big);
          
             Big_set = MakeTrainingPair(this.Big, this.GlobBigIndices);
             Small_set = MakeTrainingPair(this.Small, this.GlobSmallIndices);
           
        }

       /// <summary>
       /// Обучить НС на одном из наборов
       /// </summary>
       /// <param name="trainingset">Набор для обучения</param>
       /// <param name="network">НС которая будет обучаться</param>
       void TrainNetwork(TrainingPair trainingset,ref FeedforwardNetwork network)
       {
           string path = this.DataDirPath + @"NeuralData\";
           StreamWriter erfile = new StreamWriter(path +"Error.txt", false, Encoding.Default);
#if Rec8 
        Train train = new Backpropagation(network, trainingset.input, trainingset.ideal, 0.35, 0);
#else
           Train train = new Backpropagation(network, trainingset.input, trainingset.ideal, 0.4, 0);
#endif     
           int cyles = 200;

           this.progress_bar.Maximum = 1;
           this.progress_bar.Maximum = cyles;
           this.progress_bar.Value = 1;
           this.progress_bar.Step = 1;
           int epoch = 1;
           do
           {
               this.progress_bar.PerformStep();
               train.Iteration();
               epoch++;
               erfile.WriteLine("Эпоха №" + epoch + " Ошибка:" + train.Error);
           } while ((epoch < cyles) && (train.Error > 0.0001));
           erfile.Close();
       }

        /// <summary>
        /// Создает сети из готовых наборов и тренирует их
        /// </summary>
        /// <param name="layers">Слои с соответствующим количеством нейронов</param>
       public void CreateAndTrainNN(TrainingSet type_neural)
       {
           TrainingPair bigset;
           TrainingPair smallset;
           /// создаем тренировочные наборы
           MakeTrainingSet(out bigset, out smallset);

           switch (type_neural)
           {
               case TrainingSet.Big:
                   {
#if Rec8
                       int inp_layer = 64;
                       int hiden_layer = 17;
                       Big_NN = new FeedforwardNetwork();
                       Big_NN.AddLayer(new FeedforwardLayer(inp_layer));
                       Big_NN.AddLayer(new FeedforwardLayer(hiden_layer));
                       Big_NN.AddLayer(new FeedforwardLayer(bigset.ideal.Length));
                      
                       Big_NN.Reset();
                       TrainNetwork(bigset, ref Big_NN);
                       this.trained_state.BigNN = true;
#else
                       int inp_layer = 256;
                       int hiden_layer = 17;
                       Big_NN = new FeedforwardNetwork();
                       Big_NN.AddLayer(new FeedforwardLayer(inp_layer));
                       Big_NN.AddLayer(new FeedforwardLayer(hiden_layer));
                       Big_NN.AddLayer(new FeedforwardLayer(bigset.ideal.Length));

                       Big_NN.Reset();
                       TrainNetwork(bigset, ref Big_NN);
                       this.trained_state.BigNN = true;
#endif
                       break;
                   }

               case TrainingSet.Small:
                   {
#if Rec8
                       int inp_layer = 64;
                       int hiden_layer = 20;
                       Small_NN = new FeedforwardNetwork();
                       Small_NN.AddLayer(new FeedforwardLayer(inp_layer));
                       Small_NN.AddLayer(new FeedforwardLayer(hiden_layer));

                       Small_NN.AddLayer(new FeedforwardLayer(smallset.ideal.Length));
                       Small_NN.Reset();
                       TrainNetwork(smallset, ref Small_NN);
                       this.trained_state.SmallNN = true;
#else
                         int inp_layer = 256;
                       int hiden_layer = 19;
                       Small_NN = new FeedforwardNetwork();
                       Small_NN.AddLayer(new FeedforwardLayer(inp_layer));
                       Small_NN.AddLayer(new FeedforwardLayer(hiden_layer));

                       Small_NN.AddLayer(new FeedforwardLayer(smallset.ideal.Length));
                       Small_NN.Reset();
                       TrainNetwork(smallset, ref Small_NN);
                       this.trained_state.SmallNN = true;
#endif
                       break;
                   }
           }


       }

        
        /// <summary>
        /// Дотренировует уже обученую НС.
        /// </summary>
        /// <param name="NeuralToLoad">Сеть которая будет дотренировываться</param>
        /// <param name="learnrate">Уровень обучения</param>
        /// <param name="total_epoches">Кол-во итераций</param>
       public void LoadAndTrain(TrainingSet NeuralToLoad, double learnrate, int total_epoches)
       {
           this.CreateNNFromFile(NeuralToLoad);
            TrainingPair bigset;
           TrainingPair smallset;
           /// создаем тренировочные наборы
           MakeTrainingSet(out bigset, out smallset);

           switch (NeuralToLoad)
           {

                   ///// Тренировка большой сети
               case TrainingSet.Big: 
                   { 
               string path = this.DataDirPath + @"NeuralData\";
           StreamWriter erfile = new StreamWriter(path +"Error.txt", false, Encoding.Default);
#if Rec8 
        Train train = new Backpropagation(network, trainingset.input, trainingset.ideal, 0.35, 0);
#else
           Train train = new Backpropagation(this.Big_NN, bigset.input, bigset.ideal, learnrate, 0);
#endif     
           int cyles = total_epoches;

           this.progress_bar.Maximum = 1;
           this.progress_bar.Maximum = cyles;
           this.progress_bar.Value = 1;
           this.progress_bar.Step = 1;
           int epoch = 1;
           do
           {
               this.progress_bar.PerformStep();
               train.Iteration();
               epoch++;
               erfile.WriteLine("Эпоха №" + epoch + " Ошибка:" + train.Error);
           } while ((epoch < cyles) && (train.Error > 0.0001));
           erfile.Close();
          
          break; 
          }


               ///// Тренировка малой сети
               case TrainingSet.Small:
                {

                    string path = this.DataDirPath + @"NeuralData\";
                    StreamWriter erfile = new StreamWriter(path + "Error.txt", false, Encoding.Default);
#if Rec8 
        Train train = new Backpropagation(network, trainingset.input, trainingset.ideal, 0.35, 0);
#else
                    Train train = new Backpropagation(this.Small_NN, smallset.input, smallset.ideal, learnrate, 0);
#endif
                    int cyles = total_epoches;

                    this.progress_bar.Maximum = 1;
                    this.progress_bar.Maximum = cyles;
                    this.progress_bar.Value = 1;
                    this.progress_bar.Step = 1;
                    int epoch = 1;
                    do
                    {
                        this.progress_bar.PerformStep();
                        train.Iteration();
                        epoch++;
                        erfile.WriteLine("Эпоха №" + epoch + " Ошибка:" + train.Error);
                    } while ((epoch < cyles) && (train.Error > 0.0001));
                    erfile.Close();

                    break; 
               
               }
           }
           

       }

        /// <summary>
        /// Сохраняет обученые сети в файлы 
        /// </summary>
       public void SaveNetworks(TrainingSet type_neural)
       {
           string path = this.DataDirPath + @"NeuralData\";
           switch (type_neural)
           {
               case TrainingSet.Big: 
                   {
                       StreamWriter bgideal = new StreamWriter(path + "IdealBig.txt", false, Encoding.Default);
                       foreach (var x in this.GlobBigIndices)
                           bgideal.WriteLine(x);
                       bgideal.Close();

                       if (!this.Trainded.BigNN)
                           throw new LetterRecognException("Что-бы сожнанить сети они должны быть сначала натренированы");
                       double[] big = MatrixCODEC.NetworkToArray(this.Big_NN);
                       StreamWriter file1 = new StreamWriter(path + "Big.txt", false, Encoding.Default);
                       for (int i = 0; i < big.Length; i++)
                           file1.WriteLine(big[i]);
                       file1.Close();

                       StreamWriter str_bg = new StreamWriter(path + "Struct_Big.txt", false, Encoding.Default);
                       foreach (var x in Big_NN.Layers)
                       {
                           str_bg.WriteLine(x.NeuronCount);
                       }
                       str_bg.Close();

                       break;
                   }

               case TrainingSet.Small:
                   {
                       StreamWriter smideal = new StreamWriter(path + "IdealSmall.txt", false, Encoding.Default);
                       foreach (var x in this.GlobSmallIndices)
                           smideal.WriteLine(x);
                       smideal.Close();

                       double[] sm = MatrixCODEC.NetworkToArray(this.Small_NN);
                       StreamWriter file = new StreamWriter(path + "Small.txt", false, Encoding.Default);
                       for (int i = 0; i < sm.Length; i++)
                           file.WriteLine(sm[i]);
                       file.Close();


                       StreamWriter file2 = new StreamWriter(path + "Struct_Small.txt", false, Encoding.Default);
                       foreach (var x in Small_NN.Layers)
                       {
                           file2.WriteLine(x.NeuronCount);
                       }
                       file2.Close();

                       break;
                   }
           }

       }

        /// <summary>
        /// Распознает подаваемую на вход букву
        /// </summary>
        /// <param name="letter">Буква</param>
        /// <param name="letter_rank">Тип буквы (большая, малая...)</param>
        /// <returns>Распознаную букву</returns>
       private char RecognizeLetter(Letter letter, LetterRank letter_rank)
       {
           switch (letter_rank)
           {
                   /// Большие буквы распознаются НС для больших букв
               case LetterRank.Big: 
                   {
                       double[] input = this.TransformToNNInp(letter);
                       double[] actual = this.Big_NN.ComputeOutputs(input);
                     
                       /// распознанная буква, будет самым максимальным сигналом
                       int maxind=0;
                       double max = actual[0];
                       for (int i = 0; i < actual.Length; i++)
                       {
                           if (actual[i] > max) { max = actual[i]; maxind = i; }
                       }
                       return this.GlobBigIndices[maxind];

                   }

               /// Малые буквы распознаются НС для малых букв
               case LetterRank.Small:
                   {
                       double[] input = this.TransformToNNInp(letter);
                       double[] actual = this.Small_NN.ComputeOutputs(input);

                       /// распознанная буква, будет самым максимальным сигналом
                       int maxind = 0;
                       double max = actual[0];
                       for (int i = 0; i < actual.Length; i++)
                       {
                           if (actual[i] > max) { max = actual[i]; maxind = i; }
                       }
                       return this.GlobSmallIndices[maxind];

                   }

            ///Знаки пунктуации распознаются другими методами
               case LetterRank.UpPunctuation:
                   {
                       return this.RecognizePunctuation(letter, LetterRank.UpPunctuation);
                   }

               case LetterRank.DownPunctuation:
                   {
                       return this.RecognizePunctuation(letter, LetterRank.DownPunctuation);
                   }
           }
           return '#';
       }

        /// <summary>
        /// Распознать текст. Нужно что-бы нейронные сети перел этим были обучены.
        /// </summary>
        /// <returns>Распознанный текст</returns>
       public override string Recognize()
       {
           if ( !(this.Trainded.BigNN) || !(this.Trainded.SmallNN) )
               throw new LetterRecognException("Перед распознаванием, нужно чтобы обе нейронные сети были обучены");

           LetterRank Letter_rank;
           bool Next = false;
           Letter previous = Letters[0];
           double AverLetterWidth = this.GetAverageLetterWidth(this.Letters);
           StringWriter RecognizedString = new StringWriter();
           char recognized_char;

           /// массив типов строк(содержит/не содержит большие буквы)
           bool[] StringRanks = new bool[StringLines.Count];
           {
               int index = 0;
               foreach (var line in StringLines)
               {
                   StringRanks[index] = this.DefineStringRank(line);
                   index++;
               }
           }

           //// просматриваем все буквы
           foreach (Letter letter in Letters)
           {
               // тип буквы
               Letter_rank = this.DefineLetterRank(letter, StringRanks[letter.StringParent]);
               // если нужно добавляем пробел
               if (Next)
               {
                   if (this.Space(out recognized_char, AverLetterWidth, previous, letter))
                   {
                       if (recognized_char == '#') RecognizedString.WriteLine();
                       else
                           RecognizedString.Write(recognized_char);
                   }
               }
               /// для обычных букв распознаем нейронными сетями
               if ((Letter_rank == LetterRank.Big) || (Letter_rank == LetterRank.Small))
               {
                   recognized_char = this.RecognizeLetter(letter, Letter_rank); // 'ф', 'б' тоже как большие так и малые
                   Next = true;
                   RecognizedString.Write(recognized_char);
               }
               else /// для пунктуации другим
               {
                   recognized_char = this.RecognizePunctuation(letter, Letter_rank);
                   Next = true;
                   RecognizedString.Write(recognized_char);
               }
               previous = letter;
           }
           return RecognizedString.ToString();  /// распознанный текст
       }

        /// <summary>
        /// Приводит букву к формату который можно подать на вход НС
        /// </summary>
        /// <param name="letter">Буква для трансформации</param>
        /// <returns>Массив на вход НС</returns>
       private double[] TransformToNNInp(Letter letter)
       {
           byte[,] matr=this.CreateSegmentedLetterPattern(letter);
           double[] result = new double[matr.GetLength(0) * matr.GetLength(1)];
           int ind = 0;
           for(int i=0;i<matr.GetLength(0);i++)
               for (int j = 0; j < matr.GetLength(1); j++)
               {
                   result[ind++] = matr[i, j];
               }
           return result;

       }

       /// <summary>
       /// Ощищает все списки текущего объекта, включая наборы шрифтов и обучающего множества
       /// </summary>
       public override void Clear()
        {
            SmallList.Clear();
            BigList.Clear();
            DigitList.Clear();
            SpecSmallList.Clear();
            SpecBigList.Clear();
            this.GLMatrList.Clear();
            this.Patterns_load.Load = false;

            this.Big.Clear();
            this.Small.Clear();
            this.GlobBigIndices.Clear();
            this.GlobSmallIndices.Clear();
            this.trained_state.BigNN = false;
            this.trained_state.SmallNN = false;
        }

        /// <summary>
        /// Указывает были ли обучены нейронные сети
        /// </summary>
        public TrainedState Trainded
        {
            get { return this.trained_state; }
        }



        ///////////////////////////////////////////////
        //////////  испытание шаблона 8x8 ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////
        ///////////////////////////////////////////////

#if Rec8
        /// <summary>
        /// Производит свертку сегментированной буквы в бинарную матрицу 8x8 для распознавания
        /// </summary>
        /// <param name="letter">Сегментированная буква</param>
        /// <param name="fill_rate">Коэф. поправки(1.99 умолч.) >1 учит больше значимых пикселей (для свертки)</param>
        /// <returns>Матрицу 8х8</returns>
        public override byte[,] CreateSegmentedLetterPattern(Letter letter, double fill_rate = 1.99)
        {
            return RecognUtils.Create_Pattern_8(this.Binarized, letter, RecognUtils.Letter_8(letter), RecognUtils.FillRatio(letter, fill_rate, this.Binarized));
        }
#endif
      
    }
}
