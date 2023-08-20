using System;
using System.Collections.Generic;
using System.Text;
using DxfNet;
using SkiaSharp;

namespace Loader
{
    class HelperRefGrid
    {
        const int GRID_THICKNESS = 50;
        const int CENTERING_MARK_LEN = 100;
        const int FIELD_SIZE_MIN = 200;
        const int FIELD_SIZE_MAX = 2000;
        const int FIELD_SIZE_DEFAULT = 500;
        enum TypeOfEdge { rgEdgeLeft, rgEdgeTop, rgEdgeRight, rgEdgeBottom };


        public static void DrawRefGridForSheets(SKCanvas a_canvas, SKRect a_rect, RefGridSettings a_settings)
        {
            int li_fieldSize_tenth_mm = 10 * a_settings.FieldSize;
            if ((li_fieldSize_tenth_mm < FIELD_SIZE_MIN) || (li_fieldSize_tenth_mm > FIELD_SIZE_MAX))
            {
                li_fieldSize_tenth_mm = FIELD_SIZE_DEFAULT;
            }


            if (a_settings.Left)
            {
                DrawEdge(a_canvas, TypeOfEdge.rgEdgeLeft, a_rect.Left, a_rect.Top, a_rect.Bottom, a_settings.Top, a_settings.Bottom, li_fieldSize_tenth_mm);
            }
            if (a_settings.Top)
            {
                DrawEdge(a_canvas, TypeOfEdge.rgEdgeTop, a_rect.Top, a_rect.Left, a_rect.Right, a_settings.Left, a_settings.Right, li_fieldSize_tenth_mm);
            }
            if (a_settings.Right)
            {
                DrawEdge(a_canvas, TypeOfEdge.rgEdgeRight, a_rect.Right, a_rect.Top, a_rect.Bottom, a_settings.Top, a_settings.Bottom, li_fieldSize_tenth_mm);
            }
            if (a_settings.Bottom)
            {
                DrawEdge(a_canvas, TypeOfEdge.rgEdgeBottom, a_rect.Bottom, a_rect.Left, a_rect.Right, a_settings.Left, a_settings.Right, li_fieldSize_tenth_mm);
            }
        }
	    

