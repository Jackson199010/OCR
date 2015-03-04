using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using MYOCR.Exceptions;

namespace MYOCR.Utils
{
    /// <summary>
    /// Содержит вспомогательные методы для распознаванмя(приведение к матрице и т.д.)
    /// </summary>
    public class RecognUtils
    {
        /// <summary>
        /// Находит коэф. заполнения по которому будет проводиться свертка буквы в матрицу
        /// </summary>
        /// <param name="letter">Буква</param>
        /// <param name="proc">Коэф. поправки(1.99 умолч.) >1 учит больше значимых пикселей</param>
        /// <param name="binarized">Бинаризированная матрица</param>
        /// <returns>Коэффициэнт заполнения</returns>
        public static double FillRatio(Letter letter, double proc, byte[,] binarized)
        {
            int black = 0;
            double result;
            for (int i = letter.UpLeft.Y; i <= letter.DownRight.Y; i++)
            {
                for (int j = letter.UpLeft.X; j <= letter.DownRight.X; j++)
                {
                    if (binarized[i, j] == 1) black++;
                }
            }
            int h=letter.DownRight.Y-letter.UpLeft.Y;
            int w=letter.DownRight.X-letter.UpLeft.X;
            result=black/Math.Max(1,h*w); // отношене кол-ва черных пикселей к общему кол-ву пикселей в букве
            result *= proc; // вносим поправку
            return result;
 
        }

        /// <summary>
        /// Разбивает квадратик с буквой на 16 частей
        /// </summary>
        /// <param name="letter">Буква</param>
        /// <returns>Массив координат точек </returns>
        public static Point[] Letter_16(Letter letter)
        {
            int h = letter.DownRight.Y - letter.UpLeft.Y;
            int w = letter.DownRight.X - letter.UpLeft.X;
            Point[] XY = new Point[17];
            // Разбиваем квадратик на 16 частей
            /// ДЛИННУ
            XY[0].X = 0;
            XY[16].X = w;
            XY[8].X = XY[16].X / 2;
            XY[4].X = XY[8].X / 2;
            XY[2].X = XY[4].X / 2;
            XY[1].X = XY[2].X / 2;
            XY[3].X = (XY[4].X + XY[2].X) / 2;
            XY[6].X = (XY[8].X + XY[4].X) / 2;
            XY[5].X = (XY[6].X + XY[4].X) / 2;
            XY[7].X = (XY[8].X + XY[6].X) / 2;
            XY[12].X = (XY[16].X + XY[8].X) / 2;
            XY[10].X = (XY[12].X + XY[8].X) / 2;
            XY[14].X = (XY[16].X + XY[12].X) / 2;
            XY[9].X = (XY[10].X + XY[8].X) / 2;
            XY[11].X = (XY[12].X + XY[10].X) / 2;
            XY[13].X = (XY[14].X + XY[12].X) / 2;
            XY[15].X = (XY[16].X + XY[14].X) / 2;

            /// ВЫСОТУ
            XY[0].Y = 0;
            XY[16].Y = h;
            XY[8].Y = XY[16].Y / 2;
            XY[4].Y = XY[8].Y / 2;
            XY[2].Y = XY[4].Y / 2;
            XY[1].Y = XY[2].Y / 2;
            XY[3].Y = (XY[4].Y + XY[2].Y) / 2;
            XY[6].Y = (XY[8].Y + XY[4].Y) / 2;
            XY[5].Y = (XY[6].Y + XY[4].Y) / 2;
            XY[7].Y = (XY[8].Y + XY[6].Y) / 2;
            XY[12].Y = (XY[16].Y + XY[8].Y) / 2;
            XY[10].Y = (XY[12].Y + XY[8].Y) / 2;
            XY[14].Y = (XY[16].Y + XY[12].Y) / 2;
            XY[9].Y = (XY[10].Y + XY[8].Y) / 2;
            XY[11].Y = (XY[12].Y + XY[10].Y) / 2;
            XY[13].Y = (XY[14].Y + XY[12].Y) / 2;
            XY[15].Y = (XY[16].Y + XY[14].Y) / 2;

            return XY;
                
        }

