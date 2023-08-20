
using System;
using System.Drawing;

using DxfNet;

using SkiaSharp;
//using SkiaSharp.Views.Forms;

using System.Collections;
using System.Collections.Generic;

namespace Drawer
{
    static class Drawer
    {
        //private static SKRectI m_rect;

        private enum ArrowType
        {
            at_none = 0,
            at_sip1 = 5,
            at_filled_uni,
            at_sip3,
            at_sip4,
            at_sip5,
            at_sip6,
            at_sip7,
            at_sip8,
            at_filled_both
        }


        public static void DrawPCadDoc(SKCanvas a_canvas, SKRectI a_rect, PCadDoc a_doc)
        {
  
            DrawFrame(a_canvas, a_doc);

            Size l_size = a_doc.GetSize();
            SKRect l_rect = new SKRect(0, 0, l_size.Width, l_size.Height);
            RefGridSettings l_ref_grid_settings = a_doc.Parent.m_ref_grid;

            Loader.HelperRefGrid.DrawRefGridForSheets(a_canvas, l_rect, l_ref_grid_settings);


            foreach (DrawObj l_obj in a_doc.m_objects)
            {
                if (l_obj is QImage l_image)
                {
                    DrawerImage.DrawImage(a_canvas, l_image, a_doc.Parent.m_repo);
                }
                else if (l_obj is QIC l_ic)
                {
                    DrawerQic.DrawQIC(a_canvas, l_ic, a_doc);
                    DrawSatelites(a_canvas, l_ic.m_satelites, a_doc);
                }
                else if (l_obj is QGate l_gate)
                {
                    DrawerGate.DrawQGate(a_canvas, l_gate, a_doc);
                    DrawSatelites(a_canvas, l_gate.m_satelites, a_doc);
                }
                else if (l_obj is Trafo l_trafo)
                {
                    DrawerTrafo.DrawTrafo(a_canvas, l_trafo, a_doc);
                    DrawSatelites(a_canvas, l_trafo.m_satelites, a_doc);
                }
                else if (l_obj is Insert l_insert)
                {
                    PpdDoc l_ppd = a_doc.Parent.m_repo.FindPpdDocInRepo(l_insert.m_lG);
                    if(null != l_ppd)
                    {
                        DrawInsert(a_canvas, l_obj as Insert, l_ppd);
                        DrawSatelites(a_canvas, l_insert.m_satelites, a_doc);
                    }
                    continue;
                }
                else
                {
                    DrawQRect(a_canvas, l_obj, a_doc);
                }

            }


            DrawTitleBlock(a_canvas, a_doc);

       
        }


        public static void Flip_and_Scale(SKCanvas a_canvas, Insert a_insert)
        {
            float lf_scale_x = 1f;
            float lf_scale_y = 1f;

            if (a_insert.m_hor)
            {
                lf_scale_y = -1f;
            }
            if (a_insert.m_ver)
            {
                lf_scale_x = -1f;
            }

            lf_scale_x *= a_insert.m_scaleX;
            lf_scale_y *= a_insert.m_scaleY;

            a_canvas.Scale(lf_scale_x, lf_scale_y);
        }

        private static void DrawInsert(SKCanvas a_canvas, Insert a_insert, PpdDoc l_ppd)
        {

            System.Drawing.Point l_center = Helper.GetRectCenterPoint(a_insert.m_position);


            //shift-rotate-flip come in different order than in desktop version ??? ???
            a_canvas.Save();
            a_canvas.Translate(l_center.X, l_center.Y);


            float lf_angle = a_insert.m_angle / -10f;
            a_canvas.RotateDegrees(lf_angle);

            Flip_and_Scale(a_canvas, a_insert);


            DrawPpd(a_canvas, l_ppd);
            a_canvas.Restore();
        }


        private static void DrawPpd(SKCanvas a_canvas, PpdDoc a_doc)
        {
            foreach (DrawObj l_obj in a_doc.m_objects)
            {
                if (l_obj is Insert l_insert)
                {
                    PpdDoc l_ppd = a_doc.m_repo.FindPpdDocInRepo(l_insert.m_lG);
                    if (null != l_ppd)
                    {
                        DrawInsert(a_canvas, l_obj as Insert, l_ppd);
                    }
                    continue;
                }
                else if (l_obj is QImage l_image)
                {
                    DrawerImage.DrawImage(a_canvas, l_image, a_doc.m_repo);
                }


                DrawQRect(a_canvas, l_obj, a_doc);
            }
        }

