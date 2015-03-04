using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MYOCR.Utils;
using MYOCR.Exceptions;
using System.Drawing;

namespace MYOCR.Borders
{
    /// <summary>
    /// Предназначен для нахождения границ букв
    /// </summary>
   public class LettersBorders
    {

       byte[,] binarized;
       List<StringLine> stringlines;   /// сегментированные строки
       byte[,] binarized_with_borders;
       List<int> letters_in_string; // список количества букв в каждой строке
       List<Letter> Letters; // список крайних точек букв


       /// <summary>
       /// Нахождение границ букв в сегментированных строках на бинаризированной матрице
       /// </summary>
       /// <param name="binarizedmatr">Бинаризированная матрица</param>
       /// <param name="lines">Список сегментированных строк</param>
       public LettersBorders(byte[,] binarizedmatr,List<StringLine> lines)
       {
           this.binarized = binarizedmatr;
           this.binarized_with_borders = BordersUtil.IdentifyObjBorders(binarizedmatr);
           this.stringlines = lines;
           this.Letters=new List<Letter>();


           letters_in_string = new List<int>();

           
       }

       /// <summary>
       /// Строит обьект на основе обьекта сегментации строк
       /// </summary>
       /// <param name="lineborders"></param>
       public LettersBorders(LineBorders lineborders)
       {
           this.binarized = lineborders.GetBinarized();
           this.binarized_with_borders = BordersUtil.IdentifyObjBorders(this.binarized);
           this.stringlines = lineborders.LineBordersList;
           this.Letters = new List<Letter>();

           letters_in_string = new List<int>();
       }

        #region Методы

       /// <summary>
       /// Количество букв в строке(строки начинаются с 0). Если указанной строки нет то -1 
       /// </summary>
       /// <param name="string_line">Номер строки (начиная с 0)</param>
       /// <returns></returns>
       public int CountLettersInString(uint string_line)
       {
           if ((string_line >= this.letters_in_string.Count))
               return -1;
           else
               return this.letters_in_string[(int)string_line];

       }

