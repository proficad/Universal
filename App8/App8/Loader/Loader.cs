﻿using System;
using System.Xml;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Collections;


using DxfNet;
using App8.Core;

namespace Loader
{
    public static class Loader
    {
   
        public static CollPages Load(System.IO.Stream a_input_stream)
        {
            ContextP2A.Current.CurrentLayer = null;


            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                using (StreamReader sr = new StreamReader(a_input_stream))
                {
                    xmlDoc.Load(sr);
                }
                
            }
            catch (System.Xml.XmlException exc)
            {
                string ls_what = exc.Message;
                throw new VersionPre5();
            }

            CollPages l_collPages = new CollPages();
            


            XmlNodeList list = xmlDoc.GetElementsByTagName("document");
            XmlNode docNode = list[0];

            

            NumberFormatInfo formatInfo = CultureInfo.InvariantCulture.NumberFormat;

            string ls_type      = docNode.Attributes["type"].Value;
            string ls_version   = docNode.Attributes["version"].Value;
            decimal ld_version = decimal.Parse(ls_version, formatInfo);

            l_collPages.Version = (int)ld_version;

            if (ld_version < 7)
            {
                string ls_pgsHor = docNode.Attributes["pgsHor"].Value;
                string ls_pgsVer = docNode.Attributes["pgsVer"].Value;
                string ls_pgsOri = docNode.Attributes["pgsOri"].Value;
                string ls_pgsMrgn = docNode.Attributes["pgsMrgn"].Value;
                string ls_frame     = docNode.Attributes["frame"].Value;
                string ls_rastr     = docNode.Attributes["rastr"].Value;
                string ls_showT     = docNode.Attributes["showT"].Value;
                string ls_showV     = docNode.Attributes["showV"].Value;


                if (ld_version < 6)
                {
                    SetupLegacySheetSize(ls_pgsOri, l_collPages.m_printSettings);
                }



                l_collPages.m_settingsPage.PagesHor = int.Parse(ls_pgsHor, System.Globalization.CultureInfo.InvariantCulture);
                l_collPages.m_settingsPage.PagesVer = int.Parse(ls_pgsVer, System.Globalization.CultureInfo.InvariantCulture);
 
                l_collPages.m_show_types = (ls_showT == "YES");
                l_collPages.m_show_values = (ls_showV == "YES");

            }




            list = xmlDoc.GetElementsByTagName("summary");
            

            XmlNode l_node_ref_grid = xmlDoc.SelectSingleNode("document/RefGridSettings");
            LoadRefGrid(l_collPages.m_ref_grid, l_node_ref_grid);


            XmlNode nodePrintSettings = xmlDoc.SelectSingleNode("/document/PrintSettings");
            LoadPrintSettings(l_collPages.m_printSettings, nodePrintSettings);

            XmlNode nodePageSettings = xmlDoc.SelectSingleNode("/document/PageSettings");
            LoadPageSettings(l_collPages.m_settingsPage, nodePageSettings, l_collPages);

            XmlNode nodePrinterSettings = xmlDoc.SelectSingleNode("/document/PrinterSettings");
            LoadPrinterSettings(l_collPages.m_settingsPrinter, nodePrinterSettings, l_collPages);

            XmlNode nodeNumberingWireSettings = xmlDoc.SelectSingleNode("/document/NumberingWire");
            if(nodeNumberingWireSettings != null)
            {
                LoadNumberingWireSettings(l_collPages.m_settingsNumberingWire, nodeNumberingWireSettings, l_collPages);
            }


            XmlNode nodeFonts = xmlDoc.SelectSingleNode("/document/fonts");
            LoadFonts(l_collPages.m_fonts, nodeFonts);


            XmlNode nodeRepo = xmlDoc.SelectSingleNode("/document/repo");
            LoadRepo(l_collPages.m_repo, nodeRepo);

            XmlNode nodePtb = xmlDoc.SelectSingleNode("/document/tb");
            if (nodePtb != null)
            {
                LoadTb(l_collPages.m_ptbPosition, nodePtb);
            }


            XmlNode summaryNode = xmlDoc.SelectSingleNode("/document/summary");
            LoadSummary(l_collPages.m_summInfo, summaryNode);
            

            //setup layers
            if (ld_version < 7)
            {
                l_collPages.AddPageByName("1");
                PCadDoc l_page = l_collPages.GetLatestPage();

                if (ld_version < 6)
                {
                    l_page.m_layers.Add(new Layer { Name = "-1" });
                    l_page.m_layers.Add(new Layer { Name = "0" });
                }

                
                //version 5 has this node
                XmlNode nodeElements = xmlDoc.SelectSingleNode("/document/elements");
                if (nodeElements != null)
                {
                    LoadDrawDoc(l_page, nodeElements);
                }

                XmlNodeList layerNodes = xmlDoc.SelectNodes("/document/layers/layer[@v='1']");
                LoadLayers(layerNodes, l_page);

            }
            else 
            {
                XmlNodeList pageNodes = xmlDoc.SelectNodes("/document/pages/page");
                foreach (XmlNode l_node in pageNodes)
                {
                    string ls_pageName = l_node.Attributes["name"].Value;
                    l_collPages.AddPageByName(ls_pageName);
                    PCadDoc l_page = l_collPages.GetLatestPage();

                    //custom ori of this page
                    string ls_ori = XmlAttrToString(l_node.Attributes["ori"]);
                    if (!string.IsNullOrEmpty(ls_ori))
                    {
                        l_page.OriByPage = true;
                        l_page.Orientation = int.Parse(ls_ori);
                    }

                    // page scale
                    int li_scale = XmlAttrToInt(l_node.Attributes["scale"]);
                    l_page.Scale = li_scale;

                    //nastavit ptbPosition
                    LoadPtb(l_page, l_node);

                    XmlNode l_summaryNode = l_node.SelectSingleNode("summary");
                    LoadSummary(l_page.m_summInfo, l_summaryNode);
                    XmlNodeList layerNodes = l_node.SelectNodes("layers/layer[@v='1']");
                    LoadLayers(layerNodes, l_page);

                    XmlNode l_node_page_ptb = l_node.SelectSingleNode("tb");
                    if (l_node_page_ptb != null)
                    {
                        LoadTb(l_page.m_ptbPosition, l_node_page_ptb);
                    }

                    string ls_dim_style_name = XmlAttrToString(l_node.Attributes["dim_style_name"]);
                    l_page.m_dim_style = l_collPages.m_repo.GetDimStyle(ls_dim_style_name);

                    XmlNode l_node_page_size_settings = l_node.SelectSingleNode("PageSizeSettings");
                    if (null != l_node_page_size_settings)
                    {
                        LoadPageSizeSettings(l_page, l_node_page_size_settings);

                        XmlNode l_node_page_print_settings = l_node.SelectSingleNode("PagePrintSettings");
                        LoadPagePrintSettings(l_page, l_node_page_print_settings);
                    }
                    else
                    {
                        //version pre 10
                        SettingsPage l_settings_page = l_collPages.m_settingsPage;
                        PrintSettings l_settings_print = l_collPages.m_printSettings;
                        SettingsPrinter l_settings_printer = l_collPages.m_settingsPrinter;
                        l_page.m_page_size_settings.Init_From_Old_Page_Settings(l_settings_page, l_settings_print, l_settings_printer);
                        l_page.m_page_print_settings.Init_From_Old_Page_Settings(l_settings_page, l_settings_print, l_settings_printer);
                    }



                }
            }

            l_collPages.m_summInfo["_pc"] = l_collPages.m_pages.Count.ToString();

            ContextP2A.Current.CurrentLayer = null;

            l_collPages.SetupWireStatusConnected();

            
            return l_collPages;
        }


