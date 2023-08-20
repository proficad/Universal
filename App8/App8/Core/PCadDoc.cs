using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

using App8.Core;

namespace DxfNet
{
    public class PCadDoc : DrawDoc
    {
        public bool m_isPortrait;
        public int m_ratio_tisk;
        public int m_zoomView;
        public Rectangle m_rectUserMarginLog;
        public bool m_popisovepole;
        public bool m_oramovani;
        public Size m_size;
        public int m_rastr;//rastr in 0.1mm
        public PtbPosition m_ptbPosition = new PtbPosition();//version 8, each page may have its own TB

        public Hashtable m_summInfo = new Hashtable();//2012-11-02

        public QDimStyle m_dim_style;

        public PageSizeSettings m_page_size_settings = new PageSizeSettings();
        public PagePrintSettings m_page_print_settings = new PagePrintSettings();

        public string Name { get; set; }

        private CollPages m_parent;
        public int Scale;
        public CollPages Parent
        {
            get { return m_parent; }
        }

        public Repo GetRepo()
        {
            return m_parent.m_repo;
        }

        public List<Layer> m_layers;

        public PCadDoc(CollPages a_parent)
        {
            m_parent = a_parent;
            m_layers = new List<Layer>();
        }
        //-------------------

        public static Size GetPaperSize()
        {

            int li_width = 2036;
            int li_height = 2922;
            
            return new Size(li_width, li_height);
        }



        public void Save(string as_path)
        {
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            
            XmlWriter l_xmlWriter = XmlWriter.Create(as_path, settings);

            l_xmlWriter.WriteStartDocument();

            WriteIntroElement(l_xmlWriter);
            WriteFontCollection(l_xmlWriter);
            WritePageSettings(l_xmlWriter);
            WriteRepo(l_xmlWriter);


            WriteElements(l_xmlWriter);


            //close the Intro element
            l_xmlWriter.WriteEndElement();



            l_xmlWriter.WriteEndDocument();
            l_xmlWriter.Close();
        }

        internal new void WriteElements(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("pages");
            a_xmlWriter.WriteStartElement("page");
            a_xmlWriter.WriteAttributeString("name", "1");

            a_xmlWriter.WriteStartElement("layers");
            a_xmlWriter.WriteStartElement("layer");
            a_xmlWriter.WriteAttributeString("name", "0");
            a_xmlWriter.WriteAttributeString("v", "1");


            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.Write2Xml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
        }

        private void WritePageSettings(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("PageSettings");
            a_xmlWriter.WriteAttributeString("bgColor", "FFFFFF");
            a_xmlWriter.WriteAttributeString("pgsHor", Parent.m_settingsPage.PagesHor.ToString());
            a_xmlWriter.WriteAttributeString("pgsVer", Parent.m_settingsPage.PagesVer.ToString());
            a_xmlWriter.WriteEndElement();
        }

        private void WriteRepo(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("repo");
            foreach (PpdDoc l_ppdDoc in GetRepo().m_listPpd)
                {
                    l_ppdDoc.SaveToXml(a_xmlWriter);
                }
            a_xmlWriter.WriteEndElement();
        }

        private void WriteFontCollection(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("fonts");
                a_xmlWriter.WriteAttributeString("letter", "155,0,0,0,Lucida Sans Unicode,000000");
                a_xmlWriter.WriteAttributeString("text", "110,0,0,0,Lucida Sans Unicode,000000");
                a_xmlWriter.WriteAttributeString("type", "85,0,0,0,Arial,000000");
                a_xmlWriter.WriteAttributeString("value", "85,0,0,0,Arial,000000");
            a_xmlWriter.WriteEndElement();
        }



        private void WriteIntroElement(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("document");
            a_xmlWriter.WriteAttributeString("type", "ProfiCAD sxe");
            a_xmlWriter.WriteAttributeString("version", "7.0");
            
            a_xmlWriter.WriteAttributeString("pgsOri", "LANDSCAPE");
            a_xmlWriter.WriteAttributeString("pgsMrgn", "0, 0, 1, 1");
            a_xmlWriter.WriteAttributeString("zoom", "100");
            a_xmlWriter.WriteAttributeString("rastr", "20");
        }



        //----------------------------

        public void AddPpdDoc(PpdDoc l_ppdDoc)
        {
            GetRepo().AddPpd(l_ppdDoc);
        }

        public PpdDoc FindPpdDocInRepo(string ls_lastGuid)
        {
            PpdDoc l_ppdDoc = GetRepo().FindPpdDocInRepo(ls_lastGuid);
            return l_ppdDoc;
            
        }