        static void DrawEdge(SKCanvas a_canvas, TypeOfEdge a_edge, float ai_outer, float ai_start, float ai_end, bool ab_start, bool ab_end, int ai_fieldSize)
        {
            const int LINE_THIN = 3;
            const int LINE_THICK = 7;


            if ((ai_fieldSize < FIELD_SIZE_MIN) || (ai_fieldSize > FIELD_SIZE_MAX))
            {
                ai_fieldSize = FIELD_SIZE_DEFAULT;
            }
            int li_min_field_size = ai_fieldSize / 2;


            int li_inner = 0;
            int li_posTextX = 0;
            bool lb_turn = false;
            if ((a_edge == TypeOfEdge.rgEdgeTop) || (a_edge == TypeOfEdge.rgEdgeBottom))
            {
                lb_turn = true;
            }

            int li_outer = Convert.ToInt32(ai_outer);
            switch (a_edge)
            {
                case TypeOfEdge.rgEdgeLeft:
                case TypeOfEdge.rgEdgeTop:
                    li_inner = li_outer + GRID_THICKNESS;
                    li_posTextX = li_outer + (GRID_THICKNESS / 2);
                    break;
                case TypeOfEdge.rgEdgeRight:
                case TypeOfEdge.rgEdgeBottom:
                    li_inner = li_outer - GRID_THICKNESS;
                    li_posTextX = li_outer - (GRID_THICKNESS / 2);
                    break;
            }

            int li_startAltered = Convert.ToInt32(ai_start);    if (ab_start)   { li_startAltered   += GRID_THICKNESS; }
            int li_endAltered   = Convert.ToInt32(ai_end);      if (ab_end)     { li_endAltered     -= GRID_THICKNESS; }



            //draw left edge
            LineVer(a_canvas, lb_turn, ai_outer, ai_start, ai_end, LINE_THIN);
            LineVer(a_canvas, lb_turn, li_inner, li_startAltered, li_endAltered, LINE_THICK);

            int li_center = (li_startAltered + li_endAltered) / 2;

            //prostredni spricle
            LineHor(a_canvas, lb_turn, li_center, ai_outer, li_inner, LINE_THICK);


            int li_remainder = li_center;
            int li_distanceFromCenter = 0;


            //pocet celych poli
            int li_numberOfFieldsHalved = GetNumberOfFieldsHalved(li_center - li_startAltered, ai_fieldSize);


            int li_steps = 0;
            string ls_letter = string.Empty;

            while ((li_remainder - li_startAltered) > (ai_fieldSize + li_min_field_size))
            {
                li_remainder -= ai_fieldSize;
                li_distanceFromCenter += ai_fieldSize;

                LineHor(a_canvas, lb_turn, li_center - li_distanceFromCenter, ai_outer, li_inner, LINE_THIN);
                LineHor(a_canvas, lb_turn, li_center + li_distanceFromCenter, ai_outer, li_inner, LINE_THIN);

                Translate2Letters(li_numberOfFieldsHalved - li_steps, out ls_letter, !lb_turn);
                LetterInRefGrid(a_canvas, lb_turn, li_posTextX, li_center - li_distanceFromCenter + (ai_fieldSize / 2), ls_letter);
                Translate2Letters(li_numberOfFieldsHalved + li_steps + 1, out ls_letter, !lb_turn);
                LetterInRefGrid(a_canvas, lb_turn, li_posTextX, li_center + li_distanceFromCenter - (ai_fieldSize / 2), ls_letter);
                ++li_steps;
            }

            //last letter up
            int li_posFromEnd = (li_center - li_distanceFromCenter - li_startAltered) / 2;
            int li_lastLetter = li_startAltered + li_posFromEnd;
            Translate2Letters(li_numberOfFieldsHalved - li_steps, out ls_letter, !lb_turn);
            LetterInRefGrid(a_canvas, lb_turn, li_posTextX, li_lastLetter, ls_letter);
            //last letter down
            li_lastLetter = Convert.ToInt32(ai_end) - li_posFromEnd;
            Translate2Letters(li_numberOfFieldsHalved + li_steps + 1, out ls_letter, !lb_turn);
            LetterInRefGrid(a_canvas, lb_turn, li_posTextX, li_lastLetter, ls_letter);
        }

        private static void LineHor(SKCanvas a_canvas, bool ab_turn, float ai_outer, float ai_start, float ai_end, int ai_thickness)
        {
            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SkiaSharp.SKColors.Black,
                StrokeWidth = ai_thickness
            };

            if (ab_turn)
            {
                a_canvas.DrawLine(ai_outer, ai_start, ai_outer, ai_end, l_paint);
            }
            else
            {
                a_canvas.DrawLine(ai_start, ai_outer, ai_end, ai_outer, l_paint);
            }
        }

        private static void LineVer(SKCanvas a_canvas, bool ab_turn, float ai_outer, float ai_start, float ai_end, int ai_thickness)
        {
            SKPaint l_paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SkiaSharp.SKColors.Black,
                StrokeWidth = ai_thickness
            };

