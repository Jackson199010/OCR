using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MYOCR.Exceptions;
using MYOCR.Binarization;
using MYOCR.Utils;

namespace MYOCR.Borders
{
    /// <summary>
    /// Выделяет строки с текстом
    /// </summary>
    public class LineBorders
    {
        int h, w;
        byte[,] binarized;
        double[] matrixbrightness;
        double aver_string_size;
        List<StringLine> linebrdrs;


        /// <summary>
        /// Строит обьект на основе бинаризированной матрици изображения
        /// </summary>
        /// <param name="binarizedmatr">Матрица</param>
        public LineBorders(byte[,] binarizedmatr)
        {
            this.binarized = binarizedmatr;
            this.h = this.binarized.GetLength(0);
            this.w = this.binarized.GetLength(1);
            this.aver_string_size = 0;
        }

        /// <summary>
        /// Коэфициент черных точек в строке массив this.matrixbrightness
        /// </summary>
        void ComputeRowsBrightness()
        {
            this.matrixbrightness = new double[h];
            if (this.matrixbrightness.Length <= 4)
                throw new LineBordersException("Изображение слишком маленькое для нахождения в нем буквенных строк");
          
            /// Коэф для каждой строки вычисляется делением количества черных точек в строке
            /// на ширину строки
            for (int r = 0; r < this.h; r++)
            {

                for (int c = 0; c < this.w; c++)
                {
                    matrixbrightness[r] += this.binarized[r, c];
                }
                matrixbrightness[r] /= this.w;
           
            }
           //пусть две первых и две последних строчки белые (для определения граници буквенных линий)
            matrixbrightness[0] = 0;
            matrixbrightness[1] = 0;
            matrixbrightness[h - 1] = 0;
            matrixbrightness[h - 2] = 0;
        }

        /// <summary>
        /// Находит верхнюю и нижнюю граници всех возможных строк и псевдострок
        /// </summary>
        /// <param name="brdratio">Чем ниже коэф., тем пиксельные строки с меньшим кол-вом черных пикселей считаются
        /// частью буквенной строки(1.2) по_умолчанию </param>
        /// <returns>Список структур типа StringLine кот. содержат начала и концы буквенных строк  </returns>
        List<StringLine> FindAllLineBrdrs(double brdratio)
        {
            List<StringLine> result = new List<StringLine>();
            StringLine line = new StringLine();
            ComputeRowsBrightness();
            brdratio /= w;

            bool upbound=false;

            for (int i = 2; i < this.matrixbrightness.Length -2; i++)
            {
            
                /// ищем верхнюю границу буквенной строки
                if ((!upbound)&&(matrixbrightness[i] > brdratio) && (matrixbrightness[i + 1] > brdratio) 
                    && (matrixbrightness[i - 1] > brdratio) && (matrixbrightness[i - 2] > brdratio))
                {
                    line.Up = i;
                    upbound = true;
                }

                /// ищем нижнюю границу буквенной строки
                if((upbound)&&(matrixbrightness[i+1] < brdratio) && (matrixbrightness[i+2] < brdratio))
                {
                    line.Down = i;
                    upbound = false;

                    FindStringDividingLines(ref line);
                    line.Width = this.binarized.GetLength(1);
                    line.Height = line.Down - line.Up;
                    result.Add(line);
                }
          
            }
            return result;

        }

        /// <summary>
        /// Находит разделительные линии для буквенной строки
        /// </summary>
        /// <param name="strline">Буквенная строка</param>
        void FindStringDividingLines(ref StringLine strline)
        {
            strline.Middle = ((strline.Down - strline.Up) / 2) + strline.Up;
            strline.AMidBtwUpaMid = ((strline.Middle - strline.Up) / 2) + strline.Up;
            strline.BMidBtwUpaA = ((strline.AMidBtwUpaMid - strline.Up) / 2) + strline.Up;
            strline.СMidBtwВaA = ((strline.AMidBtwUpaMid - strline.BMidBtwUpaA) / 2) + strline.BMidBtwUpaA;
        }

        /// <summary>
        ///  Находит верхнюю и нижнюю граници буквенных строк, игнорируя строки с маленьким размером
        /// </summary>
        /// <param name="brdratio">Чем ниже коэф., тем пиксельные строки с меньшим кол-вом черных пикселей считаются
        /// частью буквенной строки(1.2) по_умолчанию </param>
        /// <param name="lineSizeRatio">Чем выше пар-р, тем меньшего размера игнорируются строки (3 по умолч)</param>
        /// <returns></returns>
        public List<StringLine> FindLineBrdrs(double brdratio=1.2, double lineSizeRatio=3.0)
        {
            List<StringLine> lineborders = FindAllLineBrdrs(brdratio);
            List<StringLine> result = new List<StringLine>();
                    
            aver_string_size = 0; // средний размер всех буквенных строк
            foreach (StringLine line in lineborders)
            {
                aver_string_size += line.Height;
            }
            aver_string_size /= lineborders.Count;
            double falsesize = aver_string_size / brdratio; /// размер меньше которого строки считаются ложными и удаляются
            
            //// удаляем ложные строки
            foreach (StringLine line in lineborders)
            {
                if ((line.Down - line.Up) > falsesize)
                {
                   result.Add(line);
                }
            }


            this.linebrdrs = result;

            if (linebrdrs.Count == 0)
                throw new LineBordersException("После отсеивания ложных строк, их вообще не осталость");

            aver_string_size = 0; // средний размер всех истинных буквенных строк находим заново
            foreach (StringLine line in linebrdrs)
            {
                aver_string_size += line.Height;
            }
            aver_string_size /= linebrdrs.Count;

            return result; 
                                                    
        }


        /// <summary>
        /// Возвращает средний размер всех буквенных строк
        /// </summary>
        public double AverStringSize
        {
            get 
            {
                 return this.aver_string_size;
                
            }
        }

        /// <summary>
        /// Список строк после последней процедуры нахождения строк
        /// </summary>
        public List<StringLine> LineBordersList
        { get { return this.linebrdrs; } }

        /// <summary>
        /// Бинаризированная матрица изображения
        /// </summary>
        /// <returns></returns>
        public byte[,] GetBinarized()
        {
            return this.binarized;
        }
    }
}
