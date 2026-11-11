namespace Entrelinhas.Controls;

public class MarqueeLabel : ScrollView
{
    private const string AnimName = "marquee";
    private const double Velocidade = 30;      // dp por segundo
    private const int PausaMs = 1500;

    private readonly Label _label;
    private CancellationTokenSource? _cts;

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(MarqueeLabel), null,
            propertyChanged: (b, _, n) => ((MarqueeLabel)b)._label.Text = n as string);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MarqueeLabel), null,
            propertyChanged: (b, _, n) => ((MarqueeLabel)b)._label.TextColor = (Color)n);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(MarqueeLabel), null,
            propertyChanged: (b, _, n) => ((MarqueeLabel)b)._label.FontFamily = n as string);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(MarqueeLabel), 14.0,
            propertyChanged: (b, _, n) => ((MarqueeLabel)b)._label.FontSize = (double)n);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(MarqueeLabel), FontAttributes.None,
            propertyChanged: (b, _, n) => ((MarqueeLabel)b)._label.FontAttributes = (FontAttributes)n);

    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Color? TextColor { get => (Color?)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public string? FontFamily { get => (string?)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public FontAttributes FontAttributes { get => (FontAttributes)GetValue(FontAttributesProperty); set => SetValue(FontAttributesProperty, value); }

    public MarqueeLabel()
    {
        Orientation = ScrollOrientation.Horizontal;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
        InputTransparent = true;

        _label = new Label { LineBreakMode = LineBreakMode.NoWrap };
        Content = _label;

        SizeChanged += (_, _) => Reiniciar();
        _label.SizeChanged += (_, _) => Reiniciar();
        Unloaded += (_, _) => Parar();
        Loaded += (_, _) => Reiniciar();
    }

    private void Parar()
    {
        _cts?.Cancel();
        _cts = null;
        this.AbortAnimation(AnimName);
        _label.TranslationX = 0;
    }

    private void Reiniciar()
    {
        Parar();

        if (Width <= 0 || _label.Width <= Width + 1) return;   // cabe: fica parado

        _cts = new CancellationTokenSource();
        _ = LoopAsync(_label.Width - Width, _cts.Token);
    }

    private async Task LoopAsync(double excesso, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(PausaMs, ct);
                await AnimarAsync(excesso, ct);
                await Task.Delay(PausaMs, ct);
                _label.TranslationX = 0;
            }
        }
        catch (OperationCanceledException) { }
    }

    private Task AnimarAsync(double excesso, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource();
        uint duracao = (uint)(excesso / Velocidade * 1000);

        new Animation(v => _label.TranslationX = -v, 0, excesso)
            .Commit(this, AnimName, 16, duracao, Easing.Linear,
                    (_, _) => tcs.TrySetResult());

        ct.Register(() => tcs.TrySetResult());
        return tcs.Task;
    }
}