using Entrelinhas.Views;

namespace Entrelinhas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("addlivro", typeof(AddLivroPage));
            Routing.RegisterRoute("detalhelivro", typeof(BookPage));
        }
    }
}
