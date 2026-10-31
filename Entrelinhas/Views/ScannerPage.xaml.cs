using ZXing.Net.Maui;

namespace Entrelinhas.Views;

public partial class ScannerPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    public ScannerPage(IServiceProvider serviceProvider)
	{
		InitializeComponent();
        _serviceProvider = serviceProvider;

        cameraBarcodeReaderView?.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional, // Foca em códigos 1D (ISBN)
            AutoRotate = true,
            Multiple = false,
            CameraResolutionSelector = availableResolutions => availableResolutions
              .OrderBy(resolution => Math.Abs((resolution.Width * resolution.Height) - (1280 * 720)))
              .ThenBy(resolution => Math.Abs(resolution.Width - 1280) + Math.Abs(resolution.Height - 720))
              .First()
        };
    }

    private void BarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
            var barcode = e.Results.FirstOrDefault();

        if (barcode != null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var addLivroPage = _serviceProvider.GetRequiredService<AddLivroPage>();

                addLivroPage.InicializarComIsbn(barcode.Value);
                await Shell.Current.Navigation.PushAsync(addLivroPage);
            });
        }
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync("//estante");
        });

        // Retorna true para avisar ao Android que você já tratou o evento
        // e evitar que o aplicativo seja fechado.
        return true;
    }
}