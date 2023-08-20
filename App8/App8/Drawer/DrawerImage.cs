using System;
using System.Collections.Generic;
using System.Text;
using DxfNet;
using SkiaSharp;
using System.IO;

namespace Drawer
{
    public static class DrawerImage
    {
        public static void DrawImage(SKCanvas a_canvas, QImage a_image, Repo a_repo)
        {
            if(a_image.m_bitmap == null)
            {
                QImageDesc l_img_desc = a_repo.FindImageInRepo(a_image.LastGuid);
                byte[] imageBytes = Convert.FromBase64String(l_img_desc.ImgEncoded);
                a_image.m_bitmap = SKBitmap.Decode(imageBytes);
            }

            SKPoint l_center_point = Helper.Point2SKPoint(a_image.GetCenterPoint());

            
            a_canvas.Save();

            a_canvas.Translate(l_center_point);
            a_canvas.RotateDegrees(a_image.Angle);

            float lf_scale_x = 1f;
            float lf_scale_y = 1f;

            if (a_image.m_hor)
            {
                lf_scale_y = -1f;
            }
            if (a_image.m_ver)
            {
                lf_scale_x = -1f;
            }

            a_canvas.Scale(lf_scale_x, lf_scale_y);

            int li_rad_x = a_image.m_position.Width / 2;
            int li_rad_y = a_image.m_position.Height / 2;
            SKRect l_rect = new SKRect(-li_rad_x, -li_rad_y, li_rad_x, li_rad_y);
            a_canvas.DrawBitmap(a_image.m_bitmap, l_rect);

            a_canvas.Restore();
            
        }


    }
}
