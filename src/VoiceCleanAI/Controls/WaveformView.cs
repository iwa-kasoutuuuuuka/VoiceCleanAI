using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI;
using System.Collections.Generic;

namespace VoiceCleanAI.Controls;

public sealed class WaveformView : Control
{
    private Canvas? _canvas;

    public static readonly DependencyProperty WaveformDataProperty =
        DependencyProperty.Register(nameof(WaveformData), typeof(float[]), typeof(WaveformView), new PropertyMetadata(null, OnWaveformDataChanged));

    public float[]? WaveformData
    {
        get => (float[]?)GetValue(WaveformDataProperty);
        set => SetValue(WaveformDataProperty, value);
    }

    public WaveformView()
    {
        this.DefaultStyleKey = typeof(WaveformView);
        this.Loaded += OnLoaded;
        this.SizeChanged += OnSizeChanged;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _canvas = GetTemplateChild("PART_Canvas") as Canvas;
        DrawWaveform();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => DrawWaveform();
    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => DrawWaveform();

    private static void OnWaveformDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WaveformView view)
        {
            view.DrawWaveform();
        }
    }

    private void DrawWaveform()
    {
        if (_canvas == null || WaveformData == null || WaveformData.Length == 0 || ActualWidth == 0 || ActualHeight == 0)
            return;

        _canvas.Children.Clear();

        double width = ActualWidth;
        double height = ActualHeight;
        double midY = height / 2;
        int pointCount = WaveformData.Length;
        double step = width / pointCount;

        var brush = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["SystemAccentColor"]);

        for (int i = 0; i < pointCount; i++)
        {
            double value = WaveformData[i];
            double rectHeight = Math.Max(2, value * height);
            
            var rect = new Rectangle
            {
                Width = Math.Max(1, step - 1),
                Height = rectHeight,
                Fill = brush,
                RadiusX = 1,
                RadiusY = 1
            };

            Canvas.SetLeft(rect, i * step);
            Canvas.SetTop(rect, midY - (rectHeight / 2));
            _canvas.Children.Add(rect);
        }
    }
}
