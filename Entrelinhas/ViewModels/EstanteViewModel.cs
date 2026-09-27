using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class EstanteViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    public EstanteViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    
}
