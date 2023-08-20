using System;
using System.Diagnostics;

using Xamarin.Forms;

using SkiaSharp;

using SkiaSharp.Views.Forms;

using DxfNet;


namespace App8
{
    

	public partial class MainPage : ContentPage
	{

        private bool m_is_zoom_to_fit = true;

        private bool m_is_large;

        private SKMatrix m_user_matrix = SKMatrix.CreateScale(1, 1, 1000, 1000);//this matrix preserves current pinch and pan

        private SKMatrix m_current_matrix = SKMatrix.CreateIdentity();
        private SKMatrix _startPanM = SKMatrix.CreateIdentity();
        private SKMatrix _startPinchM = SKMatrix.CreateIdentity();
        private Point _startPinchAnchor = Point.Zero;
        private float _totalPinchScale = 1f;
        private float _screenScale;
        private bool _panning = false;
        private bool m_is_loading = false;
        private SKRectI m_rect;

        public MainPage()
		{
			InitializeComponent();

            Title = "ProfiCAD Viewer 1.7";

#if __ANDROID__
            _screenScale = Android.App.Application.Context.Resources.DisplayMetrics.Density;
#else
			//_screenScale = (float)UIKit.UIScreen.MainScreen.Scale;
			_screenScale = 0.1f;
#endif


            m_canvasView = new SKCanvasView();
            m_canvasView.PaintSurface += CanvasView_PaintSurface;
            m_canvasView.SizeChanged += M_canvasView_SizeChanged;
            Content = m_canvasView;

            AddSupportPan(m_canvasView);
            AddSupportPinch(m_canvasView);
            AddSupportTap(m_canvasView);

        }

        private void M_canvasView_SizeChanged(object sender, EventArgs e)
        {
            //calc matrix to fit the drawing into the screen

  
        }

        //---------------------------------------------------
        private void AddSupportPan(SKCanvasView canvasView)
        {
            PanGestureRecognizer l_ges_rec = new PanGestureRecognizer();
            l_ges_rec.PanUpdated += OnPanUpdated;
            canvasView.GestureRecognizers.Add(l_ges_rec);
        }

        private void AddSupportPinch(SKCanvasView canvasView)
        {
            PinchGestureRecognizer l_ges_rec = new PinchGestureRecognizer();
            l_ges_rec.PinchUpdated += OnPinchUpdated;
            canvasView.GestureRecognizers.Add(l_ges_rec);
        }

        private void AddSupportTap(SKCanvasView canvasView)
        {
            TapGestureRecognizer l_ges_rec = new TapGestureRecognizer();
            l_ges_rec.Tapped += OnTapped;
            canvasView.GestureRecognizers.Add(l_ges_rec);
        }
        //---------------------------------------------------
        private void OnPanUpdated(object sender, PanUpdatedEventArgs puea)
        {
            Debug.WriteLine($"Pan {puea.StatusType} ({puea.TotalX},{puea.TotalY})");
            SKCanvasView l_view = sender as SKCanvasView;

            switch (puea.StatusType)
            {
                case GestureStatus.Started:
                    _startPanM = m_current_matrix;
                    _panning = true;
                    break;
                case GestureStatus.Running:
                    if (_panning)
                    {
                        float canvasTotalX = (float)puea.TotalX * _screenScale;
                        float canvasTotalY = (float)puea.TotalY * _screenScale;
                        SKMatrix canvasTranslation = SKMatrix.CreateTranslation(canvasTotalX, canvasTotalY);
                        SKMatrix.Concat(ref m_current_matrix, ref canvasTranslation, ref _startPanM);

                        l_view.InvalidateSurface();
                        m_user_matrix = m_current_matrix;

                    }
                    break;
                case GestureStatus.Completed:
                    _panning = false;
                    break;
                case GestureStatus.Canceled:
                    _panning = false;
                    break;


                default:
                  
                    break;
            }
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs puea)
        {
            Debug.WriteLine($"Pinch {puea.Status} ({puea.ScaleOrigin.X},{puea.ScaleOrigin.Y}) {puea.Scale}");
            SKCanvasView l_view = sender as SKCanvasView;

            var canvasAnchor = new Point(
                puea.ScaleOrigin.X * l_view.Width * _screenScale,
                puea.ScaleOrigin.Y * l_view.Height * _screenScale);
            switch (puea.Status)
            {
                case GestureStatus.Started:
                    _startPinchM = m_current_matrix;
                    _startPinchAnchor = canvasAnchor;
                    _totalPinchScale = 1f;
                    break;

                case GestureStatus.Running:
                    _panning = false;
                    _totalPinchScale *= (float)puea.Scale;
                    SKMatrix canvasScaling = SKMatrix.MakeScale(_totalPinchScale, _totalPinchScale, (float)_startPinchAnchor.X, (float)_startPinchAnchor.Y);
                    SKMatrix.Concat(ref m_current_matrix, ref canvasScaling, ref _startPinchM);

                    l_view.InvalidateSurface();

                    m_user_matrix = m_current_matrix;
                    break;

                default:
                    /*
					_startPinchM = SKMatrix.MakeIdentity();
					_startPinchAnchor = Point.Zero;
					_totalPinchScale = 1f;

					// force textLayer to regenerate
					_textLayer?.Dispose();
					_textLayer = null;
					_canvasV.InvalidateSurface();
                    */
                    break;
            }
        }

