using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SkiaSharp;

using DxfNet;

namespace Loader
{
    internal static class Helper
    {
        public static Point RotatePointRad(Point a_axis, double ad_angle_radians, Point a_pointToRotate)
        {
            double fi_vstupu = Math.Atan2((double)(a_pointToRotate.Y - a_axis.Y), (double)(a_pointToRotate.X - a_axis.X));
            double modul_vstupu = MyHypot(a_pointToRotate.X - a_axis.X, a_pointToRotate.Y - a_axis.Y);
            //
            Point vysledek = new Point();
            vysledek.X = a_axis.X + (int)(0.5 + (modul_vstupu * Math.Cos(fi_vstupu + ad_angle_radians)));
            vysledek.Y = a_axis.Y + (int)(0.5 + (modul_vstupu * Math.Sin(fi_vstupu + ad_angle_radians)));
            return vysledek;
        }


        public static Point RotatePoint(Point a_axis, int ai_angle, Point a_pointToRotate)
        {
            double li_angle = - Math.PI * ai_angle;
            li_angle /= 180.0;


            double fi_vstupu = Math.Atan2((double)(a_pointToRotate.Y - a_axis.Y), (double)(a_pointToRotate.X - a_axis.X));
            double modul_vstupu = MyHypot(a_pointToRotate.X - a_axis.X, a_pointToRotate.Y - a_axis.Y);
            //

            int li_x = a_axis.X + (int)(0.5 + (modul_vstupu * Math.Cos(fi_vstupu + li_angle)));
            int li_y = a_axis.Y + (int)(0.5 + (modul_vstupu * Math.Sin(fi_vstupu + li_angle)));
            
            return new Point(li_x, li_y);
        }

        public static Point PrevodBodu(Point origin, Point vektor, Point vstup)
        {
            System.Drawing.Point vysledek = new System.Drawing.Point();

            //zjistit úhel šipky
            double fi_sipky = Math.Atan2((double)(vektor.Y - origin.Y), (double)(vektor.X - origin.X));
            double fi_vstupu = Math.Atan2((double)(vstup.Y - origin.Y), (double)(vstup.X - origin.X));
            long modul_vstupu = MyHypot((vstup.Y - origin.Y), (vstup.X - origin.X));
            //
            vysledek.X = origin.X + (int)(modul_vstupu * Math.Cos(fi_vstupu + fi_sipky));
            vysledek.Y = origin.Y + (int)(modul_vstupu * Math.Sin(fi_vstupu + fi_sipky));

            return vysledek;
        }

        public static SKPoint PrevodBodu(SKPoint origin, SKPoint vektor, SKPoint vstup)
        {
            SKPoint vysledek = new SKPoint();

            //zjistit úhel šipky
            double fi_sipky = Math.Atan2((double)(vektor.Y - origin.Y), (double)(vektor.X - origin.X));
            double fi_vstupu = Math.Atan2((double)(vstup.Y - origin.Y), (double)(vstup.X - origin.X));
            float modul_vstupu = DxfNet.Helper.MyHypot((vstup.Y - origin.Y), (vstup.X - origin.X));
            //
            vysledek.X = origin.X + (int)(modul_vstupu * Math.Cos(fi_vstupu + fi_sipky));
            vysledek.Y = origin.Y + (int)(modul_vstupu * Math.Sin(fi_vstupu + fi_sipky));

            return vysledek;
        }


        public static Point PrevodBodu(Point a_vstup, PositionAspect a_aspect)
        {
            if ((a_aspect.m_angle == 0) && (a_aspect.m_vertical == false) && (a_aspect.m_horizontal == false))
            {
                return a_vstup;
            }

            QPivot pivot = new QPivot(a_aspect);

            return pivot.PrevodBodu(a_vstup);
        }

        public static Point PrevodBodu(int ai_x, int ai_y, PositionAspect a_aspect)
        {
            return PrevodBodu(new Point(ai_x, ai_y), a_aspect);
        }

 


        public static int MyHypot(int a, int b)
        {
            int sumOfSquares = a * a + b * b;
            return (int)Math.Sqrt(sumOfSquares);
        }


        public static string Sanitize(string as_input)
        {
            string ls_out = as_input;
            ls_out = ls_out.Replace(' ', '_');
            ls_out = ls_out.Replace('.', '-');
            ls_out = ls_out.Replace(',', '-');
            ls_out = ls_out.Replace(';', '-');
            ls_out = ls_out.Replace(':', '-');
            ls_out = ls_out.Replace('/', '-');
            ls_out = ls_out.Replace('\\', '-');

            return ls_out;
        }

        public static void FixRectangle(ref System.Drawing.RectangleF a_rect)
        {
            if (a_rect.Width < 0)
            {
                a_rect.X = a_rect.X + a_rect.Width;
                a_rect.Width = -a_rect.Width;
            }
            if (a_rect.Height < 0)
            {
                a_rect.Y = a_rect.Y + a_rect.Height;
                a_rect.Height = -a_rect.Height;
            }
        }
        public static void FixRectangle(ref Rectangle a_rect)
        {
            if (a_rect.Width < 0)
            {
                a_rect.X = a_rect.X + a_rect.Width;
                a_rect.Width = -a_rect.Width;
            }
            if (a_rect.Height < 0)
            {
                a_rect.Y = a_rect.Y + a_rect.Height;
                a_rect.Height = -a_rect.Height;
            }
        }

        public static void FixSize(ref Size a_size)
        {
            if (a_size.Width < 0)
            {
                a_size.Width = -a_size.Width;
            }
            if (a_size.Height < 0)
            {
                a_size.Height = -a_size.Height;
            }
        }

        public static float ParseScale(System.Xml.XmlAttribute a_attr)
        {
	        if (a_attr == null)
	        {
                return 1;
	        }
            IFormatProvider fp = System.Globalization.CultureInfo.InvariantCulture;//US format
            return float.Parse(a_attr.Value, fp);
        }

  
        private static bool ColorsAreSame(Color a_color_1, Color a_color_2)
        {
            if(a_color_1.R != a_color_2.R)
            {
                return false;
            }
            if (a_color_1.G != a_color_2.G)
            {
                return false;
            }
            if (a_color_1.B != a_color_2.B)
            {
                return false;
            }

            return true;
        }

        internal static string SanitizeLayerName(string as_nodeName)
        {
            string ls_result = string.Empty;
            foreach(char l_char in as_nodeName)
            {
                if(char.IsLetterOrDigit(l_char) || l_char == '-')
                {
                    ls_result += l_char;
                }
                else
                {
                    ls_result += " ";
                }
            }

            return ls_result.Trim();
        }

        internal static Color ColorFromWin32(int ai_color)
        {
            //return Color.FromArgb(ai_color);// <-- swaps red and blue!!!

            byte[] byteArray = BitConverter.GetBytes(ai_color);

            int li_R = byteArray[0];
            int li_G = byteArray[1];
            int li_B = byteArray[2];

            Color l_result = Color.FromArgb(li_R, li_G, li_B);
            
            return l_result;
        }

        
            

        //--------------------------------
    }
}