        /// <summary>
        /// Создайет шаблон 16х16 из буквы на изображении
        /// </summary>
        /// <param name="binarized">Бинаризированная матрица</param>
        /// <param name="letter">Буква</param>
        /// <param name="letter_16">Координатная сетка</param>
        /// <param name="fillRatio">Коэффициэнт заполнения</param>
        /// <returns>Шаблон буквы 16х16</returns>
        public static byte[,] Create_Pattern_16(byte[,] binarized, Letter letter, Point[] letter_16, double fillRatio)
        {
            int black,kh,kw;
            byte[,] Mass_16x16=new byte[16,16];
         // пробегаемся по всем ячейкам
        for(int kvi=0;kvi<16;kvi++)
         for (int kvj = 0; kvj < 16; kvj++)
         {
             black = 0;
             // тут уже в абсолютных координатах пробегаемся внутри каждой ячейки
             for(int i=letter_16[kvi].Y+letter.UpLeft.Y;i<=letter_16[kvi+1].Y+letter.UpLeft.Y;i++)
                 for (int j = letter_16[kvj].X + letter.UpLeft.X; j <= letter_16[kvj + 1].X + letter.UpLeft.X; j++)
                 {
                     if (binarized[i, j] == 1) black++;// счиаем кол-во черных пикселей
                 }
             kw=letter_16[kvj+1].X-letter_16[kvj].X;
             kh=letter_16[kvi+1].Y-letter_16[kvi].Y;
             // если отношение черных пикселей к общему их числу >proc то в матрицу стовим 1 иначе 0
             if (( (double)black / Math.Max(1, kh * kw) ) > fillRatio)
             { Mass_16x16[kvi, kvj] = 1; } else Mass_16x16[kvi, kvj] = 0;
         }
        return Mass_16x16;
        }

        /// <summary>
        ///  Создайет шаблон 16х16 из файла буквы
        /// </summary>
        /// <param name="Path_to_file">Путь к файлу буквы</param>
        /// <returns>Шаблон буквы 16х16</returns>
        public static byte[,] Load_Pattern_16(string Path_to_file)
        {
            StreamReader pattern = new StreamReader(Path_to_file, Encoding.Default);
            byte[,] Mass_16x16 = new byte[16, 16];
            string file_line;
            int index = 0;
            while ((file_line = pattern.ReadLine()) != null)
            {
                if (file_line.Length != 16)
                    throw new LetterRecognException("Размер файла неверен по ширине");

                for (int j = 0; j < 16; j++)
                {
                    if ((file_line[j] != '0') && (file_line[j] != '1'))
                        throw new LetterRecognException("В файле найден несвойственный ей символ" + file_line[j]);
                    if (file_line[j] == '0') Mass_16x16[index, j] = 0; else Mass_16x16[index, j] = 1;
                }
                index++;

                if(index==17)
                    throw new LetterRecognException("Размер файла неверен по высоте");
            }

            pattern.Close();
            return Mass_16x16;
        }



        /////////////////// letter 8x8 ////////////////////

        /// <summary>
        /// Разбивает квадратик с буквой на 8 частей
        /// </summary>
        /// <param name="letter">Буква</param>
        /// <returns>Массив координат точек </returns>
        public static Point[] Letter_8(Letter letter)
        {
            int h = letter.DownRight.Y - letter.UpLeft.Y;
            int w = letter.DownRight.X - letter.UpLeft.X;
            Point[] XY = new Point[9];
            // Разбиваем квадратик на 16 частей
            /// ДЛИННУ
            XY[0].X = 0;
            XY[8].X = w;
            XY[4].X = XY[8].X / 2;
            XY[2].X = XY[4].X / 2;
            XY[1].X = XY[2].X / 2;
            XY[3].X = (XY[4].X + XY[2].X) / 2;
            XY[6].X = (XY[8].X + XY[4].X) / 2;
            XY[5].X = (XY[6].X + XY[4].X) / 2;
            XY[7].X = (XY[8].X + XY[6].X) / 2;
         

            /// ВЫСОТУ
            XY[0].Y = 0;
            XY[8].Y = h;
            XY[4].Y = XY[8].Y / 2;
            XY[2].Y = XY[4].Y / 2;
            XY[1].Y = XY[2].Y / 2;
            XY[3].Y = (XY[4].Y + XY[2].Y) / 2;
            XY[6].Y = (XY[8].Y + XY[4].Y) / 2;
            XY[5].Y = (XY[6].Y + XY[4].Y) / 2;
            XY[7].Y = (XY[8].Y + XY[6].Y) / 2;
           
            return XY;

        }

