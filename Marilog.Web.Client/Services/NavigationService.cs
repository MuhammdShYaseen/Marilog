using Marilog.Web.Client.Common;
using Marilog.Web.Client.Models;
using System.Net.Http.Json;

namespace Marilog.Web.Client.Services
{
    public class NavigationService
    {
        private readonly HttpClient _http;

        public NavigationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<NavItemVm>> GetAsync()
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<NavItemVm>>>("api/navigation");
            return res?.Data ?? new();
        }
    }
}
