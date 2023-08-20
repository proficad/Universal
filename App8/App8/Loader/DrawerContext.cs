using System;
using System.Collections.Generic;
using System.Text;

using DxfNet;

namespace Loader
{
    class DrawerContext
    {
        private static DrawerContext _current;
        public static DrawerContext Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new DrawerContext();
                }
                return _current;
            }
        }


        public static void Setup(PCadDoc a_doc)
        {
            Current.m_pcadDoc = a_doc;
        }


        private PCadDoc m_pcadDoc;
        public PCadDoc PCadDocument
        {
            get
            {
                return m_pcadDoc;
            }
        }

        //--------------------
    }
}
