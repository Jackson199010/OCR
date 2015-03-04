#define TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using MYOCR.Exceptions;
using MYOCR.Binarization;
using MYOCR.Utils;
using MYOCR.Borders;

namespace MYOCR.Recognition
{
    /// <summary>
    /// Класс распознаваня сегментированных символов шаблонным методом
    /// </summary>
    public class PatternRecognition
    {
        #region Поля

        SortedList<char, string> SmallList;
        SortedList<char, string> BigList;
        SortedList<char, string> DigitList;
        SortedList<char, string> SpecSmallList;
        SortedList<char, string> SpecBigList;

        SortedList<char, byte[,]> GLMatrList;
        SortedList<char, int> GLDifList;

        readonly byte[,] Binarized;
        readonly List<Letter> Letters;
        readonly List<StringLine> StringLines;

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
        readonly string DataDirPath;
        
        /// Указывает была ли произведена загрука шаблонов шрифта в списки
        Patterns_Load Patterns_load;

        /// <summary>
        /// Средний размер строк
        /// </summary>
        double AverStringSize;

        #endregion

        /// <summary>
        /// Создает класс
        /// </summary>
        /// <param name="binar">Класс бинаризации</param>
        /// <param name="letrs">Продукт класса LettersBorders</param>
        /// <param name="stringbrdrs">Продукт класса LineBorders</param>
        public PatternRecognition(ABinarization binar, List<Letter> letrs, LineBorders lines)
        {
            this.Binarized = binar.GetBinarized();
            this.Letters = letrs;
            this.StringLines = lines.LineBordersList;

            this.SmallList = new SortedList<char, string>();
            this.SpecBigList = new SortedList<char, string>();
            this.SpecSmallList = new SortedList<char, string>();
            this.BigList = new SortedList<char, string>();
            this.DigitList = new SortedList<char, string>();

            this.GLDifList = new SortedList<char, int>();
            this.GLMatrList = new SortedList<char, byte[,]>();

            this.DataDirPath= Application.StartupPath + @"\Data\";
            Patterns_load = new Patterns_Load();
            this.AverStringSize = lines.AverStringSize;
         
        }

        #region Методы

        /// <summary>
        /// Загружает все необходимые шаблоны из файлов в списки
        /// </summary>
        /// <param name="font_dir_name">Имя шрифта</param>
        public void LoadLetterPatterns(FontName FName)
        {
            string font_dir_name="Non selected";
            string Font_Dir_Path;
            string dbResponse;
            string[] parts;
            StreamReader LetterDataBase = new StreamReader(this.DataDirPath + @"LetterDataBase.txt",Encoding.Default);
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
            }
            Font_Dir_Path = DataDirPath + font_dir_name+'\\';

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
                this.GLMatrList.Add(x.Key,RecognUtils.Load_Pattern_16(Font_Dir_Path+x.Value));
            }
            /// для малых букв
            foreach (var x in SmallList)
            {

                if(!GLMatrList.ContainsKey(x.Key)) 
                this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
            }

            /// для цифр
            foreach (var x in this.DigitList)
            {
                if (!GLMatrList.ContainsKey(x.Key)) 
                this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
            }

            /// для больших специальных букв
            foreach (var x in this.SpecBigList)
            {
                if (!GLMatrList.ContainsKey(x.Key)) 
                this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
            }

            /// для малых специальных букв
            foreach (var x in this.SpecSmallList)
            {
                if (!GLMatrList.ContainsKey(x.Key)) 
                this.GLMatrList.Add(x.Key, RecognUtils.Load_Pattern_16(Font_Dir_Path + x.Value));
            }

            #endregion

