namespace Entrelinhas.Views;

public partial class EstantePage : ContentPage
{
	public EstantePage()
	{
		InitializeComponent();
	}

    private Button latestButtonClicked = null;

    private void ResetLastButton()
    {
        if(latestButtonClicked != null)
        {
            latestButtonClicked.BackgroundColor = Color.FromArgb("#10D85A30");
            latestButtonClicked.TextColor = Color.FromArgb("#4A1B0C");
        }
    }

    private void btAllBooks_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btTodos = (Button)sender;
        btTodos.BackgroundColor = Color.FromArgb("#D85A30");
        btTodos.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btTodos;
    }

    private void btReading_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLendo = (Button)sender;
        btLendo.BackgroundColor = Color.FromArgb("#D85A30");
        btLendo.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLendo;
    }

    private void btRead_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLido = (Button)sender;
        btLido.BackgroundColor = Color.FromArgb("#D85A30");
        btLido.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLido;
    }

    private void btAddNewBook_Clicked(object sender, EventArgs e)
    {

    }
}