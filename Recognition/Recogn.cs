using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MYOCR.Exceptions;
using MYOCR.Binarization;
using MYOCR.Utils;
using MYOCR.Borders;
using System.Windows.Forms;

namespace MYOCR.Recognition
{
    /// <summary>
    /// Представляет инструменты такие как формирование шаблонов и загрузка шрифтов 
    /// </summary>
    public abstract class Recogn
    {
   protected SortedList<char, string> SmallList;
   protected SortedList<char, string> BigList;
   protected SortedList<char, string> DigitList;
   protected SortedList<char, string> SpecSmallList;
   protected SortedList<char, string> SpecBigList;
   
   protected SortedList<char, byte[,]> GLMatrList;

   protected readonly byte[,] Binarized;
   protected readonly List<Letter> Letters;
   protected readonly List<StringLine> StringLines;

   /// <summary>
   /// Для парсинга строки из БД
   /// </summary>
   struct DataBaseLine
   {
       public char ch_letter;
       public string path_to_letter;
       public sbyte category;
   }

   /// <summary>
   /// Путь к папке Data/ (содержит LetterDataBase)
   /// </summary>
    protected readonly string DataDirPath;

   /// Указывает была ли произведена загрука шаблонов шрифта в списки
   protected Patterns_Load Patterns_load;

   /// <summary>
   /// Средний размер строк
   /// </summary>
   protected double AverStringSize;

        /// <summary>
        /// Создает класс
        /// </summary>
        /// <param name="binar">Класс бинаризации</param>
        /// <param name="letrs">Продукт класса LettersBorders</param>
        /// <param name="stringbrdrs">Продукт класса LineBorders</param>
        public Recogn(ABinarization binar, List<Letter> letrs, LineBorders lines)
        {
            this.Binarized = binar.GetBinarized();
            this.Letters = letrs;
            this.StringLines = lines.LineBordersList;

            this.SmallList = new SortedList<char, string>();
            this.SpecBigList = new SortedList<char, string>();
            this.SpecSmallList = new SortedList<char, string>();
            this.BigList = new SortedList<char, string>();
            this.DigitList = new SortedList<char, string>();

        
            this.GLMatrList = new SortedList<char, byte[,]>();

            this.DataDirPath= Application.StartupPath + @"\Data\";
            Patterns_load = new Patterns_Load();
            this.AverStringSize = lines.AverStringSize;
         
        }

                       
        /// <summary>
        /// Вычисляет тип буквы (большая, малая , верх пунктуация, низ пунктуация)
        /// </summary>
        /// <param name="letter">буква</param>
        /// <param name="contains_big">тип строки: true-содержит большие буквы; false-не содержит</param>
        /// <returns>Перечисление указывающие на тип буквы</returns>
        public LetterRank DefineLetterRank(Letter letter, bool contains_big)
        {
            // Если строка содержит большую букву
            /*
                 #region Основное
             if (contains_big)
             {
                 // eсли верх находится над подстрокой больших буквСMidBtwВaA, а низ под средней Middle, то это большая буква
                 if ((letter.UpLeft.Y < StringLines[letter.StringParent].СMidBtwВaA) && (letter.DownRight.Y > StringLines[letter.StringParent].Middle))
                 { return LetterRank.Big; }

                 // если и верх и низ находятся над средней то это верхняя пунктуация
                 if ((letter.UpLeft.Y < StringLines[letter.StringParent].Middle) && (letter.DownRight.Y < StringLines[letter.StringParent].Middle))
                 { return LetterRank.UpPunctuation; }

                 // если и верх и низ находятся под средней то это нижняя пунктуация
                 if ((letter.UpLeft.Y >= StringLines[letter.StringParent].Middle) && (letter.DownRight.Y >= StringLines[letter.StringParent].Middle))
                 { return LetterRank.DownPunctuation; }

                 // в остальных случаях считаем букву малой
                 else
                 { return LetterRank.Small; }

             }

             // Если строка больших букв не содержит
             else

             {
                 // если и верх и низ находятся над средней то это верхняя пунктуация
                 if ((letter.UpLeft.Y < StringLines[letter.StringParent].Middle) && (letter.DownRight.Y < StringLines[letter.StringParent].Middle))
                 { return LetterRank.UpPunctuation; }

                 // если и верх и низ находятся под средней то это нижняя пунктуация
                 if ((letter.UpLeft.Y >= StringLines[letter.StringParent].Middle) && (letter.DownRight.Y >= StringLines[letter.StringParent].Middle))
                 { return LetterRank.DownPunctuation; }

                 // в остальных случаях считаем букву малой
                 else
                 { return LetterRank.Small; }
             }
                 #endregion
             */


            #region Альтернативное

            if (contains_big)
            {
                // eсли верх находится над подстрокой больших букв BMidBtwUpaA, а низ под средней Middle, то это большая буква
                if ((letter.UpLeft.Y < StringLines[letter.StringParent].BMidBtwUpaA) && (letter.DownRight.Y > StringLines[letter.StringParent].Middle))
                { return LetterRank.Big; }

                // если и верх и низ находятся над средней то это верхняя пунктуация
                if ((letter.UpLeft.Y < StringLines[letter.StringParent].Middle) && (letter.DownRight.Y < StringLines[letter.StringParent].Middle))
                { return LetterRank.UpPunctuation; }

                // если и верх и низ находятся под средней то это нижняя пунктуация
                if ((letter.UpLeft.Y >= StringLines[letter.StringParent].Middle) && (letter.DownRight.Y >= StringLines[letter.StringParent].Middle))
                { return LetterRank.DownPunctuation; }

                // в остальных случаях считаем букву малой
                else
                { return LetterRank.Small; }

            }

            // Если строка больших букв не содержит
            else
            {
                // если и верх и низ находятся над средней то это верхняя пунктуация
                if ((letter.UpLeft.Y < StringLines[letter.StringParent].Middle) && (letter.DownRight.Y < StringLines[letter.StringParent].Middle))
                { return LetterRank.UpPunctuation; }

                // если и верх и низ находятся под средней то это нижняя пунктуация
                if ((letter.UpLeft.Y >= StringLines[letter.StringParent].Middle) && (letter.DownRight.Y >= StringLines[letter.StringParent].Middle))
                { return LetterRank.DownPunctuation; }

                // в остальных случаях считаем букву малой
                else
                { return LetterRank.Small; }
            }

            #endregion
        }

