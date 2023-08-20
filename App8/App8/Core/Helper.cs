using System;
using System.Collections.Generic;
using System.Linq;

using System.Drawing;
using SkiaSharp;
using App8.Core;

namespace DxfNet
{
    public static class Helper
    {
        public enum EnumPageOri { OriPortrait, OriLandscape };


        private const short _PROFICAD_DMPAPER_A0 = -10;
        private const short _PROFICAD_DMPAPER_A1 = -11;
        private const short DMPAPER_A2 = 66;
        private const short DMPAPER_A3 = 8;
        private const short DMPAPER_A4 = 9;
        private const short DMPAPER_LETTER = 1;
        private const short DMPAPER_LEGAL = 5;
        private const short DMPAPER_LEDGER = 4;

        internal static Color ColorFromHtml(string as_color)
        {
            SKColor l_sk_color = new SKColor(0,0,0);
            SKColor.TryParse(as_color, out l_sk_color);

            return Color.FromArgb(l_sk_color.Red, l_sk_color.Green, l_sk_color.Blue);

        }


        internal static string EncodeHtml(string as_input)
        {
            as_input = as_input.Replace("&", "&amp;");
            as_input = as_input.Replace("<", "&lt;");
            as_input = as_input.Replace(">", "&gt;");
            as_input = as_input.Replace("'", "&apos;");
            as_input = as_input.Replace("\"", "&quot;");
            return as_input;
        }

        internal static string Color2String(Color a_input)
        {
            string ls_color = string.Format("{0:D2}{1:D2}{2:D2}", a_input.R, a_input.G, a_input.B);
            return ls_color;
        }

        public static System.Drawing.Point GetRectCenterPoint(MyRect a_rect)
        {
            int li_x = (a_rect.Left + a_rect.Right) / 2;
            int li_y = (a_rect.Top + a_rect.Bottom) / 2;
            return new System.Drawing.Point(li_x, li_y);
        }

        internal static Size Size_Tenths_mm_2_mm(Size a_size)
        {
            Size l_size = new Size(
                Tenths_2_mm_Round(a_size.Width),
                Tenths_2_mm_Round(a_size.Height)
            );

            return l_size;
        }

        public static System.Drawing.Point GetRectCenterPoint(System.Drawing.Rectangle a_rect)
        {
            int li_x = a_rect.Left + (a_rect.Width / 2);
            int li_y = a_rect.Top + (a_rect.Height / 2);
            return new System.Drawing.Point(li_x, li_y);
        }

        public static System.Drawing.Point GetRectCenterPoint(System.Drawing.RectangleF a_rect)
        {
            int li_x = (int)(a_rect.Left + (a_rect.Width / 2));
            int li_y = (int)(a_rect.Top + (a_rect.Height / 2));
            return new System.Drawing.Point(li_x, li_y);
        }

        public static void Swap<T>(ref T x, ref T y)
        {
            T t = y;
            y = x;
            x = t;
        }

        public static int Distance2Points(Point a_point1, Point a_point2)
        {
            int li_diff_x = a_point1.X - a_point2.X;
            int li_diff_y = a_point1.Y - a_point2.Y;
            return (int)Math.Sqrt(
                (li_diff_x * li_diff_x) + (li_diff_y * li_diff_y)
            );
        }
        internal static int EasyDistance2Points(System.Drawing.Point a_point1, System.Drawing.Point a_point2)
        {
            return
            Math.Abs(a_point1.X - a_point2.X) + Math.Abs(a_point1.Y - a_point2.Y);
        }

        private static int Tenths_2_mm_Round(int ai_input)
        {
            int li_div = ai_input / 10;
            int li_mod = ai_input % 10;

            if (li_mod > 5)
            {
                ++li_div;
            }

            return li_div;
        }

        internal static SKColor Color2SKColor(Color a_color)
        {
            SKColor l_result = new SKColor((byte)a_color.R, (byte)a_color.G, (byte)a_color.B);
            return l_result;
        }

        internal static SKRect Rect2SKRect(System.Drawing.Rectangle a_position)
        {
            SKRect l_result = new SKRect(a_position.Left, a_position.Top, a_position.Right, a_position.Bottom);
            return l_result;
        }