        private static void LoadPrinterSettings(SettingsPrinter a_settingsPrinter, XmlNode a_nodePrinterSettings, CollPages a_collPages)
        {
            if (a_settingsPrinter == null)
            {
                return;
            }
            if(a_nodePrinterSettings == null)
            {
                return;
            }

            int li_width  = int.Parse(a_nodePrinterSettings.Attributes["PaperSizeX"].Value);
            int li_height = int.Parse(a_nodePrinterSettings.Attributes["PaperSizeY"].Value);

            a_settingsPrinter.PaperSize = new Size(li_width, li_height);

            int li_x = int.Parse(a_nodePrinterSettings.Attributes["PhysicalOffestTenthMm_X"].Value);
            int li_y = int.Parse(a_nodePrinterSettings.Attributes["PhysicalOffestTenthMm_Y"].Value);

            a_settingsPrinter.PhysicalOffsetTenthsMm = new Point(li_x, li_y);

        }

        private static void LoadRefGrid(RefGridSettings m_ref_grid, XmlNode a_node)
        {
            if(a_node == null)
            {
                return;
            }

            m_ref_grid.Left     = XmlAttrToBool(a_node.Attributes["GridLeft"]);
            m_ref_grid.Top      = XmlAttrToBool(a_node.Attributes["GridTop"]);
            m_ref_grid.Right    = XmlAttrToBool(a_node.Attributes["GridRight"]);
            m_ref_grid.Bottom   = XmlAttrToBool(a_node.Attributes["GridBottom"]);

            m_ref_grid.FieldSize = XmlAttrToInt(a_node.Attributes["FieldSize"]);
        }


        private static void LoadNumberingWireSettings(global::DxfNet.SettingsNumberingWire a_setting, XmlNode a_node, CollPages a_collPages)
        {
            a_setting.Enabled       = XmlAttrToBool(a_node.Attributes[SettingsNumberingWire.Attr_Enabled]);
            a_setting.Vertically    = XmlAttrToBool(a_node.Attributes[SettingsNumberingWire.Attr_Vertically]);
            a_setting.Over          = XmlAttrToBool(a_node.Attributes[SettingsNumberingWire.Attr_Over]);

            int li_show_wire_numbers = XmlAttrToInt(a_node.Attributes[SettingsNumberingWire.Attr_ShowWireName]);
            a_setting.ShowWireNumbers = (SettingsNumberingWire.EnumShowWireNumbers)li_show_wire_numbers;

            a_setting.WireLabelDist_A = XmlAttrToInt(a_node.Attributes[SettingsNumberingWire.Attr_WireLabelDist_a]);
            a_setting.WireLabelDist_B = XmlAttrToInt(a_node.Attributes[SettingsNumberingWire.Attr_WireLabelDist_b]);
            a_setting.WireLabelDist_C = XmlAttrToInt(a_node.Attributes[SettingsNumberingWire.Attr_WireLabelDist_c]);

            a_setting.Long_Wire_Len = XmlAttrToIntWithDefault(a_node.Attributes[SettingsNumberingWire.Attr_LongWireLength], 50);
        }


        private static void LoadPageSettings(global::DxfNet.SettingsPage settingsPage, XmlNode nodePageSettings, CollPages a_collPages)
        {
            if (nodePageSettings == null)
            {
                return;
            }

            settingsPage.PagesHor = XmlAttrToIntWithDefault(nodePageSettings.Attributes["pgsHor"], 1);
            settingsPage.PagesVer = XmlAttrToIntWithDefault(nodePageSettings.Attributes["pgsVer"], 1);

            string ls_showT     = nodePageSettings.Attributes["showT"].Value;
            string ls_showV     = nodePageSettings.Attributes["showV"].Value;

            a_collPages.m_show_types    = (ls_showT == "YES");
            a_collPages.m_show_values   = (ls_showV == "YES");

            string ls_drawFrame = nodePageSettings.Attributes["frame"].Value;
            settingsPage.DrawFrame = (ls_drawFrame == "YES");

            string ls_pageMargins = XmlAttrToString(nodePageSettings.Attributes["pageMargins"]);

            string[] ls_margins = System.Text.RegularExpressions.Regex.Split(ls_pageMargins, ", ");
            if (ls_margins.Length == 4)
            {
                int li_left, li_top, li_right, li_bottom;

                int.TryParse(ls_margins[0], out li_left);
                int.TryParse(ls_margins[1], out li_top);
                int.TryParse(ls_margins[2], out li_right);
                int.TryParse(ls_margins[3], out li_bottom);

                settingsPage.PageMargins = new MyRect { Left = li_left, Top = li_top, Right = li_right, Bottom = li_bottom };

                settingsPage.IncludeMargins = nodePageSettings.Attributes["includeMargins"].Value == "YES";
            }

        }

  

        private static void LoadLayers(XmlNodeList layerNodes, PCadDoc l_page)
        {
            foreach (XmlNode l_node in layerNodes)
            {
                string ls_nodeName = l_node.Attributes["name"].Value;
                Layer l_layer = new Layer();
                l_layer.Name = Helper.SanitizeLayerName(ls_nodeName);
                l_page.m_layers.Add(l_layer);

                //setup current layer, any object added to a doc will be set to that layer
                ContextP2A.Current.CurrentLayer = l_layer;

                LoadDrawDoc(l_page, l_node);
            }
        }


        private static void LoadPrintSettings(PrintSettings printSettings, XmlNode a_nodePrintSettings)
        {
            if (a_nodePrintSettings == null)
            {
                return;
            }

            printSettings.PaperSizeEnum = (short) XmlAttrToInt(a_nodePrintSettings, "PaperSize");
            printSettings.FormName = XmlAttrToString(a_nodePrintSettings, "FormName");

            printSettings.SheetSizeX = int.Parse(a_nodePrintSettings.Attributes["SheetSizeX"].Value);
            printSettings.SheetSizeY = int.Parse(a_nodePrintSettings.Attributes["SheetSizeY"].Value);

            printSettings.WantsCustom = (1 == int.Parse(a_nodePrintSettings.Attributes["WantsCustom"].Value));

            printSettings.CustomSizeX = int.Parse(a_nodePrintSettings.Attributes["CustomSizeX"].Value);
            printSettings.CustomSizeY = int.Parse(a_nodePrintSettings.Attributes["CustomSizeY"].Value);


        }


        private static void LoadSummary(Hashtable a_hashTable, XmlNode a_node)
        {
            //old way
            foreach (XmlAttribute l_attr in a_node.Attributes)
            {
                a_hashTable[l_attr.Name] = l_attr.Value;
            }

            //new way
            XmlNodeList fieldNodes = a_node.SelectNodes("field");
            foreach(XmlNode l_node in fieldNodes)
            {
                string ls_name = XmlAttrToString(l_node.Attributes["name"]);
                string ls_value = XmlAttrToString(l_node.Attributes["value"]);
                a_hashTable[ls_name] = ls_value;
            }


        }

        //for version < 8, position and TB was stored together
        private static void LoadTb(PtbPosition a_ptbPosition, XmlNode a_node)
        {
            a_ptbPosition.m_useTb = XmlAttrToBool(a_node.Attributes["use"]);
            a_ptbPosition.m_horDist = XmlAttrToInt(a_node.Attributes["h"]);
            a_ptbPosition.m_verDist = XmlAttrToInt(a_node.Attributes["v"]);
            a_ptbPosition.m_turn = XmlAttrToBool(a_node.Attributes["t"]);
            a_ptbPosition.Path = Helper.Sanitize(XmlAttrToString(a_node.Attributes["name"]));

            a_ptbPosition.m_pPtb = new PtbDoc();
             

            XmlNode nodeRepo = a_node.SelectSingleNode("/document/repo");
            if (nodeRepo != null)
            {
                LoadRepo(a_ptbPosition.m_pPtb.m_repo, nodeRepo);
            }

            XmlNode nodeElements = a_node.SelectSingleNode("descendant::elements");
            if (nodeElements != null) //ver 5
            {
                LoadDrawDoc(a_ptbPosition.m_pPtb, nodeElements);
            }
            else //ver 6
            {
                XmlNodeList layerNodes = a_node.SelectNodes("layers/layer");
                foreach (XmlNode l_node in layerNodes)
                {
                    LoadDrawDoc(a_ptbPosition.m_pPtb, l_node);
                }
            }
        }


