using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Audion.Visualization
{
    [TemplatePart(Name = "PART_Waveform", Type = typeof(Canvas))]
    public class Waveform : Control
    {
        private Source _source;

        private Canvas waveformCanvas;
        private readonly Path leftPath = new Path();
        private readonly Path rightPath = new Path();
        private readonly Line centerLine = new Line();

        #region Dependency Properties

        #region Source Property

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Source),
            typeof(Waveform), new UIPropertyMetadata(null, OnSourceChanged, OnCoerceSource));

        private static object OnCoerceSource(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceSource((Source)value);
            else
                return value;
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnSourceChanged((Source)e.OldValue, (Source)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="Source"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="Source"/></param>
        /// <returns>The adjusted value of <see cref="Source"/></returns>
        protected virtual Source OnCoerceSource(Source value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="Source"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="Source"/></param>
        /// <param name="newValue">The new value of <see cref="Source"/></param>
        protected virtual void OnSourceChanged(Source oldValue, Source newValue)
        {
            _source = Source;
            _source.SourceEvent += _source_SourceEvent;
            _source.SourcePropertyChangedEvent += _source_SourcePropertyChangedEvent;
        }

        private void _source_SourceEvent(object sender, SourceEventArgs e)
        {
            if (e.Event == Source.Event.Loaded)
            {
                // new track loaded into the source.
                Source.GetData(Resolution);
            }
        }

        private void _source_SourcePropertyChangedEvent(object sender, SourcePropertyChangedEventArgs e)
        {
            if (e.Property == Source.Property.WaveformData)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    UpdateWaveform();
                });
            }
        }

        /// <summary>
        /// Gets or sets a Source for the Waveform.
        /// </summary>        
        public Source Source
        {
            get
            {
                return (Source)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        #endregion

        #region Resolution Property

        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(int),
            typeof(Waveform), new UIPropertyMetadata(500, OnResolutionChanged, OnCoerceResolution));

        private static object OnCoerceResolution(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceResolution((int)value);
            else
                return value;
        }

        private static void OnResolutionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnResolutionChanged((int)e.OldValue, (int)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="Resolution"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="Resolution"/></param>
        /// <returns>The adjusted value of <see cref="Resolution"/></returns>
        protected virtual int OnCoerceResolution(int value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="Resolution"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="Resolution"/></param>
        /// <param name="newValue">The new value of <see cref="Resolution"/></param>
        protected virtual void OnResolutionChanged(int oldValue, int newValue)
        {
            // make sure that the resolution is an even number
            if (newValue % 2 != 0)
            {
                Resolution = newValue - 1;
            }
        }

        /// <summary>
        /// Gets or sets a Resolution for the Waveform.
        /// </summary>        
        public int Resolution
        {
            get
            {
                return (int)GetValue(ResolutionProperty);
            }
            set
            {
                SetValue(ResolutionProperty, value);
            }
        }

        #endregion

        #region LeftBrush Property

        public static readonly DependencyProperty LeftBrushProperty = DependencyProperty.Register("LeftBrush", typeof(Brush),
            typeof(Waveform), new UIPropertyMetadata(null, OnLeftBrushChanged, OnCoerceLeftBrush));

        private static object OnCoerceLeftBrush(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceLeftBrush((Brush)value);
            else
                return value;
        }

        private static void OnLeftBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnLeftBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="LeftBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="LeftBrush"/></param>
        /// <returns>The adjusted value of <see cref="LeftBrush"/></returns>
        protected virtual Brush OnCoerceLeftBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="LeftBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="LeftBrush"/></param>
        /// <param name="newValue">The new value of <see cref="LeftBrush"/></param>
        protected virtual void OnLeftBrushChanged(Brush oldValue, Brush newValue)
        {
            var frozen = newValue.Clone();
            frozen.Freeze();
            leftPath.Fill = frozen;
            UpdateWaveform();
        }

        /// <summary>
        /// Gets or sets a LeftBrush for the Waveform.
        /// </summary>        
        public Brush LeftBrush
        {
            get
            {
                return (Brush)GetValue(LeftBrushProperty);
            }
            set
            {
                SetValue(LeftBrushProperty, value);
            }
        }

        #endregion

        #region RightBrush Property

        public static readonly DependencyProperty RightBrushProperty = DependencyProperty.Register("RightBrush", typeof(Brush),
            typeof(Waveform), new UIPropertyMetadata(null, OnRightBrushChanged, OnCoerceRightBrush));

        private static object OnCoerceRightBrush(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceRightBrush((Brush)value);
            else
                return value;
        }

        private static void OnRightBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnRightBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="RightBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="RightBrush"/></param>
        /// <returns>The adjusted value of <see cref="RightBrush"/></returns>
        protected virtual Brush OnCoerceRightBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="RightBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="RightBrush"/></param>
        /// <param name="newValue">The new value of <see cref="RightBrush"/></param>
        protected virtual void OnRightBrushChanged(Brush oldValue, Brush newValue)
        {
            var frozen = newValue.Clone();
            frozen.Freeze();
            rightPath.Fill = frozen;
            UpdateWaveform();
        }

        /// <summary>
        /// Gets or sets a RightBrush for the Waveform.
        /// </summary>        
        public Brush RightBrush
        {
            get
            {
                return (Brush)GetValue(RightBrushProperty);
            }
            set
            {
                SetValue(RightBrushProperty, value);
            }
        }

        #endregion

        #region LeftStroke Property

        public static readonly DependencyProperty LeftStrokeProperty = DependencyProperty.Register("LeftStroke", typeof(Brush),
            typeof(Waveform), new UIPropertyMetadata(null, OnLeftStrokeChanged, OnCoerceLeftStroke));

        private static object OnCoerceLeftStroke(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceLeftStroke((Brush)value);
            else
                return value;
        }

        private static void OnLeftStrokeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnLeftStrokeChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="LeftStroke"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="LeftStroke"/></param>
        /// <returns>The adjusted value of <see cref="LeftStroke"/></returns>
        protected virtual Brush OnCoerceLeftStroke(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="LeftStroke"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="LeftStroke"/></param>
        /// <param name="newValue">The new value of <see cref="LeftStroke"/></param>
        protected virtual void OnLeftStrokeChanged(Brush oldValue, Brush newValue)
        {
            var frozen = newValue.Clone();
            frozen.Freeze();
            leftPath.Stroke = frozen;
            UpdateWaveform();
        }

        /// <summary>
        /// Gets or sets a LeftStroke for the Waveform.
        /// </summary>        
        public Brush LeftStroke
        {
            get
            {
                return (Brush)GetValue(LeftStrokeProperty);
            }
            set
            {
                SetValue(LeftStrokeProperty, value);
            }
        }

        #endregion

        #region RightStroke Property

        public static readonly DependencyProperty RightStrokeProperty = DependencyProperty.Register("RightStroke", typeof(Brush),
            typeof(Waveform), new UIPropertyMetadata(null, OnRightStrokeChanged, OnCoerceRightStroke));

        private static object OnCoerceRightStroke(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceRightStroke((Brush)value);
            else
                return value;
        }

        private static void OnRightStrokeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnRightStrokeChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="RightStroke"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="RightStroke"/></param>
        /// <returns>The adjusted value of <see cref="RightStroke"/></returns>
        protected virtual Brush OnCoerceRightStroke(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="RightStroke"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="RightStroke"/></param>
        /// <param name="newValue">The new value of <see cref="RightStroke"/></param>
        protected virtual void OnRightStrokeChanged(Brush oldValue, Brush newValue)
        {
            var frozen = newValue.Clone();
            frozen.Freeze();
            rightPath.Stroke = frozen;
            UpdateWaveform();
        }

        /// <summary>
        /// Gets or sets a RightStroke for the Waveform.
        /// </summary>        
        public Brush RightStroke
        {
            get
            {
                return (Brush)GetValue(RightStrokeProperty);
            }
            set
            {
                SetValue(RightStrokeProperty, value);
            }
        }

        #endregion

        #region LeftStrokeThickness Property

        public static readonly DependencyProperty LeftStrokeThicknessProperty = DependencyProperty.Register("LeftStrokeThickness", typeof(double),
            typeof(Waveform), new UIPropertyMetadata(0.0d, OnLeftStrokeThicknessChanged, OnCoerceLeftStrokeThickness));

        private static object OnCoerceLeftStrokeThickness(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceLeftStrokeThickness((double)value);
            else
                return value;
        }

        private static void OnLeftStrokeThicknessChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnLeftStrokeThicknessChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="LeftStrokeThickness"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="LeftStrokeThickness"/></param>
        /// <returns>The adjusted value of <see cref="LeftStrokeThickness"/></returns>
        protected virtual double OnCoerceLeftStrokeThickness(double value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="LeftStrokeThickness"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="LeftStrokeThickness"/></param>
        /// <param name="newValue">The new value of <see cref="LeftStrokeThickness"/></param>
        protected virtual void OnLeftStrokeThicknessChanged(double oldValue, double newValue)
        {
            leftPath.StrokeThickness = newValue;
        }

        /// <summary>
        /// Gets or sets a LeftStrokeThickness for the Waveform.
        /// </summary>        
        public double LeftStrokeThickness
        {
            get
            {
                return (double)GetValue(LeftStrokeThicknessProperty);
            }
            set
            {
                SetValue(LeftStrokeThicknessProperty, value);
            }
        }

        #endregion

        #region RightStrokeThickness Property

        public static readonly DependencyProperty RightStrokeThicknessProperty = DependencyProperty.Register("RightStrokeThickness", typeof(double),
            typeof(Waveform), new UIPropertyMetadata(0.0d, OnRightStrokeThicknessChanged, OnCoerceRightStrokeThickness));

        private static object OnCoerceRightStrokeThickness(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceRightStrokeThickness((double)value);
            else
                return value;
        }

        private static void OnRightStrokeThicknessChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnRightStrokeThicknessChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="RightStrokeThickness"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="RightStrokeThickness"/></param>
        /// <returns>The adjusted value of <see cref="RightStrokeThickness"/></returns>
        protected virtual double OnCoerceRightStrokeThickness(double value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="RightStrokeThickness"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="RightStrokeThickness"/></param>
        /// <param name="newValue">The new value of <see cref="RightStrokeThickness"/></param>
        protected virtual void OnRightStrokeThicknessChanged(double oldValue, double newValue)
        {
            rightPath.StrokeThickness = newValue;
        }

        /// <summary>
        /// Gets or sets a RightStrokeThickness for the Waveform.
        /// </summary>        
        public double RightStrokeThickness
        {
            get
            {
                return (double)GetValue(RightStrokeThicknessProperty);
            }
            set
            {
                SetValue(RightStrokeThicknessProperty, value);
            }
        }

        #endregion

        #region CenterLineBrush Property
        
        public static readonly DependencyProperty CenterLineBrushProperty = DependencyProperty.Register("CenterLineBrush", typeof(Brush),
            typeof(Waveform), new UIPropertyMetadata(Brushes.White, OnCenterLineBrushChanged, OnCoerceCenterLineBrush));

        private static object OnCoerceCenterLineBrush(DependencyObject o, object value)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                return waveform.OnCoerceCenterLineBrush((Brush)value);
            else
                return value;
        }

        private static void OnCenterLineBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Waveform waveform = o as Waveform;

            if (waveform != null)
                waveform.OnCenterLineBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="CenterLineBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="CenterLineBrush"/></param>
        /// <returns>The adjusted value of <see cref="CenterLineBrush"/></returns>
        protected virtual Brush OnCoerceCenterLineBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="CenterLineBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="CenterLineBrush"/></param>
        /// <param name="newValue">The new value of <see cref="CenterLineBrush"/></param>
        protected virtual void OnCenterLineBrushChanged(Brush oldValue, Brush newValue)
        {
            var frozen = newValue.Clone();
            frozen.Freeze();
            centerLine.Stroke = frozen;
            UpdateWaveform();
        }

        /// <summary>
        /// Gets or sets a CenterLineBrush for the Waveform.
        /// </summary>        
        public Brush CenterLineBrush
        {
            get
            {
                return (Brush)GetValue(CenterLineBrushProperty);
            }
            set
            {
                SetValue(CenterLineBrushProperty, value);
            }
        }

        #endregion

        #endregion

        static Waveform()
        {
            Waveform.DefaultStyleKeyProperty.OverrideMetadata(typeof(Waveform), new FrameworkPropertyMetadata(typeof(Waveform)));

            Application.SetFramerate();
        }

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            waveformCanvas = GetTemplateChild("PART_Waveform") as Canvas;
            waveformCanvas.CacheMode = new BitmapCache();

            waveformCanvas.Children.Add(leftPath);
            waveformCanvas.Children.Add(rightPath);
            waveformCanvas.Children.Add(centerLine);

            centerLine.StrokeThickness = 1d;

            UpdateWaveformCacheScaling();
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);

            if (waveformCanvas != null)
            {
                waveformCanvas.Children.Clear();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateWaveform();
            UpdateWaveformCacheScaling();
        }

        #endregion

        private void UpdateWaveformCacheScaling()
        {
            if (waveformCanvas == null)
                return;

            BitmapCache waveformCache = (BitmapCache)waveformCanvas.CacheMode;

            if (/*AutoScaleWaveformCache*/true)
            {
                double totalTransformScale = GetTotalTransformScale();
                if (waveformCache.RenderAtScale != totalTransformScale)
                    waveformCache.RenderAtScale = totalTransformScale;
            }
            else
            {
                waveformCache.RenderAtScale = 1.0d;
            }
        }

        private double GetTotalTransformScale()
        {
            double totalTransform = 1.0d;
            DependencyObject currentVisualTreeElement = this;
            do
            {
                Visual visual = currentVisualTreeElement as Visual;
                if (visual != null)
                {
                    Transform transform = VisualTreeHelper.GetTransform(visual);

                    // This condition is a way of determining if it
                    // was a uniform scale transform. Is there some better way?
                    if ((transform != null) &&
                        (transform.Value.M12 == 0) &&
                        (transform.Value.M21 == 0) &&
                        (transform.Value.OffsetX == 0) &&
                        (transform.Value.OffsetY == 0) &&
                        (transform.Value.M11 == transform.Value.M22))
                    {
                        totalTransform *= transform.Value.M11;
                    }
                }
                currentVisualTreeElement = VisualTreeHelper.GetParent(currentVisualTreeElement);
            }
            while (currentVisualTreeElement != null);

            return totalTransform;
        }

        private void UpdateWaveform()
        {
            const double minValue = 0;
            const double maxValue = 1.5;
            const double dbScale = (maxValue - minValue);

            if (_source == null || Source.WaveformData == null || waveformCanvas == null ||
                waveformCanvas.RenderSize.Width < 1 || waveformCanvas.RenderSize.Height < 1)
            {
                return;
            }

            double leftRenderHeight;
            double rightRenderHeight;

            int pointCount = (int)(Source.WaveformData.Length / 2.0d);
            double pointThickness = waveformCanvas.RenderSize.Width / pointCount;
            double waveformSideHeight = waveformCanvas.RenderSize.Height / 2.0d;
            double centerHeight = waveformSideHeight;

            if (CenterLineBrush != null)
            {
                centerLine.X1 = 0;
                centerLine.X2 = waveformCanvas.RenderSize.Width;
                centerLine.Y1 = centerHeight;
                centerLine.Y2 = centerHeight;
            }

            if (Source.WaveformData != null && Source.WaveformData.Length > 1)
            {
                PolyLineSegment leftWaveformPolyLine = new PolyLineSegment();
                leftWaveformPolyLine.Points.Add(new Point(0, centerHeight));

                PolyLineSegment rightWaveformPolyLine = new PolyLineSegment();
                rightWaveformPolyLine.Points.Add(new Point(0, centerHeight));

                double xLocation = 0.0d;

                for (int i = 0; i < Source.WaveformData.Length; i += 2)
                {
                    xLocation = (i / 2) * pointThickness;
                    leftRenderHeight = ((Source.WaveformData[i] - minValue) / dbScale) * waveformSideHeight;
                    leftWaveformPolyLine.Points.Add(new Point(xLocation, centerHeight - leftRenderHeight));
                    rightRenderHeight = ((Source.WaveformData[i + 1] - minValue) / dbScale) * waveformSideHeight;
                    rightWaveformPolyLine.Points.Add(new Point(xLocation, centerHeight + rightRenderHeight));
                }

                leftWaveformPolyLine.Points.Add(new Point(xLocation, centerHeight));
                leftWaveformPolyLine.Points.Add(new Point(0, centerHeight));
                rightWaveformPolyLine.Points.Add(new Point(xLocation, centerHeight));
                rightWaveformPolyLine.Points.Add(new Point(0, centerHeight));

                PathGeometry leftGeometry = new PathGeometry();
                PathFigure leftPathFigure = new PathFigure();
                leftPathFigure.Segments.Add(leftWaveformPolyLine);
                leftPathFigure.StartPoint = leftWaveformPolyLine.Points[0];
                leftGeometry.Figures.Add(leftPathFigure);
                PathGeometry rightGeometry = new PathGeometry();
                PathFigure rightPathFigure = new PathFigure();
                rightPathFigure.Segments.Add(rightWaveformPolyLine);
                rightPathFigure.StartPoint = rightWaveformPolyLine.Points[0];
                rightGeometry.Figures.Add(rightPathFigure);

                leftPath.Data = leftGeometry;
                rightPath.Data = rightGeometry;
            }
            else
            {
                leftPath.Data = null;
                rightPath.Data = null;
            }
        }

    }
}
