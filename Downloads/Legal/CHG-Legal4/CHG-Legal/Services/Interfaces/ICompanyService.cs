using CHG_Legal.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CHG_Legal.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyViewModel>> GetAllCompaniesAsync();
        Task<CompanyViewModel> GetCompanyByIdAsync(int id);
        Task<CompanyViewModel> CreateCompanyAsync(CompanyViewModel model);
        Task<CompanyViewModel> UpdateCompanyAsync(CompanyViewModel model);

        Task<bool> DeleteCompanyAsync(int id);
        Task<double?> GetCurrentShareValueAsync(); 
        Task<List<string>> GetBankGroupsAsync();

    }
}