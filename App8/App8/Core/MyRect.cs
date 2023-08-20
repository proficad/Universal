using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public struct MyRect
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public void OffsetRect(int ai_x, int ai_y)
        {
            Left    += ai_x;
            Top     += ai_y;
            Right   += ai_x;
            Bottom  += ai_y;
        }
        //---------------
    }
}