        internal static SKPaint PaintFromObjProps(ObjProps a_objProps, bool ab_color2)
        {
            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = a_objProps.m_logpen.m_width,
                StrokeCap = LineEnding2StrokeCap(a_objProps.m_line_ending)
            };

            if(ab_color2)
            {
                l_paint.Color = Helper.Color2SKColor(a_objProps.m_contour2.m_color);
            }
            else
            {
                l_paint.Color = Helper.Color2SKColor(a_objProps.m_logpen.m_color);
            }

            if(!ab_color2)
            {
                if (a_objProps.m_lin.m_name != null)
                {
                    string[] ls_lineSegments = a_objProps.m_lin.m_body.Split(new char[] { ',' });
                    if (ls_lineSegments[0] == "A")
                    {
                        int li_segmentsCount = ls_lineSegments.Length - 1; // remove "A" segment
                        float[] ld_arraySegments = new float[li_segmentsCount];
                        for (int i = 0; i < ld_arraySegments.Length; i++)
                        {
                            string ls_item = ls_lineSegments[i + 1];
                            float lf_item = MyString2Float(ls_item);
                            if (lf_item < 0)
                            {
                                lf_item = -lf_item;
                            }

                            lf_item *= 100;// a_objProps.m_line_scale;
                            if (lf_item == 0)
                            {
                                lf_item = a_objProps.m_logpen.m_width;//0 means "dot", that is the width of the line
                            }
                            ld_arraySegments[i] = lf_item;
                        }
                        l_paint.PathEffect = SKPathEffect.CreateDash(ld_arraySegments, 0);
                    }
                }
            }

            return l_paint;
        }


        private static float MyString2Float(string as_what)
        {
            string ls_corrected = as_what;
            if (as_what[0] == '.')
            {
                ls_corrected = "0" + as_what;
            }
            if (as_what[0] == '-')
            {
                ls_corrected = "-0" + as_what.Substring(1);
            }

            return Single.Parse(ls_corrected, System.Globalization.CultureInfo.InvariantCulture);
        }



        internal static SKPoint[] ListPoints2ArraySKPoints(List<System.Drawing.Point> a_points)
        {
            SKPoint[] l_points = new SKPoint[a_points.Count];
            for (int li_i = 0; li_i < a_points.Count; li_i++)
            {
                l_points[li_i].X = a_points[li_i].X;
                l_points[li_i].Y = a_points[li_i].Y;
            }
            return l_points;
        }

        private static SKStrokeCap LineEnding2StrokeCap(ObjProps.EnumLineEnding a_line_ending)
        {
            SKStrokeCap l_stroke_cap = SKStrokeCap.Round;
            switch (a_line_ending)
            {
                case ObjProps.EnumLineEnding.le_square: l_stroke_cap = SKStrokeCap.Square; break;
                case ObjProps.EnumLineEnding.le_flat: l_stroke_cap = SKStrokeCap.Butt; break;
            }

            return l_stroke_cap;
        }

        public static SKPoint Point2SKPoint(Point a_point)
        {
            SKPoint l_point = new SKPoint(a_point.X, a_point.Y);
            return l_point;
        }

        public static float Rad2Deg(float a_input)
        {
            double l_d = 180 * a_input;
            l_d = l_d / System.Math.PI;
            return (float)l_d;
        }

        public static void DrawRect2Angles(DrawRect a_drawRect, out float a_startAngle, out float a_sweepAngle)
        {
            float endAngle = Helper.Rad2Deg((float)System.Math.Atan2(a_drawRect.m_arcBegin.Height, a_drawRect.m_arcBegin.Width));
            a_startAngle = Helper.Rad2Deg((float)System.Math.Atan2(a_drawRect.m_arcEnd.Height, a_drawRect.m_arcEnd.Width));

            if (endAngle < a_startAngle)
            {
                endAngle += 360;
            }

            float sweepAngle = endAngle - a_startAngle;
            if (sweepAngle < 0)
            {
                sweepAngle += 360;
            }

            a_sweepAngle = sweepAngle;
        }