        /// <summary>
        /// Создайет шаблон 16х16 из буквы на изображении
        /// </summary>
        /// <param name="binarized">Бинаризированная матрица</param>
        /// <param name="letter">Буква</param>
        /// <param name="letter_8">Координатная сетка</param>
        /// <param name="fillRatio">Коэффициэнт заполнения</param>
        /// <returns>Шаблон буквы 8х8</returns>
        public static byte[,] Create_Pattern_8(byte[,] binarized, Letter letter, Point[] letter_8, double fillRatio)
        {
            int black, kh, kw;
            byte[,] Mass_8x8 = new byte[8, 8];
            // пробегаемся по всем ячейкам
            for (int kvi = 0; kvi < 8; kvi++)
                for (int kvj = 0; kvj < 8; kvj++)
                {
                    black = 0;
                    // тут уже в абсолютных координатах пробегаемся внутри каждой ячейки
                    for (int i = letter_8[kvi].Y + letter.UpLeft.Y; i <= letter_8[kvi + 1].Y + letter.UpLeft.Y; i++)
                        for (int j = letter_8[kvj].X + letter.UpLeft.X; j <= letter_8[kvj + 1].X + letter.UpLeft.X; j++)
                        {
                            if (binarized[i, j] == 1) black++;// счиаем кол-во черных пикселей
                        }
                    kw = letter_8[kvj + 1].X - letter_8[kvj].X;
                    kh = letter_8[kvi + 1].Y - letter_8[kvi].Y;
                    // если отношение черных пикселей к общему их числу >proc то в матрицу стовим 1 иначе 0
                    if (((double)black / Math.Max(1, kh * kw)) > fillRatio)
                    { Mass_8x8[kvi, kvj] = 1; }
                    else Mass_8x8[kvi, kvj] = 0;
                }
            return Mass_8x8;
        }

        /// <summary>
        ///  Создайет шаблон 8х8 из файла буквы
        /// </summary>
        /// <param name="Path_to_file">Путь к файлу буквы</param>
        /// <returns>Шаблон буквы 8х8</returns>
        public static byte[,] Load_Pattern_8(string Path_to_file)
        {
            StreamReader pattern = new StreamReader(Path_to_file, Encoding.Default);
            byte[,] Mass_8x8 = new byte[8, 8];
            string file_line;
            int index = 0;
            while ((file_line = pattern.ReadLine()) != null)
            {
                if (file_line.Length != 8)
                    throw new LetterRecognException("Размер файла неверен по ширине");

                for (int j = 0; j < 8; j++)
                {
                    if ((file_line[j] != '0') && (file_line[j] != '1'))
                        throw new LetterRecognException("В файле найден несвойственный ей символ" + file_line[j]);
                    if (file_line[j] == '0') Mass_8x8[index, j] = 0; else Mass_8x8[index, j] = 1;
                }
                index++;

                if (index == 9)
                    throw new LetterRecognException("Размер файла неверен по высоте");
            }

            pattern.Close();
            return Mass_8x8;
        }




        /// <summary>
        /// Создает базу данных букв
        /// </summary>
        /// <param name="pattern_dir"></param>
        public static void CreateLetterDatabase(string pattern_dir)
        {
            string databse_path=Application.StartupPath+@"\Data\";
            StreamWriter LetterDatabase = new StreamWriter(databse_path+@"LetterDataBase.txt",false,Encoding.Default);
           
            string[] dirs = Directory.GetDirectories(databse_path+pattern_dir);
            string data_line;
            string dir_name;
            string file_name;
            string full_file_name;
            sbyte category = 0;

            foreach (var dir in dirs)
            {
                dir_name = GetDirName(dir);

                switch (dir_name)
                {
                    case "Big": { category = 0; break; }
                    case "Small": { category = 1; break; }
                    case "Digits": { category = 2; break; }
                    case "SpecialBig": { category = 3; break; }
                    case "SpecialSmall": { category = 4; break; }
                    default: { category = -1; break; } 
                }

                foreach(string file in Directory.GetFiles(dir))
                {
                    full_file_name = dir_name + '\\' + Path.GetFileName(file);
                    file_name =GetDirName( Path.GetFileNameWithoutExtension(file));
                    
                    data_line = String.Join(" ", file_name, full_file_name, category);
                    LetterDatabase.WriteLine(data_line);
                }
               
            }

            LetterDatabase.Close();

        }

        /// <summary>
        /// Возвращаяет имя папки из ее длянного имени(не должен иметь / вконце)
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static string GetDirName(string directory)
        {
            string[] parts = directory.Split('\\');
            return parts[parts.GetUpperBound(0)];
        }
    }
}
