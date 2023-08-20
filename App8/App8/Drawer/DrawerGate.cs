using DxfNet;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Drawer
{
    public class DrawerGate
    {


        public static void DrawQGate(SKCanvas a_canvas, QGate a_gate, PCadDoc a_doc)
        {

            System.Drawing.Point l_center = Helper.GetRectCenterPoint(a_gate.m_position);
        

            SKPaint l_paint_border = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_gate.m_objProps.m_logpen.m_color),
                StrokeWidth = a_gate.m_objProps.m_logpen.m_width
            };


            a_canvas.Save();
            a_canvas.Translate(l_center.X, l_center.Y);


            float lf_angle = a_gate.m_angle / -10f;
            a_canvas.RotateDegrees(lf_angle);

            Drawer.Flip_and_Scale(a_canvas, a_gate);

            int X = 0;
            int Y = 0;


            //nakreslit obrys
            if (a_gate.m_ASA)
            {
                switch (a_gate.m_tvar)
                {
                    case GateShapeType.gst_inv:
                        My_Kruh(a_canvas, X + 30, Y, l_paint_border);
                        goto case GateShapeType.gst_bud;
                    case GateShapeType.gst_bud:
                        using (SKPath path = new SKPath())
                        {
                            path.MoveTo(X - 36, Y - 36);
                            path.LineTo(X + 25, Y);
                            path.LineTo(X - 36, Y + 36);
                            path.Close();
                            a_canvas.DrawPath(path, l_paint_border);
                        }
                        break;
                    case GateShapeType.gst_nand:
                        My_Kruh(a_canvas, X + 79, Y, l_paint_border);
                        goto case GateShapeType.gst_and;
                    case GateShapeType.gst_and:
                        using (SKPath path = new SKPath())
                        {
                            path.MoveTo(X + 36, Y + 36);
                            path.LineTo(X - 36, Y + 36);
                            path.LineTo(X - 36, Y - 36);
                            path.LineTo(X + 36, Y - 36);
                            a_canvas.DrawPath(path, l_paint_border);
                        }
                        My_Oblouk(a_canvas, l_paint_border, X + 36, Y,  36, -90, 180);
                        break;
                    case GateShapeType.gst_nor:
                        My_Kruh(a_canvas, X + 84, Y, l_paint_border);
                        goto case GateShapeType.gst_or;
                    case GateShapeType.gst_or:
                        a_canvas.DrawLine(X + 20, Y + 36, X - 36, Y + 36, l_paint_border);
                        a_canvas.DrawLine(X + 20, Y - 36, X - 36, Y - 36, l_paint_border);
                        {
                            using (SKPath path = new SKPath())
                            {
                                path.MoveTo(X - 36, Y - 36);
                                path.CubicTo(X - 25, Y - 25,
                                             X - 25, Y + 25,
                                             X - 36, Y + 36);

                                path.MoveTo(X + 79, Y);
                                path.CubicTo(X + 71, Y + 10,
                                             X + 51, Y + 28,
                                             X + 20, Y + 36);

                                path.MoveTo(X + 79, Y);
                                path.CubicTo(X + 71, Y - 10,
                                             X + 51, Y - 28,
                                             X + 20, Y - 36);

                                a_canvas.DrawPath(path, l_paint_border);
                            }
                        }

                        break;
                    case GateShapeType.gst_exnor:
                        My_Kruh(a_canvas, X + 98, Y, l_paint_border);
                        goto case GateShapeType.gst_exor;
                    case GateShapeType.gst_exor:
                        a_canvas.DrawLine(X + 20, Y + 36, X - 25, Y + 36, l_paint_border);
                        a_canvas.DrawLine(X + 20, Y - 36, X - 25, Y - 36, l_paint_border);
                        using (SKPath path = new SKPath())
                        {
                            path.MoveTo(X - 25, Y - 36);
                            path.CubicTo(X - 15, Y - 25,
                                         X - 15, Y + 25,
                                         X - 25, Y + 36);

                            path.MoveTo(X - 36, Y - 36);
                            path.CubicTo(X - 25, Y - 25,
                                         X - 25, Y + 25,
                                         X - 36, Y + 36);

                            path.MoveTo(X + 89, Y);
                            path.CubicTo(X + 71, Y + 10,
                                         X + 51, Y + 28,
                                         X + 20, Y + 36);

                            path.MoveTo(X + 89, Y);
                            path.CubicTo(X + 71, Y - 10,
                                         X + 51, Y - 28,
                                         X + 20, Y - 36);

                            a_canvas.DrawPath(path, l_paint_border);
                        }

                        break;
                }//switch
            }//if
            else
            {//ČSN
                SKRect l_rect = new SKRect(X - 36, Y - 71, X + 36, Y + 71);
                a_canvas.DrawRect(l_rect, l_paint_border);
                a_canvas.DrawLine(X + 36, Y, X + 51, Y, l_paint_border);

                switch (a_gate.m_tvar)
                {
                    case GateShapeType.gst_nand:
                        a_canvas.DrawLine(X + 36, Y - 10, X + 51, Y, l_paint_border);
                        break;
                    case GateShapeType.gst_nor:
                        a_canvas.DrawLine(X + 36, Y - 10, X + 51, Y, l_paint_border);
                        break;
                    case GateShapeType.gst_or:
                        break;
                    case GateShapeType.gst_inv:
                        a_canvas.DrawLine(X + 36, Y - 10, X + 51, Y, l_paint_border);
                        break;
                    case GateShapeType.gst_bud:
                        break;
                    case GateShapeType.gst_exnor:
                        a_canvas.DrawLine(X + 36, Y - 10, X + 51, Y, l_paint_border);
                        break;
                    case GateShapeType.gst_exor:
                        break;
                }//switch
            }//else

            //rozšíření kvůli vývodům
            if (!a_gate.m_stesnat)
            {//ne stěsnaně
                if (
                    (a_gate.m_tvar == GateShapeType.gst_nand)
                                        ||
                    (a_gate.m_tvar == GateShapeType.gst_and)
                                        ||
                    (a_gate.m_tvar == GateShapeType.gst_nor)
                                        ||
                    (a_gate.m_tvar == GateShapeType.gst_or)
                                        ||
                    (a_gate.m_tvar == GateShapeType.gst_exnor)
                                        ||
                    (a_gate.m_tvar == GateShapeType.gst_exor)
                )
                {
                    a_canvas.DrawLine(X - 36, Y - 36, X - 36, Y - 80, l_paint_border);
                    a_canvas.DrawLine(X - 36, Y + 36, X - 36, Y + 80, l_paint_border);
                }
            }


            //nakreslit vývody
            if ((a_gate.m_stesnat) && (a_gate.m_ASA))
            {//kreslit stěsnaně a je to asa
                switch (a_gate.m_tvar)
                {
                    case GateShapeType.gst_nand:
                    case GateShapeType.gst_and:
                        //vývody
                        switch (a_gate.m_pocetvstupu)
                        {
                            case 0://1 vývod
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c1);
                                break;
                            case 1://2 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c2);
                                break;
                            case 2://3 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c3);
                                break;
                            case 3://4 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 25, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 13, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 13, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 25, a_gate.m_c4);
                                break;
                            case 4://5 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 25, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 13, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 13, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 25, a_gate.m_c5);
                                break;
                            case 5://6 vývodů
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 23, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 15, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 8, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 8, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 15, a_gate.m_c5);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 23, a_gate.m_c6);
                                break;
                            case 6://7 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 23, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 15, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 8, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 8, a_gate.m_c5);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 15, a_gate.m_c6);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 23, a_gate.m_c7);
                                break;
                            case 7://8 vývodů
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 30, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 23, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 15, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y + 8, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 8, a_gate.m_c5);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 15, a_gate.m_c6);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 23, a_gate.m_c7);
                                MyVyvod(a_canvas, l_paint_border, X - 36, Y - 30, a_gate.m_c8);
                                break;
                        }
                        break;
                    case GateShapeType.gst_nor:
                    case GateShapeType.gst_or:
                    case GateShapeType.gst_exnor:
                    case GateShapeType.gst_exor:
                        //vývody - pravá strana do oblouku
                        switch (a_gate.m_pocetvstupu)
                        {
                            case 0://1 vývod
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y, a_gate.m_c1);
                                break;
                            case 1://2 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 20, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 20, a_gate.m_c2);
                                break;
                            case 2://3 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 20, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 20, a_gate.m_c3);
                                break;
                            case 3://4 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 25, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 13, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y - 13, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 25, a_gate.m_c4);
                                break;
                            case 4://5 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 25, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 13, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y - 13, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 25, a_gate.m_c5);
                                break;
                            case 5://6 vývodů
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 23, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 15, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 8, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y - 8, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 15, a_gate.m_c5);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 23, a_gate.m_c6);
                                break;
                            case 6://7 vývody
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 23, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 15, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 8, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y - 8, a_gate.m_c5);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 15, a_gate.m_c6);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 23, a_gate.m_c7);
                                break;
                            case 7://8 vývodů
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 30, a_gate.m_c1);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y + 23, a_gate.m_c2);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 15, a_gate.m_c3);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y + 8, a_gate.m_c4);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y - 8, a_gate.m_c5);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 15, a_gate.m_c6);
                                MyVyvod(a_canvas, l_paint_border, X - 30, Y - 23, a_gate.m_c7);
                                MyVyvod(a_canvas, l_paint_border, X - 28, Y - 30, a_gate.m_c8);
                                break;
                        }
                        break;
                }
            }
            else//kreslit ne stěsnaně
            {
                switch (a_gate.m_pocetvstupu)
                {
                    case 7://8 vývodů
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 80, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 60, a_gate.m_c2);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 40, a_gate.m_c3);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c4);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c5);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 40, a_gate.m_c6);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 60, a_gate.m_c7);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 80, a_gate.m_c8);
                        break;
                    case 5://6 vývodů
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 60, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 40, a_gate.m_c2);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c3);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c4);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 40, a_gate.m_c5);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 60, a_gate.m_c6);
                        break;
                    case 3://4 vývody
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 60, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c2);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c3);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 60, a_gate.m_c4);
                        break;
                    case 1://2 vývody
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c2);
                        break;
                    case 6://7 vývody
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 60, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 40, a_gate.m_c2);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c3);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c4);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c5);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 40, a_gate.m_c6);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 60, a_gate.m_c7);
                        break;
                    case 4://5 vývody
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 40, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 20, a_gate.m_c2);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c3);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 20, a_gate.m_c4);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 40, a_gate.m_c5);
                        break;
                    case 2://3 vývody
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y + 40, a_gate.m_c1);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c2);
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y - 40, a_gate.m_c3);
                        break;
                    case 0://1 vývod
                        MyVyvod(a_canvas, l_paint_border, X - 36, Y, a_gate.m_c1);
                        break;
                }
            }

            a_canvas.Restore();
        }


        private static void MyVyvod(SKCanvas a_canvas, SKPaint a_paint, int ai_x, int ai_y, bool ab_inv)
        {
            if (ab_inv)
            {
                a_canvas.DrawCircle(ai_x - 5, ai_y, 5, a_paint);
            }
            else
            {
                a_canvas.DrawLine(ai_x, ai_y, ai_x - 15, ai_y, a_paint);
                //My_MoveTo(pDC, x, y); My_LineTo(pDC, GetAxis().x - 51, y);
            }
        }


        private static void My_Kruh(SKCanvas a_canvas, int ai_x, int ai_y, SKPaint a_paint)
        {
            a_canvas.DrawCircle(ai_x, ai_y, 8, a_paint);
        }

        private static void My_Oblouk(SKCanvas a_canvas, SKPaint a_paint, int ai_centr_x, int ai_center_y, int ai_radius, int ai_start_angle, int ai_cil_angle)
        {
            using (SKPath path = new SKPath())
            {
                SKRect l_rect = new SKRect(ai_centr_x - ai_radius, ai_center_y - ai_radius, ai_centr_x + ai_radius, ai_center_y + ai_radius);
                path.AddArc(l_rect, ai_start_angle, ai_cil_angle);
                a_canvas.DrawPath(path, a_paint);
            }

        }
        //---------------------
    }
}
