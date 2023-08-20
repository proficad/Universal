using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    internal class QLink : DrawObj
    {
        protected QLink(Rectangle a_rect) : base(Shape.shapeNone, a_rect)
        {

        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        { }

        internal override void MoveBy(Size l_offset)
        {
            m_position.X += l_offset.Width;
            m_position.Y += l_offset.Height;
        }

        public override void RecalcPosition()
        { }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        { }
    }
}
