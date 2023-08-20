using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

using DxfNet;
using SkiaSharp;



namespace Drawer
{
    public static class DrawerWire
    {
        public static void DrawWire(SKCanvas a_canvas, Wire a_wire, PCadDoc pCadDoc)
        {
            Drawer.DrawPolyline(a_canvas, a_wire);

            if (a_wire.GetDrop1())
            {
                DrawJoint(a_canvas, a_wire.m_points[0], a_wire.m_objProps.m_logpen.m_color);
            }
            if (a_wire.GetDrop2())
            {
                int li_lastIndex = a_wire.m_points.Count - 1;
                DrawJoint(a_canvas, a_wire.m_points[li_lastIndex], a_wire.m_objProps.m_logpen.m_color);
            }

            Draw_Wire_Label(a_canvas, pCadDoc.Parent.m_fonts.m_fontRef, pCadDoc.Parent.m_settingsNumberingWire, a_wire);
        }


        private static void DrawJoint(SKCanvas a_canvas, Point a_point, Color a_color)
        {
            const int li_radiusJoint = 7;

            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Helper.Color2SKColor(a_color)
            };

            a_canvas.DrawCircle(a_point.X, a_point.Y, li_radiusJoint, l_paint);
        }


        private static void Draw_Wire_Label(SKCanvas a_canvas, EFont a_efont, SettingsNumberingWire pSettings, Wire a_wire)
        {
            if (string.IsNullOrWhiteSpace(a_wire.GetName()))
            {
                return;
            }

            SettingsNumberingWire.EnumShowWireNumbers l_swn = pSettings.ShowWireNumbers;
            if (l_swn != SettingsNumberingWire.EnumShowWireNumbers.swn_no)
            {
                if ((l_swn == SettingsNumberingWire.EnumShowWireNumbers.swn_both) && (a_wire.IsWireShort(pSettings.Long_Wire_Len)))
                {
                    Export_Straight_Wire_Label(a_canvas, a_wire, a_efont, pSettings);
                }
                else
                {
                    if ((!a_wire.Is_connected_first) || l_swn == SettingsNumberingWire.EnumShowWireNumbers.swn_both)
                    {
                        ExportWireLabel(a_canvas, a_wire, true, a_efont,
                            pSettings.WireLabelDist_A, pSettings.WireLabelDist_B, pSettings.Vertically, pSettings.Over);
                    }
                    if ((!a_wire.Is_connected_last) || l_swn == SettingsNumberingWire.EnumShowWireNumbers.swn_both)
                    {
                        ExportWireLabel(a_canvas, a_wire, false, a_efont,
                            pSettings.WireLabelDist_A, pSettings.WireLabelDist_B, pSettings.Vertically, pSettings.Over);
                    }
                }
            }
        }