        public void RecalcToFitInPaper()
        {
            Rectangle l_rectDocument;

            if (m_objects.Count == 0)
            {
                return;
            }

            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.RecalcPosition();
            }            
            
            l_rectDocument = m_objects[0].m_position;


            foreach (DrawObj a_obj in m_objects)
            {
                l_rectDocument = Rectangle.Union(l_rectDocument, a_obj.m_position);
            }


            Size l_offset = new Size(0, 0);
            if (l_rectDocument.X < 0)
            {
                l_offset.Width = -l_rectDocument.X;
            }
            if (l_rectDocument.Y < 0)
            {
                l_offset.Height = -l_rectDocument.Y;
            }
            if((l_offset.Width != 0) || (l_offset.Height != 0))
            {
                foreach (DrawObj a_obj in m_objects)
                {
                    a_obj.MoveBy(l_offset);
                }
            }

            Size l_paperSize = GetPaperSize();

            if (l_rectDocument.Width > l_rectDocument.Height)//landscape
            {
                int li_temp = l_paperSize.Width;
                l_paperSize.Width = l_paperSize.Height;
                l_paperSize.Height = li_temp;
            }

            Parent.m_settingsPage.PagesHor = 1 + (l_rectDocument.Width / l_paperSize.Width);
            Parent.m_settingsPage.PagesVer = 1 + (l_rectDocument.Height / l_paperSize.Height);

        }

        public void SetSize(Size a_size)
        {
            Size l_paperSize = GetPaperSize();


            Parent.m_settingsPage.PagesHor = 1 + (a_size.Width / l_paperSize.Width);
            Parent.m_settingsPage.PagesVer = 1 + (a_size.Height / l_paperSize.Height);

        }



        


        public bool OriByPage { get; set; }

        public int Orientation { get; set; }//0 == portrait    1 == landscape

        public Size GetSize()
        {
            Size l_size = new Size(0,0);


            if (m_page_size_settings.m_source == PageSizeSettings.EnumPaperSizeSource.PSS_Custom)
            {
                l_size = m_page_size_settings.sheet_size;
                SubtractPaperMargins(ref l_size);
                l_size.Width *= 10;
                l_size.Height *= 10;

            }
            else
            {
                if (m_page_size_settings.m_source == PageSizeSettings.EnumPaperSizeSource.PSS_Predefined)
                {
                    l_size = m_page_size_settings.sheet_size;
                }
                else if (m_page_size_settings.m_source == PageSizeSettings.EnumPaperSizeSource.PSS_Print)
                {
                    l_size = m_page_print_settings.sheet_size;
                }

                SubtractPaperMargins(ref l_size);
                l_size.Width *= 10;
                l_size.Height *= 10;


                l_size.Width *= m_page_size_settings.SheetsCount.Width;
                l_size.Height *= m_page_size_settings.SheetsCount.Height;
            }

            if ((l_size.Width < 1) && (l_size.Height < 1))
            {
                l_size = new Size(2770, 1900);//A4 without margins
            }


            return l_size;
        }


        private void SubtractPaperMargins(ref Size a_size)
        {
            a_size.Width = a_size.Width - m_page_size_settings.PageMargins.Left;
            a_size.Width -= m_page_size_settings.PageMargins.Right;

            a_size.Height -= m_page_size_settings.PageMargins.Top;
            a_size.Height -= m_page_size_settings.PageMargins.Bottom;
        }


        internal void SetupWireStatusConnected()
        {
            List<Wire> l_list_of_wires = new List<Wire>();
            GetAllWiresFromThisPage(l_list_of_wires);

            List<Point> l_vyvody = new List<Point>();

            PripravVyvody(l_vyvody);

            foreach(Wire l_wire in l_list_of_wires)
            {
                SetupWireStatusConnected(l_wire, l_vyvody);
                l_wire.SetupWireStatusConnectedKapky();
            }

        }

        private void SetupWireStatusConnected(Wire a_wire, List<Point> a_vyvody)
        {
            a_wire.Is_connected_first = false;
            a_wire.Is_connected_last = false;

            foreach(Point l_point in a_vyvody)
            {
                if (l_point == a_wire.GetEndingPoint(true))
                {
                    a_wire.Is_connected_first = true;

                    if (a_wire.Is_connected_first && a_wire.Is_connected_last)
                    {
                        return;
                    }
                }
                if (l_point == a_wire.GetEndingPoint(false))
                {
                    a_wire.Is_connected_last = true;

                    if (a_wire.Is_connected_first && a_wire.Is_connected_last)
                    {
                        return;
                    }
                }


            }
        }


