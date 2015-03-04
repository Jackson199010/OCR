using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MYOCR.Binarization
{
      public abstract class ABinarization
    {
         
          
          /// <summary>
          /// Бинаризирует изображение по-умолчанию
          /// </summary>
         public abstract void Binarize();

          /// <summary>
          /// Бинаризирует изображение по передаваемым параметрам
          /// </summary>
          /// <param name="arg1">первый параметр</param>
          /// <param name="arg2">второй параметр </param>
         public abstract void Binarize(double arg1, double arg2);

          /// <summary>
          /// Бинаризирует выделенную область изображения по умолчанию
          /// </summary>
          /// <param name="A">Точка A</param>
          /// <param name="B">Точка B</param>
         public abstract void Binarize(Point A, Point B);

          /// <summary>
         /// Бинаризирует выделенную область изображения по передаваемым параметрам
          /// </summary>
         /// <param name="A">Точка A</param>
         /// <param name="B">Точка B</param>
         /// <param name="arg1">первый параметр</param>
         /// <param name="arg2">второй параметр </param>
         public abstract void Binarize(Point A, Point B, double arg1, double arg2);

          /// <summary>
          /// Очищает результат прежней бинаризации
          /// </summary>
         public abstract void Clear();

          /// <summary>
          /// Получить бинаризируванную матрицу где: true- черный цвет; false- белый;
          /// </summary>
          /// <returns></returns>
         public abstract byte[,] GetBinarized();

          /// <summary>
          /// Загрузить новое изображение
          /// </summary>
          /// <param name="image">изображение</param>
         public abstract void Reset(Bitmap image);

         public abstract int Height { get; }
         public abstract int Width { get; }

         public abstract byte this[uint r, uint c] { get; set; }


    }
}