        private static void LoadFonts(QFontsCollection a_fontsCollection, XmlNode nodeFonts)
        {
            string ls_letter = nodeFonts.Attributes["letter"].Value;
            EFont l_efont = EFont.StringToEfont(ls_letter);
            a_fontsCollection.m_fontLetter = l_efont;
            a_fontsCollection.m_fontText = EFont.StringToEfont(nodeFonts.Attributes["text"].Value);

            if (null == nodeFonts.Attributes["refs"]) //before version 7
            {
                a_fontsCollection.m_fontRef = EFont.StringToEfont(XmlAttrToString(nodeFonts.Attributes["type"]));
                a_fontsCollection.m_fontType = EFont.StringToEfont(XmlAttrToString(nodeFonts.Attributes["value"]));
            }
            else
            {
                a_fontsCollection.m_fontRef = EFont.StringToEfont(XmlAttrToString(nodeFonts.Attributes["refs"]));
                a_fontsCollection.m_fontType = EFont.StringToEfont(XmlAttrToString(nodeFonts.Attributes["type"]));
                a_fontsCollection.m_fontCrossRef = EFont.StringToEfont(XmlAttrToString(nodeFonts.Attributes["cross_refs"]));
                a_fontsCollection.m_fontOutlets = EFont.StringToEfont(XmlAttrToString(nodeFonts.Attributes["outlets"]));
            }


        }


        private static void AddArcPieChord(DrawDoc doc, XmlNode nodeElement)
        {
            Rectangle rect = RectFromXml(nodeElement);
            DrawRect l_arcPieChord = null;
            switch (nodeElement.Name)
            {
                case "arc":
                    l_arcPieChord = new DrawRect(Shape.arc, rect);
                    break;
                case "pie":
                    l_arcPieChord = new DrawRect(Shape.pie, rect);
                    break;
                case "chord":
                    l_arcPieChord = new DrawRect(Shape.chord, rect);
                    break;
                default:
                    throw new Exception("invalid node name " + nodeElement.Name);
            }
    
            
            ObjProps l_props = ObjPropsFromXml(nodeElement);
            l_arcPieChord.m_objProps = l_props;

            int li_bX = int.Parse(nodeElement.Attributes["bX"].Value);
            int li_bY = int.Parse(nodeElement.Attributes["bY"].Value);
            int li_eX = int.Parse(nodeElement.Attributes["eX"].Value);
            int li_eY = int.Parse(nodeElement.Attributes["eY"].Value);

            l_arcPieChord.m_arcBegin.Width = li_bX;
            l_arcPieChord.m_arcBegin.Height = li_bY;
            l_arcPieChord.m_arcEnd.Width = li_eX;
            l_arcPieChord.m_arcEnd.Height = li_eY;


            doc.Add(l_arcPieChord, ContextP2A.Current.CurrentLayer);
        }



        private static void AddRect(DrawDoc doc, XmlNode a_node)
        {
            Rectangle rect = RectFromXml(a_node);
            DrawRect drawRect = new DrawRect(Shape.rectangle, rect);
            ObjProps l_props = ObjPropsFromXml(a_node);
            drawRect.m_objProps = l_props;

            AddTextToDrawRect(drawRect, a_node);
            doc.Add(drawRect, ContextP2A.Current.CurrentLayer);
        }

        private static void AddEllipse(DrawDoc doc, XmlNode a_node)
        {
            Rectangle rect = RectFromXml(a_node);
            DrawRect drawRect = new DrawRect(Shape.ellipse, rect);
            ObjProps l_props = ObjPropsFromXml(a_node);
            drawRect.m_objProps = l_props;

            AddTextToDrawRect(drawRect, a_node);

            doc.Add(drawRect, ContextP2A.Current.CurrentLayer);
        }

        private static void AddCircle(DrawDoc doc, XmlNode a_node)
        {
            Point l_center = XmlAttrToPoint(a_node, "center");
            Point l_tangent = XmlAttrToPoint(a_node, "tangent");
            QCircle l_circle = new QCircle(l_center, l_tangent);

            ObjProps l_props = ObjPropsFromXml(a_node);
            l_circle.m_objProps = l_props;

            AddTextToQCircle(l_circle, a_node);

            doc.Add(l_circle, ContextP2A.Current.CurrentLayer);
        }

        private static void AddTextToQCircle(QCircle a_circle, XmlNode a_node)
        {
            XmlNode textNode = a_node.SelectSingleNode("descendant::text");
            if (textNode != null)
            {
                string ls_font = textNode.Attributes["font"].Value;
                string ls_text = textNode.InnerText;
                int li_angle = Loader.TurnsToAngle(XmlAttrToInt(textNode.Attributes["turns"]));

                a_circle.m_text = ls_text;
                a_circle.m_efont = EFont.StringToEfont(ls_font);
                a_circle.m_text_angle = li_angle;
            }
        }

        private static void AddTextToDrawRect(DrawRect a_drawRect, XmlNode a_node)
        {
            XmlNode textNode = a_node.SelectSingleNode("descendant::text");
            if (textNode != null)
            {
                string ls_font = textNode.Attributes["font"].Value;
                string ls_text = textNode.InnerText;
                int li_angle = Loader.TurnsToAngle(XmlAttrToInt(textNode.Attributes["turns"]));

                a_drawRect.m_text = ls_text;
                a_drawRect.m_efont = EFont.StringToEfont(ls_font);
                a_drawRect.m_text_angle = li_angle;
            }
        }

        private static Rectangle RectFromXml(XmlNode nodeElement)
        {
            int li_left = int.Parse(nodeElement.Attributes["left"].Value);
            int li_top = int.Parse(nodeElement.Attributes["top"].Value);
            int li_right = int.Parse(nodeElement.Attributes["right"].Value);
            int li_bottom = int.Parse(nodeElement.Attributes["bottom"].Value);
            int li_width = li_right - li_left;
            int li_height = li_bottom - li_top;
            Rectangle rect = new Rectangle(li_left, li_top, li_width, li_height);
            Helper.FixRectangle(ref rect);
            return rect;
        }

        private static void AddRoundRect(DrawDoc doc, XmlNode a_node)
        {
            Rectangle rect = RectFromXml(a_node);
            DrawRect drawRect = new DrawRect(Shape.roundRectangle, rect);
            drawRect.m_rX = int.Parse(a_node.Attributes["rX"].Value);
            drawRect.m_rY = int.Parse(a_node.Attributes["rY"].Value);


            ObjProps l_props = ObjPropsFromXml(a_node);
            drawRect.m_objProps = l_props;

            AddTextToDrawRect(drawRect, a_node);
            doc.Add(drawRect, ContextP2A.Current.CurrentLayer);

        }