            Patterns_load.Load = true;
            Patterns_load.Font_Name = FName;
           
        }

        /// <summary>
        /// Распознает текст
        /// </summary>
        /// <returns>Строку с распознанным текстом</returns>
        public string Recognize()
        {
            if (!(this.Check_State.Load))
                throw new LetterRecognException("Перед распознаванием, сперва нужно загрузить шрифты LoadLetterPatterns");

            LetterRank Letter_rank;
            bool Next = false;
            Letter previous=Letters[0];
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
                /// для обычных букв распознаем одним методом
                if ((Letter_rank == LetterRank.Big) || (Letter_rank == LetterRank.Small))
                {
                    recognized_char = this.RecognizeLetter(this.XORLetter(letter, Letter_rank, 'ф', 'б','і','ї','й')); // 'ф', 'б' тоже как большие так и малые
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
        /// Распознать букву из списка XOR несовпадений
        /// </summary>
        /// <param name="XORLetrs">Список</param>
        /// <returns>Распознанная буква</returns>
        private char RecognizeLetter(SortedList<char, int> XORLetrs)
        {
            if (XORLetrs.Count == 0)
                throw new LetterRecognException("Матрица ошибок по XOR не должна быть пустой");
            char minc = XORLetrs.ElementAt(0).Key;
            int mind = XORLetrs.ElementAt(0).Value;

            foreach (var x in XORLetrs)
            {
                if (x.Value < mind) { mind = x.Value; minc = x.Key; }
            }
            return minc;

        }


        /// <summary>
        /// Распознать букву признаковым методом для пунктуации
        /// </summary>
        /// <param name="letter">Сегментированная буква</param>
        /// <param name="LRank">Тип буквы</param>
        /// <returns></returns>
        private char RecognizePunctuation(Letter letter, LetterRank LRank)
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
        /// Список несовпадений для сегмент. буквы по буквам шрифта
        /// </summary>
        /// <param name="letter">Сегментированная буква</param>
        /// <param name="LetterRank">Тип буквы</param>
        /// <param name="extra">Буквы которые обязательно должны быть проверены</param>
        /// <returns></returns>
        private SortedList<char, int> XORLetter(Letter letter, LetterRank LRank, params char[] extra)
        {
            byte[,] SegmentedLetterPattern = this.CreateSegmentedLetterPattern(letter);
          //  LetterRank LRank = this.DefineLetterRank(letter, this.DefineStringRank(this.StringLines[letter.StringParent]));
         //   LetterRank LRank = this.DefineLetterRank(letter, StringRank);
            SortedList<char, int> result = new SortedList<char, int>();

            // Подсчет ошибок для больших букв в общем
            if (LRank == LetterRank.Big)
            {
                // для больших
                foreach (var l in this.BigList)
                {
                      result.Add(l.Key,this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }

                // для больших специальных
                foreach (var l in this.SpecBigList)
                {
                   result.Add(l.Key, this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }

                // для цифр
                foreach (var l in this.DigitList)
                {
                    result.Add(l.Key, this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }

                /// обязательные для распознавания
                if(extra!=null)
                foreach (char ch in extra)
                {
                    if (!result.ContainsKey(ch))
                        result.Add(ch, this.CalculateXORMismatches(GLMatrList[ch], SegmentedLetterPattern));
                }
            }

            // Подсчет ошибок для больших малых в общем
            if (LRank == LetterRank.Small)
            {
                // для малых
                foreach (var l in this.SmallList)
                {
                   result.Add(l.Key, this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }

                // для больших специальных но малых
                foreach (var l in this.SpecBigList)
                {
                   result.Add(l.Key, this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }

                /// обязательные для распознавания
                if(extra!=null)
                foreach (char ch in extra)
                {
                    if (!result.ContainsKey(ch))
                        result.Add(ch, this.CalculateXORMismatches(GLMatrList[ch], SegmentedLetterPattern));
                }
            }

            // Дописать распознавание пунктуации
            if (LRank == LetterRank.DownPunctuation)
            {
                // для больших специальных но малых
                foreach (var l in this.SpecSmallList)
                {
                    result.Add(l.Key, this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }
            }

            if (LRank == LetterRank.UpPunctuation)
            {
                // для больших специальных но малых
                foreach (var l in this.SpecSmallList)
                {
                    result.Add(l.Key, this.CalculateXORMismatches(GLMatrList[l.Key], SegmentedLetterPattern));
                }
            }

            return result;
        }


        /// <summary>
        /// Производит свертку сегментированной буквы в бинарную матрицу для распознавания
        /// </summary>
        /// <param name="letter">Сегментированная буква</param>
        /// <param name="fill_rate">Коэф. поправки(1.99 умолч.) >1 учит больше значимых пикселей (для свертки)</param>
        /// <returns>Матрицу 16х16</returns>
        public byte[,] CreateSegmentedLetterPattern(Letter letter, double fill_rate = 1.99)
        {
            return RecognUtils.Create_Pattern_16(this.Binarized, letter, RecognUtils.Letter_16(letter), RecognUtils.FillRatio(letter, fill_rate, this.Binarized));
        }


        /// <summary>
        /// Подсчитывает кол-во несовпадений при наложении матриц друг на друга
        /// </summary>
        /// <param name="pattern1">матрица1</param>
        /// <param name="pattern2">матрица2</param>
        /// <returns>количество несовпадений</returns>
        private int CalculateXORMismatches(byte[,] pattern1, byte[,] pattern2)
        {
            int mism = 0;
            if ((pattern1.GetLength(0) != pattern2.GetLength(0)) || (pattern1.GetLength(1) != pattern2.GetLength(1)))
                throw new LetterRecognException("Матрици не совпадают по размерах, для операции XOR");
            
            for(int i=0;i<pattern1.GetLength(0);i++)
                for (int j = 0; j < pattern1.GetLength(1); j++)
                {
                    mism += pattern1[i, j] ^ pattern2[i, j];
                }
            return mism;
        }


        /// <summary>
        /// Вычисляет тип буквы (большая, малая , верх пунктуация, низ пунктуация)
        /// </summary>
        /// <param name="letter">буква</param>
        /// <param name="contains_big">тип строки: true-содержит большие буквы; false-не содержит</param>
        /// <returns>Перечисление указывающие на тип буквы</returns>
        private LetterRank DefineLetterRank(Letter letter, bool contains_big)
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
        private bool DefineStringRank(StringLine line, double ratio = 0.75)
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
        private double GetAverageLetterWidth(List<Letter> letts)
        {
            double res=0;
            foreach (var x in letts)
            {
                res += x.DownRight.X - x.UpLeft.X;
            }
            return res/letts.Count;
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
        private bool Space(out char space, double aver_letter_size, Letter previous, Letter current, double ratio = 1.8)
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
        public void Clear()
        {
            SmallList.Clear();
            BigList.Clear();
            DigitList.Clear();
            SpecSmallList.Clear();
            SpecBigList.Clear();

            this.GLDifList.Clear();
            this.GLMatrList.Clear();

            this.Patterns_load.Load = false;
        }

        #endregion

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
