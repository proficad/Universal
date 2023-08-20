using DxfNet;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Drawer
{
    public static class DrawerTrafo
    {
        private static int VYSKAOBLOUKU = 20;

        public static void DrawTrafo(SKCanvas a_canvas, Trafo a_trafo, PCadDoc a_doc)
        {
            CalculateDimensions(ref a_trafo);

            System.Drawing.Point l_center = Helper.GetRectCenterPoint(a_trafo.m_position);

            SKPaint l_paint_border = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_trafo.m_objProps.m_logpen.m_color),
                StrokeWidth = a_trafo.m_objProps.m_logpen.m_width
            };

            a_canvas.Save();
            a_canvas.Translate(l_center.X, l_center.Y);


            float lf_angle = a_trafo.m_angle / -10f;
            a_canvas.RotateDegrees(lf_angle);

            Drawer.Flip_and_Scale(a_canvas, a_trafo);


            int X = 0;
            int Y = 0; 

            DrawWinding(a_canvas, l_paint_border, a_trafo.m_pri, X - (25), true);
            DrawWinding(a_canvas, l_paint_border, a_trafo.m_sec, X + (25), false);

            DrawCore(a_canvas, l_paint_border, a_trafo.m_jadro, a_trafo.m_vyskajadra, X, Y);

            a_canvas.Restore();
        }


        private static void DrawWinding(SKCanvas a_canvas, SKPaint a_paint, TrafoWinding a_winding, 
                                            int ai_x, bool ab_prim)
        {
        
            int zacatek = (a_winding.WindingHeight / 2) - a_winding.Stair;
            if (a_winding.m_w1 > 0)
            {
                Civka(a_canvas, a_paint, ai_x, zacatek, a_winding.m_w1, ab_prim); zacatek -= ((a_winding.m_w1 + 1) * VYSKAOBLOUKU * 1);
                if (a_winding.m_w2 > 0)
                {
                    Civka(a_canvas, a_paint, ai_x, zacatek, a_winding.m_w2, ab_prim); zacatek -= ((a_winding.m_w2 + 1) * VYSKAOBLOUKU * 1);
                    if (a_winding.m_w3 > 0)
                    {
                        Civka(a_canvas, a_paint, ai_x, zacatek, a_winding.m_w3, ab_prim); zacatek -= ((a_winding.m_w3 + 1) * VYSKAOBLOUKU * 1);
                        if (a_winding.m_w4 > 0)
                        {
                            Civka(a_canvas, a_paint, ai_x, zacatek, a_winding.m_w4, ab_prim); zacatek -= ((a_winding.m_w4 + 1) * VYSKAOBLOUKU * 1);
                            if (a_winding.m_w5 > 0)
                            {
                                Civka(a_canvas, a_paint, ai_x, zacatek, a_winding.m_w5, ab_prim); zacatek -= ((a_winding.m_w5 + 1) * VYSKAOBLOUKU * 1);
                                if (a_winding.m_w6 > 0)
                                {
                                    Civka(a_canvas, a_paint, ai_x, zacatek, a_winding.m_w6, ab_prim);
                                }
                            }
                        }
                    }
                }
            }
        }



        private static void Civka(SKCanvas a_canvas, SKPaint a_paint, int ai_x, int ai_y, int ai_count, bool ab_prim)
        {

            for (int li_i = 0; li_i < ai_count; li_i++)
            {
                if (ab_prim)
                {//primar
                    My_BOblouk(a_canvas, a_paint, VYSKAOBLOUKU, ai_x, ai_y);
                    ai_y -= VYSKAOBLOUKU;
                }
                else
                {//sek
                    My_BOblouk(a_canvas, a_paint, -VYSKAOBLOUKU, ai_x, ai_y);
                    ai_y -= VYSKAOBLOUKU;
                }
            }
        }



        private static void My_BOblouk(SKCanvas a_canvas, SKPaint a_paint, int ai_width, int ai_x, int ai_y)
        {
            using (SKPath path = new SKPath())
            {
                path.MoveTo(ai_x, ai_y - VYSKAOBLOUKU);
                path.CubicTo(ai_x + ai_width, ai_y - VYSKAOBLOUKU - 1,
                             ai_x + ai_width, ai_y + 1,
                             ai_x, ai_y);

                a_canvas.DrawPath(path, a_paint);
            }
        }



        private static void DrawCore(SKCanvas a_canvas, SKPaint a_paint, trafoCoreType a_core_type, int ai_height,int ai_x, int ai_y)
        {

            Point start = new Point();
            Point cil = new Point();
            start.X = 0;
            start.Y = 0 - (ai_height / 2);
            cil.X = 0;
            cil.Y = 0 - (ai_height / 2);

            float lf_1 =  (ai_height / 2);
            float lf_2 = -(ai_height / 2);

            int li_spacingForDouble = 3;

            switch (a_core_type)
            {
                case trafoCoreType.tct_solid:
                    a_canvas.DrawLine(0, lf_1, 0, lf_2, a_paint);
                    break;
                case trafoCoreType.tct_double:
                    a_canvas.DrawLine(-li_spacingForDouble, lf_1, -li_spacingForDouble, lf_2, a_paint);
                    a_canvas.DrawLine( li_spacingForDouble, lf_1,  li_spacingForDouble, lf_2, a_paint);
                    break;
                case trafoCoreType.tct_dash:
                    DrawDashedLine(a_canvas, 0, lf_1, 0, lf_2, a_paint);
                    break;
            }

        }


        private static void CalculateDimensions(ref Trafo a_trafo)
        {
            CalculateWindingHeight(ref a_trafo.m_pri);
            CalculateWindingHeight(ref a_trafo.m_sec);

            a_trafo.m_vyskajadra = Math.Max((a_trafo.m_pri.WindingHeight + a_trafo.m_pri.Stair), (a_trafo.m_sec.WindingHeight + a_trafo.m_sec.Stair));
        }


        private static void CalculateWindingHeight(ref TrafoWinding a_winding)
        {
            a_winding.WindingHeight = a_winding.m_w1 * VYSKAOBLOUKU;
            if(a_winding.m_w2 > 0)
            {
                a_winding.WindingHeight += VYSKAOBLOUKU;//mezera
                a_winding.WindingHeight += a_winding.m_w2 * VYSKAOBLOUKU;
                if(a_winding.m_w3 > 0)
                {
                    a_winding.WindingHeight += VYSKAOBLOUKU;//mezera
                    a_winding.WindingHeight += a_winding.m_w3 * VYSKAOBLOUKU;
                    if (a_winding.m_w4 > 0)
                    {
                        a_winding.WindingHeight += VYSKAOBLOUKU;//mezera
                        a_winding.WindingHeight += a_winding.m_w4 * VYSKAOBLOUKU;
                        if (a_winding.m_w5 > 0)
                        {
                            a_winding.WindingHeight += VYSKAOBLOUKU;//mezera
                            a_winding.WindingHeight += a_winding.m_w5 * VYSKAOBLOUKU;
                            if (a_winding.m_w6 > 0)
                            {
                                a_winding.WindingHeight += VYSKAOBLOUKU;//mezera
                                a_winding.WindingHeight += a_winding.m_w6 * VYSKAOBLOUKU;
                            }
                        }
                    }
                }
            }


            int li_number_arcs_and_spaces = a_winding.WindingHeight / VYSKAOBLOUKU;
            if((li_number_arcs_and_spaces % 2) != 0)
            {
                a_winding.Stair = VYSKAOBLOUKU / 2;
            }

        }



        private static void DrawDashedLine(SKCanvas a_canvas, float ai_x1, float ai_y1, float ai_x2, float ai_y2, SKPaint a_paint)
        {
            float[] l_segments = { 20f, 10f };
            a_paint.PathEffect = SKPathEffect.CreateDash(l_segments, 0);
            a_canvas.DrawLine(ai_x1, ai_y1, ai_x2, ai_y2, a_paint);
        }



        //---------------------------
    }
}
