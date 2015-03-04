using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace MYOCR.Utils
{
    class Util
    {
        /// <summary>
        /// Сохранить двухмерный массив в файл в оригинально виде
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool PrintToFile<T>(T[,] array, string filename)
        {
            string fullname =Application.StartupPath+ @"\DebugFiles\" + filename;
            
            FileStream fs = new FileStream(fullname, FileMode.Create);
            StreamWriter fswr = new StreamWriter(fs);
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    fswr.Write(array[i, j]);
                }
                fswr.WriteLine();
            }

            fswr.Close();
            fs.Close();
            return true;
        }

        /// <summary>
        /// Нарисовать на картинке граници сегментированных строк
        /// </summary>
        /// <param name="img"></param>
        /// <param name="?"></param>
        public static Image DrawStringLines(Bitmap bmp, List<StringLine> Lines)
    {
           
           Graphics g = Graphics.FromImage(bmp);
          
          
           Pen penup = new Pen(Color.Blue);
           Pen pendown = new Pen(Color.Red);

           foreach (StringLine line in Lines)
           {
               Point startup=new Point(0,line.Up);
               Point finishup=new Point(bmp.Width,line.Up);
               g.DrawLine(penup, startup, finishup);
               
               Point startdown=new Point(0,line.Down);
               Point finishdown=new Point(bmp.Width,line.Down);
               g.DrawLine(pendown, startdown, finishdown);

           }
           return bmp;
       
       }

        /// <summary>
        /// Нарисовать на картинке подстроки
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="Lines"></param>
        /// <param name="pencolor"></param>
        /// <returns></returns>
        public static Image DrawSubLines(Bitmap bmp, List<StringLine> Lines, Color pencolor)
        {
            Graphics g = Graphics.FromImage(bmp);
            Pen pen = new Pen(pencolor);
            int[] sublines = new int[4];
          
            foreach (StringLine line in Lines)
            {
                sublines[0]=line.Middle;
                sublines[1] = line.AMidBtwUpaMid;
                sublines[2] = line.BMidBtwUpaA;
                sublines[3] = line.СMidBtwВaA;

                foreach (int coord in sublines)
                {
                    Point start = new Point(0, coord);
                    Point finish = new Point(bmp.Width, coord);
                    g.DrawLine(pen, start, finish);
                }

            }
            return bmp;

        }

        /// <summary>
        /// Выделяет буквы в квадратики на изображении
        /// </summary>
        /// <param name="bmp">Изображение</param>
        /// <param name="letters">Список букв</param>
        /// <returns></returns>
        public static Image DrawLetters(Bitmap bmp, List<Letter> letters)
        {
            Graphics g = Graphics.FromImage(bmp);
            Pen pen = new Pen(Color.Green);

            foreach (Letter let in letters)
                g.DrawRectangle(pen, let.UpLeft.X, let.UpLeft.Y, let.DownRight.X - let.UpLeft.X, let.DownRight.Y - let.UpLeft.Y);
            return bmp;
        }
    }
}
