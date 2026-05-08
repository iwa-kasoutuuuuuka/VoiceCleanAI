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

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
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

    private void CreateBars()
    {
        this.Children.Clear();
        _bars.Clear();

        double width = this.ActualWidth;
        double height = this.ActualHeight;
        if (width <= 0 || height <= 0) return;

        int barCount = (int)(width / 6);
        for (int i = 0; i < barCount; i++)
        {
            var bar = new Rectangle
            {
                Width = 4,
                Height = height * 0.2,
                Fill = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"],
                RadiusX = 2,
                RadiusY = 2,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            Canvas.SetLeft(bar, i * 6);
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
        foreach (var bar in _bars)
        {
            bar.Height = this.ActualHeight * 0.2;
            Canvas.SetTop(bar, (this.ActualHeight - bar.Height) / 2);
        }
    }

    private void UpdateBars()
    {
        double height = this.ActualHeight;
        foreach (var bar in _bars)
        {
            double targetHeight = height * (0.1 + _random.NextDouble() * 0.8);
            bar.Height = Math.Max(4, targetHeight);
            Canvas.SetTop(bar, (height - bar.Height) / 2);
        }
    }
}
