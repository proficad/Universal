﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Drawing;

namespace DxfNet
{
    public class DrawPoly : DrawObj
    {
        public DrawPoly(Shape a_shape) : base(a_shape)
        {
        }

        public DrawPoly(Shape a_shape, int ai_thickness, Color a_colorBorder) : base(a_shape)
        {
            m_objProps.m_logpen.m_width = ai_thickness;
            m_objProps.m_logpen.m_color = a_colorBorder;
        }

        public DrawPoly(Shape a_shape, int ai_thickness, Color a_colorBorder, Point a_from, Point a_to) : base(a_shape)
        {
            m_objProps.m_logpen.m_width = ai_thickness;
            m_objProps.m_logpen.m_color = a_colorBorder;
            AddPoint(a_from);
            AddPoint(a_to);
        }

        public DrawPoly(Shape a_shape, int ai_thickness, Color a_colorBorder, Point [] a_points) : base(a_shape)
        {
            m_objProps.m_logpen.m_width = ai_thickness;
            m_objProps.m_logpen.m_color = a_colorBorder;
            foreach (Point l_point in a_points)
            {
                AddPoint(l_point);
            }
        }

        public List<Point> m_points = new List<Point>();

        public float Scale_arrow_x { get; internal set; }
        public float Scale_arrow_y { get; internal set; }


        public void AddPoint(int ai_x, int ai_y)
        {
            Point p = new Point(ai_x, ai_y);
            m_points.Add(p);
        }
        public void AddPoint(Point a_point)
        {
            m_points.Add(a_point);
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {
           
           	switch(m_nShape){
        	case Shape.spoj:
                a_xmlWriter.WriteStartElement("wire");
		        break;
	        case Shape.poly:
                a_xmlWriter.WriteStartElement("polygon");
        		break;
	        case Shape.polyline:
                a_xmlWriter.WriteStartElement("polyline");
        		break;
	        case Shape.bezier:
                a_xmlWriter.WriteStartElement("polybezier");
        		break;
	        default:
                throw new Exception("invalid shape");
		        
	        }
            if (m_nShape == Shape.poly)
	        {
                m_objProps.SaveToXml(a_xmlWriter);
	        }
	        else
	        {
                m_objProps.SaveLineToXml(a_xmlWriter);
	        }


        	System.Text.StringBuilder ls_points = new StringBuilder();
            foreach (Point point in m_points)
            {
                ls_points.Append(string.Format("{0},{1},", point.X, point.Y));
            }

            a_xmlWriter.WriteAttributeString("pts", ls_points.ToString());

            a_xmlWriter.WriteEndElement();

        }

        internal override void RecalcBounds(ref MyRect l_bounds) 
        {
            foreach (Point point in m_points)
            {
                if (point.X < l_bounds.Left)
                {
                    l_bounds.Left = point.X;
                }
                if (point.Y < l_bounds.Top)
                {
                    l_bounds.Top = point.Y;
                }
                if (point.X > l_bounds.Right)
                {
                    l_bounds.Right = point.X;
                }
                if (point.Y > l_bounds.Bottom)
                {
                    l_bounds.Bottom = point.Y;
                }


            }
        }


        internal void RecalcBounds(ref Rectangle l_bounds)
        {
            foreach (Point point in m_points)
            {
                if (point.X < l_bounds.Left)
                {
                    l_bounds.X = point.X;
                    l_bounds.Width += (l_bounds.Left - point.X);
                }
                if (point.Y < l_bounds.Top)
                {
                    l_bounds.Y = point.Y;
                    l_bounds.Height += (l_bounds.Top - point.Y);
                }
                if (point.X > l_bounds.Right)
                {
                    l_bounds.Width += (point.X - l_bounds.X);
                }
                if (point.Y > l_bounds.Bottom)
                {
                    l_bounds.Height += (point.Y - l_bounds.Y);
                }


            }
        }

        internal override void MoveBy(Size l_offset)
        {
            for (int li_i = 0; li_i < m_points.Count; li_i++)
            {
                Point l_point = m_points[li_i];
                l_point.X += l_offset.Width;
                l_point.Y += l_offset.Height;

                m_points[li_i] = l_point;
            }
        }


        //-------------------------

        public override void RecalcPosition()
        {
            if (m_points.Count == 0)
            {
                return;
            }
            m_position.X = m_points[0].X;
            m_position.Y = m_points[0].Y;

            RecalcBounds(ref m_position);
        }
    }
}