        private static void AddPolyLine(DrawDoc doc, XmlNode a_node)
        {
            string ls_points = a_node.Attributes["pts"].Value;
            string[] ls_arrPoints = ls_points.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

            if((ls_arrPoints.Length % 2) != 0)
            {
                throw new Exception("number of points not even");
            }

            int[] li_arrPoints = new int[ls_arrPoints.Length];
            for (int i = 0; i < ls_arrPoints.Length; i++)
            {
                li_arrPoints[i] = int.Parse(ls_arrPoints[i]);
            }

            DrawPoly poly = null;
            if (a_node.Name.Equals("polygon"))
            {
                poly = new DrawPoly(Shape.poly);
            }
            if (a_node.Name.Equals("polyline"))
            {
                poly = new DrawPoly(Shape.polyline);
                poly.Scale_arrow_x = XmlAttrToFloat(a_node.Attributes["arrow_scale_x"]);
                poly.Scale_arrow_y = XmlAttrToFloat(a_node.Attributes["arrow_scale_y"]);
            }
            if (a_node.Name.Equals("polybezier"))
            {
                poly = new DrawPoly(Shape.bezier);
            }
            if (a_node.Name.Equals("wire"))
            {
                poly = new Wire();
            }
            if (poly == null)
            {
                throw new Exception("wrong tag name");
            }
            

            for (int y = 0; y < ls_arrPoints.Length; y += 2)
            {
                int li_x = li_arrPoints[y];
                int li_y = li_arrPoints[y + 1];
                poly.AddPoint(li_x, li_y);
            }
            ObjProps l_props = ObjPropsFromXml(a_node);
            //add obj props
            poly.m_objProps = l_props;

            if (poly is Wire)
            {
                Wire l_wire = poly as Wire;
                l_wire.SetDrop1(XmlAttrToBool(a_node.Attributes["drop1"]));
                l_wire.SetDrop2(XmlAttrToBool(a_node.Attributes["drop2"]));
                l_wire.SetName(XmlAttrToString(a_node.Attributes["name"]));


                FromXmlPositionsOfLinks(l_wire, a_node);
                FromXmlPositionsOfLabels(l_wire, a_node);


            }


            doc.Add(poly, ContextP2A.Current.CurrentLayer);

        }

        private static ObjProps ObjPropsFromXml(XmlNode node)
        {
            ObjProps l_props = new ObjProps();

            XmlNode linNode = node.SelectSingleNode("descendant::op-lt");
            if (linNode != null)
            {
                string ls_head = linNode.Attributes["head"].Value;
                string ls_body = linNode.Attributes["body"].Value;
                l_props.m_lin.Init(ls_head, ls_body);
            }


            string ls_linewidth = XmlAttrToString(node.Attributes["op-lw"]);
            string ls_linecolor = XmlAttrToString(node.Attributes["op-lc"]);
            string ls_filled    = XmlAttrToString(node.Attributes["op-fd"]);
            string ls_fillcolor = XmlAttrToString(node.Attributes["op-fc"]);
            string ls_arrowType = XmlAttrToString(node.Attributes["op-at"]);


            string ls_contour2 = XmlAttrToString(node.Attributes["op-contour2"]);
            if(!string.IsNullOrEmpty(ls_contour2))
            {
                l_props.m_contour2.IsOn = true;
                int li_countour2_color = int.Parse(ls_contour2);
                l_props.m_contour2.m_color = Helper.ColorFromWin32(li_countour2_color);
            }


            if (ls_linewidth.Length > 0)
            {
                l_props.m_logpen.m_width = int.Parse(ls_linewidth);
            }
      
            if (ls_linecolor.Length > 0)
            {
                int li_lineColor = int.Parse(ls_linecolor);

                l_props.m_logpen.m_color = Helper.ColorFromWin32(li_lineColor);
            }
            if (ls_filled.Length > 0)
            {
                l_props.m_bBrush = ls_filled.Equals("1");
            }
            if (ls_fillcolor.Length > 0)
            {
                int li_fillcolor = int.Parse(ls_fillcolor);
                l_props.m_logbrush.m_color = Helper.ColorFromWin32(li_fillcolor);
            }
            if (ls_arrowType.Length > 0)
            {
                l_props.m_logpen.m_style = int.Parse(ls_arrowType);
            }


            XmlNode hatchNode = node.SelectSingleNode("descendant::hatch");
            if (hatchNode != null)
            {
     
                string ls_hatchtype    = XmlAttrToString(hatchNode.Attributes["type"]);
                string ls_hatchpensize = XmlAttrToString(hatchNode.Attributes["penSize"]);
                string ls_hatchspacing = XmlAttrToString(hatchNode.Attributes["spacing"]);
                string ls_hatchoffsetx = XmlAttrToString(hatchNode.Attributes["offSetX"]);
                string ls_hatchoffsety = XmlAttrToString(hatchNode.Attributes["offSetY"]);


                if (ls_hatchtype.Length > 0)
                {
                    l_props.m_hatchtype = (HatchType)int.Parse(ls_hatchtype);
                }
                if (ls_hatchpensize.Length > 0)
                {
                    l_props.m_hatchpensize = int.Parse(ls_hatchpensize);
                }
                if (ls_hatchspacing.Length > 0)
                {
                    l_props.m_hatchspacing = int.Parse(ls_hatchspacing);
                }
                if (ls_hatchoffsetx.Length > 0)
                {
                    l_props.m_hatchoffset.Width = int.Parse(ls_hatchoffsetx);
                }
                if (ls_hatchoffsety.Length > 0)
                {
                    l_props.m_hatchoffset.Height = int.Parse(ls_hatchoffsety);
                }

            }           



            return l_props;
        }

        public static string XmlChildNodeToString(XmlNode a_node, string as_childName)
        {
            if (a_node != null)
            {
                XmlNode l_childNode = a_node.SelectSingleNode(as_childName);
                if (l_childNode != null)
                {
                    return l_childNode.InnerText;
                }
            }
            return string.Empty;
        }

        public static string XmlAttrToString(XmlNode a_node, string as_name)
        {
            XmlAttribute l_attr = a_node.Attributes[as_name];
            if(null != l_attr)
            {
                return l_attr.Value;
            }
            else
            {
                return null;
            }
        }

        public static string XmlAttrToString(XmlAttribute a_attr)
        {
            string ls_result = string.Empty;
            if (a_attr != null)
            {
                ls_result = a_attr.Value;
            }
            return ls_result;
        }

        public static float XmlAttrToFloat(XmlAttribute a_attr)
        {
            float lf_result = 1;
            if (a_attr != null)
            {
                IFormatProvider fp = CultureInfo.InvariantCulture;//US format
                lf_result = float.Parse(a_attr.Value, fp);
            }
            return lf_result;
        }

        public static bool XmlAttrToBool(XmlAttribute a_attr)
        {
            string ls_result = string.Empty;
            if (a_attr != null)
            {
                ls_result = a_attr.Value;
            }
            return (ls_result == "1");
        }

        private static MyRect XmlAttrib2Rect(XmlNode a_node, string as_attrib, int ai_def)
        {

            XmlAttribute l_attrib = a_node.Attributes[as_attrib];
            if (l_attrib == null)
            {
                return new MyRect { Left = ai_def, Top = ai_def, Right = ai_def, Bottom = ai_def };
            }

            string ls_rect = l_attrib.Value;

            string[] ls_margins = System.Text.RegularExpressions.Regex.Split(ls_rect, ", ");
            if (ls_margins.Length == 4)
            {
                int li_left, li_top, li_right, li_bottom;

                int.TryParse(ls_margins[0], out li_left);
                int.TryParse(ls_margins[1], out li_top);
                int.TryParse(ls_margins[2], out li_right);
                int.TryParse(ls_margins[3], out li_bottom);

                return new MyRect { Left = li_left, Top = li_top, Right = li_right, Bottom = li_bottom };

            }

            return new MyRect();
        }

        public static Size XmlAttrToSize(XmlNode a_node, string as_attrib, int ai_def)
        {
            Size ls_size = new Size(
                XmlAttrToIntWithDefault(a_node.Attributes[as_attrib + ("x")], ai_def),
                XmlAttrToIntWithDefault(a_node.Attributes[as_attrib + ("y")], ai_def)
            );

            return ls_size;
        }

