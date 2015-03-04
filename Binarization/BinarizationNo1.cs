using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using MYOCR.Exceptions;

namespace MYOCR.Binarization
{
    public class BinarizationNo1:ABinarization
    {
        Bitmap Image;
        bool isbinarized;
        byte[,] Binarized; 

        /// <summary>
        /// Изображение которое нужно будет бинаризировать одним из методов
        /// </summary>
        /// <param name="image"></param>
        public BinarizationNo1(Bitmap image)
        {
            this.Image = new Bitmap(image);
            isbinarized = false;
        }

        #region Методы

        /// <summary>
        /// Бинаризирует изображение по-умолчанию
        /// </summary>
        public override void Binarize()
        {
            byte[,] result = new byte[this.Image.Height, this.Image.Width];
            int minc, maxc;
            minc = this.Image.GetPixel(0, 0).ToArgb(); // минимальный цвет по умолчанию на картинке
            maxc = this.Image.GetPixel(0, 0).ToArgb(); // максимальный цвет по умолчанию на картинке
        
            /// находим макс и мин цвет на половине изображения и считаем что остальная половина такая же
            for (int i = 0; i < (Image.Height / 2); i++)
            {
                for (int j = 0; j < (Image.Width / 2); j++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() > maxc) maxc = this.Image.GetPixel(j, i).ToArgb();
                    if (this.Image.GetPixel(j, i).ToArgb() < minc) minc = this.Image.GetPixel(j, i).ToArgb();
                }
            }

      // бинаризируем изображение
            for (int i = 0; i <Image.Height; i++)
            {
                for (int j = 0; j < Image.Width ; j++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() < Math.Round(2.0 * maxc / 3.0 + minc / 2.0))
                    {
                        result[i, j] = 1; /// черный
                    }
                    else
                        result[i, j] = 0; /// белый
                }
            }