        /// <summary>
        /// Содержит ли строка большие буквы
        /// </summary>
        /// <param name="line">Строка</param>
        /// <param name="ratio">если отнош разм стр к средн разм строк мен 0.75 то не содер больш букв</param>
        /// <returns>true - если содержит</returns>
        public bool DefineStringRank(StringLine line, double ratio = 0.75)
        {
            if ((line.Height / this.AverStringSize) < ratio)
                return false;
            else return true;
        }

        /// <summary>
        /// Получить среднюю ширину всех букв 
        /// </summary>
        /// <param name="letts">Буква</param>
        /// <returns></returns>
        public double GetAverageLetterWidth(List<Letter> letts)
        {
            double res = 0;
            foreach (var x in letts)
            {
                res += x.DownRight.X - x.UpLeft.X;
            }
            return res / letts.Count;
        }

        /// <summary>
        /// Определяет находится ли пробел или новая строка между двумя буквами
        /// </summary>
        /// <param name="space">получаемый символ(' ' или '\n')</param>
        /// <param name="aver_letter_size">Средний размер букв</param>
        /// <param name="previous">Предыдущая буква</param>
        /// <param name="current">Следующая буква</param>
        /// <param name="ratio">Коэф на кот делится средний размер букв</param>
        /// <returns>true - если имеется пробел или новая строка</returns>
        public bool Space(out char space, double aver_letter_size, Letter previous, Letter current, double ratio = 1.8)
        {
            if (previous.StringParent == current.StringParent)
            {
                if ((current.UpLeft.X - previous.DownRight.X) > aver_letter_size / ratio)
                {
                    space = ' ';
                    return (true);
                }
            }
            else if (previous.StringParent != current.StringParent)
            {
                {
                    space = '#';
                    return (true);
                }
            }

            space = '~'; return false; // заглушка

        }

        /// <summary>
        /// Очищает списки от предыдущих шаблонов шрифта
        /// </summary>
        public abstract void Clear();
     /*   {
    
            SmallList.Clear();
            BigList.Clear();
            DigitList.Clear();
            SpecSmallList.Clear();
            SpecBigList.Clear();

            //this.GLDifList.Clear();
            this.GLMatrList.Clear();

            this.Patterns_load.Load = false;
       
        } */
        /// <summary>
        /// Распознать букву признаковым методом для пунктуации
        /// </summary>
        /// <param name="letter">Сегментированная буква</param>
        /// <param name="LRank">Тип буквы</param>
        /// <returns></returns>
        protected char RecognizePunctuation(Letter letter, LetterRank LRank)
        {
            char ch = '~';
            int h = letter.DownRight.Y - letter.UpLeft.Y;
            int w = letter.DownRight.X - letter.UpLeft.X;
            if (LRank == LetterRank.DownPunctuation)
            {
                if (Math.Abs(h - w) <= 4) ch = '.';
                else if ((Math.Abs(h - w) > 4) && (Math.Abs(h - w) < 10)) ch = ',';
                //if ((Abs(h-w)>=10) and (Abs(h-w)<28))
                else ch = '-';
            }
            if (LRank == LetterRank.UpPunctuation)
            {
                if ((Math.Abs(h - w) >= 10) && (Math.Abs(h - w) < 28)) ch = '-';
                //if ((Abs(h-w)>=10) and (Abs(h-w)<28))
                else ch = '\'';
            }
            return ch;
        }