        private static void Export_Straight_Wire_Label(SKCanvas a_canvas, DxfNet.Wire a_wire, EFont a_efont, SettingsNumberingWire pSettings)
        {
            string ls_name = a_wire.GetName();


            //adjust the center point depending on the orientation of the wire
            UtilsMath.cardinal_directions l_cd = UtilsMath.GetDirection(a_wire.m_points[0], a_wire.m_points[1]);
            if (l_cd == UtilsMath.cardinal_directions.cd_none)
            {
                return;
            }

            //draw fixed label (label dragged by the user)
            if (a_wire.m_label_mid != null)
            {
                int li_angle_tenths = 0;
                if ((l_cd == UtilsMath.cardinal_directions.cd_north) || (l_cd == UtilsMath.cardinal_directions.cd_south))
                {
                    if (pSettings.Vertically)
                    {
                        li_angle_tenths = 900;
                    }
                }
                DrawerText.Draw_Centered_Text(a_canvas, ls_name, a_wire.m_label_mid.m_point.X, a_wire.m_label_mid.m_point.Y, a_efont, li_angle_tenths, true);
                return;
            }

            PointF l_center_point = a_wire.GetLineCenterPoint();

            if ((l_cd == UtilsMath.cardinal_directions.cd_west) || (l_cd == UtilsMath.cardinal_directions.cd_east))
            {
                if(pSettings.Over)
                {
                    DrawerText.Draw_Centered_Text(a_canvas, ls_name, l_center_point.X, l_center_point.Y, a_efont, 0, true);
                }
                else
                {
                    l_center_point.Y -= pSettings.WireLabelDist_A;
                    SKPaint l_paint = Helper.Efont2SKPaint(a_efont, QTextAlignment.AL_MM);
                    DrawerText.Draw_Text_Centered(a_canvas, ls_name, l_center_point, l_paint, DrawerText.Centered_Text_Top_Bottom.Bottom);
                }
            }
            else if ((l_cd == UtilsMath.cardinal_directions.cd_north) || (l_cd == UtilsMath.cardinal_directions.cd_south))
            {
                if (!pSettings.Over)
                {
                    l_center_point.X -= pSettings.WireLabelDist_A;
                }

                if (pSettings.Vertically)
                {
                    SKPaint l_paint = Helper.Efont2SKPaint(a_efont, QTextAlignment.AL_MM);
                    if (pSettings.Over)
                    {
                        SKRect l_rect = new SKRect();
                        l_paint.MeasureText(ls_name, ref l_rect);
                    }
                    else
                    {
                        l_paint.GetFontMetrics(out SKFontMetrics l_metrics);
                        l_center_point.X -= l_metrics.Descent;
                    }
                    DrawerText.Draw_Centered_Text(a_canvas, ls_name, l_center_point.X, l_center_point.Y, a_efont, 900, true);
                }
                else
                {
                    SKPaint l_paint = Helper.Efont2SKPaint(a_efont, QTextAlignment.AL_RM);
                    SKRect l_rect = new SKRect();
                    l_paint.MeasureText(ls_name, ref l_rect);

                    if (pSettings.Over)
                    {
                        l_center_point.Y += (l_rect.Height / 2);
                        DrawerText.Draw_Text_Centered(a_canvas, ls_name, l_center_point, l_paint, DrawerText.Centered_Text_Top_Bottom.Bottom);
                    }
                    else
                    {
                        l_center_point.Y += l_rect.Height;
                        DrawerText.Draw_Wire_Label(a_canvas, ls_name, l_center_point.X, l_center_point.Y, l_paint, pSettings.Over);
                    }
                }
            }
        
        }


        private static void ExportWireLabel(SKCanvas a_canvas, DxfNet.Wire a_wire, bool ab_first, EFont a_efont, int ai_a, int ai_b, bool ab_vertically, bool ab_over)
        {
            Point l_point_nearest, l_point_next;
            if (ab_first)
            {
                l_point_nearest = a_wire.m_points[0];
                l_point_next = a_wire.m_points[1];
            }
            else
            {
                l_point_nearest = a_wire.m_points.Last();
                l_point_next = a_wire.GetLastButOne();
            }

            UtilsMath.cardinal_directions l_cd = UtilsMath.GetDirection(l_point_nearest, l_point_next);
            if (l_cd == UtilsMath.cardinal_directions.cd_none)
            {
                return;
            }

            //draw fixed label (label dragged by the user)
            WireLabelPos l_wire_label_pos = a_wire.GetEndingLabelVis(ab_first);
            if (l_wire_label_pos != null)
            {
                int li_angle_tenths = 0;
                if ((l_cd == UtilsMath.cardinal_directions.cd_north) || (l_cd == UtilsMath.cardinal_directions.cd_south))
                {
                    if (ab_vertically)
                    {
                        li_angle_tenths = 900;
                    }
                }
                DrawerText.Draw_Centered_Text(a_canvas, a_wire.GetName(), l_wire_label_pos.m_point.X, l_wire_label_pos.m_point.Y, a_efont, li_angle_tenths, true);
                return;
            }


            DrawWireLabelInternal(a_canvas, a_wire.GetName(), a_efont, l_cd, l_point_nearest, ai_a, ai_b, ab_vertically, ab_over);
        }