        private void OnTapped(object sender, EventArgs e)
        {

            if (sender is SKCanvasView l_view)
            {
                /*
                m_is_zoom_to_fit = !m_is_zoom_to_fit;

                if(m_is_zoom_to_fit)
                {
                    m_rect.Right = 0;//enforce recalculation of the zoom to fit

                }
                else
                {
                    m_current_matrix = m_user_matrix;
                }
                */


                
                 if(NavigationPage.GetHasNavigationBar(this))
                 {
                     NavigationPage.SetHasNavigationBar(this, false);
                     m_is_large = false;
                 }
                 else
                 {
                    if(m_is_large)
                    {
                        NavigationPage.SetHasNavigationBar(this, true);
                        m_is_large = false;
                    }
                    else
                    {
                        m_is_large = true;
                        m_current_matrix = m_user_matrix;
                    }
                 }
                 

                 l_view.InvalidateSurface();

            }
        }
        //---------------------------------------------------


   

        public void Load_Drawing(System.IO.Stream a_stream)
        {

            if (Xamarin.Forms.Application.Current is App8.App l_app)
            {
                try
                {
                    l_app.m_doc = Loader.Loader.Load(a_stream);

                    if(l_app.m_doc.HasMoreThanOnePage())
                    {
                        if(!ToolbarItems.Contains(m_toolbar_item_pages))
                        {
                            ToolbarItems.Add(m_toolbar_item_pages);
                        }
                    }
                    else
                    {
                        ToolbarItems.Remove(m_toolbar_item_pages);
                    }
                    

                    

                }
                catch (Exception e)
                {
                    DisplayAlert("ProfiCAD", "This drawing cannot be viewed.", "OK");
                    return;
                }

                m_canvasView.InvalidateSurface();
            }
        }


        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            
            // we get the current surface from the event args
            SKSurface surface = e.Surface;
            // then we get the canvas that we can draw on
            SKCanvas canvas = surface.Canvas;
            // clear the canvas / view
            canvas.Clear(SKColors.White);

            if(m_is_loading)
            {

                SKPaint l_paint_text = new SKPaint
                {
                    Typeface = SKTypeface.FromFamilyName(
                                   "Arial",
                                   SKFontStyleWeight.Normal,
                                   SKFontStyleWidth.Normal,
                                   SKFontStyleSlant.Upright),
                    TextSize = 50,
                    TextAlign = SKTextAlign.Center
                };
                canvas.DrawText("loading drawing...", 1000, 1000, l_paint_text);
                return;
            }



            if(m_rect != e.Info.Rect)
            {
                Set_Zoom_To_Fit(e.Info.Rect);

                m_rect = e.Info.Rect;
            }

            canvas.SetMatrix(m_current_matrix);


            CollPages l_doc = App8.App.GetCurrentDoc;
            if (l_doc != null)
            {
                Drawer.Drawer.DrawPCadDoc(canvas, e.Info.Rect, l_doc.GetCurrentPage());
            }
        

        }

        private void Set_Zoom_To_Fit(SKRectI a_rect)
        {
            if (a_rect.Width == 0)
            {
                return;
            }

            float lf_padding = 10f;
            float lf_screen_width  = a_rect.Width  - lf_padding - lf_padding;
            float lf_screen_height = a_rect.Height - lf_padding - lf_padding;

            CollPages l_doc = App8.App.GetCurrentDoc;
            if (l_doc != null)
            {
                System.Drawing.SizeF l_size = l_doc.Get_Size_Current_Page();
                float lf_factor_w = lf_screen_width / l_size.Width;
                float lf_factor_h = lf_screen_height / l_size.Height;
                float lf_factor_def = Math.Min(lf_factor_w, lf_factor_h);

                float lf_rem_w = 0, lf_rem_h = 0;
                if (lf_factor_w < lf_factor_h)
                {
                    lf_rem_h = (lf_screen_height - (l_size.Height * lf_factor_def)) / 2;
                }
                else
                {
                    lf_rem_w = (lf_screen_width - (l_size.Width * lf_factor_def)) / 2;
                }
                lf_rem_w += lf_padding;
                lf_rem_h += lf_padding;
                SKMatrix.CreateScaleTranslation(lf_factor_def, lf_factor_def, lf_rem_w, lf_rem_h);
            }
        }

        async void OnClick(object sender, EventArgs e)
        {
            if (sender is Xamarin.Forms.ToolbarItem l_toolbar_item)
            {

                switch (l_toolbar_item.Text)
                {
                    case "About":
                        await Navigation.PushAsync(new About());
                        break;
                    case "Pages":
                        await Navigation.PushAsync(new ListOfPages());
                        break;

                }

            }

        }
        //-------------------
    }
}
