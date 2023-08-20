using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxfNet
{
    public class QFontsCollection
    {
        public EFont m_fontLetter = new EFont();
        public EFont m_fontText = new EFont();

        public EFont m_fontRef = new EFont();
        public EFont m_fontType = new EFont();

        public EFont m_fontCrossRef = new EFont();
        public EFont m_fontOutlets = new EFont();

        internal EFont Get_Efont(string as_attrib_name)
        {
            switch(as_attrib_name)
            {
                case "_ref": return m_fontRef;
                case "_type": return m_fontType;

                default: return m_fontRef;
            }
        }
    }
}