       /// <summary>
       /// Выполняет сегментацию без фильтрации от "ложных" букв
       /// </summary>
       public void ProceedSegmentation()
       {
           int curr_string; // текущая строка в кот. сегментируются буквы
           bool kontur = false; // индикатор контура буквы
           bool letter_proceeded = false; ;  // индикатор обработки буквы
           bool next_point_founded = false; // найдена ли следующая точка к просмотру
           Point Next_Letter_point = new Point(0, 0); // следующая точка к просмотру
           Point Curr_Letter_point = new Point(0, 0); ; // точка кот. пренадлежит букве
           int Str_Width, Str_Height; // высота и ширина текущей строки
           List<Point> Letter_Coords = new List<Point>();  // список точек которые составляют контур буквы
           Stack<Point> stack = new Stack<Point>(); // стек для хранения точек контура
           bool stackflag = false;

          for (curr_string = 0; curr_string < this.stringlines.Count; curr_string++)  //// проход по всем строкам
           {
               Str_Width = stringlines[curr_string].Width; Str_Height = stringlines[curr_string].Height;
               letters_in_string.Add(curr_string);
               /// проход по каждой колонке строки
               for (int w = 1; w < stringlines[curr_string].Width - 1; w++)
               {
                   /// проход по каждому элементу в колонке(сверху вниз) строки
                   for (int h = stringlines[curr_string].Up + 1; h < stringlines[curr_string].Down; h++)
                   {
                       /// если встретилась 2, то значит предп это контур, смотрим 8 соседних символов вокруг этой 2
                       if (binarized_with_borders[h, w] == 2) { kontur = true; Curr_Letter_point = new Point(w, h); }

                       #region Просмотр контура буквы
                       while (kontur) // пока в букве
                       {
                           ///////////////////////////////////////////////////////////////////////////////
                           ////////////////// Просмотр 8-ми точек окружности вокруг найденой 2-ки /////////
                           ////////////////////////////////////////////////////////////////////////////////


                           int y0 = Math.Max(Curr_Letter_point.Y - 1, 0), y1 = Math.Min(Curr_Letter_point.Y + 1, stringlines[curr_string].Down - 1);
                           int x0 = Math.Max(Curr_Letter_point.X - 1, 0), x1 = Math.Min(Curr_Letter_point.X + 1, Str_Width - 1);
                           for (int y = y0; y <= y1; y++)
                               for (int x = x0; x <= x1; x++)
                               {
                                   /// просмотр центральной точки


                                   if ((y == Curr_Letter_point.Y) && (x == Curr_Letter_point.X)
                                       && (binarized_with_borders[y, x] == 2))  /// центральная точка
                                   {
                                       Letter_Coords.Add(Curr_Letter_point); // заносим ее как одну из точек граници буквы
                                       binarized_with_borders[y, x] = 3; // 3 -озн. что точка уже проверенная
                                       continue;
                                   }

                                   /// если это уже не первая встречная 2-ка то она заносится в стек
                                   if ((binarized_with_borders[y, x] == 2) && (stackflag))
                                   {
                                       stack.Push(new Point(x, y));
                                   }

                                   /// если это первая встречная 2-ка то устанавливаем ее к следующую центральную
                                   if ((binarized_with_borders[y, x] == 2) && (!stackflag))
                                   {
                                       Next_Letter_point = new Point(x, y); next_point_founded = true; stackflag = true;
                                   }
                               }
                           ///////////////////////////////////////////////////////////
                           ///////////////////////////////////////////////////////////

                           // если 2 не встречалось, то смотрим стэк, если он не пуст, то след центр точкой будет последняя точка их стэка
                           if ((!next_point_founded) && (stack.Count != 0))
                           { Curr_Letter_point = stack.Pop(); }

                           // если 2 не встречалось и стэк пуст, то мы прошли весь контур
                           else if ((!next_point_founded) && (stack.Count == 0))
                           {
                               kontur = false;
                               letter_proceeded = true; // буква обработана
                           }

                           /// если вокруг центральной были 2-ки, то устанавливаем ее как следующею централную
                           else if (next_point_founded)
                           {
                               Curr_Letter_point = Next_Letter_point;
                               next_point_founded = false;
                               stackflag = false;
                           }




                           ///////////////////////////////////////////////////////////////////////////////
                           ////////////////// Конец блока просмотров 8-ми точек //////////////////////////
                           ///////////////////////////////////////////////////////////////////////////////
                       }
                       #endregion

                     #region Обработка буквы
                       // если координаты буквы найдены то ищем крайние точки символа, и заносим букву в список букв
                       if ((letter_proceeded)&&(!kontur))
                       {
                           Letter letter = new Letter();

                           /// крайние точки
                         
                           int minx = Letter_Coords[0].X, miny=Letter_Coords[0].Y;
                           int maxx = Letter_Coords[0].X, maxy = Letter_Coords[0].Y;
                           foreach (Point lp in Letter_Coords)
                           {
                               if (lp.X > maxx) maxx = lp.X;
                               if (lp.Y > maxy) maxy = lp.Y;
                               if (lp.X < minx) minx = lp.X;
                               if (lp.Y < miny) miny = lp.Y;
                           }

                           letter.UpLeft = new Point(minx,miny);
                           letter.DownRight = new Point(maxx,maxy);
                           letter.Letter_Shape = Letter_Coords;  // контур буквы
                           letter.StringParent = curr_string; // строка в кот находятся буквы

                            letters_in_string[curr_string]++; // кол-во букв в каждой строке
                            this.Letters.Add(letter); // заносим букву в главный список

                            Letter_Coords.Clear();
                            stack.Clear();
                            letter_proceeded = false;
                            next_point_founded = false;
                            stackflag = false;


                       }

                     #endregion


                   }

                   
               }
           }
       }

