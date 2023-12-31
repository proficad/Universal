﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using App8.Core;

namespace DxfNet
{
    public class Repo
    {
        public List<PpdDoc>     m_listPpd       = new List<PpdDoc>();
#pragma warning disable S1104 // Fields should not have public accessibility
        public List<QImageDesc> m_listImgDesc   = new List<QImageDesc>();
#pragma warning restore S1104 // Fields should not have public accessibility
        public List<PtbDoc>     m_listPtb       = new List<PtbDoc>();
        public List<QDimStyle> m_listDimStyles  = new List<QDimStyle>();

        public QDimStyle GetDimStyle(string as_name)
        {
            QDimStyle l_dimstyle = m_listDimStyles.FirstOrDefault(x => x.m_name == as_name);

            if(l_dimstyle == null)
            {
                l_dimstyle = new QDimStyle(as_name);
            }
            return l_dimstyle;
        }

        public PpdDoc FindPpdDocInRepo(string as_lastGuid)
        {
            foreach(PpdDoc l_doc in m_listPpd)
            {
                if(l_doc.m_lG == as_lastGuid)
                {
                    return l_doc;
                }
            }
            return null;
        }

        public QImageDesc FindImageInRepo(string as_lastGuid)
        {
            foreach (QImageDesc l_img_desc in m_listImgDesc)
            {
                if (l_img_desc.LastGuid == as_lastGuid)
                {
                    return l_img_desc;
                }
            }
            return null;
        }


        public PtbDoc GetPtb(string as_name)
        {
            foreach (PtbDoc l_doc in m_listPtb)
            {
                if (l_doc.Path == as_name)
                {
                    return l_doc;
                }
            }

            return null;
        }

        public void AddPpd(PpdDoc a_ppdDoc)
        {
            m_listPpd.Add(a_ppdDoc);
        }

        public void AddImgDesc(QImageDesc a_imgDesc)
        {
            m_listImgDesc.Add(a_imgDesc);
        }

        public void AddTb(PtbDoc a_ptbDoc)
        {
            m_listPtb.Add(a_ptbDoc);
        }

        public void AddDimStyle(QDimStyle a_style)
        {
            m_listDimStyles.Add(a_style);
        }


    }
}
