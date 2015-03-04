using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;
using MYOCR.Exceptions;

namespace MYOCR.Binarization
{
    public class BinarizationNo1Quick:ABinarization
    {
        
         Bitmap Image;
        bool isbinarized;
        byte[,] Binarized;

        /// <summary>
        /// Изображение которое нужно будет бинаризировать одним из методов
        /// </summary>
        /// <param name="image"></param>
        public BinarizationNo1Quick(Bitmap image)
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
            BitmapData bitmap_data;
            int color; /// значения цвета
            /// 
            unsafe
            {


                byte* first_pixel_ptr; // указатель на первый пиксель
                byte* pixel; // указатель на текущий пиксель
                byte byteperpixel = 4; //точка занимает 4 байта

                //// замораживает обьект в памяти
                bitmap_data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

                first_pixel_ptr = (byte*)(bitmap_data.Scan0).ToPointer();

                /// мин и макс цвета
                minc = *first_pixel_ptr;
                maxc = *first_pixel_ptr;
                // находим максимальный и минимальный цвет на изображении
                for (int i = 0; i < Image.Height/2; i++)
                {
                    for (int j = 0; j < Image.Width/2; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// вычисляем максимальный и минимальный цвет
                        color = *pixel;
                        if (color > maxc) maxc = color;
                        if (color < minc) minc = color;

                       
                    }

                }

                // бинаризируем изображение
                for (int i = 0; i < Image.Height; i++)
                {
                    for (int j = 0; j < Image.Width; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// сама бинаризация
                        color = *pixel;
                        if (color < Math.Round(2.0 * maxc / 3.0 + minc / 2.0))
                            result[i, j] = 1; /// черный
                        else
                            result[i, j] = 0; /// белый
                    }

                }

                /// размораживаем
                Image.UnlockBits(bitmap_data);
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
            BitmapData bitmap_data;
            int color; /// значения цвета
            /// 
            unsafe
            {


                byte* first_pixel_ptr; // указатель на первый пиксель
                byte* pixel; // указатель на текущий пиксель
                byte byteperpixel = 4; //точка занимает 4 байта

                //// замораживает обьект в памяти
                bitmap_data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

                first_pixel_ptr = (byte*)(bitmap_data.Scan0).ToPointer();

                /// мин и макс цвета
                minc = *first_pixel_ptr;
                maxc = *first_pixel_ptr;
                // находим максимальный и минимальный цвет на изображении
                for (int i = 0; i < Image.Height / 2; i++)
                {
                    for (int j = 0; j < Image.Width / 2; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// вычисляем максимальный и минимальный цвет
                        color = *pixel;
                        if (color > maxc) maxc = color;
                        if (color < minc) minc = color;


                    }

                }

                // бинаризируем изображение
                for (int i = 0; i < Image.Height; i++)
                {
                    for (int j = 0; j < Image.Width; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// сама бинаризация
                        color = *pixel;
                        if (color < Math.Round(2.0 * maxc / arg1 + minc / arg2))
                            result[i, j] = 1; /// черный
                        else
                            result[i, j] = 0; /// белый
                    }

                }

                /// размораживаем
                Image.UnlockBits(bitmap_data);
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
            int minc, maxc;
            BitmapData bitmap_data;
            int color; /// значения цвета
            /// 
            unsafe
            {


                byte* first_pixel_ptr; // указатель на первый пиксель
                byte* pixel; // указатель на текущий пиксель
                byte byteperpixel = 4; //точка занимает 4 байта

                //// замораживает обьект в памяти
                Rectangle bitmap_rect = new Rectangle(A.X, A.Y, W, H);
                bitmap_data = Image.LockBits(bitmap_rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

                first_pixel_ptr = (byte*)(bitmap_data.Scan0).ToPointer();

                /// мин и макс цвета
                minc = *first_pixel_ptr;
                maxc = *first_pixel_ptr;
                // находим максимальный и минимальный цвет на изображении
                for (int i = 0; i < bitmap_rect.Height; i++)
                {
                    for (int j = 0; j < bitmap_rect.Width; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// вычисляем максимальный и минимальный цвет
                        color = *pixel;
                        if (color > maxc) maxc = color;
                        if (color < minc) minc = color;


                    }

                }

                // бинаризируем изображение
                for (int i = 0; i < bitmap_rect.Height; i++)
                {
                    for (int j = 0; j < bitmap_rect.Width; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// сама бинаризация
                        color = *pixel;
                        if (color < Math.Round(2.0 * maxc / 3.0 + minc / 2.0))
                            result[i, j] = 1; /// черный
                        else
                            result[i, j] = 0; /// белый
                    }

                }

                /// размораживаем
                Image.UnlockBits(bitmap_data);
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
        public override void Binarize(Point A, Point B, double arg1, double arg2)
        {
            int H, W;
            H = Math.Abs(B.Y - A.Y);
            W = Math.Abs(B.X - A.X);
            byte[,] result = new byte[H, W];
            int minc, maxc;
            BitmapData bitmap_data;
            int color; /// значения цвета
            /// 
            unsafe
            {


                byte* first_pixel_ptr; // указатель на первый пиксель
                byte* pixel; // указатель на текущий пиксель
                byte byteperpixel = 4; //точка занимает 4 байта

                //// замораживает обьект в памяти
                Rectangle bitmap_rect = new Rectangle(A.X, A.Y, W, H);
                bitmap_data = Image.LockBits(bitmap_rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

                first_pixel_ptr = (byte*)(bitmap_data.Scan0).ToPointer();

                /// мин и макс цвета
                minc = *first_pixel_ptr;
                maxc = *first_pixel_ptr;
                // находим максимальный и минимальный цвет на изображении
                for (int i = 0; i < bitmap_rect.Height; i++)
                {
                    for (int j = 0; j < bitmap_rect.Width; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// вычисляем максимальный и минимальный цвет
                        color = *pixel;
                        if (color > maxc) maxc = color;
                        if (color < minc) minc = color;


                    }

                }

                // бинаризируем изображение
                for (int i = 0; i < bitmap_rect.Height; i++)
                {
                    for (int j = 0; j < bitmap_rect.Width; j++)
                    {
                        pixel = (bitmap_data.Stride * i) + (j * byteperpixel) + first_pixel_ptr;

                        /// сама бинаризация
                        color = *pixel;
                        if (color < Math.Round(2.0 * maxc / arg1 + minc / arg2))
                            result[i, j] = 1; /// черный
                        else
                            result[i, j] = 0; /// белый
                    }

                }

                /// размораживаем
                Image.UnlockBits(bitmap_data);
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