        internal void GetAllWiresFromThisPage(List<Wire> a_list_of_wires)
        {
           foreach(DrawObj l_obj in m_objects)
           {
               if(l_obj is Wire)
               {
                   Wire l_wire = l_obj as Wire;
                   a_list_of_wires.Add(l_wire);
               }
           }
        }


        internal void GetAllWiresFromThisPage(List<Tuple<Wire, int>> a_list_of_wires, int ai_index)
        {
            foreach (DrawObj l_obj in m_objects)
            {
                if (l_obj is Wire)
                {
                    Wire l_wire = l_obj as Wire;
                    a_list_of_wires.Add(Tuple.Create(l_wire, ai_index));
                }
            }
        }

        private void PripravVyvody(List<Point> a_vyvody)
        {
            List<Insert> l_list_CElems = new List<Insert>();
	
	        GetAllCElems(l_list_CElems);


            foreach(Insert l_insert in l_list_CElems)
	        {
                Point l_center_point = Helper.GetRectCenterPoint(l_insert.m_position);

                PpdDoc l_ppd = FindPpdDocInRepo(l_insert.m_lG);
                if(l_ppd != null)
                {
                    foreach(Point l_vyvod in l_ppd.Vyvody)
                    {
                        Point l_point = l_vyvod;
                        l_point.X += l_center_point.X;
                        l_point.Y += l_center_point.Y;
                        a_vyvody.Add(l_insert.RecalculatePoint(l_point));
                    }
                }

	        } 

        }


        private void GetAllCElems(List<Insert> l_list_CElems)
        {
            foreach (DrawObj l_obj in m_objects)
            {
                if (l_obj is Insert)
                {
                    Insert l_wire = l_obj as Insert;
                    l_list_CElems.Add(l_wire);
                }
            }
        }


        public Rectangle GetRect()
        {
            Size l_size = GetSize();
            Rectangle l_rect = new Rectangle(0, 0, l_size.Width, l_size.Height);
            return l_rect;
        }



        public void SetupWireStatusLinks()
        {
            List<Wire> l_wires_this_page = new List<Wire>();
            GetAllWiresFromThisPage(l_wires_this_page);

            List<Tuple<Wire, int>> l_wires_other_pages = new List<Tuple<Wire, int>>();
            Parent.Get_Wires_Other_Pages(this, l_wires_other_pages);

            foreach(Wire l_wire in l_wires_this_page)
            {
                SetupWireStatusLinks(l_wire, l_wires_other_pages);
            }

        }

        private void SetupWireStatusLinks(Wire a_wire, List<Tuple<Wire, int>> a_wires_other_pages)
        {
            string ls_name = a_wire.GetName();
            if (string.IsNullOrWhiteSpace(ls_name))
            {
                return;
            }

            if ((a_wire.Is_connected_first) && (a_wire.Is_connected_last))
            {
                return;
            }


            int li_our_index = Parent.GetIndex(this);

            if (!a_wire.Is_connected_first)
            {
                NajdiNapojenySpoj(a_wire, li_our_index, a_wires_other_pages, true);
            }

            if (!a_wire.Is_connected_last)
            {
                NajdiNapojenySpoj(a_wire, li_our_index, a_wires_other_pages, false);
            }
        }


        private void NajdiNapojenySpoj(Wire a_wire, int ai_our_index, List<Tuple<Wire, int>> a_wires_other_pages, bool ab_first)
        {
           

            a_wire.ResetReferenceLabel(ab_first);

            string ls_our_wire_name = a_wire.GetName();
            UtilsMath.cardinal_directions l_dir = a_wire.GetWireDirection(ab_first);
            if ((l_dir == UtilsMath.cardinal_directions.cd_south) || (l_dir == UtilsMath.cardinal_directions.cd_east))
            {
                HledejNapojenySpojNahoru(ai_our_index, a_wires_other_pages, ls_our_wire_name, out string ls_reference_label);
                if (!string.IsNullOrEmpty(ls_reference_label))
                {
                    a_wire.SetReference_label(ab_first, ls_reference_label, ls_our_wire_name);
                }
                else
                {
                    //not found in the correct direction, let us search in the opposite
                    HledejNapojenySpojDolu(ai_our_index, a_wires_other_pages, ls_our_wire_name, out ls_reference_label);
                    if (!string.IsNullOrEmpty(ls_reference_label))
                    {
                        a_wire.SetReference_label(ab_first, ls_reference_label, ls_our_wire_name);
                    }
                }
            }

            if ((l_dir == UtilsMath.cardinal_directions.cd_north) || (l_dir == UtilsMath.cardinal_directions.cd_west))
            {
                HledejNapojenySpojDolu(ai_our_index, a_wires_other_pages, ls_our_wire_name, out string ls_reference_label);
                if (!string.IsNullOrEmpty(ls_reference_label))
                {
                    a_wire.SetReference_label(ab_first, ls_reference_label, ls_our_wire_name);
                }
                else
                {
                    //not found in the correct direction, let us search in the opposite
                    HledejNapojenySpojNahoru(ai_our_index, a_wires_other_pages, ls_our_wire_name, out ls_reference_label);
                    if (!string.IsNullOrEmpty(ls_reference_label))
                    {
                        a_wire.SetReference_label(ab_first, ls_reference_label, ls_our_wire_name);
                    }
                }
            }
        }


