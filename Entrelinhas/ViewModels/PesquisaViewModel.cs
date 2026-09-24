using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class PesquisaViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly DatabaseService _databaseService;
    public PesquisaViewModel(DatabaseService databaseService, ApiService apiService)
    {
        _databaseService = databaseService;
        _apiService = apiService;
    }




}
