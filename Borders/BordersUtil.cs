using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MYOCR.Exceptions;
namespace MYOCR.Borders
{
    /// <summary>
    /// Утилиты для выделения границ обьектов в бинаризированном изображении
    /// </summary>
    public class BordersUtil
    {
        /// <summary>
        /// Идентифицирует граници обьектов в бинаризированной матрице выставляя на их границах 2 
        /// </summary>
        /// <param name="binarized">Бинаризированный массив</param>
        /// <returns>Бинаризированный массив с 2-ми на границах обьектов</returns>
        public static byte[,] IdentifyObjBorders(byte[,] binarized)
        {
            int H,W;
            H=binarized.GetLength(0);
            W=binarized.GetLength(1);
            byte[,] objborders=new byte[H,W];

            /// выставляем на границах обьектов 2-ки
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {

                    if (binarized[i, j] == 0) objborders[i, j] = 0;
                    if (binarized[i, j] == 1)
                    {
                        if ((i - 1 < 0) || (i + 1 >= H) || (j - 1 < 0) || (j + 1 >= W)) // если выходим за рамки массива
                        { objborders[i, j] = 2; }
                        else
                            if ((binarized[i + 1, j] == 0) || (binarized[i, j + 1] == 0) || (binarized[i - 1, j] == 0) || (binarized[i, j - 1]) == 0)
                            { objborders[i, j] = 2; }
                            else
                                objborders[i, j] = 1;
                    }

                    if ((binarized[i, j] != 1) && (binarized[i, j] != 0))
                        throw new BorderException("В бинаризированнрй матрице содержится неверный символ " + binarized[i, j]);
                   
                }

            }

            return objborders;
              
        }
    }
}
