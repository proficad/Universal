using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using DxfNet;

namespace Drawer
{
    public static class DrawerText
    {

        public static void Draw_Text(SKCanvas a_canvas, string as_text, float ai_x, float ai_y_center, SKPaint a_paint)
        {
            if (as_text.Contains("\r\n"))
            {
                Draw_MuliLine_Text(a_canvas, as_text, ai_x, ai_y_center, a_paint);
            }
            else
            {
                Draw_SingleLine_Text(a_canvas, as_text, ai_x, ai_y_center, a_paint);
            }
        }

        internal enum Centered_Text_Top_Bottom { Top, Bottom };
        // 
        internal static void Draw_Text_Centered(SKCanvas a_canvas, string as_text, PointF a_pivot, SKPaint a_paint, Centered_Text_Top_Bottom a_top_bottom)
        {
            a_paint.TextAlign = SKTextAlign.Center;

            if (a_top_bottom == Centered_Text_Top_Bottom.Top)
            {
                SKFontMetrics l_metrics;
                a_paint.GetFontMetrics(out l_metrics);
                float li_line_height = (-l_metrics.Ascent + l_metrics.Descent);
                a_pivot.Y += li_line_height;
            }
            else
            {
                float lf_height = GetLineHeight(a_paint);
                
            }


            SKRect l_rect = new SKRect();
            a_paint.MeasureText(as_text, ref l_rect);

       

            l_rect.Offset(-l_rect.MidX, 0);
            //make the text background opaque
            l_rect.Offset(a_pivot.X, a_pivot.Y);
            SKPaint l_paint_bg = new SKPaint { Color = SKColors.White };
            a_canvas.DrawRect(l_rect, l_paint_bg);

            a_canvas.DrawText(as_text, a_pivot.X, a_pivot.Y, a_paint);
        }


        public static void Draw_SingleLine_Text(SKCanvas a_canvas, string as_text, float ai_x, float ai_y_center, SKPaint a_paint)
        {
            a_paint.GetFontMetrics(out SKFontMetrics l_metrics);
            float lf_correct = l_metrics.CapHeight / 2f;
            ai_y_center += lf_correct;


            a_canvas.DrawText(as_text, ai_x, ai_y_center, a_paint);
        }

        private static float GetLineHeight(SKPaint a_paint)
        {
            SKFontMetrics l_metrics;
            a_paint.GetFontMetrics(out l_metrics);
            float li_line_height = (-l_metrics.Ascent + l_metrics.Descent);
            return li_line_height;
        }

        /// <summary>
        ///   ai_x ... depends on align (left, middle, right)
        ///   ai_y_center ... vertical center of the text
        /// </summary>
        private static void Draw_MuliLine_Text(SKCanvas a_canvas, string as_text, float ai_x, float ai_y_center, SKPaint a_paint)
        {
            string[] ls_sep = { "\r\n" };
            string[] ls_lines = as_text.Split(ls_sep, StringSplitOptions.None);
            float lf_line_height = GetLineHeight(a_paint);

            float li_block_height = ls_lines.Length * lf_line_height;

            ai_y_center -= (li_block_height / 2);
          
            ai_y_center += lf_line_height;

            a_paint.GetFontMetrics(out SKFontMetrics l_metrics);
            ai_y_center -= l_metrics.Descent;

            foreach (string ls_line in ls_lines)
            {
                a_canvas.DrawText(ls_line, ai_x, ai_y_center, a_paint);
                ai_y_center += lf_line_height;
            }
        }

        public static void Draw_Wire_Label(SKCanvas a_canvas, string as_name, float ai_x, float ai_y, SKPaint a_paint, bool ab_over)
        {
            a_paint.GetFontMetrics(out SKFontMetrics l_metrics);
            if(ab_over)
            {
                SKRect l_rect = new SKRect();
                a_paint.MeasureText(as_name, ref l_rect);
                ai_y += l_rect.Height / 2;

                //make the text background opaque
                float lf_shift_rect = 0;
                if(a_paint.TextAlign == SKTextAlign.Left)
                {
                    lf_shift_rect = 0;
                }
                else if(a_paint.TextAlign == SKTextAlign.Right)
                {
                    lf_shift_rect = -l_rect.Width;
                }

                l_rect.Offset(lf_shift_rect, 0);
                l_rect.Offset(ai_x, ai_y);
                SKPaint l_paint_bg = new SKPaint { Color = SKColors.White };
                a_canvas.DrawRect(l_rect, l_paint_bg);
            }
            else
            {
                ai_y -= l_metrics.Descent;
            }



            a_canvas.DrawText(as_name, ai_x, ai_y, a_paint);
        }

        internal static void Draw_Centered_Text(SKCanvas a_canvas, string as_text, float af_x, float af_y, EFont a_efont, int ai_angle_tenths, bool ab_opaque)
        {
            QTextAlignment l_align = QTextAlignment.AL_MM;
            SKPaint l_paint = Helper.Efont2SKPaint(a_efont, l_align);



            a_canvas.Save();
            a_canvas.Translate(af_x, af_y);
            a_canvas.RotateDegrees(ai_angle_tenths / -10f);


            if(ab_opaque)                 //make the text background opaque
            {
                SKRect l_rect = new SKRect();
                l_paint.MeasureText(as_text, ref l_rect);
                l_rect.Offset(-l_rect.MidX, -l_rect.MidY);

                SKPaint l_paint_bg = new SKPaint { Color = SKColors.White };
                a_canvas.DrawRect(l_rect, l_paint_bg);
            }


            Draw_MuliLine_Text(a_canvas, as_text, 0, 0, l_paint);
            a_canvas.Restore();

        }

        //-------------------------------------
    }
}
