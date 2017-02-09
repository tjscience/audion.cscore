using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Audion.Visualization
{
    [TemplatePart(Name = "PART_Timeline", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_Length", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_ControlContainer")]
    [TemplatePart(Name = "PART_ProgressLine", Type = typeof(Border))]
    public class Timeline : Control
    {
        private Source _source;
        private Grid timelineGrid;
        private Grid controlContainer;
        private Grid lengthGrid;
        private Border progressLine;

        #region Dependency Properties

        #region Source Property

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Source),
            typeof(Timeline), new UIPropertyMetadata(null, OnSourceChanged, OnCoerceSource));

        private static object OnCoerceSource(DependencyObject o, object value)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                return timeline.OnCoerceSource((Source)value);
            else
                return value;
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                timeline.OnSourceChanged((Source)e.OldValue, (Source)e.NewValue);
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
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (Source.Length > TimeSpan.Zero)
                {
                    // The source has data. Show various UI.
                    progressLine.Visibility = Visibility.Visible;
                }
                else
                {
                    // The source does not have data. Hide various UI.
                    progressLine.Visibility = Visibility.Collapsed;
                }

                UpdateTimeline();
            });
        }

        private void _source_SourcePropertyChangedEvent(object sender, SourcePropertyChangedEventArgs e)
        {
            if (e.Property == Source.Property.Position)
            {
                TimeSpan position = (TimeSpan)e.Value;
                Dispatcher.BeginInvoke((Action)delegate
                {
                    var x = 0d;

                    if (_source.Length.TotalMilliseconds != 0)
                    {
                        double progressPercent = position.TotalMilliseconds / _source.Length.TotalMilliseconds;
                        x = progressPercent * lengthGrid.RenderSize.Width;
                    }

                    progressLine.Width = x;
                });
            }
        }

        /// <summary>
        /// Gets or sets a Source for the Timeline.
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

        #region Position Property

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(TimeSpan),
            typeof(Timeline), new UIPropertyMetadata(TimeSpan.Zero, OnPositionChanged, OnCoercePosition));

        private static object OnCoercePosition(DependencyObject o, object value)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                return timeline.OnCoercePosition((TimeSpan)value);
            else
                return value;
        }

        private static void OnPositionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                timeline.OnPositionChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="Position"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="Position"/></param>
        /// <returns>The adjusted value of <see cref="Position"/></returns>
        protected virtual TimeSpan OnCoercePosition(TimeSpan value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="Position"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="Position"/></param>
        /// <param name="newValue">The new value of <see cref="Position"/></param>
        protected virtual void OnPositionChanged(TimeSpan oldValue, TimeSpan newValue)
        {

        }

        /// <summary>
        /// Gets or sets a Position for the Timeline.
        /// </summary>        
        public TimeSpan Position
        {
            get
            {
                return (TimeSpan)GetValue(PositionProperty);
            }
            set
            {
                SetValue(PositionProperty, value);
            }
        }

        #endregion

        #region TickBrush Property

        public static readonly DependencyProperty TickBrushProperty = DependencyProperty.Register("TickBrush", typeof(Brush),
            typeof(Timeline), new UIPropertyMetadata(Brushes.Silver, OnTickBrushChanged, OnCoerceTickBrush));

        private static object OnCoerceTickBrush(DependencyObject o, object value)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                return timeline.OnCoerceTickBrush((Brush)value);
            else
                return value;
        }

        private static void OnTickBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                timeline.OnTickBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="TickBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="TickBrush"/></param>
        /// <returns>The adjusted value of <see cref="TickBrush"/></returns>
        protected virtual Brush OnCoerceTickBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="TickBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="TickBrush"/></param>
        /// <param name="newValue">The new value of <see cref="TickBrush"/></param>
        protected virtual void OnTickBrushChanged(Brush oldValue, Brush newValue)
        {

        }

        /// <summary>
        /// Gets or sets a TickBrush for the Timeline.
        /// </summary>        
        public Brush TickBrush
        {
            get
            {
                return (Brush)GetValue(TickBrushProperty);
            }
            set
            {
                SetValue(TickBrushProperty, value);
            }
        }

        #endregion

        #region TimeBrush Property

        public static readonly DependencyProperty TimeBrushProperty = DependencyProperty.Register("TimeBrush", typeof(Brush),
            typeof(Timeline), new UIPropertyMetadata(Brushes.Silver, OnTimeBrushChanged, OnCoerceTimeBrush));

        private static object OnCoerceTimeBrush(DependencyObject o, object value)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                return timeline.OnCoerceTimeBrush((Brush)value);
            else
                return value;
        }

        private static void OnTimeBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                timeline.OnTimeBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="TimeBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="TimeBrush"/></param>
        /// <returns>The adjusted value of <see cref="TimeBrush"/></returns>
        protected virtual Brush OnCoerceTimeBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="TimeBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="TimeBrush"/></param>
        /// <param name="newValue">The new value of <see cref="TimeBrush"/></param>
        protected virtual void OnTimeBrushChanged(Brush oldValue, Brush newValue)
        {

        }

        /// <summary>
        /// Gets or sets a TimeBrush for the Timeline.
        /// </summary>        
        public Brush TimeBrush
        {
            get
            {
                return (Brush)GetValue(TimeBrushProperty);
            }
            set
            {
                SetValue(TimeBrushProperty, value);
            }
        }

        #endregion

        #region ProgressLineBrush Property

        public static readonly DependencyProperty ProgressLineBrushProperty = DependencyProperty.Register("ProgressLineBrush", typeof(Brush),
            typeof(Timeline), new UIPropertyMetadata(Brushes.Silver, OnProgressLineBrushChanged, OnCoerceProgressLineBrush));

        private static object OnCoerceProgressLineBrush(DependencyObject o, object value)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                return timeline.OnCoerceProgressLineBrush((Brush)value);
            else
                return value;
        }

        private static void OnProgressLineBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                timeline.OnProgressLineBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="ProgressLineBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="ProgressLineBrush"/></param>
        /// <returns>The adjusted value of <see cref="ProgressLineBrush"/></returns>
        protected virtual Brush OnCoerceProgressLineBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="ProgressLineBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="ProgressLineBrush"/></param>
        /// <param name="newValue">The new value of <see cref="ProgressLineBrush"/></param>
        protected virtual void OnProgressLineBrushChanged(Brush oldValue, Brush newValue)
        {
            
        }

        /// <summary>
        /// Gets or sets a ProgressLineBrush for the Timeline.
        /// </summary>        
        public Brush ProgressLineBrush
        {
            get
            {
                return (Brush)GetValue(ProgressLineBrushProperty);
            }
            set
            {
                SetValue(ProgressLineBrushProperty, value);
            }
        }

        #endregion

        #region ProgressBrush Property

        public static readonly DependencyProperty ProgressBrushProperty = DependencyProperty.Register("ProgressBrush", typeof(Brush),
            typeof(Timeline), new UIPropertyMetadata(null, OnProgressBrushChanged, OnCoerceProgressBrush));

        private static object OnCoerceProgressBrush(DependencyObject o, object value)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                return timeline.OnCoerceProgressBrush((Brush)value);
            else
                return value;
        }

        private static void OnProgressBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Timeline timeline = o as Timeline;

            if (timeline != null)
                timeline.OnProgressBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
        }

        /// <summary>
        /// Coerces the value of <see cref="ProgressBrush"/> when a new value is applied.
        /// </summary>
        /// <param name="value">The value that was set on <see cref="ProgressBrush"/></param>
        /// <returns>The adjusted value of <see cref="ProgressBrush"/></returns>
        protected virtual Brush OnCoerceProgressBrush(Brush value)
        {
            return value;
        }

        /// <summary>
        /// Called after the <see cref="ProgressBrush"/> value has changed.
        /// </summary>
        /// <param name="oldValue">The previous value of <see cref="ProgressBrush"/></param>
        /// <param name="newValue">The new value of <see cref="ProgressBrush"/></param>
        protected virtual void OnProgressBrushChanged(Brush oldValue, Brush newValue)
        {

        }

        /// <summary>
        /// Gets or sets a ProgressBrush for the Timeline.
        /// </summary>        
        public Brush ProgressBrush
        {
            get
            {
                return (Brush)GetValue(ProgressBrushProperty);
            }
            set
            {
                SetValue(ProgressBrushProperty, value);
            }
        }

        #endregion

        #endregion

        static Timeline()
        {
            Timeline.DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));

            Application.SetFramerate();
        }

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            timelineGrid = GetTemplateChild("PART_Timeline") as Grid;
            controlContainer = GetTemplateChild("PART_ControlContainer") as Grid;
            lengthGrid = GetTemplateChild("PART_Length") as Grid;
            progressLine = GetTemplateChild("PART_ProgressLine") as Border;
            timelineGrid.CacheMode = new BitmapCache();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateTimeline();
        }

        #endregion

        private void UpdateTimeline()
        {
            if (_source == null || Source.Length == TimeSpan.Zero || lengthGrid.RenderSize.Width < 1 || 
                lengthGrid.RenderSize.Height < 1)
            {
                return;
            }

            lengthGrid.Children.Clear();

            // freeze brushes
            var tickBrush = TickBrush.Clone();
            var timeBrush = TimeBrush.Clone();
            tickBrush.Freeze();
            timeBrush.Freeze();

            // Draw the bottom border
            var bottomBorder = new Border();
            bottomBorder.Background = tickBrush;
            bottomBorder.Height = 1;
            bottomBorder.VerticalAlignment = VerticalAlignment.Bottom;
            bottomBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            lengthGrid.Children.Add(bottomBorder);

            // Determine the number of major ticks that we should display.
            // This depends on the width of the timeline.
            var width = lengthGrid.RenderSize.Width;
            var majorTickCount = Math.Floor(width / 100);
            var totalSeconds = _source.Length.TotalSeconds;
            var majorTickSecondInterval = Math.Floor(totalSeconds / majorTickCount);
            majorTickSecondInterval = Math.Ceiling(majorTickSecondInterval / 10) * 10;
            var minorTickInterval = majorTickSecondInterval / 5;
            var minorTickCount = totalSeconds / minorTickInterval;

            for (var i = 0; i < minorTickCount; i++)
            {
                var interval = i * minorTickInterval;
                double positionPercent = interval / totalSeconds;
                double x = positionPercent * width;

                if (interval % majorTickSecondInterval != 0)
                {
                    // Minor tick
                    var tick = new Border();
                    tick.Width = 1;
                    tick.Height = 7;
                    tick.VerticalAlignment = VerticalAlignment.Bottom;
                    tick.HorizontalAlignment = HorizontalAlignment.Left;
                    tick.Background = tickBrush;
                    tick.Margin = new Thickness(x, 0, 0, 0);
                    lengthGrid.Children.Add(tick);
                }
                else
                {
                    // Major tick
                    var tick = new Border();
                    tick.Width = 1;
                    tick.VerticalAlignment = VerticalAlignment.Stretch;
                    tick.HorizontalAlignment = HorizontalAlignment.Left;
                    tick.Background = tickBrush;
                    tick.Margin = new Thickness(x, 0, 0, 0);
                    lengthGrid.Children.Add(tick);

                    // Add time label
                    var time = new TextBlock();
                    time.VerticalAlignment = VerticalAlignment.Bottom;
                    time.HorizontalAlignment = HorizontalAlignment.Left;
                    time.Foreground = timeBrush;
                    var ts = TimeSpan.FromSeconds(interval);
                    time.Text = ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"mm\:ss");
                    time.Margin = new Thickness(x + 5, 0, 0, 7);
                    lengthGrid.Children.Add(time);
                }
            }
        }
    }
}
