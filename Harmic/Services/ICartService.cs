using Harmic.Models;
using System.Threading.Tasks;

namespace Harmic.Services
{
    public interface ICartService
    {
        Task<TbOrder> GetOrCreateCartAsync();
        Task<TbOrder?> GetCartAsync();
        Task AddItemAsync(int productId, int quantity = 1);
        Task UpdateItemAsync(int productId, int quantity);
        Task RemoveItemAsync(int productId);
        Task ClearAsync();
        Task<int> GetItemCountAsync();
        Task MergeSessionCartToUserAsync(int accountId);
        Task<bool> CheckoutAsync(string customerName, string phone, string address);
    }
}