namespace Entrelinhas.Views;

public partial class SobrePage : ContentPage
{
    public SobrePage()
    {
        InitializeComponent();
        lbVersao.Text = $"versão {AppInfo.VersionString}";
    }

    private async void OnVoltarClicado(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}