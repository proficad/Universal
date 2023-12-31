﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class QDimStyle
    {
     
        public QDimStyle(string as_name) { m_name = as_name; }//should be nonnullable string

        const string ATTR_LINE_THICK = "op-lw";
        const string ATTR_LINE_COLOR = "op-lc";

        public int m_arrow_index; //0..normal, 1..floor plan, 2..filled

        public EFont m_label_font = new EFont();

        public bool m_align_text_with_dim_line;
        public string m_name;

        public enum Text_Position { text_position_above, text_position_over, text_position_below };
        public Text_Position m_text_position;// 0..above, 1..over, 2..below

        public struct ColorThickness { public Color m_color; public int m_thickness; }
        public ColorThickness m_line_ext, m_line_dim;


    }
}
