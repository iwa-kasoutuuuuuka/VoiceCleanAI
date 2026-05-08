using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;

namespace VoiceCleanAI.Controls;

public class WaveformView : Canvas
{
    private readonly Random _random = new Random();
    private readonly List<Rectangle> _bars = new List<Rectangle>();
    private DispatcherTimer? _timer;
    private bool _isAnimating;

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register("IsActive", typeof(bool), typeof(WaveformView), new PropertyMetadata(false, OnIsActiveChanged));

    public static readonly DependencyProperty WaveformDataProperty =
        DependencyProperty.Register("WaveformData", typeof(float[]), typeof(WaveformView), new PropertyMetadata(null, OnWaveformDataChanged));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public float[]? WaveformData
    {
        get => (float[]?)GetValue(WaveformDataProperty);
        set => SetValue(WaveformDataProperty, value);
    }

    public WaveformView()
    {
        this.Loaded += WaveformView_Loaded;
        this.SizeChanged += WaveformView_SizeChanged;
    }

    private void WaveformView_Loaded(object sender, RoutedEventArgs e)
    {
        CreateBars();
    }

    private void WaveformView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        CreateBars();
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (WaveformView)d;
        if ((bool)e.NewValue) control.StartAnimation();
        else control.StopAnimation();
    }

    private static void OnWaveformDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (WaveformView)d;
        control.CreateBars();
    }

    private void CreateBars()
    {
        this.Children.Clear();
        _bars.Clear();

        double width = this.ActualWidth;
        double height = this.ActualHeight;
        if (width <= 0 || height <= 0) return;

        float[]? data = WaveformData;
        int barCount = (data != null && data.Length > 0) ? data.Length : (int)(width / 6);
        double barWidth = Math.Max(2, (width / barCount) - 2);

        for (int i = 0; i < barCount; i++)
        {
            float val = (data != null && i < data.Length) ? data[i] : 0.2f;
            
            var bar = new Rectangle
            {
                Width = barWidth,
                Height = Math.Max(2, height * val),
                Fill = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"],
                RadiusX = barWidth / 2,
                RadiusY = barWidth / 2,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = (data != null) ? 1.0 : 0.3
            };
            
            Canvas.SetLeft(bar, i * (barWidth + 2));
            Canvas.SetTop(bar, (height - bar.Height) / 2);
            
            this.Children.Add(bar);
            _bars.Add(bar);
        }
    }

    public void StartAnimation()
    {
        if (_isAnimating) return;
        _isAnimating = true;

        if (_timer == null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += (s, e) => UpdateBars();
        }
        _timer.Start();
    }

    public void StopAnimation()
    {
        _isAnimating = false;
        _timer?.Stop();
        CreateBars(); // Reset to static waveform
    }

    private void UpdateBars()
    {
        double height = this.ActualHeight;
        float[]? data = WaveformData;

        for (int i = 0; i < _bars.Count; i++)
        {
            var bar = _bars[i];
            double baseVal = (data != null && i < data.Length) ? data[i] : 0.2;
            double targetHeight = height * (baseVal * (0.5 + _random.NextDouble() * 1.0));
            
            bar.Height = Math.Max(2, Math.Min(height, targetHeight));
            Canvas.SetTop(bar, (height - bar.Height) / 2);
        }
    }
}
