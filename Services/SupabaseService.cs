using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;
using WiseMaestroRBAC.Models;
using static Supabase.Postgrest.Constants;

namespace WiseMaestroRBAC.Services
{
    public class SupabaseService
    {
        private readonly Client _client;

        public SupabaseService(Client client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<UserModel?> CreateUserAsync(UserModel user)
        {
            try
            {
                var response = await _client
                .From<UserModel>()
                .Insert(user);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                throw;
            }
        }
        public async Task<UserModel?> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _client
                    .From<UserModel>()
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                    .Get();
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
                throw;
            }
        }
        public async Task InitializeAsync()
        {
            try
            {
                await _client.InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Supabase client: {ex.Message}");
                throw;
            }
        }

        public async Task CreateAsync(UserModel userModel)
        {
            try
            {
                await _client.From<UserModel>().Insert(userModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating weather data: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(UserModel userModel)
        {
            try
            {
                await _client.From<UserModel>().Update(userModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating weather data: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _client.From<UserModel>().Filter("Id", Operator.Equals, id).Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting weather data: {ex.Message}");
                throw;
            }
        }
    }
}
