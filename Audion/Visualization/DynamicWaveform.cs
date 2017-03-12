using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Audion.Visualization
{
    [TemplatePart(Name = "PART_Waveform", Type = typeof(Canvas))]
    public class DynamicWaveform : Control
    {
        private ISource _source;

        private Canvas waveformCanvas;
        private readonly Path leftPath = new Path();
        private readonly Path rightPath = new Path();
        private readonly Line centerLine = new Line();
        private Border progressLine;

        float[] wavelengthDataSnapshot;
        double cachedSeconds = 0;
        double cachedPosition = 0;
        double byteToResolutionRatio;

        #region Dependency Properties

        #region Source Property

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ISource),
            typeof(DynamicWaveform), new UIPropertyMetadata(null, OnSourceChanged, OnCoerceSource));

        private static object OnCoerceSource(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceSource((ISource)value);
            else
                return value;
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnSourceChanged((ISource)e.OldValue, (ISource)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="Source"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="Source"/></param>
        /// <returns>The adjusted value of <see cref="Source"/></returns>
        protected virtual ISource OnCoerceSource(ISource value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="Source"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="Source"/></param>
        /// <param name="newValue">The new value of <see cref="Source"/></param>
        protected virtual void OnSourceChanged(ISource oldValue, ISource newValue)
        {
            _source = Source;
            _source.SourceEvent += _source_SourceEvent;
            _source.SourcePropertyChangedEvent += _source_SourcePropertyChangedEvent;
        }

        private void _source_SourcePropertyChangedEvent(object sender, SourcePropertyChangedEventArgs e)
        {
            if (e.Property == Audion.SourceProperty.Position)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    UpdateWaveform();
                });
            }
            else if (e.Property == Audion.SourceProperty.PlaybackState)
            {
                if ((CSCore.SoundOut.PlaybackState)e.Value == CSCore.SoundOut.PlaybackState.Stopped)
                {
                    wavelengthDataSnapshot = null;
                    cachedSeconds = 0;
                    cachedPosition = 0;
                    byteToResolutionRatio = 0;
                }
            }
        }

        private void _source_SourceEvent(object sender, SourceEventArgs e)
        {
            if (e.Event == SourceEventType.Loaded)
            {
                wavelengthDataSnapshot = null;
                cachedSeconds = 0;
                cachedPosition = 0;
                byteToResolutionRatio = 0;

                // new track loaded into the source. Get the waveform data as fast as possible.
                Dispatcher.BeginInvoke((Action)delegate
                {
                    UpdateWaveform();

                });
            }
        }

        /// <summary>
        /// Gets or sets a Source for the DynamicWaveform.
        /// </summary>        
        public ISource Source
        {
            get
            {
                return (ISource)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        #endregion

        #region Resolution Property

        public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register("Resolution", typeof(int),
            typeof(DynamicWaveform), new UIPropertyMetadata(2048, OnResolutionChanged, OnCoerceResolution));

        private static object OnCoerceResolution(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceResolution((int)value);
            else
                return value;
        }

        private static void OnResolutionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnResolutionChanged((int)e.OldValue, (int)e.NewValue);
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
        /// Gets or sets a Resolution for the DynamicWaveform.
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

        #region WaveformLength Property

        public static readonly DependencyProperty WaveformLengthProperty = DependencyProperty.Register("WaveformLength", typeof(double),
            typeof(DynamicWaveform), new UIPropertyMetadata(1.0D, OnWaveformLengthChanged, OnCoerceWaveformLength));

        private static object OnCoerceWaveformLength(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceWaveformLength((double)value);
            else
                return value;
        }

        private static void OnWaveformLengthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnWaveformLengthChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="WaveformLength"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="WaveformLength"/></param>
        /// <returns>The adjusted value of <see cref="WaveformLength"/></returns>
        protected virtual double OnCoerceWaveformLength(double value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="WaveformLength"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="WaveformLength"/></param>
        /// <param name="newValue">The new value of <see cref="WaveformLength"/></param>
        protected virtual void OnWaveformLengthChanged(double oldValue, double newValue)
        {
            
        }

        /// <summary>
        /// Gets or sets a WaveformLength for the DynamicWaveform. This is the number
        /// of seconds of waveform to display.
        /// </summary>        
        public double WaveformLength
        {
            get
            {
                return (double)GetValue(WaveformLengthProperty);
            }
            set
            {
                SetValue(WaveformLengthProperty, value);
            }
        }

        #endregion

        #region LeftBrush Property

        public static readonly DependencyProperty LeftBrushProperty = DependencyProperty.Register("LeftBrush", typeof(Brush),
            typeof(DynamicWaveform), new UIPropertyMetadata(null, OnLeftBrushChanged, OnCoerceLeftBrush));

        private static object OnCoerceLeftBrush(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceLeftBrush((Brush)value);
            else
                return value;
        }

        private static void OnLeftBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnLeftBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
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
        /// Gets or sets a LeftBrush for the DynamicWaveform.
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
            typeof(DynamicWaveform), new UIPropertyMetadata(null, OnRightBrushChanged, OnCoerceRightBrush));

        private static object OnCoerceRightBrush(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceRightBrush((Brush)value);
            else
                return value;
        }

        private static void OnRightBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnRightBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
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
        /// Gets or sets a RightBrush for the DynamicWaveform.
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
            typeof(DynamicWaveform), new UIPropertyMetadata(null, OnLeftStrokeChanged, OnCoerceLeftStroke));

        private static object OnCoerceLeftStroke(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceLeftStroke((Brush)value);
            else
                return value;
        }

        private static void OnLeftStrokeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnLeftStrokeChanged((Brush)e.OldValue, (Brush)e.NewValue);
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
        /// Gets or sets a LeftStroke for the DynamicWaveform.
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
            typeof(DynamicWaveform), new UIPropertyMetadata(null, OnRightStrokeChanged, OnCoerceRightStroke));

        private static object OnCoerceRightStroke(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceRightStroke((Brush)value);
            else
                return value;
        }

        private static void OnRightStrokeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnRightStrokeChanged((Brush)e.OldValue, (Brush)e.NewValue);
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
        /// Gets or sets a RightStroke for the DynamicWaveform.
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
            typeof(DynamicWaveform), new UIPropertyMetadata(0.0d, OnLeftStrokeThicknessChanged, OnCoerceLeftStrokeThickness));

        private static object OnCoerceLeftStrokeThickness(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceLeftStrokeThickness((double)value);
            else
                return value;
        }

        private static void OnLeftStrokeThicknessChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnLeftStrokeThicknessChanged((double)e.OldValue, (double)e.NewValue);
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
        /// Gets or sets a LeftStrokeThickness for the DynamicWaveform.
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
            typeof(DynamicWaveform), new UIPropertyMetadata(0.0d, OnRightStrokeThicknessChanged, OnCoerceRightStrokeThickness));

        private static object OnCoerceRightStrokeThickness(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceRightStrokeThickness((double)value);
            else
                return value;
        }

        private static void OnRightStrokeThicknessChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnRightStrokeThicknessChanged((double)e.OldValue, (double)e.NewValue);
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
        /// Gets or sets a RightStrokeThickness for the DynamicWaveform.
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
            typeof(DynamicWaveform), new UIPropertyMetadata(Brushes.White, OnCenterLineBrushChanged, OnCoerceCenterLineBrush));

        private static object OnCoerceCenterLineBrush(DependencyObject o, object value)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                return DynamicWaveform.OnCoerceCenterLineBrush((Brush)value);
            else
                return value;
        }

        private static void OnCenterLineBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DynamicWaveform DynamicWaveform = o as DynamicWaveform;

            if (DynamicWaveform != null)
                DynamicWaveform.OnCenterLineBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
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
        /// Gets or sets a CenterLineBrush for the DynamicWaveform.
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

        static DynamicWaveform()
        {
            DynamicWaveform.DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicWaveform), new FrameworkPropertyMetadata(typeof(DynamicWaveform)));

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

            double totalTransformScale = GetTotalTransformScale();

            if (waveformCache.RenderAtScale != totalTransformScale)
            {
                waveformCache.RenderAtScale = totalTransformScale;
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

            if (Source == null || waveformCanvas == null || Source.SampleLength == 0 ||
                waveformCanvas.RenderSize.Width < 1 || waveformCanvas.RenderSize.Height < 1)
            {
                return;
            }

            if (wavelengthDataSnapshot == null)
            {
                var start = Source.BytesPerSecond * Source.Position.TotalSeconds;
                wavelengthDataSnapshot = Source.GetDataRange((int)start, (int)(Source.BytesPerSecond * WaveformLength), Resolution);
                cachedSeconds = WaveformLength;
                byteToResolutionRatio = (Source.BytesPerSecond * WaveformLength) / Resolution;
            }
            else
            {
                var timeChange = Source.Position.TotalSeconds - cachedPosition;
                // Since we have progressed in time through the media, we need to draw this in the waveform.
                var start = Source.BytesPerSecond * cachedSeconds;
                var end = (Source.BytesPerSecond * timeChange) + start;
                var length = end - start;
                cachedPosition = Source.Position.TotalSeconds;
                cachedSeconds += timeChange;
                var resolution = (int)(length / byteToResolutionRatio);

                if (resolution > Resolution)
                    resolution = Resolution;

                var timeChangeSnapshot = Source.GetDataRange((int)start, (int)length, resolution);

                if (timeChangeSnapshot != null)
                {
                    // shift the wavelengthDataSnapshot to include the time change.
                    Array.Copy(wavelengthDataSnapshot, resolution, wavelengthDataSnapshot, 0, wavelengthDataSnapshot.Length - resolution);
                    Array.Copy(timeChangeSnapshot, 0, wavelengthDataSnapshot, wavelengthDataSnapshot.Length - resolution, resolution);
                }
            }

            if (wavelengthDataSnapshot == null)
                return;

            double leftRenderHeight;
            double rightRenderHeight;

            int pointCount = wavelengthDataSnapshot.Length / 2;
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

            if (wavelengthDataSnapshot.Length > 1)
            {
                PolyLineSegment leftWaveformPolyLine = new PolyLineSegment();
                leftWaveformPolyLine.Points.Add(new Point(0, centerHeight));

                PolyLineSegment rightWaveformPolyLine = new PolyLineSegment();
                rightWaveformPolyLine.Points.Add(new Point(0, centerHeight));

                double xLocation = 0.0d;

                for (int i = 0; i < wavelengthDataSnapshot.Length; i += 2)
                {
                    xLocation = (i / 2) * pointThickness;
                    leftRenderHeight = ((wavelengthDataSnapshot[i] - minValue) / dbScale) * waveformSideHeight;
                    leftWaveformPolyLine.Points.Add(new Point(xLocation, centerHeight - leftRenderHeight));
                    rightRenderHeight = ((wavelengthDataSnapshot[i + 1] - minValue) / dbScale) * waveformSideHeight;
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