        public static SKPaint Efont2SKPaint(EFont a_efont, QTextAlignment a_align)
        {
            int li_magic_number = 3;

            SKFontStyleWeight l_weight = a_efont.m_bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            SKFontStyleSlant l_slant = a_efont.m_ital ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

            SKPaint l_paint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName(
                                    a_efont.m_faceName,
                                    l_weight,
                                    SKFontStyleWidth.Normal,
                                    l_slant),
                TextSize = (a_efont.m_size / li_magic_number),
                TextAlign = SKTextAlign.Center
            };


            if (a_align == QTextAlignment.AL_LM)
            {
                l_paint.TextAlign = SKTextAlign.Left;
            }
            else if (a_align == QTextAlignment.AL_RM)
            {
                l_paint.TextAlign = SKTextAlign.Right;
            }

            l_paint.Color = Helper.Color2SKColor(a_efont.m_color);

            return l_paint;
        }

        public static bool IsStringNumeric(string as_ret)
        {
            return int.TryParse(as_ret, out var n);
        }

        internal static float MyHypot(float a, float b)
        {
            double sumOfSquares = (a * a) + (b * b);
            return (float)Math.Sqrt(sumOfSquares);
        }

        public static int GetLength(Point a_point_1, Point a_point_2)
        {
            double li_lenX = a_point_2.X - a_point_1.X;
            double li_lenY = a_point_2.Y - a_point_1.Y;
            double li_len = Math.Sqrt((li_lenX * li_lenX) + (li_lenY * li_lenY));

            return (int)Math.Round(li_len);
        }

        public static Size Sniff_Page_Size(int ai_paper_size_enum, string as_form_name)
        {
            List<QPaperSize> l_list = Get_List_Paper_Sizes();

            Size l_size = l_list.Where(x => x.PaperSizeEnum == ai_paper_size_enum).Select(q => q.SheetSize).SingleOrDefault();
          
            if(l_size.IsEmpty)
            {
                l_size = l_list.Where(x => Form_Name_Match(x.FormName, as_form_name)).Select(q => q.SheetSize).SingleOrDefault();
            }
            
            return l_size;
        }


        private static bool Form_Name_Match(string as_name_1, string as_name_2)
        {
            string ls_name_1 = Get_First_Part(as_name_1);
            string ls_name_2 = Get_First_Part(as_name_2);

            return (ls_name_1 == ls_name_2);
        }

        private static string Get_First_Part(string as_input)
        {
            return as_input.Split(' ')[0];
        }

        private static List<QPaperSize> Get_List_Paper_Sizes()
        {
            List<QPaperSize> l_list = new List<QPaperSize>();

            l_list.Add(new QPaperSize(_PROFICAD_DMPAPER_A0, "A0 (1189 x 841 mm)",   new Size(841, 1189)));
            l_list.Add(new QPaperSize(_PROFICAD_DMPAPER_A1, "A1 (841 x 594 mm)",    new Size(594, 841)));
            l_list.Add(new QPaperSize(DMPAPER_A2,           "A2 (594 x 420 mm)",    new Size(420, 594)));
            l_list.Add(new QPaperSize(DMPAPER_A3,           "A3 (420 x 297 mm)",    new Size(297, 420)));
            l_list.Add(new QPaperSize(DMPAPER_A4,           "A4 (297 x 210 mm)",    new Size(210, 297)));
            l_list.Add(new QPaperSize(DMPAPER_LETTER,       "letter (11 x 8.5\")",  new Size(216, 279)));
            l_list.Add(new QPaperSize(DMPAPER_LEGAL,        "legal (14 x 8.5\")",   new Size(216, 356)));
            l_list.Add(new QPaperSize(DMPAPER_LEDGER,       "ledger (17 x 11\")",   new Size(432, 279)));


            return l_list;
        }

        public static void Point2Attrib(System.Xml.XmlWriter a_xmlWriter, String as_attrib, Point a_point)
        {
            a_xmlWriter.WriteAttributeString(as_attrib + "x", a_point.X.ToString());
            a_xmlWriter.WriteAttributeString(as_attrib + "y", a_point.Y.ToString());
        }


        //------------------------------------   
    }
}
