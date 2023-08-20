using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public class QDimLine : DrawObj
    {
        public Point A, B, C;
        public QLabel Label;

        public enum DimDirection { dimdir_none, dimdir_hor, dimdir_ver, dimdir_aligned };
        public DimDirection m_dir;

        public QDimLine(Point a_a, Point a_b, Point a_c, DimDirection a_dir) : base(Shape.dim_line, new Rectangle(a_a.X, a_a.Y, 1, 1))
        {
            A = a_a;
            B = a_b;
            C = a_c;
            m_dir = a_dir;
        }

        internal override void Write2Xml(System.Xml.XmlWriter a_xmlWriter)
        {

        }

        internal override void RecalcBounds(ref MyRect l_bounds)
        {

        }

        internal override void MoveBy(Size a_offset)
        {
            A.Offset(a_offset.Width, a_offset.Height);
            B.Offset(a_offset.Width, a_offset.Height);
            C.Offset(a_offset.Width, a_offset.Height);

        }

        public override void RecalcPosition()
        {

        }

        public void Calc_Dim_Points(out Point a_point_1, out Point a_point_2)
        {
            a_point_1 = new Point();
            a_point_2 = new Point();

            if (m_dir == DimDirection.dimdir_hor)
            {
                a_point_1.X = A.X;
                a_point_1.Y = C.Y;
                a_point_2.X = B.X;
                a_point_2.Y = C.Y;
            }
            else if (m_dir == DimDirection.dimdir_ver)
            {
                a_point_1.X = C.X;
                a_point_1.Y = A.Y;
                a_point_2.X = C.X;
                a_point_2.Y = B.Y;
            }
            else if (m_dir == DimDirection.dimdir_aligned)
            {
                Calc_Aligned_Dim_Points(out a_point_1, out a_point_2);
            }
        }

        private void Calc_Aligned_Dim_Points(out Point a_point_1, out Point a_point_2)
        {
            double ld_angle1 = Math.Atan2(B.Y - A.Y, B.X - A.X);

            Point l_b = Loader.Helper.RotatePointRad(A, -ld_angle1, B);
            Point l_c = Loader.Helper.RotatePointRad(A, -ld_angle1, C);


            Point l_d = new Point(  A.X, l_c.Y);
            Point l_e = new Point(l_b.X, l_c.Y);

            a_point_1 = Loader.Helper.RotatePointRad(A, ld_angle1, l_d);
            a_point_2 = Loader.Helper.RotatePointRad(A, ld_angle1, l_e);
        }

        //--------------------------------------
    }
}