        public static int XmlAttrToInt(XmlNode a_node, string as_name)
        {
            XmlAttribute l_attr = a_node.Attributes[as_name];
            return XmlAttrToIntWithDefault(l_attr, 0);
        }

        public static int XmlAttrToInt(XmlAttribute a_attr)
        {
            return XmlAttrToIntWithDefault(a_attr, 0);
        }

        public static Point XmlAttrToPoint(XmlNode a_node, string as_attr_prefix)
        {
            Point l_point = new Point();
            l_point.X = XmlAttrToInt(a_node.Attributes[as_attr_prefix + "x"]);
            l_point.Y = XmlAttrToInt(a_node.Attributes[as_attr_prefix + "y"]);

            return l_point;
        }

        public static int XmlAttrToIntWithDefault(XmlAttribute a_attr, int ai_default)
        {
            string ls_result = string.Empty;
            if (a_attr != null)
            {
                ls_result = a_attr.Value;
            }
            if (ls_result.Length == 0)
            {
                return ai_default;
            }
            return int.Parse(ls_result);
        }


        public static void LoadDrawDoc(DrawDoc a_drawDoc, XmlNode a_node)
        {

            foreach (XmlNode nodeElement in a_node)
            {
                if (nodeElement.NodeType == XmlNodeType.Element)
                {
                    switch (nodeElement.Name)
                    {
                        case "polygon":
                        case "wire":      
                        case "polyline":    
                        case "polybezier":  AddPolyLine     (a_drawDoc, nodeElement); break;
                        case "roundRect":   AddRoundRect    (a_drawDoc, nodeElement); break;
                        case "rect":        AddRect         (a_drawDoc, nodeElement); break;
                        case "ellipse":     AddEllipse      (a_drawDoc, nodeElement); break;
                        case "circle":      AddCircle       (a_drawDoc, nodeElement); break;
                        case "pie": 
                        case "chord": 
                        case "arc":         AddArcPieChord  (a_drawDoc, nodeElement); break;
                        case "elem":        AddInsert       (a_drawDoc, nodeElement); break;
                        case "text":        AddText         (a_drawDoc, nodeElement); break;
                        case "trafo":       AddTrafo        (a_drawDoc, nodeElement); break;
                        case "ic":          AddQIC          (a_drawDoc, nodeElement); break;
                        case "gate":        AddGate         (a_drawDoc, nodeElement); break;
                        case "img":         AddImage        (a_drawDoc, nodeElement); break;
                        case "CableSymbol": AddCableSymbol  (a_drawDoc, nodeElement); break;
                        case "outlet":      AddOutlet       (a_drawDoc, nodeElement); break;
                        case "dim_line":    AddDimLine      (a_drawDoc, nodeElement); break;
                        case "dim_circle":  AddDimCircle    (a_drawDoc, nodeElement); break;

                    }
                }
            }
        }

        private static void AddDimCircle(DrawDoc a_drawDoc, XmlNode a_node)
        {
            Point l_a = XmlAttrToPoint(a_node, "a");
            Point l_b = XmlAttrToPoint(a_node, "b");

            bool lb_has_2_arrows = XmlAttrToBool(a_node.Attributes["arr2"]);

            Point l_label_pos = XmlAttrToPoint(a_node, "lp");

            QDimCircle l_dim = new QDimCircle(l_a, l_b, lb_has_2_arrows, l_label_pos);
            a_drawDoc.Add(l_dim, ContextP2A.Current.CurrentLayer);

            XmlNode l_node_label = a_node.SelectSingleNode("label");
            l_dim.Label = LoadQLabel(l_node_label);
        }

        private static void AddDimLine(DrawDoc a_drawDoc, XmlNode a_node)
        {
            Point l_a = XmlAttrToPoint(a_node, "a");
            Point l_b = XmlAttrToPoint(a_node, "b");
            Point l_c = XmlAttrToPoint(a_node, "c");

            QDimLine.DimDirection l_dir = (QDimLine.DimDirection)XmlAttrToInt(a_node.Attributes["dir"]);


            QDimLine l_dim = new QDimLine(l_a, l_b, l_c, l_dir);
            a_drawDoc.Add(l_dim, ContextP2A.Current.CurrentLayer);

            XmlNode l_node_label = a_node.SelectSingleNode("label");
            l_dim.Label = LoadQLabel(l_node_label);
        }


        private static QLabel LoadQLabel(XmlNode a_node)
        {
            if(null != a_node)
            {
                Point l_center_point = XmlAttrToPoint(a_node, "pos_");
                string ls_text = XmlAttrToString(a_node, "text");
                int li_angle = XmlAttrToInt(a_node, "angle");
                QLabel l_label = new QLabel { Center = l_center_point, Text = ls_text, AngleTenths = li_angle };
                return l_label;
            }

            return null;
        }


        private static void AddOutlet(DrawDoc a_drawDoc, XmlNode a_node)
        {
            int li_x = XmlAttrToInt(a_node.Attributes["x"]);
            int li_y = XmlAttrToInt(a_node.Attributes["y"]);

            Outlet l_outlet = new Outlet(li_x, li_y);
            a_drawDoc.Add(l_outlet, ContextP2A.Current.CurrentLayer);
        }

        private static void AddCableSymbol(DrawDoc a_drawDoc, XmlNode a_node)
        {
            int li_min, li_common, li_max; bool lb_hor;

            li_min = XmlAttrToInt(a_node.Attributes["min"]);
            li_common = XmlAttrToInt(a_node.Attributes["com"]);
            li_max = XmlAttrToInt(a_node.Attributes["max"]);
            lb_hor = XmlAttrToBool(a_node.Attributes["hor"]);

            //calculate position of the symbol - BOGUS
            //TODO add real calculation when position is needed
            Rectangle l_rect = new Rectangle(li_min, li_common, 100, 100);


            CableSymbol l_cable_symbol = new CableSymbol(li_min, li_common, li_max, lb_hor, l_rect);
            a_drawDoc.Add(l_cable_symbol, ContextP2A.Current.CurrentLayer);

            foreach (XmlNode nodeElement in a_node)
            {
                if (nodeElement.NodeType == XmlNodeType.Element)
                {
                    switch (nodeElement.Name)
                    {
                        case "type":
                        case "value":

                          
                            break;
                        case "attributes":
                            AddAttributesToCable(l_cable_symbol, nodeElement);
                            break;
                    }
                }
            }

        }


        private static void AddImage(DrawDoc a_drawDoc, XmlNode a_node)
        {
            Rectangle l_rect = RectFromXml(a_node);

            string ls_lastGuid = XmlAttrToString(a_node, "lG");
            if (string.IsNullOrEmpty(ls_lastGuid))
            {
                ls_lastGuid = a_node.Attributes["lastGuid"].Value;
            }

            int li_angle = XmlAttrToInt(a_node.Attributes["a"]);

            bool lb_hor = XmlAttrToBool(a_node.Attributes["h"]);
            bool lb_ver = XmlAttrToBool(a_node.Attributes["v"]);

            QImage l_image = new QImage(Shape.shapeNone, l_rect, ls_lastGuid, li_angle, lb_hor, lb_ver);
            a_drawDoc.Add(l_image, ContextP2A.Current.CurrentLayer);
        }