            this.Binarized = result;
            isbinarized = true; // бинаризировано
        }


        /// <summary>
        /// Бинаризирует изображение по передаваемым параметрам
        /// </summary>
        /// <param name="arg1">первый параметр</param>
        /// <param name="arg2">второй параметр </param>
        public override void Binarize(double arg1, double arg2)
        {
            byte[,] result = new byte[this.Image.Height, this.Image.Width];
            int minc, maxc;
            minc = this.Image.GetPixel(0, 0).ToArgb(); // минимальный цвет по умолчанию на картинке
            maxc = this.Image.GetPixel(0, 0).ToArgb(); // максимальный цвет по умолчанию на картинке

            /// находим макс и мин цвет на половине изображения и считаем что остальная половина такая же
            for (int i = 0; i < (Image.Height / 2); i++)
            {
                for (int j = 0; j < (Image.Width / 2); j++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() > maxc) maxc = this.Image.GetPixel(j, i).ToArgb();
                    if (this.Image.GetPixel(j, i).ToArgb() < minc) minc = this.Image.GetPixel(j, i).ToArgb();
                }
            }

            // бинаризируем изображение
            for (int i = 0; i < Image.Height; i++)
            {
                for (int j = 0; j < Image.Width; j++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() < Math.Round(2.0 * maxc / arg1 + minc / arg2))
                    {
                        result[i, j] = 1; /// черный
                    }
                    else
                        result[i, j] = 0; /// белый
                }
            }

            this.Binarized = result;
            isbinarized = true; // бинаризировано
        }
       
        /// <summary>
        /// Бинаризирует выделенную область изображения по умолчанию
        /// </summary>
        /// <param name="A">Точка A</param>
        /// <param name="B">Точка B</param>
        public override void Binarize(System.Drawing.Point A, System.Drawing.Point B)
        {
            int H, W;
            H = B.Y - A.Y;
            W = B.X - A.X;
            byte[,] result = new byte[H, W];
            int minc, maxc;
            minc = this.Image.GetPixel(A.X,A.Y).ToArgb(); // минимальный цвет по умолчанию на картинке
            maxc = this.Image.GetPixel(A.X, A.Y).ToArgb(); // максимальный цвет по умолчанию на картинке

            /// находим макс и мин цвет на изображении. Если выделенная область, меньше 1/3 изображения,
            /// то макс и мин цвета находить на половине всего изоюражения
            int hh, ww,h0,w0;
            if (((this.Image.Height * this.Image.Width) / 3.0) > (H * W))
            { hh = this.Image.Height; ww = this.Image.Width; h0 = 0; w0 = 0; }
            else { hh = (H) + A.Y; ww = (W) + A.X; h0 = A.Y; w0 = A.X; }
            
            for (int i = h0; i < hh/2; i++)
            {
                for (int j = w0; j < ww / 2; j++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() > maxc) maxc = this.Image.GetPixel(j, i).ToArgb();
                    if (this.Image.GetPixel(j, i).ToArgb() < minc) minc = this.Image.GetPixel(j, i).ToArgb();
                }
            }

            // бинаризируем изображение
            for (int i = A.Y, m = 0; i < H + A.Y; i++, m++)
            {
                for (int j = A.X, n = 0; j < W + A.X; j++, n++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() < Math.Round(2.0 * maxc / 3.0 + minc / 2.0))
                    {
                        result[m, n] = 1; /// черный
                    }
                    else
                        result[m, n] = 0; /// белый
                }
            }

            this.Binarized = result;
            isbinarized = true; // бинаризировано
        }

        /// <summary>
        /// Бинаризирует выделенную область изображения по передаваемым параметрам
        /// </summary>
        /// <param name="A">Точка A</param>
        /// <param name="B">Точка B</param>
        /// <param name="arg1">первый параметр</param>
        /// <param name="arg2">второй параметр </param>
        public override void Binarize(System.Drawing.Point A, System.Drawing.Point B, double arg1, double arg2)
        {
            int H, W;
            H = B.Y - A.Y;
            W = B.X - A.X;
            byte[,] result = new byte[H, W];
            int minc, maxc;
            minc = this.Image.GetPixel(A.X, A.Y).ToArgb(); // минимальный цвет по умолчанию на картинке
            maxc = this.Image.GetPixel(A.X, A.Y).ToArgb(); // максимальный цвет по умолчанию на картинке

            /// находим макс и мин цвет на изображении. Если выделенная область, меньше 1/3 изображения,
            /// то макс и мин цвета находить на половине всего изоюражения
            int hh, ww, h0, w0;
            if (((this.Image.Height * this.Image.Width) / 3.0) > (H * W))
            { hh = this.Image.Height; ww = this.Image.Width; h0 = 0; w0 = 0; }
            else { hh = (H) + A.Y; ww = (W) + A.X; h0 = A.Y; w0 = A.X; }

            for (int i = h0; i < hh / 2; i++)
            {
                for (int j = w0; j < ww / 2; j++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() > maxc) maxc = this.Image.GetPixel(j, i).ToArgb();
                    if (this.Image.GetPixel(j, i).ToArgb() < minc) minc = this.Image.GetPixel(j, i).ToArgb();
                }
            }

            // бинаризируем изображение
            for (int i = A.Y, m = 0; i < H + A.Y; i++, m++)
            {
                for (int j = A.X, n = 0; j < W + A.X; j++, n++)
                {
                    if (this.Image.GetPixel(j, i).ToArgb() < Math.Round(2.0 * maxc / arg1 + minc / arg2))
                    {
                        result[m, n] = 1; /// черный
                    }
                    else
                        result[m, n] = 0; /// белый
                }
            }

            this.Binarized = result;
            isbinarized = true; // бинаризировано
        }

        /// <summary>
        /// Очищает результат прежней бинаризации
        /// </summary>
        public override void Clear()
        {
            if(this.Binarized!=null)
                for (int i = 0; i < this.Binarized.GetLength(0); i++)
                {
                    for (int j = 0; j < this.Binarized.GetLength(1); j++)
                    {
                        this.Binarized[i, j] = 0;
                    }
               
                }
            this.isbinarized = false;
        }

        /// <summary>
        /// Получить бинаризируванную матрицу где: true- черный цвет; false-белый;
        /// </summary>
        /// <returns>Бинаризированный массив или BinarizationException если бинаризация не происходила</returns>
        public override byte[,] GetBinarized()
        {
            byte[,] result = new byte[this.Binarized.GetLength(0), this.Binarized.GetLength(1)];
            if (this.isbinarized)
            {
                for (int i = 0; i < this.Binarized.GetLength(0); i++)
                {
                    for (int j = 0; j < this.Binarized.GetLength(1); j++)
                    {
                        result[i, j] = this.Binarized[i, j];
                    }

                }
                return result;
            }
            else
                throw new BinarizationException("Картинка не бинаризированная");
        }

        /// <summary>
        /// Загрузить новое изображение и удалить результаты прежней бинаризации
        /// </summary>
        /// <param name="image">изображение</param>
        public override void Reset(System.Drawing.Bitmap image)
        {
            this.Image = new Bitmap(image);
            isbinarized = false;
            this.Binarized = null;
        }

        #endregion

        #region Свойства
        /// <summary>
        /// Высота бинаризированной матрици
        /// </summary>
        public override int Height
        {
            get 
            {
                if (isbinarized)
                    return this.Binarized.GetLength(0);
                else
                    throw new BinarizationException("Картинка не бинаризированная");
            }
        }

        /// <summary>
        /// Ширина бинаризированной матрици
        /// </summary>
        public override int Width
        {
            get 
            {
                if (isbinarized)
                return this.Binarized.GetLength(1);  
                  else
                    throw new BinarizationException("Картинка не бинаризированная");
            }
            
        }
        #endregion

        /// <summary>
        /// Индексатор доступа к бинаризированной матрице
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override byte this[uint r, uint c]
        {
            get
            {
                if ((r > this.Height - 1) || (c > this.Width - 1))
                    throw new BinarizationException("Выход за приделы массива");
                else
                    return this.Binarized[r, c];
            }
            set
            {
                if ((r > this.Height - 1) || (c > this.Width - 1))
                    throw new BinarizationException("Выход за приделы массива");
                else
                    this.Binarized[r, c] = value;
            }
        }
    }
}
