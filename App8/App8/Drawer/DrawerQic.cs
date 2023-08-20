using System;
using DxfNet;

using SkiaSharp;
using System.Drawing;

namespace Drawer
{
    public static class DrawerQic
    {


        public static void DrawQIC(SKCanvas a_canvas, QIC a_ic, PCadDoc a_pCadDoc)
        {
            System.Drawing.Point l_center = Helper.GetRectCenterPoint(a_ic.m_position);
            PositionAspect l_aspect = new PositionAspect(l_center, a_ic.m_angle, a_ic.m_hor, a_ic.m_ver);

            int li_polomer_x, li_polomer_y;
            if (a_ic.m_numberOfOutletsHor > 5)
            {
                li_polomer_x = (a_ic.m_numberOfOutletsHor * (QIC.INT_VYV / 2)) + (QIC.INT_VYV / 2);
            }
            else
            {
                li_polomer_x = (3 * QIC.INT_VYV);
            }
            li_polomer_y = (a_ic.m_numberOfOutletsVer - 1) * (QIC.INT_VYV / 2) + 2 * QIC.INT_VYV;



            // obdélník
            int li_left, li_top, li_bottom, li_right;

            li_left = -li_polomer_x;
            li_right = li_polomer_x;
            li_top = -li_polomer_y;
            li_bottom = li_polomer_y;


            a_canvas.Save();
            DrawBox(a_canvas, ref l_center, a_ic, li_left, li_top, li_right, li_bottom);
            a_canvas.Restore();

            DrawLabels(a_pCadDoc, a_ic, a_canvas, l_aspect, li_left, li_top, li_bottom, li_right);
            


        }


        private static void QText(SKCanvas a_canvas, PositionAspect a_aspect, SKPaint ai_paint, string as_ret, Point a_point, 
            QTextAlignment a_align, SKColor a_color, bool ab_horizontal_edge)
        {

            DrawerText.Centered_Text_Top_Bottom l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Bottom;

            if ((a_aspect.m_angle == -900) || (a_aspect.m_angle == 900))
            {
                ab_horizontal_edge = !ab_horizontal_edge;
            }

            switch (a_aspect.m_angle)
            {
                case 900:
                    if (a_align == QTextAlignment.AL_LM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Left;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Bottom;
                    }
                    else if (a_align == QTextAlignment.AL_RM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Right;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Top;
                    }
                    Switch_Align_On_Horizontal_Edge(ab_horizontal_edge, ref ai_paint);
                    break;
                case -1800:
                case 1800:
                    if (a_align == QTextAlignment.AL_LM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Right;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Top;
                    }
                    else if (a_align == QTextAlignment.AL_RM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Left;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Bottom;
                    }
                    break;
                case -900:
                case 2700:
                    if (a_align == QTextAlignment.AL_LM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Right;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Top;
                    }
                    else if (a_align == QTextAlignment.AL_RM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Left;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Bottom;
                    }
                    Switch_Align_On_Horizontal_Edge(ab_horizontal_edge, ref ai_paint);
                    break;
                default:
                    if (a_align == QTextAlignment.AL_LM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Left;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Bottom;
                    }
                    else if (a_align == QTextAlignment.AL_RM)
                    {
                        ai_paint.TextAlign = SKTextAlign.Right;
                        l_top_bottom = DrawerText.Centered_Text_Top_Bottom.Top;
                    }
                    break;

            }

  



            bool lb_otacet = ((as_ret.Length > 2) || (!Helper.IsStringNumeric(as_ret)));//not a number or longer > 2


            //rotate point
            Point l_point = new Point(0,0);
            QPivot l_pivot = new QPivot(l_point, a_aspect.m_angle, a_aspect.m_horizontal, a_aspect.m_vertical);
          
            if(a_aspect.m_angle != 0)
            {
                a_point = l_pivot.PrevodBodu(a_point);
            }

            a_point.X += a_aspect.m_pivot.X;
            a_point.Y += a_aspect.m_pivot.Y;

            
            if (ab_horizontal_edge)
            {
                if(lb_otacet)
                {
                    a_canvas.Save();
                    a_canvas.RotateDegrees(-90, a_point.X, a_point.Y);
                    DrawerText.Draw_SingleLine_Text(a_canvas, as_ret, a_point.X, a_point.Y, ai_paint);
                    a_canvas.Restore();
                }
                else
                {
                    DrawerText.Draw_Text_Centered(a_canvas, as_ret, a_point, ai_paint, l_top_bottom);
                }
            }
            else // svislá hrana
            {
                DrawerText.Draw_SingleLine_Text(a_canvas, as_ret, a_point.X, a_point.Y, ai_paint);
            }


        }
          

        private static string GetLabel(string[] as_arr, int ai_index)
        {
            //here it is 0 based
            ai_index--;
            if (ai_index < as_arr.Length)
            {
                return as_arr[ai_index];
            }
            return string.Empty;
        }