        private static void AddTrafo(DrawDoc a_drawDoc, XmlNode a_node)
        {
            int li_x = int.Parse(a_node.Attributes["x"].Value);
            int li_y = int.Parse(a_node.Attributes["y"].Value);

            float lf_scaleX = Helper.ParseScale(a_node.Attributes["sX"]);
            float lf_scaleY = Helper.ParseScale(a_node.Attributes["sY"]);

            Trafo l_trafo = new Trafo(li_x, li_y, lf_scaleX, lf_scaleY);
            l_trafo.m_jadro = (trafoCoreType)int.Parse(a_node.Attributes["core"].Value);

            XmlNode nodePri = a_node.SelectSingleNode("pri");
            AddTrafoWiring(l_trafo.m_pri, nodePri);
            XmlNode nodeSec = a_node.SelectSingleNode("sec");
            AddTrafoWiring(l_trafo.m_sec, nodeSec);

            SetupInsertCommon(a_drawDoc, a_node, l_trafo);
        }

        private static void AddTrafoWiring(TrafoWinding a_trafoWinding, XmlNode a_node)
        {
            int li_w = XmlAttrToInt(a_node.Attributes["w1"]);
            if(li_w == 0)
            {
                return;
            }
            a_trafoWinding.m_w1 = li_w;
            li_w = XmlAttrToInt(a_node.Attributes["w2"]);
            if (li_w == 0)
            {
                return;
            }
            a_trafoWinding.m_w2 = li_w;
            li_w = XmlAttrToInt(a_node.Attributes["w3"]);
            if (li_w == 0)
            {
                return;
            }
            a_trafoWinding.m_w3 = li_w;
            li_w = XmlAttrToInt(a_node.Attributes["w4"]);
            if (li_w == 0)
            {
                return;
            }
            a_trafoWinding.m_w4 = li_w;
            li_w = XmlAttrToInt(a_node.Attributes["w5"]);
            if (li_w == 0)
            {
                return;
            }
            a_trafoWinding.m_w5 = li_w;
            li_w = XmlAttrToInt(a_node.Attributes["w6"]);
            if (li_w == 0)
            {
                return;
            }
            a_trafoWinding.m_w6 = li_w;

        }

        private static void SetupInsertCommon(DrawDoc doc, XmlNode a_node, Insert a_insert)
        {
            a_insert.m_hor = XmlAttrToBool(a_node.Attributes["h"]);
            a_insert.m_ver = XmlAttrToBool(a_node.Attributes["v"]);


            XmlAttribute l_attr_turns = a_node.Attributes["t"];
            if(l_attr_turns != null)
            {
                int li_turns = XmlAttrToInt(a_node.Attributes["t"]) % 8;
                a_insert.m_angle = TurnsToAngle(li_turns);
            }
            else
            {
                a_insert.m_angle = XmlAttrToInt(a_node.Attributes["a"]);
            }


            string ls_linecolor = XmlAttrToString(a_node.Attributes["c"]);
            if (ls_linecolor.Length > 0)
            {
                a_insert.m_objProps.m_logpen.m_color = DxfNet.Helper.ColorFromHtml("#" + ls_linecolor);
            }

            string ls_fill_color = XmlAttrToString(a_node.Attributes["op-fc"]);
            if(!string.IsNullOrWhiteSpace(ls_fill_color))
            {
                int li_color;
                if(int.TryParse(ls_fill_color, out li_color))
                {
                    a_insert.m_objProps.m_logbrush.m_color = Color.FromArgb(li_color);
                }
                
            }


            foreach (XmlNode nodeElement in a_node)
            {
                if (nodeElement.NodeType == XmlNodeType.Element)
                {
                    switch (nodeElement.Name)
                    {
                        case "type":
                        case "value":

                            AddSateliteToInsert(a_insert, nodeElement);
                            break;
                        case "attributes":
                            AddAttributesToInsert(a_insert, nodeElement);
                            break;
                    }
                }
            }

            doc.Add(a_insert, ContextP2A.Current.CurrentLayer);

        }

        private static void AddText(DrawDoc a_doc, XmlNode a_node)
        {
            int li_x = int.Parse(a_node.Attributes["x"].Value);
            int li_y = int.Parse(a_node.Attributes["y"].Value);
            string ls_font = a_node.Attributes["f"].Value;

            int li_angle = 0;
            XmlAttribute l_attr_turns = a_node.Attributes["t"];
            if (l_attr_turns != null)
            {
                int li_turns = XmlAttrToInt(a_node.Attributes["t"]) % 8;
                li_angle = TurnsToAngle(li_turns);
            }
            else
            {
                li_angle = XmlAttrToInt(a_node.Attributes["a"]);
            }

            Rectangle l_rect = new Rectangle(li_x, li_y, 0, 0);

            string ls_text = System.Net.WebUtility.HtmlDecode(a_node.InnerText);//2009-2-7 znacka jistic >
            EFont l_efont = EFont.StringToEfont(ls_font);
            FreeText l_text = new FreeText(ls_text, l_efont, l_rect, 0);

            l_text.m_angle = li_angle;
            l_text.m_alignment = (QTextAlignment) XmlAttrToInt(a_node.Attributes["al"]); //empty value gives 0, which is the default AL_MM

            if (a_doc is PtbDoc)
            {
                l_text.SetTb();
            }

            a_doc.Add(l_text, ContextP2A.Current.CurrentLayer);
        }


        public static void LoadRepo(Repo a_repo, XmlNode a_node)
        {
            if (a_node == null)
            {
                return;
            }

            foreach (XmlNode nodeRepoElement in a_node)
            {
                if (nodeRepoElement.NodeType == XmlNodeType.Element)
                {
                    switch (nodeRepoElement.Name)
                    {
                        case "ppd":     AddPpd      (a_repo, nodeRepoElement); break;
                        case "imgDesc": AddImageDesc(a_repo, nodeRepoElement); break;
                        case "tb":      AddTb       (a_repo, nodeRepoElement); break;
                        case "DimStyle": AddDimStyle(a_repo, nodeRepoElement); break;
                    }
                }
            }
        }

        private static void AddDimStyle(Repo a_repo, XmlNode a_node)
        {
            string ls_name = a_node.Attributes["name"].Value;
            QDimStyle l_style = new QDimStyle(ls_name);
            l_style.m_align_text_with_dim_line = XmlAttrToBool(a_node.Attributes["align_text"]);
            l_style.m_text_position = (QDimStyle.Text_Position)XmlAttrToInt(a_node.Attributes["lbl_pos"]);

            const string TAG_DIM_LINE = "DimLine";
            const string TAG_EXT_LINE = "ExtLine";

            Load_Dim_Line(a_node, TAG_DIM_LINE, ref l_style.m_line_dim);
            Load_Dim_Line(a_node, TAG_EXT_LINE, ref l_style.m_line_ext);

            l_style.m_arrow_index = XmlAttrToInt(a_node, "arrow");
            l_style.m_label_font = EFont.StringToEfont(XmlAttrToString(a_node.Attributes["f"]));

            a_repo.AddDimStyle(l_style);
        }

        private static void AddImageDesc(Repo a_repo, XmlNode a_node)
        {
            string ls_lastGuid = XmlAttrToString(a_node, "lG");
            if (string.IsNullOrEmpty(ls_lastGuid))
            {
                ls_lastGuid = a_node.Attributes["lastGuid"].Value;
            }

            int li_imgType = int.Parse(a_node.Attributes["imgType"].Value);

            if (li_imgType == 10)  // do not support WMF
            {
                return;
            }

            string ls_imgEncoded = a_node.InnerText;

            QImageDesc l_imgDesc = new QImageDesc(ls_lastGuid, li_imgType, ls_imgEncoded);
            a_repo.AddImgDesc(l_imgDesc);
        }


