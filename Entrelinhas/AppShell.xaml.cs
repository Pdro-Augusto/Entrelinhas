using Entrelinhas.Views;

namespace Entrelinhas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("detalhelivro", typeof(BookPage));
        }
    }
}
