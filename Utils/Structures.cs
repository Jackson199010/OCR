using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MYOCR.Utils
{
    /// <summary>
    /// Имя шрифта
    /// </summary>
    public enum FontName { TimesNewRoman, Arial, ArialBlack, CourierNew, MSSansSerif, TimesNewRoman8x8 };

    /// <summary>
    /// Тип буквы (большая, маленькая, верхняя пунктуация, нижняя)
    /// </summary>
    public enum LetterRank { Big, Small, UpPunctuation, DownPunctuation };
    /// <summary>
    /// Были ли загружены шаблоны
    /// </summary>
    public struct Patterns_Load
    {
        /// <summary>
        /// Указывает была ли произведена загрука шаблонов шрифта в списки
        /// </summary>
        public bool Load{get; set;}
        /// <summary>
        /// Какой шрифт загружен
        /// </summary>
        public FontName Font_Name { get; set; }

        public override string ToString()
        {
            return String.Format("Шрифт " + (Load ? "Загружен" : "Не загружен") + Font_Name.ToString());
        }
    }

    /// <summary>
    /// Граници буквенной строки и ее внутренних разделительных линий
    /// </summary>
    public struct StringLine
    {
        /// <summary>
        /// Верхняя граница строки
        /// </summary>
        public int Up
        { get; set; }

        /// <summary>
        /// Нижняя граница строки
        /// </summary>
        public int Down
        { get; set; }

        /// <summary>
        /// Средина строки
        /// </summary>
        public int Middle
        { get; set; }

        /// <summary>
        /// Средина между срединой строки и ее верхом 
        /// </summary>
        public int AMidBtwUpaMid
        { get; set; }

        /// <summary>
        /// Средина между AMid и верхом строки
        /// </summary>
        public int BMidBtwUpaA
        { get; set; }

        /// <summary>
        /// Средина между BMid и AMid
        /// </summary>
        public int СMidBtwВaA
        { get; set; }

        /// <summary>
        /// Ширина строки
        /// </summary>
        public int Width
        { get; set; }

        /// <summary>
        /// Высота строки
        /// </summary>
        public int Height
        { get; set; }

                    

    }

    public struct Letter
    {
        /// <summary>
        /// Координаты верхней левой точки буквы
        /// </summary>
        public Point UpLeft { get; set; }

        /// <summary>
        /// Координаты нижней правой точки буквы
        /// </summary>
        public Point DownRight { get; set; }

        /// <summary>
        /// В какой строке находится буква
        /// </summary>
        public int StringParent { get; set; }

        /// <summary>
        /// Список координат контура буквы
        /// </summary>
        public List<Point> Letter_Shape { get; set; }
    }

    /// <summary>
    /// Тренировочные набоы для НС
    /// </summary>
   public enum TrainingSet { Small, Big };

    /// <summary>
    /// Были ли сети обучены
    /// </summary>
    public struct TrainedState
    {
        public bool BigNN;
        public bool SmallNN;
    }

}