        // a = distance from wire
        // b = distance from end of wire
        private static void DrawWireLabelInternal(SKCanvas a_canvas, string as_name, EFont a_efont, UtilsMath.cardinal_directions a_cd, PointF a_point_nearest, int ai_a, int ai_b, bool ab_vertically, bool ab_over)
        {
            if ((a_cd == UtilsMath.cardinal_directions.cd_west) || (a_cd == UtilsMath.cardinal_directions.cd_east))
            {
                if(!ab_over)
                {
                    a_point_nearest.Y -= ai_a;
                }

                QTextAlignment l_alignment;
                if (a_cd == UtilsMath.cardinal_directions.cd_west)
                {
                    a_point_nearest.X -= ai_b;
                    l_alignment = QTextAlignment.AL_RM;
                }
                else
                {
                    a_point_nearest.X += ai_b;
                    l_alignment = QTextAlignment.AL_LM;
                }
                
                SKPaint l_paint = Helper.Efont2SKPaint(a_efont, l_alignment);
                DrawerText.Draw_Wire_Label(a_canvas, as_name, a_point_nearest.X, a_point_nearest.Y, l_paint, ab_over);              
            }
            else if ((a_cd == UtilsMath.cardinal_directions.cd_north) || (a_cd == UtilsMath.cardinal_directions.cd_south))
            {
                if(!ab_over)
                {
                    a_point_nearest.X -= ai_a;
                }

                QTextAlignment l_align_vertical;
                if (a_cd == UtilsMath.cardinal_directions.cd_north)
                {
                    a_point_nearest.Y -= ai_b;
                    l_align_vertical = QTextAlignment.AL_LM;
                }
                else
                {
                    a_point_nearest.Y += ai_b;
                    l_align_vertical = QTextAlignment.AL_RM;
                }

                if (ab_vertically)
                {
                    SKPaint l_paint = Helper.Efont2SKPaint(a_efont, l_align_vertical);
                    l_paint.GetFontMetrics(out SKFontMetrics l_metrics);

                    if (ab_over)
                    {
                        SKRect l_rect = new SKRect();
                        l_paint.MeasureText(as_name, ref l_rect);

                        if (a_cd == UtilsMath.cardinal_directions.cd_north)
                        {
                            a_point_nearest.Y -= (l_rect.Width / 2);
                        }
                        else
                        {
                            a_point_nearest.Y += (l_rect.Width / 2);
                        }
                    }
                    else
                    {
                        a_point_nearest.X -= l_metrics.Descent;
                    }

                    DrawerText.Draw_Centered_Text(a_canvas, as_name, a_point_nearest.X, a_point_nearest.Y, a_efont, 900, true);
                }
                else
                {
                    SKPaint l_paint = Helper.Efont2SKPaint(a_efont, QTextAlignment.AL_RM);


                    DrawerText.Centered_Text_Top_Bottom l_top_bottom =
                    l_align_vertical == QTextAlignment.AL_LM
                    ?
                    DrawerText.Centered_Text_Top_Bottom.Bottom
                    :
                    DrawerText.Centered_Text_Top_Bottom.Top;

                    if(!ab_over)
                    {
                        SKRect l_rect = new SKRect();
                        l_paint.MeasureText(as_name, ref l_rect);
                        a_point_nearest.X -= (l_rect.Width / 2);
                        a_point_nearest.Y -= (l_rect.Height / 2);
                    }

                    DrawerText.Draw_Text_Centered(a_canvas, as_name, a_point_nearest, l_paint, l_top_bottom);
                }

            }

        }


        //-----------------------
        }
    }