         /// <summary>
       /// Выполняет сегментацию с фильтрацией от "ложных" букв
       /// </summary>
       /// <param name="false_letter_ratio">Чем ниже коэф тем выше размер ложной буквы(2,875 по-умолчанию)</param>
       public void ProceedSegmentation(double false_letter_ratio)
       {
           int curr_string; // текущая строка в кот. сегментируются буквы
           bool kontur = false; // индикатор контура буквы
           bool letter_proceeded = false; ;  // индикатор обработки буквы
           bool next_point_founded = false; // найдена ли следующая точка к просмотру
           Point Next_Letter_point = new Point(0, 0); // следующая точка к просмотру
           Point Curr_Letter_point = new Point(0, 0); ; // точка кот. пренадлежит букве
           int Str_Width, Str_Height; // высота и ширина текущей строки
           List<Point> Letter_Coords = new List<Point>();  // список точек которые составляют контур буквы
           Stack<Point> stack = new Stack<Point>(); // стек для хранения точек контура
           bool stackflag = false;

           double threshold = Math.Max(0.1, false_letter_ratio); /// порог для фильтрации ложных букв 

           for (curr_string = 0; curr_string < this.stringlines.Count; curr_string++)  //// проход по всем строкам
           {
               Str_Width = stringlines[curr_string].Width; Str_Height = stringlines[curr_string].Height;
               letters_in_string.Add(curr_string);
               /// проход по каждой колонке строки
               for (int w = 1; w < stringlines[curr_string].Width - 1; w++)
               {
                   /// проход по каждому элементу в колонке(сверху вниз) строки
                   for (int h = stringlines[curr_string].Up + 1; h < stringlines[curr_string].Down; h++)
                   {
                       /// если встретилась 2, то значит предп это контур, смотрим 8 соседних символов вокруг этой 2
                       if (binarized_with_borders[h, w] == 2) { kontur = true; Curr_Letter_point = new Point(w, h); }

                       #region Просмотр контура буквы
                       while (kontur) // пока в букве
                       {
                           ///////////////////////////////////////////////////////////////////////////////
                           ////////////////// Просмотр 8-ми точек окружности вокруг найденой 2-ки /////////
                           ////////////////////////////////////////////////////////////////////////////////


                           int y0 = Math.Max(Curr_Letter_point.Y - 1, 0), y1 = Math.Min(Curr_Letter_point.Y + 1, stringlines[curr_string].Down - 1);
                           int x0 = Math.Max(Curr_Letter_point.X - 1, 0), x1 = Math.Min(Curr_Letter_point.X + 1, Str_Width - 1);
                           for (int y = y0; y <= y1; y++)
                               for (int x = x0; x <= x1; x++)
                               {
                                   /// просмотр центральной точки

                                 
                                   if ((y == Curr_Letter_point.Y) && (x == Curr_Letter_point.X)
                                       && (binarized_with_borders[y, x] == 2))  /// центральная точка
                                   {
                                       Letter_Coords.Add(Curr_Letter_point); // заносим ее как одну из точек граници буквы
                                       binarized_with_borders[y, x] = 3; // 3 -озн. что точка уже проверенная
                                       continue;
                                   }

                                   /// если это уже не первая встречная 2-ка то она заносится в стек
                                   if ((binarized_with_borders[y, x] == 2) && (stackflag))
                                   {
                                       stack.Push(new Point(x, y));
                                   }

                                   /// если это первая встречная 2-ка то устанавливаем ее к следующую центральную
                                   if ((binarized_with_borders[y, x] == 2) && (!stackflag))
                                   {
                                       Next_Letter_point = new Point(x, y); next_point_founded = true; stackflag = true;
                                   }
                               }
                                   ///////////////////////////////////////////////////////////
                                   ///////////////////////////////////////////////////////////
                               
                                    // если 2 не встречалось, то смотрим стэк, если он не пуст, то след центр точкой будет последняя точка их стэка
                                   if ((!next_point_founded) && (stack.Count != 0))
                                   { Curr_Letter_point = stack.Pop(); }

                                   // если 2 не встречалось и стэк пуст, то мы прошли весь контур
                                   else if ((!next_point_founded) && (stack.Count == 0))
                                   {
                                       kontur = false;
                                       letter_proceeded = true; // буква обработана
                                   }

                                   /// если вокруг центральной были 2-ки, то устанавливаем ее как следующею централную
                                   else if (next_point_founded)
                                   { 
                                       Curr_Letter_point = Next_Letter_point;
                                       next_point_founded = false;
                                       stackflag = false;
                                   }

                                   
                               

                           ///////////////////////////////////////////////////////////////////////////////
                           ////////////////// Конец блока просмотров 8-ми точек //////////////////////////
                           ///////////////////////////////////////////////////////////////////////////////
                       }
                       #endregion

                       #region Обработка буквы
                       // если координаты буквы найдены то ищем крайние точки символа, и заносим букву в список букв
                       if ((letter_proceeded)&&(!kontur))
                       {
                           Letter letter = new Letter();

                           /// крайние точки
                         
                           int minx = Letter_Coords[0].X, miny=Letter_Coords[0].Y;
                           int maxx = Letter_Coords[0].X, maxy = Letter_Coords[0].Y;
                           foreach (Point lp in Letter_Coords)
                           {
                               if (lp.X > maxx) maxx = lp.X;
                               if (lp.Y > maxy) maxy = lp.Y;
                               if (lp.X < minx) minx = lp.X;
                               if (lp.Y < miny) miny = lp.Y;
                           }

                           letter.UpLeft = new Point(minx,miny);
                           letter.DownRight = new Point(maxx,maxy);
                           letter.Letter_Shape = Letter_Coords;  // контур буквы
                           letter.StringParent = curr_string; // строка в кот находятся буквы

                           /////// отсеиваем мусор  ///////
                           // мелкий мусор просто не записываем
                           // для вычисл разм мусора берем размер строки и делим его
                           // на мусорный коэф treshold=2,875 Чем ниже коэф тем выше размер ложной буквы (в pix)
                           bool trash = false;
                           int container = 0;
                           for (int i = letter.UpLeft.Y; i <= letter.DownRight.Y; i++)
                               for (int j = letter.UpLeft.X; j <= letter.DownRight.X; j++)
                               {
                                   if (binarized[i, j] == 1) container++; // подсчет черных пикселей в букве
                                   if (container > Math.Round((this.stringlines[curr_string].Down - this.stringlines[curr_string].Up) / threshold)) break;
                               }
                           if (container <= Math.Round((this.stringlines[curr_string].Down - this.stringlines[curr_string].Up) / threshold)) trash = true;
                           ///////////////////////////////
                           if (!trash)
                           {
                               letters_in_string[curr_string]++; // кол-во букв в каждой строке
                               this.Letters.Add(letter); // заносим букву в главный список

                           }

                           Letter_Coords.Clear();
                           stack.Clear();
                           letter_proceeded = false;
                           next_point_founded = false;
                           stackflag = false;
                        

                       }

                       #endregion

                   }


               }
           }
       }