        private static void AddPpd(Repo a_repo, XmlNode nodeElement)
        {
            //create a ppd
            PpdDoc doc = new PpdDoc();
            doc.m_name = XmlAttrToString(nodeElement.Attributes["name"]);

            doc.m_defType   = XmlAttrToString(nodeElement.Attributes["t"]);
            doc.m_defValue  = XmlAttrToString(nodeElement.Attributes["v"]);
            doc.m_norm      = XmlAttrToString(nodeElement.Attributes["n"]);
            doc.m_memo      = XmlAttrToString(nodeElement.Attributes["m"]);
            doc.m_fG        = XmlAttrToString(nodeElement.Attributes["fG"]);
            doc.m_lG        = XmlAttrToString(nodeElement.Attributes["lG"]);

            //repo in repo
            XmlNode nodeRepo = nodeElement.SelectSingleNode("repo");
            if (nodeRepo != null)
            {
                LoadRepo(doc.m_repo, nodeRepo);
            }


            //find "elements" element
            XmlNode node = nodeElement.SelectSingleNode("elements");
            if (node != null) //ver 5
            {
                LoadDrawDoc(doc, node);
            }
            else // ver 6
            {
                XmlNodeList layerNodes = nodeElement.SelectNodes("layers/layer");
                foreach (XmlNode l_node in layerNodes)
                {
                    LoadDrawDoc(doc, l_node);
                }
            }

            a_repo.AddPpd(doc);
        }


        private static void AddTb(Repo a_repo, XmlNode nodeElement)
        {
            //create a ppd
            PtbDoc doc = new PtbDoc();
            doc.Path = XmlAttrToString(nodeElement.Attributes["path"]);
            doc.Path = Helper.Sanitize(doc.Path);

            XmlNode nodeRepo = nodeElement.SelectSingleNode("repo");
            if (nodeRepo != null)
            {
                LoadRepo(doc.m_repo, nodeRepo);
            }

     
            XmlNodeList layerNodes = nodeElement.SelectNodes("layers/layer");
            foreach (XmlNode l_node in layerNodes)
            {
                LoadDrawDoc(doc, l_node);
            }
            

            a_repo.AddTb(doc);
            //add it to coll
        }


        private static void AddQIC(DrawDoc doc, XmlNode a_node)
        {
            int li_x = int.Parse(a_node.Attributes["x"].Value);
            int li_y = int.Parse(a_node.Attributes["y"].Value);

            float lf_scaleX = Helper.ParseScale(a_node.Attributes["sX"]);
            float lf_scaleY = Helper.ParseScale(a_node.Attributes["sY"]);
            
            QIC l_ic = new QIC(li_x, li_y, lf_scaleX, lf_scaleY);



            SetupInsertCommon(doc, a_node, l_ic);
            XmlNode picdNode = a_node.SelectSingleNode("descendant::picd");
            
            if (picdNode == null)
            {
                throw new Exception("IC tag does not have child PICD");
            }
            l_ic.m_mark                 = XmlAttrToBool(picdNode.Attributes["mark"]);
            l_ic.m_numberOfOutletsHor   = XmlAttrToInt(picdNode.Attributes["outlets_hor"]);
            l_ic.m_numberOfOutletsVer   = XmlAttrToInt(picdNode.Attributes["outlets_ver"]);
            l_ic.m_name                 = XmlAttrToString(picdNode.Attributes["name"]);
            l_ic.m_ver_left             = XmlAttrToBool(picdNode.Attributes["ver_left"]);
            l_ic.m_ver_right            = XmlAttrToBool(picdNode.Attributes["ver_right"]);

            l_ic.m_out_n                = XmlChildNodeToString(a_node, "descendant::out_n");
            l_ic.m_out_n_inv            = XmlChildNodeToString(a_node, "descendant::out_n_inv");
            l_ic.m_desc                 = XmlChildNodeToString(a_node, "descendant::desc");
            l_ic.m_out_d                = XmlChildNodeToString(a_node, "descendant::out_d");
            l_ic.m_pos_hor              = XmlChildNodeToString(a_node, "descendant::pos_hor");

        }

        private static void AddGate(DrawDoc a_doc, XmlNode a_node)
        {
            int li_x = int.Parse(a_node.Attributes["x"].Value);
            int li_y = int.Parse(a_node.Attributes["y"].Value);

            float lf_scaleX = Helper.ParseScale(a_node.Attributes["sX"]);
            float lf_scaleY = Helper.ParseScale(a_node.Attributes["sY"]);

            QGate l_gate = new QGate(li_x, li_y, lf_scaleX, lf_scaleY);

            l_gate.m_ASA        = XmlAttrToBool(a_node.Attributes["asa"]);
            l_gate.m_stesnat    = XmlAttrToBool(a_node.Attributes["ser"]);
            l_gate.m_pocetvstupu = XmlAttrToInt(a_node.Attributes["inpCount"]);
            l_gate.m_tvar       = (GateShapeType)XmlAttrToInt(a_node.Attributes["gType"]);
            int li_inv = XmlAttrToInt(a_node.Attributes["inv"]);

            l_gate.m_c1 = (li_inv & 1) != 0;
            l_gate.m_c2 = (li_inv & 2) != 0;
            l_gate.m_c3 = (li_inv & 4) != 0;
            l_gate.m_c4 = (li_inv & 8) != 0;
            l_gate.m_c5 = (li_inv & 16) != 0;
            l_gate.m_c6 = (li_inv & 32) != 0;
            l_gate.m_c7 = (li_inv & 64) != 0;
            l_gate.m_c8 = (li_inv & 128) != 0;

            SetupInsertCommon(a_doc, a_node, l_gate);
           
        }


        private static void AddInsert(DrawDoc doc, XmlNode a_node)
        {
            int li_x = int.Parse(a_node.Attributes["x"].Value);
            int li_y = int.Parse(a_node.Attributes["y"].Value);

            float lf_scaleX = Helper.ParseScale(a_node.Attributes["sX"]);
            float lf_scaleY = Helper.ParseScale(a_node.Attributes["sY"]);

            Insert l_insert   = new Insert(Shape.soucastka, li_x, li_y, lf_scaleX, lf_scaleY);

            l_insert.m_lG = XmlAttrToString(a_node, "lG");
            if (string.IsNullOrEmpty(l_insert.m_lG))
            {
                l_insert.m_lG = XmlAttrToString(a_node.Attributes["lGuid"]);
            }

            XmlNode parametersNode = a_node.SelectSingleNode("parameters");
            LoadParameters(l_insert.m_parameters, parametersNode);

            SetupInsertCommon(doc, a_node, l_insert);
        }


        private static void LoadParameters(Hashtable a_hashTable, XmlNode a_node)
        {
            if(a_hashTable == null)
            {
                return;
            }
            if(a_node == null)
            {
                return;
            }
            XmlNodeList l_node_list = a_node.SelectNodes("parameter");
            foreach (XmlNode l_node_param in l_node_list)
            {
                string ls_key = l_node_param.Attributes["name"].Value;
                string ls_value = l_node_param.Attributes["value"].Value;
                if(!string.IsNullOrWhiteSpace(ls_key) && !string.IsNullOrWhiteSpace(ls_value))
                {
                    if(!a_hashTable.ContainsKey(ls_key))
                    {
                        a_hashTable.Add(ls_key, ls_value);
                    }
                }
            }
        }


        private static void AddSateliteToInsert(Insert insert, XmlNode a_node)
        {
            string ls_name = "_" + a_node.Name;//99 2013-04-09 renaming to avoid conflict with attributes introduced in version 7.5
            string ls_value = a_node.InnerText;
            int li_x = int.Parse(a_node.Attributes["x"].Value);
            int li_y = int.Parse(a_node.Attributes["y"].Value);
            bool lb_visible = XmlAttrToBool(a_node.Attributes["v"]);
            int li_turns = XmlAttrToInt(a_node.Attributes["t"]);

            Insert.Satelite l_satelite = new Insert.Satelite(ls_name, ls_value, li_x, li_y, lb_visible, li_turns);


            l_satelite.m_alignment = QTextAlignment.AL_LM;// 2013-04-10 for now we assume ref + type are left aligned
            if (null != a_node.Attributes["al"])
            {
                string ls_align = a_node.Attributes["al"].Value;
                int li_align = int.Parse(ls_align);
                l_satelite.m_alignment = (QTextAlignment)li_align;
            }


            insert.m_satelites.Add(l_satelite);

        }

