using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class EstanteViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly DatabaseService _databaseService;
    public EstanteViewModel(DatabaseService databaseService, ApiService apiService)
    {
        _databaseService = databaseService;
        _apiService = apiService;
    }

    

    
}