       /// <summary>
       /// Получить букву по указанному индексу
       /// </summary>
       /// <param name="index"></param>
       /// <returns></returns>
       public Letter GetLetter(int index)
       {
           if (this.Letters == null)
               throw new LetterSegmentationException("Сперва нужно произвести сегментацию");
           if(index>this.Letters.Count-1)
               throw new LetterSegmentationException("Указанный индекс превышает количество сегментированных букв");
           return this.Letters[index];
       }

     
       /// <summary>
       /// Получить список всех сегментированных букв 
       /// </summary>
       /// <returns></returns>
       public List<Letter> GetLetters()
       {
           if (this.Letters.Count==0)
               throw new LetterSegmentationException("Сперва нужно произвести сегментацию");
               return new List<Letter>(this.Letters);
        }

       /// <summary>
       /// Собирает в списке букв, части разные части одной буквы воедино
       /// </summary>
       /// <param name="ratio">Чем выше тем меньшая соедин площадь (2.5 по-умолч)</param>
       public void Assemble_Letters(double ratio)
       {
          // double ratio=2.5;
           if (this.Letters.Count == 0)
               throw new LetterSegmentationException("Нет букв чтобы их модифицировать, нужно сначала произвести сегментацию");
          Letter [] Letter_array=Letters.ToArray();
          List<Letter> result = new List<Letter>();
          Letter L = new Letter();
          Point UpL = new Point();
          Point DwnR = new Point();
      
          int x1, x2, z1, z2,cy1,cy2,py1,py2;
          result.Add(Letter_array[0]);
          for (int i = 1; i < Letter_array.Length; i++)
          {
              z1=Letter_array[i].UpLeft.X; z2=Letter_array[i].DownRight.X;
              x1=Letter_array[i - 1].UpLeft.X; x2=Letter_array[i - 1].DownRight.X; 
              cy1=Letter_array[i].UpLeft.Y; cy2=Letter_array[i].DownRight.Y; 
              py1=Letter_array[i-1].UpLeft.Y; py2=Letter_array[i-1].DownRight.Y;
              
              if ( (Letter_array[i].StringParent==Letter_array[i-1].StringParent)&&
                  (
                  (((z2 - z1) / ratio + z1) <= x2) ||
                  ((x2 - (x2 - x1) / 2) >= z1)
                  )
                  )
              {
                  if (z2 > x2) DwnR.X = z2; else DwnR.X = x2;
                  if (z1 < x1) UpL.X = z1; else UpL.X = x1;
                  if (cy1 < py1) UpL.Y = cy1; else UpL.Y = py1;
                  if (cy2 > py2) DwnR.Y = cy2; else DwnR.Y = py2;

                  L.DownRight = DwnR; L.UpLeft = UpL;
                  L.StringParent = Letter_array[i].StringParent;
                  result.RemoveAt(result.Count - 1);
                  result.Add(L);
              }
              else
                  result.Add(Letter_array[i]);
       
              
          }
          this.Letters= result;
          
       }
        #endregion
       
       
   
     
        #region Свойства

       /// <summary>
       /// Список границ сегментированных букв
       /// </summary>
       List<Letter> Letters_List
       {
           get 
           {
               if (this.Letters!=null) return new List<Letter>(this.Letters);
               else
                   throw new LetterSegmentationException("Сперва нужно произвести сегментацию");

           }
       }
        #endregion
    }
}