        private static void Switch_Align_On_Horizontal_Edge(bool ab_horizontal_edge, ref SKPaint ai_paint)
        {
            if(!ab_horizontal_edge)
            {
                if(ai_paint.TextAlign == SKTextAlign.Left)
                {
                    ai_paint.TextAlign = SKTextAlign.Right;
                }
                else
                {
                    ai_paint.TextAlign = SKTextAlign.Left;
                }
            }
        }


        private static void DrawBox(SKCanvas a_canvas, ref System.Drawing.Point l_center, QIC a_ic, int li_left, int li_top, int li_right, int li_bottom)
        {
            a_canvas.Translate(l_center.X, l_center.Y);

            float lf_angle = a_ic.m_angle / -10f;
            a_canvas.RotateDegrees(lf_angle);

            SKRect l_rect = new SKRect(li_left, li_top, li_right, li_bottom);

            if (a_ic.m_objProps.m_bBrush)
            {
                SKPaint l_paint_fill = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Helper.Color2SKColor(a_ic.m_objProps.m_logbrush.m_color),
                };
                a_canvas.DrawRect(l_rect, l_paint_fill);
            }


            SKPaint l_paint_border = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_ic.m_objProps.m_logpen.m_color),
                StrokeWidth = a_ic.m_objProps.m_logpen.m_width
            };

            a_canvas.DrawRect(l_rect, l_paint_border);

            DrawCircles(a_canvas, l_paint_border, a_ic, li_left, li_top, li_right, li_bottom);

            DrawSeparatorLines(a_canvas, l_paint_border, a_ic, li_left, li_top, li_right, li_bottom);

        }


        private static void DrawLabels(PCadDoc a_pCadDoc, QIC a_ic, SKCanvas a_canvas, PositionAspect l_aspect, int li_left, int li_top, int li_bottom, int li_right)
        {
            //labels
            QFontsCollection l_fonts_coll = a_pCadDoc.Parent.m_fonts;
            EFont l_efont = l_fonts_coll.Get_Efont("_type");
            SKPaint l_paint = Helper.Efont2SKPaint(l_efont, QTextAlignment.AL_LM);


            SKColor l_textColor = Helper.Color2SKColor(a_ic.m_objProps.m_logpen.m_color);
            string[] l_arrOut_d = a_ic.m_out_d.Split(new Char[] { ',' });
            string[] l_arrOut_n = a_ic.m_out_n.Split(new Char[] { ',' });
            string ls_ret;

            int i;
            //	===============================NAPISY UVNITR ===============================
            // labely - označení vývodů - nalevo(shora dolů)
            for (i = 1; i <= a_ic.m_numberOfOutletsVer; i++)
            {
                ls_ret = GetLabel(l_arrOut_d, i);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_left + 8, li_top + ((i + 1) * QIC.INT_VYV)), QTextAlignment.AL_LM, l_textColor, false);
            }
            // labely - označení vývodů - dole(zleva doprava)
            for (i = 1; i <= a_ic.m_numberOfOutletsHor; i++)
            {
                ls_ret = GetLabel(l_arrOut_d, i + a_ic.m_numberOfOutletsVer);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_left + (QIC.INT_VYV * i), li_bottom - 8), QTextAlignment.AL_LM, l_textColor, true);
            }
            // labely - označení vývodů - vpravo(sdola nahoru)
            for (i = a_ic.m_numberOfOutletsVer; i > 0; i--)
            {
                ls_ret = GetLabel(l_arrOut_d, i + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_right - 8, li_bottom - ((i + 1) * QIC.INT_VYV)), QTextAlignment.AL_RM, l_textColor, false);
            }
            // labely - označení vývodů - nahoře(zprava doleva)
            for (i = a_ic.m_numberOfOutletsHor; i > 0; i--)
            {
                ls_ret = GetLabel(l_arrOut_d, i + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_left + QIC.INT_VYV + (QIC.INT_VYV * (a_ic.m_numberOfOutletsHor - i)), li_top + 8), QTextAlignment.AL_RM, l_textColor, true);
            }

            //  ==========================================================
            // labely - čísla vývodů - nalevo(shora dolů)  - NÁPISY VENKU =====vvv======
            for (i = 1; i <= a_ic.m_numberOfOutletsVer; i++)
            {
                ls_ret = GetLabel(l_arrOut_n, i);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_left - 8, li_top + ((i + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2), QTextAlignment.AL_RM, l_textColor, false);
            }
            // labely - čísla vývodů - dole(zleva doprava)
            for (i = 1; i <= a_ic.m_numberOfOutletsHor; i++)
            {
                ls_ret = GetLabel(l_arrOut_n, i + a_ic.m_numberOfOutletsVer);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_left + (QIC.INT_VYV * i) - (QIC.INT_VYV / 2), li_bottom + 8), QTextAlignment.AL_RM, l_textColor, true);
            }
            // labely - čísla vývodů - vpravo(sdola nahoru)
            for (i = a_ic.m_numberOfOutletsVer; i > 0; i--)
            {
                ls_ret = GetLabel(l_arrOut_n, i + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_right + 8, li_bottom - ((i + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2), QTextAlignment.AL_LM, l_textColor, false);
            }
            // labely - čísla vývodů - nahoře(zprava doleva)
            for (i = a_ic.m_numberOfOutletsHor; i > 0; i--)
            {
                ls_ret = GetLabel(l_arrOut_n, i + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                QText(a_canvas, l_aspect, l_paint, ls_ret, new Point(li_left + QIC.INT_VYV / 2 + (QIC.INT_VYV * (a_ic.m_numberOfOutletsHor - i)), li_top - 8), QTextAlignment.AL_LM, l_textColor, true);
            }
        }


        private static void DrawCircles(SKCanvas a_canvas, SKPaint a_paint, QIC a_ic, int li_left, int li_top, int li_right, int li_bottom)
        {
            string[] l_arr_kulicky = a_ic.m_out_n_inv.Split(new Char[] { ',' });

            foreach (string ls_circle in l_arr_kulicky)
            {
                if (int.TryParse(ls_circle, out int li_circle))
                {
                    //udělej kroužek
                    if ((li_circle > 0) && (li_circle <= a_ic.m_numberOfOutletsVer))
                    {
                        My_Kruh(a_canvas, a_paint,
                            li_left - 5, 
                            li_top + ((li_circle + 1) * QIC.INT_VYV), 
                            5);
                    }
                    if ((li_circle > a_ic.m_numberOfOutletsVer) && (li_circle <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor)))
                    {
                        li_circle -= a_ic.m_numberOfOutletsVer;
                        My_Kruh(a_canvas, a_paint,
                            li_left + ((li_circle) * QIC.INT_VYV), 
                            li_bottom + 5, 
                            5);
                    }
                    if ((li_circle > a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor) && (li_circle <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer)))
                    {
                        li_circle -= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor);
                        My_Kruh(a_canvas, a_paint,
                            li_right + 5, 
                            li_bottom - ((li_circle + 1) * QIC.INT_VYV), 
                            5);
                    }
                    if ((li_circle > a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer) && (li_circle <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor)))
                    {
                        li_circle -= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                        My_Kruh(a_canvas, a_paint,
                            li_left + (QIC.INT_VYV * (a_ic.m_numberOfOutletsHor - li_circle + 1)), 
                            li_top - 5, 
                            5);
                    }

                }
            }



        }

        private static void My_Kruh(SKCanvas a_canvas, SKPaint a_paint, int ai_x, int ai_y, int ai_radius)
        {
            a_canvas.DrawCircle(ai_x, ai_y, ai_radius, a_paint);
        }



        private static void DrawSeparatorLines(SKCanvas a_canvas, SKPaint l_paint_border, QIC a_ic, int li_left, int li_top, int li_right, int li_bottom)
        {
            // svislé oddělovače
            if (a_ic.m_ver_left)
            {
                a_canvas.DrawLine(li_left + 44, li_top, li_left + 44, li_bottom, l_paint_border);
            }
            if (a_ic.m_ver_right)
            {
                a_canvas.DrawLine(li_right - 44, li_top, li_right - 44, li_bottom, l_paint_border);
            }

            using (SKPath path = new SKPath())
            {
                int stred = (li_left + li_right) / 2;
                path.MoveTo(stred - 11, li_top);
                path.CubicTo(
                    stred - 10, li_top + 12,
                    stred + 10, li_top + 12,
                    stred + 11, li_top
                );

                a_canvas.DrawPath(path, l_paint_border);
            }

            DrawHorizontalSeparators(a_ic, li_top, a_canvas, li_left, l_paint_border, li_right);

        }


        private static void DrawHorizontalSeparators(QIC a_ic, int li_top, SKCanvas a_canvas, int li_left, SKPaint l_paint_border, int li_right)
        {
            string[] l_arr_hor_seps = a_ic.m_pos_hor.Split(new Char[] { ',' });

            foreach (string ls_hor_sep in l_arr_hor_seps)
            {
                if (int.TryParse(ls_hor_sep, out int li_hir_sep))
                {
                    //udělej čárku
                    if ((li_hir_sep > 0) && (li_hir_sep <= (a_ic.m_numberOfOutletsVer - 1)))
                    {
                        int li_y = li_top + ((li_hir_sep + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2;
                        a_canvas.DrawLine(li_left, li_y, li_left + 44, li_y, l_paint_border);
                    }
                    if ((li_hir_sep > a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + 1) && (li_hir_sep <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer)))
                    {
                        li_hir_sep -= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor);
                        int li_y = li_top + ((2 + a_ic.m_numberOfOutletsVer - li_hir_sep) * QIC.INT_VYV) + QIC.INT_VYV / 2;
                        a_canvas.DrawLine(li_right - 44, li_y, li_right, li_y, l_paint_border);
                    }
                }
            }//foreach
        }
        //---------------------
    }


}
