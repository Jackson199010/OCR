using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using MYOCR.Exceptions;
namespace MYOCR.Binarization
{
      
 
    public  class BinarizationNo2:ABinarization
    {
        Bitmap Image;
        bool isbinarized;
        byte[,] Binarized;

        /// <summary>
        /// Изображение которое нужно будет бинаризировать одним из методов
        /// </summary>
        /// <param name="image"></param>
        public BinarizationNo2(Bitmap image)
        {
            this.Image = new Bitmap(image);
            isbinarized = false;
        }


   
    #region Методы

        /// <summary>
        /// Получить яркость пикселя
        /// </summary>
        /// <param name="img">Изображение</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <returns></returns>
        double Getbr(Bitmap img,int x,int y)
        {
            Color c=img.GetPixel(x,y);
            double result = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
            return result;
        }

        /// <summary>
        /// Бинаризирует изображение по-умолчанию
        /// </summary>
        public override void Binarize()
        {
            double bri;
            byte[,] result = new byte[this.Image.Height, this.Image.Width];

            // бинаризируем изображение
            for (int i = 0; i < Image.Height; i++)
            {
                for (int j = 0; j < Image.Width; j++)
                {
                    bri = Getbr(this.Image, j, i); // яркость
                    if (bri >= Math.Round(255 / 1.5)) result[i, j] = 0;
                    else result[i, j] = 1;
                } 
                  
             }
            this.Binarized = result;
            isbinarized = true; // бинаризировано  
            
        }

        /// <summary>
        /// Бинаризирует изображение по передаваемым параметрам
        /// </summary>
        /// <param name="arg1">Порог. Чем выше, тем больше черных точек пропускается. 
        /// 1.5 по-умолчанию </param>
        /// <param name="arg2">не используется 0 </param>
        public override void Binarize(double arg1, double arg2)
        {
            double bri;
            byte[,] result = new byte[this.Image.Height, this.Image.Width];

            // бинаризируем изображение
            for (int i = 0; i < Image.Height; i++)
            {
                for (int j = 0; j < Image.Width; j++)
                {
                    bri = Getbr(this.Image, j, i); // яркость
                    if (bri >= Math.Round(255 / arg1)) result[i, j] = 0;
                    else result[i, j] = 1;
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
        public override void Binarize(Point A, Point B)
        {
            int H, W;
            H = Math.Abs(B.Y - A.Y);
            W = Math.Abs(B.X - A.X);
            byte[,] result = new byte[H, W];
            double bri;

             // бинаризируем изображение
            for (int i = A.Y,m=0; i < H + A.Y; i++,m++)
            {
                for (int j = A.X,n=0; j < W + A.X; j++,n++)
                {
                    bri = Getbr(this.Image, j, i); // яркость
                    if (bri >= Math.Round(255 / 1.5)) result[m, n] = 0;
                    else result[m, n] = 1;
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
        /// <param name="arg1">Порог. Чем выше, тем больше черных точек пропускается. 
        /// по-умолчанию 1.5</param>
        /// <param name="arg2">не используется 0  </param>
        public override void Binarize(Point A, Point B, double arg1, double arg2)
        {
            int H, W;
            H = B.Y - A.Y;
            W = B.X - A.X;
            byte[,] result = new byte[H, W];
            double bri;

             // бинаризируем изображение
            for (int i = A.Y, m = 0; i < H + A.Y; i++, m++)
            {
                for (int j = A.X, n = 0; j < W + A.X; j++, n++)
                {
                    bri = Getbr(this.Image, j, i); // яркость
                    if (bri >= Math.Round(255 / arg1)) result[m, n] = 0;
                    else result[m, n] = 1;
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
            if (this.Binarized != null)
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
        /// Получить бинаризируванную матрицу где: true- черный цвет; false- белый;
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
