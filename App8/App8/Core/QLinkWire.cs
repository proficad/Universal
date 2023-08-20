using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    internal class QLinkWire : QLink
    {
        private string m_label;
//        private string m_wire_name;
//        private bool m_my_wire_is_vertically;

        public QLinkWire(Rectangle a_rect, string as_wire_name, bool ab_vertically) : base(a_rect)
        {

        }

        internal void SetLabel(string as_what)
        {
            m_label = as_what;
        }
    }
}