        private static void DrawPtb(SKCanvas a_canvas, PtbDoc a_doc)
        {
            foreach (DrawObj l_obj in a_doc.m_objects)
            {
                if (l_obj is Insert l_insert)
                {
                    PpdDoc l_ppd = a_doc.m_repo.FindPpdDocInRepo(l_insert.m_lG);
                    if (null != l_ppd)
                    {
                        DrawInsert(a_canvas, l_obj as Insert, l_ppd);
                    }
                    continue;
                }
                else if (l_obj is QImage l_image)
                {
                    DrawerImage.DrawImage(a_canvas, l_image, a_doc.m_repo);
                }

                DrawQRect(a_canvas, l_obj, a_doc);
            }
        }


        private static void DrawQRect(SKCanvas a_canvas, DrawObj obj, DrawDoc a_doc)
        {
            switch (obj.m_nShape)
            {
                case Shape.poly:        DrawPolygon(a_canvas, obj as DrawPoly); break;
                case Shape.polyline:    DrawPolyline(a_canvas, obj as DrawPoly); break;
                case Shape.spoj:        DrawerWire.DrawWire(a_canvas, obj as DxfNet.Wire, a_doc as PCadDoc); break;
                case Shape.bezier:      DrawBezier(a_canvas, obj as DrawPoly); break;
                case Shape.ellipse:     DrawEllipse(a_canvas, obj as DrawRect); break;
                case Shape.circle:      DrawCircle(a_canvas, obj as QCircle); break;
                case Shape.pie:
                case Shape.chord:       DrawPieChord(a_canvas, obj as DrawRect); break;
                case Shape.arc:         DrawArc(a_canvas, obj as DrawRect); break;
                case Shape.rectangle:   MyDrawRect(a_canvas, obj as DrawRect); break;
                case Shape.roundRectangle: DrawRoundRect(a_canvas, obj as DrawRect); break;
                case Shape.text:        DrawFreeText(a_canvas, obj as FreeText); break;
                case Shape.cable:       DrawCable(a_canvas, obj as CableSymbol, a_doc as PCadDoc); break;
                case Shape.dim_line:    DrawDimLine(a_canvas, obj as QDimLine, a_doc as PCadDoc); break;
                case Shape.dim_circle:  DrawDimCircle(a_canvas, obj as QDimCircle, a_doc as PCadDoc); break;
                    
            }
        }



        private static void DrawCable(SKCanvas a_canvas, CableSymbol a_cableSymbol, PCadDoc pCadDoc)
        {
            Position2EndPoints(a_cableSymbol, out PointF l_point_1, out PointF l_point_2);

            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_cableSymbol.m_objProps.m_logpen.m_color),
                StrokeWidth = 2
            };
            a_canvas.DrawLine(l_point_1.X, l_point_1.Y, l_point_2.X, l_point_2.Y, l_paint);