            if (ab_turn)
            {
                a_canvas.DrawLine(ai_start, ai_outer, ai_end, ai_outer, l_paint);
            }
            else
            {
                a_canvas.DrawLine(ai_outer, ai_start, ai_outer, ai_end, l_paint);
            }
        }

        private static int GetNumberOfFieldsHalved(int ai_center, int ai_fieldSize)
        {
            int li_min_field_size = ai_fieldSize / 2;

            int li_numberOfFieldsHalved = (ai_center / ai_fieldSize);
            if ((ai_center % ai_fieldSize) > li_min_field_size)
            {
                ++li_numberOfFieldsHalved;
            }

            return li_numberOfFieldsHalved;
        }

        private static void Translate2Letters(int ai_input, out string as_letters, bool ab_translate)
        {
            if (!ab_translate)
            {
                as_letters = ai_input.ToString();
                return;
            }

            as_letters = string.Empty;

            --ai_input;

            const int l_start = 65;
            const int li_size = 26 - 2;

            int l_prvni = ai_input % li_size;
            int l_druha = ai_input / li_size;

       

            if (l_druha > 0)
            {
                char fst = Convert.ToChar(GetCharSkipped(l_druha - 1) + l_start);
                char snd = Convert.ToChar(GetCharSkipped(l_prvni) + l_start);
                as_letters += fst;
                as_letters += snd;
                return;
            }
            as_letters += Convert.ToChar(GetCharSkipped(l_prvni) + l_start);

        }

        private static int GetCharSkipped(int ai_input)
        {
            //skip I and O
            const int li_I = 8;
            const int li_O = li_I + 6;

            if (ai_input >= li_I)
            {
                ai_input++;
            }
            if (ai_input >= li_O)
            {
                ai_input++;
            }

            return ai_input;
        }

        private static void LetterInRefGrid(SKCanvas a_canvas, bool ab_turn, int ai_x, int ai_y, string as_what)
        {
            if (ab_turn)
            {
                //swap
                int li_temp = ai_x;
                ai_x = ai_y;
                ai_y = li_temp;
            }
           
            EFont l_efont = new EFont();
            Drawer.DrawerText.Draw_Centered_Text(a_canvas, as_what, ai_x, ai_y, l_efont, 0, false);
        }
        



        public static void GetCoordinates(SKRect a_rect, SKPoint a_point, out string as_coords, RefGridSettings a_settings)
        {

            int li_fieldSize_tenth_mm = 10 * a_settings.FieldSize;
	        if ((li_fieldSize_tenth_mm<FIELD_SIZE_MIN) || (li_fieldSize_tenth_mm > FIELD_SIZE_MAX))
	        {
		        li_fieldSize_tenth_mm = FIELD_SIZE_DEFAULT;
	        }


	        if (a_settings.Left)
	        {
		        a_rect.Left += GRID_THICKNESS;
	        }
	        if (a_settings.Top)
	        {
		        a_rect.Top += GRID_THICKNESS;
	        }
	        if (a_settings.Right)
	        {
		        a_rect.Right -= GRID_THICKNESS;
	        }
	        if (a_settings.Bottom)
	        {
		        a_rect.Bottom -= GRID_THICKNESS;
	        }


	        if (!a_rect.Contains(a_point))
	        {
		        as_coords = "";
		        return;
	        }

	        int li_x = GetCoordinate(a_rect.Left, a_rect.Right, a_point.X, li_fieldSize_tenth_mm);
            int li_y = GetCoordinate(a_rect.Top, a_rect.Bottom, a_point.Y, li_fieldSize_tenth_mm);
        
            string ls_y;
            Translate2Letters(li_y, out ls_y, true);

            as_coords = ls_y + li_x.ToString();
        }

        static int GetCoordinate(float ai_start, float ai_end, float ai_pos, int ai_fieldSize)
        {
            int li_center = Convert.ToInt32(ai_start + ai_end) / 2;
            int li_numberOfFieldsHalved = GetNumberOfFieldsHalved(Convert.ToInt32(li_center - ai_start), ai_fieldSize);

            int li_posOut = 1;

            for (int li_i = 1 - li_numberOfFieldsHalved; li_i < li_numberOfFieldsHalved; li_i++)
            {
                if (ai_pos < (li_center + (li_i * ai_fieldSize)))
                {
                    return li_posOut;
                }
                ++li_posOut;
            }

            return li_posOut;
        }

        //----------------
    }
}