        private void HledejNapojenySpojNahoru(int ai_our_index, List<Tuple<Wire, int>> a_wires_other_pages, string as_wire_name, out string as_reference_label)
        {
            for(int li_i = a_wires_other_pages.Count - 1; li_i >= 0; li_i --)
            {
                Wire l_wire_other = a_wires_other_pages[li_i].Item1;
                int li_index = a_wires_other_pages[li_i].Item2;
                if (li_index < ai_our_index)
                {
                    if (l_wire_other.GetName() == as_wire_name)
                    {
                        if (!l_wire_other.Is_connected_first)
                        {
                            as_reference_label = GetFormatedCrossReference(l_wire_other, true);
                            return;
                        }
                        if (!l_wire_other.Is_connected_last)
                        {
                            as_reference_label = GetFormatedCrossReference(l_wire_other, false);
                            return;
                        }
                    }
                }

            }
            as_reference_label = "";

        }


        private void HledejNapojenySpojDolu(int ai_our_index, List<Tuple<Wire, int>> a_wires_other_pages, string as_wire_name, out string as_reference_label)
        {
            for (int li_i = 0; li_i < a_wires_other_pages.Count; li_i++)
            {
                Wire l_wire_other = a_wires_other_pages[li_i].Item1;
                int li_index = a_wires_other_pages[li_i].Item2;
                if (li_index > ai_our_index)
                {
                    if (l_wire_other.GetName() == as_wire_name)
                    {
                        if (!l_wire_other.Is_connected_first)
                        {
                            as_reference_label = GetFormatedCrossReference(l_wire_other, true);
                            return;
                        }
                        if (!l_wire_other.Is_connected_last)
                        {
                            as_reference_label = GetFormatedCrossReference(l_wire_other, false);
                            return;
                        }
                    }
                }

            }
            as_reference_label = "";
        }

        private string GetFormatedCrossReference(Wire a_wire_other, bool ab_first)
        {
            string ls_pageAb;
            int li_pageNo;
            string ls_zone;

            Get_PageAbbrev_PageNo_Zone(a_wire_other, ab_first, out ls_pageAb, out li_pageNo, out ls_zone);
            string ls_format = "/{ pa}.{ zone}";

            ls_format = ls_format.Replace("{po}", (1 + li_pageNo).ToString());
            ls_format = ls_format.Replace("{pa}", ls_pageAb);
            ls_format = ls_format.Replace("{zone}", ls_zone);

            return ls_format;
        }


        private void Get_PageAbbrev_PageNo_Zone(Wire a_wire_other, bool ab_first, out string as_pageAb, out int ai_pageNo, out string as_zone)
        {
            Point l_point = a_wire_other.GetEndingPoint(ab_first);

            PCadDoc pDocSxe = a_wire_other.GetDocument();
            Loader.HelperRefGrid.GetCoordinates(
                Helper.Rect2SKRect(pDocSxe.GetRect()), 
                Helper.Point2SKPoint(l_point), 
                out as_zone, 
                pDocSxe.Parent.m_ref_grid);

            as_pageAb = a_wire_other.GetDocument().Name;
            ai_pageNo = 0;// TODO a_wire_other.GetDocument().Index;
        }


        public string GetName()
        {
            string ls_name = string.Empty;
            object l_name = m_summInfo["_pa"];
            if(l_name != null)
            {
                ls_name = l_name.ToString();
            }
            return ls_name;
        }



        //-------------------------
    }
}