        /// <summary>
        /// Загружает все необходимые шаблоны из файлов в списки
        /// </summary>
        /// <param name="font_dir_name">Имя шрифта</param>
        public void LoadLetterPatterns(FontName FName)
        {
            string font_dir_name = "Non selected";
            string Font_Dir_Path;
            string dbResponse;
            string[] parts;
            StreamReader LetterDataBase = new StreamReader(this.DataDirPath + @"LetterDataBase.txt", Encoding.Default);
            DataBaseLine dbLine = new DataBaseLine();

            // Очистка, пока может и ненужная (нужно вынести в метод очистки)
            SmallList.Clear();
            BigList.Clear();
            DigitList.Clear();
            SpecSmallList.Clear();
            SpecBigList.Clear();

            /// Инициализируем имя шрифта
            switch (FName)
            {
                case FontName.Arial: { font_dir_name = "Arial"; break; }
                case FontName.TimesNewRoman: { font_dir_name = "Times New Roman"; break; }
                case FontName.ArialBlack: { font_dir_name = "Arial Black"; break; }
                case FontName.CourierNew: { font_dir_name = "Courier New"; break; }
                case FontName.MSSansSerif: { font_dir_name = "MS Sans Serif"; break; }
                case FontName.TimesNewRoman8x8: { font_dir_name = "Times New Roman 8x8"; break; }
            }
            Font_Dir_Path = DataDirPath + font_dir_name + '\\';

            // Загружаем из БД в списки названия букв и относительные пути кним
            while ((dbResponse = LetterDataBase.ReadLine()) != null)
            {
                parts = dbResponse.Split(' ');
                dbLine.ch_letter = Char.Parse(parts[0]);
                dbLine.path_to_letter = parts[1];
                dbLine.category = SByte.Parse(parts[2]);

                // на основе категории из БД вычисляем пренадлежность буквы к списку
                switch (dbLine.category)
                {
                    case 0: { BigList.Add(dbLine.ch_letter, dbLine.path_to_letter); break; }
                    case 1: { SmallList.Add(dbLine.ch_letter, dbLine.path_to_letter); break; }
                    case 2: { DigitList.Add(dbLine.ch_letter, dbLine.path_to_letter); break; }
                    case 3: { SpecBigList.Add(dbLine.ch_letter, dbLine.path_to_letter); break; }
                    case 4: { SpecSmallList.Add(dbLine.ch_letter, dbLine.path_to_letter); break; }
                }
            }

            LetterDataBase.Close();

            #region Загрузка шаблонов из файлов

            /// для больших букв
            foreach (var x in BigList)
            {
                if (!GLMatrList.ContainsKey(x.Key))
                {
                    //// для загрузки 8х8 другой метод
                    if (FName == FontName.TimesNewRoman8x8)
                    {
                        this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_8(Font_Dir_Path + x.Value));
                    }
                    else
                        this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
                }
            }
            /// для малых букв
            foreach (var x in SmallList)
            {

                if (!GLMatrList.ContainsKey(x.Key))
                {
                    //// для загрузки 8х8 другой метод
                    if (FName == FontName.TimesNewRoman8x8)
                    {
                        this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_8(Font_Dir_Path + x.Value));
                    }
                    else
                    this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
                }
            }

            /// для цифр
            foreach (var x in this.DigitList)
            {
                if (!GLMatrList.ContainsKey(x.Key))
                {
                    //// для загрузки 8х8 другой метод
                    if (FName == FontName.TimesNewRoman8x8)
                    {
                        this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_8(Font_Dir_Path + x.Value));
                    }
                    else
                    this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
                }
            }

            /// для больших специальных букв
            foreach (var x in this.SpecBigList)
            {
                if (!GLMatrList.ContainsKey(x.Key))
                {
                      //// для загрузки 8х8 другой метод
                    if (FName == FontName.TimesNewRoman8x8)
                    { this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_8(Font_Dir_Path + x.Value)); }
                    else
                    this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
                }
            }

            /// для малых специальных букв
            foreach (var x in this.SpecSmallList)
            {
                if (!GLMatrList.ContainsKey(x.Key))
                {
                    //// для загрузки 8х8 другой метод
                    if (FName == FontName.TimesNewRoman8x8)
                    { this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_8(Font_Dir_Path + x.Value)); }
                    else
                    this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
                }
            }

            #endregion

            Patterns_load.Load = true;
            Patterns_load.Font_Name = FName;

        }
        /// <summary>
        /// Производит свертку сегментированной буквы в бинарную матрицу для распознавания
        /// </summary>
        /// <param name="letter">Сегментированная буква</param>
        /// <param name="fill_rate">Коэф. поправки(1.99 умолч.) >1 учит больше значимых пикселей (для свертки)</param>
        /// <returns>Матрицу 16х16</returns>
        public virtual byte[,] CreateSegmentedLetterPattern(Letter letter, double fill_rate = 1.99)
        {
            return RecognUtils.Create_Pattern_16(this.Binarized, letter, RecognUtils.Letter_16(letter), RecognUtils.FillRatio(letter, fill_rate, this.Binarized));
        }

        public abstract string Recognize();

        #region Свойства
        /// <summary>
        /// Состояние списков
        /// </summary>
        public Patterns_Load Check_State
        {
            get { return this.Patterns_load; }
        }

        #endregion

    }
}
