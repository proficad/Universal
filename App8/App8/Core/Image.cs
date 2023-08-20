using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SkiaSharp;

namespace DxfNet
{
    public class QImage : DrawObj
    {
        public QImage(Shape a_shape, Rectangle a_rect, string as_lastGuid, 
            int a_angle_tenths, bool ab_hor, bool ab_ver) 
            : base(a_shape, a_rect)
        {
            LastGuid = as_lastGuid;
            Angle = a_angle_tenths / -10f;

            m_hor = ab_hor;
            m_ver = ab_ver;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {

        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        {

        }

        internal override void MoveBy(Size l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public override void RecalcPosition()
        {

        }

        public QImageDesc ImgDesc { get; set; }
        public string LastGuid;

        public float Angle { get; private set; }
        public SKBitmap m_bitmap;

        public bool m_hor;
        public bool m_ver;


        public double GetWidth()
        {
            return m_position.Width;
        }

        public double GetHeight()
        {
            return m_position.Height;
        }


    }
}