            DrawSatelites(a_canvas, a_cableSymbol.m_satelites, pCadDoc);

        }


        private static void DrawFreeText(SKCanvas a_canvas, FreeText freeText)
        {
            SKPaint l_paint = Helper.Efont2SKPaint(freeText.m_efont, freeText.m_alignment);


            float lf_angle = (float)(freeText.m_angle / -10);
            Point l_center = DxfNet.Helper.GetRectCenterPoint(freeText.m_position);
            float lf_x = l_center.X;
            float lf_y = l_center.Y;


            a_canvas.Save();
            a_canvas.RotateDegrees(lf_angle, lf_x, lf_y);
            DrawerText.Draw_Text(a_canvas, freeText.m_text, freeText.m_position.Left, freeText.m_position.Top, l_paint);
            a_canvas.Restore();

        }

        private static void DrawRoundRect(SKCanvas a_canvas, DrawRect a_drawRect)
        {
            SKRect l_rect = Helper.Rect2SKRect(a_drawRect.m_position);

            if (a_drawRect.m_objProps.m_bBrush)
            {
                SKPaint l_paint_fill = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Helper.Color2SKColor(a_drawRect.m_objProps.m_logbrush.m_color),
                };
                a_canvas.DrawRoundRect(l_rect, a_drawRect.m_rX, a_drawRect.m_rY, l_paint_fill);
            }


            SKPaint l_paint_border = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_drawRect.m_objProps.m_logpen.m_color),
                StrokeWidth = a_drawRect.m_objProps.m_logpen.m_width
            };

            a_canvas.DrawRoundRect(l_rect, a_drawRect.m_rX, a_drawRect.m_rY, l_paint_border);
            Draw_Rect_Text(a_drawRect, a_canvas);

        }


        public delegate void ROrE(SKRect a_rect, SKPaint a_paint);

        private static void Draw_Rect_Text(DrawRect a_drawRect, SKCanvas a_canvas)
        {
            if (!string.IsNullOrEmpty(a_drawRect.m_text))
            {
                Point l_center_point = a_drawRect.GetCenterPoint();
                float lf_x = l_center_point.X;
                float lf_y = l_center_point.Y;
                DrawerText.Draw_Centered_Text(a_canvas, a_drawRect.m_text, lf_x, lf_y, a_drawRect.m_efont, a_drawRect.m_text_angle, false);
            }
        }

        private static void DrawRectOrEllipse(SKCanvas a_canvas, DrawRect a_drawRect, ROrE a_method_to_draw)
        {
            SKRect l_rect = Helper.Rect2SKRect(a_drawRect.m_position);

            if (a_drawRect.m_objProps.m_bBrush)
            {
                SKPaint l_paint_fill = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Helper.Color2SKColor(a_drawRect.m_objProps.m_logbrush.m_color),
                };
                a_method_to_draw(l_rect, l_paint_fill);
            }

 
            SKPaint l_paint_border = Helper.PaintFromObjProps(a_drawRect.m_objProps, false);

            a_method_to_draw(l_rect, l_paint_border);

            Draw_Rect_Text(a_drawRect, a_canvas);


        }

        private static void MyDrawRect(SKCanvas a_canvas, DrawRect drawRect)
        {
            ROrE x = a_canvas.DrawRect;
            DrawRectOrEllipse(a_canvas, drawRect, x);
        }



        private static void DrawEllipse(SKCanvas a_canvas, DrawRect drawRect)
        {
            ROrE x = a_canvas.DrawOval;
            DrawRectOrEllipse(a_canvas, drawRect, x);
        }


        private static void DrawCircle(SKCanvas a_canvas, QCircle a_circle)
        {
            SKPaint l_paint = Helper.PaintFromObjProps(a_circle.m_objProps, false);

            int li_radius = DxfNet.Helper.Distance2Points(a_circle.m_center, a_circle.m_tangent);
            SKPoint l_center_point = new SKPoint(a_circle.m_center.X, a_circle.m_center.Y);
            a_canvas.DrawCircle(l_center_point, li_radius, l_paint);

        }



        private static void DrawArc(SKCanvas a_canvas, DrawRect a_drawRect)
        {
            SKPaint l_paint = Helper.PaintFromObjProps(a_drawRect.m_objProps, false);
            Helper.DrawRect2Angles(a_drawRect, out float startAngle, out float sweepAngle);
            SKRect l_rect = Helper.Rect2SKRect(a_drawRect.m_position);

            using (SKPath path = new SKPath())
            {
                path.AddArc(l_rect, startAngle, sweepAngle);
                a_canvas.DrawPath(path, l_paint);
            }
        }

        private static void DrawPieChord(SKCanvas a_canvas, DrawRect a_drawRect)
        {
            SKPaint l_paint = Helper.PaintFromObjProps(a_drawRect.m_objProps, false);
            Helper.DrawRect2Angles(a_drawRect, out float startAngle, out float sweepAngle);
            SKRect l_rect = Helper.Rect2SKRect(a_drawRect.m_position);

            using (SKPath path = new SKPath())
            {
                if(a_drawRect.m_nShape == Shape.pie)
                {
                    Point l_center = a_drawRect.GetCenterPoint();
                    SKPoint l_center_sk = Helper.Point2SKPoint(l_center);
                    path.MoveTo(l_center_sk);
                    path.ArcTo(l_rect, startAngle, sweepAngle, false);
                    path.Close();
                }
                if(a_drawRect.m_nShape == Shape.chord)
                {
                    path.AddArc(l_rect, startAngle, sweepAngle);
                    path.Close();
                }

                a_canvas.DrawPath(path, l_paint);
            }
        }

        private static void DrawBezier(SKCanvas a_canvas, DrawPoly a_drawPoly)
        {
            SKPaint l_paint = Helper.PaintFromObjProps(a_drawPoly.m_objProps, false);
            SKPoint[] l_point = Helper.ListPoints2ArraySKPoints(a_drawPoly.m_points);
            using (SKPath path = new SKPath())
            {
                path.MoveTo(l_point[0]);
                for (int li_i = 3; li_i < l_point.Length; li_i += 3)
                {
                    path.CubicTo(
                        l_point[li_i - 2],
                        l_point[li_i - 1],
                        l_point[li_i]
                    );
                }

                a_canvas.DrawPath(path, l_paint);
            }
        }

 





        public static void DrawPolyline(SKCanvas a_canvas, DrawPoly a_drawPoly)
        {
            SKPoint[] l_points = Helper.ListPoints2ArraySKPoints(a_drawPoly.m_points);

            if (a_drawPoly.m_objProps.m_contour2.IsOn)
            {
                SKPaint l_paint_back = Helper.PaintFromObjProps(a_drawPoly.m_objProps, true);
                DrawPolyLineInner(a_canvas, l_points, l_paint_back);
            }

            SKPaint l_paint_fore = Helper.PaintFromObjProps(a_drawPoly.m_objProps, false);
            DrawPolyLineInner(a_canvas, l_points, l_paint_fore);

            ArrowType li_arrow_style = (ArrowType)a_drawPoly.m_objProps.m_logpen.m_style;
            if(li_arrow_style != 0)
            {
                float li_scale_x = 1f;
                float li_scale_y = 1f;
                DrawArrowWithoutStem(a_canvas, l_points, l_paint_fore, li_arrow_style, li_scale_x, li_scale_y);
            }
        }


        private static void DrawPolyLineInner(SKCanvas a_canvas, SKPoint[] a_points, SKPaint a_paint)
        {
            SKPath l_path = new SKPath();
            l_path.MoveTo(a_points[0]);
            for (int li_i = 1; li_i < a_points.Length; li_i++)
            {
                l_path.LineTo(a_points[li_i]);
            }
            a_canvas.DrawPath(l_path, a_paint);
        }


        private static void DrawPolygon(SKCanvas a_canvas, DrawPoly a_drawPoly)
        {
            SKPoint[] l_points = Helper.ListPoints2ArraySKPoints(a_drawPoly.m_points);

            SKPath l_path = new SKPath();

            if (a_drawPoly.m_objProps.m_bBrush)
            {
                SKPaint l_paint_fill = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Helper.Color2SKColor(a_drawPoly.m_objProps.m_logbrush.m_color),
                };

                l_path.MoveTo(l_points[0]);
                for (int li_i = 1; li_i < l_points.Length; li_i++)
                {
                    l_path.LineTo(l_points[li_i]);
                }
                a_canvas.DrawPath(l_path, l_paint_fill);
            }


            SKPaint l_paint_border = Helper.PaintFromObjProps(a_drawPoly.m_objProps, false);
            l_path.MoveTo(l_points[0]);
            for (int li_i = 1; li_i < l_points.Length; li_i++)
            {
                l_path.LineTo(l_points[li_i]);
            }
            l_path.Close();
            a_canvas.DrawPath(l_path, l_paint_border);

        }


        private static void DrawLayer(Layer l_layer)
        {
        }




        private static void DrawSatelites(SKCanvas a_canvas, System.Collections.Generic.List<Insert.Satelite> a_list, PCadDoc a_pCadDoc)
        {

            QFontsCollection l_fonts_coll = a_pCadDoc.Parent.m_fonts;

            foreach (Insert.Satelite l_sat in a_list)
            {
                if(!l_sat.m_visible)
                {
                    continue;
                }

                EFont l_efont = l_fonts_coll.Get_Efont(l_sat.m_name);
                SKPaint l_paint = Helper.Efont2SKPaint(l_efont, l_sat.m_alignment);

                

                a_canvas.Save();

                a_canvas.Translate(l_sat.m_x, l_sat.m_y);
                int li_angle = Loader.Loader.TurnsToAngle(l_sat.m_turns);
                float lf_angle = li_angle / -10f;
                a_canvas.RotateDegrees(lf_angle);

                DrawerText.Draw_Text(a_canvas, l_sat.m_value, 0, 0, l_paint);

                a_canvas.Restore();
            }

        }

        private static void DrawFrame(SKCanvas a_canvas, PCadDoc a_doc)
        {
            Size l_size = a_doc.GetSize();

            SKRect l_rect = new SKRect(0, 0, l_size.Width, l_size.Height);
            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SkiaSharp.SKColors.LightGray,
                StrokeWidth = 7
            };
            a_canvas.DrawRect(l_rect, l_paint);
        }


        private static void DrawArrowWithoutStem(SKCanvas a_canvas, SKPoint[] a_points, 
            SKPaint a_paint_fore, ArrowType ai_arrow_style, float ai_scale_x, float ai_scale_y)
        {
            int li_pointsCount = a_points.Length;
            SipkaWithoutStem(a_canvas, ai_arrow_style, 
                a_points[li_pointsCount - 2], a_points[li_pointsCount - 1],
                a_paint_fore, true, ai_scale_x, ai_scale_y);

            SipkaWithoutStem(a_canvas, ai_arrow_style,
                a_points[1], a_points[0],                
                a_paint_fore, false, ai_scale_x, ai_scale_y);
        }


        private static void SipkaWithoutStem(SKCanvas a_canvas, ArrowType ai_arrow_style, 
            SKPoint a_start, SKPoint a_cil, 
            SKPaint a_paint_fore, bool ab_is_ending, float a_scaleX, float a_scaleY)
        {

            int li_coef_x_12 = (int)((12.0 * a_scaleX) + 0.499);
            int li_coef_x_24 = (int)((24.0 * a_scaleX) + 0.499);

            int li_coef_y_5 = (int)((5.0 * a_scaleY) + 0.499);

            int li_coef_x_7 = (int)((7.0 * a_scaleX) + 0.499);
            int li_coef_y_7 = (int)((7.0 * a_scaleY) + 0.499);


            int li_dist_80 = 80;
            int li_dist_60 = 60;



            int li_coef = 2;//shown in drawing

            float li_len =  Helper.MyHypot(a_start.X - a_cil.X, a_start.Y - a_cil.Y);


            switch (ai_arrow_style)
            {
                case ArrowType.at_sip1://velká
                    List<SKPoint> l_points_1 = new List<SKPoint>();
                    if (ab_is_ending)
                    {
                        l_points_1.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - li_coef_y_5 * li_coef)));
                        l_points_1.Add(a_cil);
                        l_points_1.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + li_coef_y_5 * li_coef)));
                        DrawPolyLineInner(a_canvas, l_points_1.ToArray(), a_paint_fore);
                    }
                    
                    break;

                case ArrowType.at_sip6://velka duta uzavrena (proudy), added to version 3.0 on 9-MAR-2004
                    {
                        if (ab_is_ending)
                        {
                            List<SKPoint> l_points_6 = new List<SKPoint>();
                            l_points_6.Add(a_cil);
                            l_points_6.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - li_coef_y_5 * li_coef)));
                            l_points_6.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + li_coef_y_5 * li_coef)));
                            l_points_6.Add(a_cil);
                            DrawFilledPolygon(a_canvas, l_points_6, SKColors.White);
                            DrawPolyLineInner(a_canvas, l_points_6.ToArray(), a_paint_fore);
                        }
                    }
                    break;
                case ArrowType.at_filled_uni://malá
                case ArrowType.at_filled_both://malá

                    if (ab_is_ending || ArrowType.at_filled_both == ai_arrow_style)
                    {
                        List<SKPoint> l_points_2 = new List<SKPoint>();
                        l_points_2.Add(a_cil);
                        l_points_2.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - 12 * li_coef, a_start.Y - 3 * li_coef)));
                        l_points_2.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - 12 * li_coef, a_start.Y + 3 * li_coef)));
                        DrawFilledPolygon(a_canvas, l_points_2, a_paint_fore.Color);
                    }
                    break;
                case ArrowType.at_sip3://malá s ocasem
                    if (ab_is_ending)
                    {
                        List<SKPoint> l_points_3a = new List<SKPoint>();
                        l_points_3a.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - 3 * li_coef)));
                        l_points_3a.Add(a_cil);
                        l_points_3a.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + 3 * li_coef)));
                        DrawPolyLineInner(a_canvas, l_points_3a.ToArray(), a_paint_fore);
                    }
                    else
                    {
                        List<SKPoint> l_points_3b = new List<SKPoint>();
                        l_points_3b.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len + li_coef_x_24, a_start.Y - 3 * li_coef)));
                        l_points_3b.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len, a_start.Y)));
                        l_points_3b.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len + li_coef_x_24, a_start.Y + 3 * li_coef)));
                        DrawPolyLineInner(a_canvas, l_points_3b.ToArray(), a_paint_fore);
                    }
                    break;
                case ArrowType.at_sip4://velká oboustranná
                    {
                        List<SKPoint> l_points_4 = new List<SKPoint>();
                        l_points_4.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - 20 * li_coef, a_start.Y - li_coef_y_5 * li_coef)));
                        l_points_4.Add(a_cil);
                        l_points_4.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - 20 * li_coef, a_start.Y + li_coef_y_5 * li_coef)));
                        DrawPolyLineInner(a_canvas, l_points_4.ToArray(), a_paint_fore);
                    }
                    break;

                case ArrowType.at_sip5://malá oboustranná
                    {
                        List<SKPoint> l_points_5 = new List<SKPoint>();
                        l_points_5.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - li_coef_y_5 * li_coef)));
                        l_points_5.Add(a_cil);
                        l_points_5.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + li_coef_y_5 * li_coef)));
                        DrawPolyLineInner(a_canvas, l_points_5.ToArray(), a_paint_fore);
                    }
                    break;

                case ArrowType.at_sip7://mala s sipkami dovnitr na kotovani der zvenku added version 4.1

                    List<SKPoint> l_points_7 = new List<SKPoint>();
                    l_points_7.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - (li_dist_80 * li_coef), a_start.Y)));
                    l_points_7.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - (li_dist_60 * li_coef), a_start.Y - li_coef_y_5 * li_coef)));
                    DrawPolyLineInner(a_canvas, l_points_7.ToArray(), a_paint_fore);

                    l_points_7.Clear();
                    l_points_7.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - (li_dist_80 * li_coef), a_start.Y)));
                    l_points_7.Add(Loader.Helper.PrevodBodu(a_start, a_cil, new SKPoint(a_start.X + li_len - (li_dist_60 * li_coef), a_start.Y + li_coef_y_5 * li_coef)));
                    DrawPolyLineInner(a_canvas, l_points_7.ToArray(), a_paint_fore);
                    break;

                case ArrowType.at_sip8://    /------------------/ stavební

                    List<SKPoint> l_points_8 = new List<SKPoint>();
                    l_points_8.Add(Loader.Helper.PrevodBodu(a_cil, a_start, new SKPoint(a_cil.X + li_coef_x_7 * li_coef, a_cil.Y - li_coef_y_7 * li_coef)));
                    l_points_8.Add(Loader.Helper.PrevodBodu(a_cil, a_start, new SKPoint(a_cil.X - li_coef_x_7 * li_coef, a_cil.Y + li_coef_y_7 * li_coef)));
                    DrawPolyLineInner(a_canvas, l_points_8.ToArray(), a_paint_fore);
                    break;
            }



        }

        private static void DrawFilledPolygon(SKCanvas a_canvas, List<SKPoint> a_points, SKColor a_color)
        {
            

            SKPath l_path = new SKPath();
            l_path.MoveTo(a_points[0]);
            for (int li_i = 1; li_i < a_points.Count; li_i++)
            {
                l_path.LineTo(a_points[li_i]);
            }
            l_path.Close();

            SKPaint l_paint_fill = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = a_color
            };
            a_canvas.DrawPath(l_path, l_paint_fill);
        }


        private static void Position2EndPoints(CableSymbol a_cableSymbol, out PointF a_point_1, out PointF a_point_2)
        {
            a_point_1 = new PointF();
            a_point_2 = new PointF();

            const int MARGIN_POSITION = 20;

            if (a_cableSymbol.Hor)
            {
                a_point_1.X = a_cableSymbol.Min + MARGIN_POSITION;
                a_point_2.X = a_cableSymbol.Max - MARGIN_POSITION;

                a_point_1.Y = a_cableSymbol.Common;
                a_point_2.Y = a_cableSymbol.Common;
            }
            else
            {
                a_point_1.Y = a_cableSymbol.Min + MARGIN_POSITION;
                a_point_2.Y = a_cableSymbol.Max - MARGIN_POSITION;

                a_point_1.X = a_cableSymbol.Common;
                a_point_2.X = a_cableSymbol.Common;
            }

        }

        private static void DrawTitleBlock(SKCanvas a_canvas, PCadDoc a_doc)
        {
            if(!a_doc.m_ptbPosition.m_useTb)
            {
                return;
            }

            RefGridSettings l_ref_grid_settings = a_doc.Parent.m_ref_grid;
            Size l_size = a_doc.GetSize();


            if (l_ref_grid_settings.Right)
            {
                l_size.Width -= RefGridSettings.GRID_THICKNESS;
            }
            if (l_ref_grid_settings.Bottom)
            {
                l_size.Height -= RefGridSettings.GRID_THICKNESS;
            }

            //pokud je TB otoceny a nahore je refgrid, musime TB posunout dolu
            int li_shiftDown = l_ref_grid_settings.Top ? RefGridSettings.GRID_THICKNESS : 0;


            PtbPosition l_ptb_position = GetQPtbPositionAuto(a_doc);



            DrawTitleBlockInner(a_canvas, l_size, li_shiftDown, l_ptb_position, a_doc.Parent.m_repo, a_doc);

        }

        private static void DrawTitleBlockInner(SKCanvas a_canvas, Size a_size, int ai_shiftDown, PtbPosition a_ptb_position, Repo a_repo, PCadDoc a_doc)
        {
            PtbDoc l_ptb_doc = a_repo.GetPtb(a_ptb_position.Path);
            if(null == l_ptb_doc)
            {
                return;
            }

            ResolveTexts(l_ptb_doc, a_doc.Parent.m_summInfo, a_doc.m_summInfo);

            Rectangle l_rectUsed = l_ptb_doc.GetUsedRect();

            int li_tbBorderRight = l_rectUsed.Right;
            int li_tbBorderBottom = l_rectUsed.Bottom;

            Point l_centerPoint = new Point();
            int li_turns = a_ptb_position.m_turn ? 2 : 0;
            if (li_turns == 2)
            {
                li_tbBorderRight = l_rectUsed.Bottom;
                li_tbBorderBottom = -l_rectUsed.Left;

                l_centerPoint.X = a_size.Width - li_tbBorderRight;
                l_centerPoint.Y = l_rectUsed.Right + ai_shiftDown;
            }
            else
            {
                l_centerPoint.X = a_size.Width - li_tbBorderRight;
                l_centerPoint.Y = a_size.Height - li_tbBorderBottom;
            }


            l_centerPoint.X -= a_ptb_position.m_horDist;
            l_centerPoint.Y -= a_ptb_position.m_verDist;

            a_canvas.Save();
            a_canvas.Translate(l_centerPoint.X, l_centerPoint.Y);

            if (li_turns == 2)
            {
                a_canvas.RotateDegrees(-90);
            }

            DrawPtb(a_canvas, l_ptb_doc);
            a_canvas.Restore();



        }

        private static PtbPosition GetQPtbPositionAuto(PCadDoc a_doc)
        {
            if (!string.IsNullOrEmpty(a_doc.m_ptbPosition.Path))
            {
                return a_doc.m_ptbPosition;
            }

            return a_doc.Parent.m_ptbPosition;
        }

        private static void ResolveTexts(PtbDoc a_ptb_doc, Hashtable a_summInfo_all, Hashtable a_summInfo_page)
        {
            foreach(DrawObj l_obj in a_ptb_doc.m_objects)
            {
                if (l_obj is FreeText l_text)
                {
                    ResolveTextsInner(ref l_text.m_text, a_summInfo_all);
                    ResolveTextsInner(ref l_text.m_text, a_summInfo_page);
                }
            }
        }

        private static void ResolveTextsInner(ref string as_what, Hashtable a_hash_table)
        {
            foreach (string ls_key in a_hash_table.Keys)
            {
                string ls_template = string.Format("{{{0}}}", ls_key);
                as_what = as_what.Replace(ls_template, a_hash_table[ls_key].ToString());
            }
        }

        private static void DrawDimLine(SKCanvas a_canvas, QDimLine a_dim, PCadDoc a_doc)
        {
            Point l_point_1, l_point_2;
            a_dim.Calc_Dim_Points(out l_point_1, out l_point_2);

            Draw_Ext_Lines(a_canvas, a_dim, a_doc, l_point_1, l_point_2);

            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_doc.m_dim_style.m_line_dim.m_color),
                StrokeWidth = a_doc.m_dim_style.m_line_dim.m_thickness
            };

            bool lb_filled = (2 == a_doc.m_dim_style.m_arrow_index);

            switch (a_doc.m_dim_style.m_arrow_index)
            {
                case 0:
                    Draw_Dim_Line_With_Arrows(a_canvas, l_point_1, l_point_2, a_doc.m_dim_style.m_line_dim.m_color, l_paint, lb_filled);
                    break;
                case 1:
                    Draw_Dim_Line_Floor_Plan (a_canvas, l_point_1, l_point_2, a_doc.m_dim_style.m_line_dim.m_color, l_paint);
                    break;
            }

            DrawQLabel(a_canvas, a_dim.Label, a_doc);
        }


        private static void Draw_Ext_Lines(SKCanvas a_canvas, QDimLine a_dim, PCadDoc a_doc, Point a_point_1, Point a_point_2)
        {
            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_doc.m_dim_style.m_line_ext.m_color),
                StrokeWidth = a_doc.m_dim_style.m_line_ext.m_thickness
            };
            a_canvas.DrawLine(a_dim.A.X, a_dim.A.Y, a_point_1.X, a_point_1.Y, l_paint);
            a_canvas.DrawLine(a_dim.B.X, a_dim.B.Y, a_point_2.X, a_point_2.Y, l_paint);

        }

        private static void Draw_Dim_Line_With_Arrows(SKCanvas a_canvas, Point a_point_1, Point a_point_2, Color a_color, SKPaint a_paint, bool ab_filled)
        {
            //if dim line longer then this, draw arrows pointing outwards
            const int MIN_DIM_LINE_ARROWS_IN = 100;

            int li_len = Helper.GetLength(a_point_1, a_point_2);
            if (li_len > MIN_DIM_LINE_ARROWS_IN) //arrows from inside
            {
                a_canvas.DrawLine(a_point_1.X, a_point_1.Y, a_point_2.X, a_point_2.Y, a_paint);
                SipkaWithoutStem(a_canvas, ArrowType.at_sip5, Helper.Point2SKPoint(a_point_1), Helper.Point2SKPoint(a_point_2), a_paint,  true, 1, 0.33f);
                SipkaWithoutStem(a_canvas, ArrowType.at_sip5, Helper.Point2SKPoint(a_point_2), Helper.Point2SKPoint(a_point_1), a_paint, false, 1, 0.33f);
            }
            else //arrows from outside
            {
                const int EXTEND_BY = 48;//extension of the outer arrow

                Point l_point_A, l_point_B;
                Extend_Line_Segment_Both(a_point_1, a_point_2, EXTEND_BY, out l_point_A, out l_point_B);
                a_canvas.DrawLine(l_point_A.X, l_point_A.Y, l_point_B.X, l_point_B.Y, a_paint);
                SipkaWithoutStem(a_canvas, ArrowType.at_sip5, Helper.Point2SKPoint(l_point_A), Helper.Point2SKPoint(a_point_1), a_paint, true, 1, 0.33f);
                SipkaWithoutStem(a_canvas, ArrowType.at_sip5, Helper.Point2SKPoint(l_point_B), Helper.Point2SKPoint(a_point_2), a_paint, true, 1, 0.33f);
            }
        }

        private static void Draw_Dim_Line_Floor_Plan(SKCanvas a_canvas, Point a_point_1, Point a_point_2, Color a_color, SKPaint a_paint)
        {
            a_canvas.DrawLine(a_point_1.X, a_point_1.Y, a_point_2.X, a_point_2.Y, a_paint);
            SipkaWithoutStem(a_canvas, ArrowType.at_sip8, Helper.Point2SKPoint(a_point_1), Helper.Point2SKPoint(a_point_2), a_paint, true, 1, 0.33f);
            SipkaWithoutStem(a_canvas, ArrowType.at_sip8, Helper.Point2SKPoint(a_point_2), Helper.Point2SKPoint(a_point_1), a_paint, true, 1, 0.33f);

        }


        private static void DrawDimCircle(SKCanvas a_canvas, QDimCircle a_dim, PCadDoc a_doc)
        {
            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Helper.Color2SKColor(a_doc.m_dim_style.m_line_dim.m_color),
                StrokeWidth = a_doc.m_dim_style.m_line_dim.m_thickness
            };

            if (a_dim.m_has_2_arrows)
            {
                bool lb_filled = (2 == a_doc.m_dim_style.m_arrow_index);
                Draw_Dim_Line_With_Arrows(a_canvas, a_dim.A, a_dim.B, a_doc.m_dim_style.m_line_dim.m_color, l_paint, lb_filled);
            }
            else
            {
                a_canvas.DrawLine(a_dim.A.X, a_dim.A.Y, a_dim.B.X, a_dim.B.Y, l_paint);
                SipkaWithoutStem(a_canvas, ArrowType.at_sip5, Helper.Point2SKPoint(a_dim.B), Helper.Point2SKPoint(a_dim.A), l_paint, true, 1, 0.33f);
            }

            DrawQLabel(a_canvas, a_dim.Label, a_doc);
        }


        private static void Extend_Line_Segment_Both(Point a_point_A, Point a_point_B, int ai_len_diff, out Point a_point_C, out Point a_point_D)
        {
            Extend_Line_Segment_One(a_point_A, a_point_B, ai_len_diff, out a_point_D);
            Extend_Line_Segment_One(a_point_B, a_point_A, ai_len_diff, out a_point_C);
        }

        private static void Extend_Line_Segment_One(Point a_point_A, Point a_point_B, int ai_len_diff, out Point a_point_C)
        {
            int li_len_old = Helper.GetLength(a_point_A, a_point_B);
            if(0 == li_len_old)
            {
                li_len_old = 1;//prevent division by zero
            }

            int li_len_new = li_len_old + ai_len_diff;

            a_point_C = new Point();
            a_point_C.X = a_point_A.X + MulDiv(a_point_B.X - a_point_A.X, li_len_new, li_len_old);
            a_point_C.Y = a_point_A.Y + MulDiv(a_point_B.Y - a_point_A.Y, li_len_new, li_len_old);
        }

        private static int MulDiv(int ai_number, int ai_numerator, int ai_denominator)
        {
            return ai_number * ai_numerator / ai_denominator;
        }

        private static void DrawQLabel(SKCanvas a_canvas, QLabel a_label, PCadDoc a_doc)
        {
            //SKPaint l_paint = Helper.Efont2SKPaint(a_doc.m_dim_style.m_label_font, QTextAlignment.AL_MM);

            a_canvas.Save();

            float lf_angle = (float)(a_label.AngleTenths / -10);
            //a_canvas.RotateDegrees(lf_angle, a_label.Center.X, a_label.Center.Y);

            int li_angle_tenths = 0;
            if(a_doc.m_dim_style.m_align_text_with_dim_line)
            {
                li_angle_tenths = a_label.AngleTenths;
                //li_angle_tenths = a_label.AngleTenths / -10;
            }

            DrawerText.Draw_Centered_Text(a_canvas, 
                a_label.Text, 
                a_label.Center.X, a_label.Center.Y, 
                a_doc.m_dim_style.m_label_font,
                li_angle_tenths,
                false
            );

            a_canvas.Restore();
        }


        //----------------------------
    }
}
