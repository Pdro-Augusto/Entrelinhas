using ZXing.Net.Maui;

namespace Entrelinhas.Views;

public partial class ScannerPage : ContentPage
{
	public ScannerPage()
	{
		InitializeComponent();

        cameraBarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional, // Foca em códigos 1D (ISBN)
            AutoRotate = true,
            Multiple = false
        };

        Unloaded += (sender, e) => { cameraBarcodeReaderView.Handler?.DisconnectHandler(); };
    }

    private void BarcodeDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        
        var barcode = e.Results.FirstOrDefault();

        if (barcode != null)
        {
            // Aqui você pode processar o código de barras detectado, por exemplo, exibir em um Label ou fazer uma busca.
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Para o scanner enquanto navega
                cameraBarcodeReaderView.IsDetecting = false;

                // Navega para a página de detalhes, passando o código lido
                await Shell.Current.GoToAsync($"addlivro?isbn={barcode.Value}");
            });
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Garante que a câmera está ligada quando a página aparece
        if (cameraBarcodeReaderView != null)
        {
            cameraBarcodeReaderView.IsDetecting = true;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Desliga a detecção quando você sai da página
        if (cameraBarcodeReaderView != null)
        {
            cameraBarcodeReaderView.IsDetecting = false;
        }
    }
}