        private static void AddAttributesToCable(CableSymbol a_cable, XmlNode a_node)
        {
            foreach (XmlNode nodeElement in a_node)
            {
                if (nodeElement.Name == "attribute")
                {
                    string ls_name = nodeElement.Attributes["name"].Value;
                    string ls_value = nodeElement.Attributes["value"].Value;
                    XmlAttribute l_attr_visible = nodeElement.Attributes["v"];
                    if (null == l_attr_visible)
                    {
                        l_attr_visible = nodeElement.Attributes["visible"];
                    }
                    bool lb_visible = (l_attr_visible.Value == "1");

                    int li_x = int.Parse(nodeElement.Attributes["x"].Value);
                    int li_y = int.Parse(nodeElement.Attributes["y"].Value);
                    int li_turns = XmlAttrToInt(nodeElement.Attributes["t"]);

                    Insert.Satelite l_satelite = new Insert.Satelite(ls_name, ls_value, li_x, li_y, lb_visible, li_turns);
                    l_satelite.m_alignment = (QTextAlignment)XmlAttrToInt(nodeElement.Attributes["al"]);

                    a_cable.m_satelites.Add(l_satelite);

                }
            }
        }

        private static void AddAttributesToInsert(Insert a_insert, XmlNode a_node)
        {
            foreach (XmlNode nodeElement in a_node)
            {
                if (nodeElement.Name == "attribute")
                {
                    string ls_name = nodeElement.Attributes["name"].Value;
                    string ls_value = nodeElement.Attributes["value"].Value;

                    XmlAttribute l_attr_visible = nodeElement.Attributes["v"];
                    if(null == l_attr_visible)
                    {
                        l_attr_visible = nodeElement.Attributes["visible"];
                    }
                    bool lb_visible = (l_attr_visible.Value == "1");


                    int li_x = int.Parse(nodeElement.Attributes["x"].Value);
                    int li_y = int.Parse(nodeElement.Attributes["y"].Value);
                    int li_turns = XmlAttrToInt(nodeElement.Attributes["t"]);

                    Insert.Satelite l_satelite = new Insert.Satelite(ls_name, ls_value, li_x, li_y, lb_visible, li_turns);
                    l_satelite.m_alignment = (QTextAlignment)XmlAttrToInt(nodeElement.Attributes["al"]);

                    a_insert.m_satelites.Add(l_satelite);

                }
            }
        }


        private static void SetupLegacySheetSize(string as_pgsOri, PrintSettings a_printSettings)
        {
            int li_long = 283;
            int li_short = 203;

            if (as_pgsOri == "LANDSCAPE")
            {
                a_printSettings.SheetSizeX = li_long;
                a_printSettings.SheetSizeY = li_short;
            }
            else
            {
                a_printSettings.SheetSizeX = li_short;
                a_printSettings.SheetSizeY = li_long;
            }
             
        }

        private static void LoadPtb(PCadDoc a_page, XmlNode a_node)
        {
            XmlNode l_node_tb = a_node.SelectSingleNode("tb");
            if (l_node_tb != null)
            {
                a_page.m_ptbPosition.Path       = XmlAttrToString(l_node_tb.Attributes["name"]);
                a_page.m_ptbPosition.m_useTb    = XmlAttrToBool  (l_node_tb.Attributes["use"]);
                a_page.m_ptbPosition.m_turn     = XmlAttrToBool  (l_node_tb.Attributes["t"]);
            }
        }

        public static int TurnsToAngle(int ai_turns)
        {
            int li_result = 450 * ai_turns;
            return li_result;
        }

        private static void FromXmlPositionsOfLabels(Wire a_wire, XmlNode a_node)
        {
            XmlNode l_node = a_node.SelectSingleNode("fixed_labels");
            if(null != l_node)
            {
                Load_Wire_Label(l_node, "beg", ref a_wire.m_label_beg);
                Load_Wire_Label(l_node, "mid", ref a_wire.m_label_mid);
                Load_Wire_Label(l_node, "end", ref a_wire.m_label_end);
            }
        }

        private static void Load_Wire_Label(XmlNode a_node, String as_name, ref DxfNet.WireLabelPos a_label)
        {
            XmlNode l_node = a_node.SelectSingleNode(as_name);
            if (null != l_node)
            {
                Point l_point = XmlAttrToPoint(l_node, "");
                a_label = new DxfNet.WireLabelPos(l_point);
            }
        }

        private static void FromXmlPositionsOfLinks(Wire a_wire, XmlNode a_node)
        {
            FromXmlPositionOfLink(a_wire, a_node, true);
            FromXmlPositionOfLink(a_wire, a_node, false);
        }

        private static void FromXmlPositionOfLink(Wire a_wire, XmlNode a_node, bool ab_first)
        {
            string ls_tagName = ab_first ? "linkFirst" : "linkLast";

            XmlNode l_childNode = a_node.SelectSingleNode(ls_tagName);
            if (null == l_childNode)
            {
                return;
            }


            string ls_x = XmlAttrToString(l_childNode.Attributes["x"]);
            if (!string.IsNullOrWhiteSpace(ls_x))
            {
                string ls_y = XmlAttrToString(l_childNode.Attributes["y"]);

                int li_x = int.Parse(ls_x);
                int li_y = int.Parse(ls_y);
                Size l_size = new Size(li_x, li_y);

                if (ab_first)
                {
                    a_wire.m_posDiffLinkFirst = l_size;
                }
                else
                {
                    a_wire.m_posDiffLinkLast = l_size;
                }


                return;
            }
        }

        private static void LoadPageSizeSettings(PCadDoc a_page, XmlNode a_node)
        {
            a_page.m_page_size_settings.PaperSizeEnum = XmlAttrToInt(a_node.Attributes["paper_size_enum"]);
            a_page.m_page_size_settings.sheet_size = XmlAttrToSize(a_node, "sheet_size_", 0);

            Size l_size = XmlAttrToSize(a_node, "sheet_count_", 1);
            a_page.m_page_size_settings.SheetsCount = l_size;

            a_page.m_page_size_settings.m_source = (PageSizeSettings.EnumPaperSizeSource)XmlAttrToInt(a_node.Attributes["source"]);

            a_page.m_page_size_settings.PageMargins = XmlAttrib2Rect(a_node, "margins", 10);
        }



        private static void LoadPagePrintSettings(PCadDoc a_page, XmlNode a_node)
        {
            a_page.m_page_print_settings.PaperSizeEnum = XmlAttrToInt(a_node.Attributes["paper_size_enum"]);
            a_page.m_page_print_settings.sheet_size = XmlAttrToSize(a_node, "sheet_size_", 0);

        }

        private static void Load_Dim_Line(XmlNode a_node, string as_tag_name, ref QDimStyle.ColorThickness a_line_dim)
        {
            XmlNode l_node_line = a_node.SelectSingleNode(as_tag_name);
            if (null != l_node_line)
            {
                string ls_color = XmlAttrToString(l_node_line.Attributes["op-lc"]);
                int li_color = int.Parse(ls_color);

                a_line_dim.m_color = Helper.ColorFromWin32(li_color);

                a_line_dim.m_thickness = XmlAttrToInt(l_node_line.Attributes["op-lw"]);
            }

        }




        //----------------------------


    }
}
