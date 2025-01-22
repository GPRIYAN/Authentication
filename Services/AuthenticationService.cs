using System;
using System.Threading.Tasks;
using Supabase;
using WiseMaestroRBAC.Models;

namespace WiseMaestroRBAC.Services
{
    public class AuthenticationService
    {
        private readonly Supabase.Client _client;

        public AuthenticationService(Supabase.Client client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<string> RegisterAsync(RegisterModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    throw new ArgumentException("Email and Password cannot be empty.");
                }

                // First, create the auth user
                var session = await _client.Auth.SignUp(model.Email, model.Password);

                if (session?.AccessToken == null)
                {
                    throw new Exception("Registration failed. Please try again.");
                }

                // then store additional user data
                var userModel = new UserModel
                {
                    Id = session.User?.Id, // Link to auth.users
                    Username = model.Username,
                    Role = model.Role,
                    Email = model.Email,
                    Created_at = DateTime.UtcNow
                };

                await _client.From<UserModel>()
                .Insert(userModel);

                return session.AccessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Registration failed: {ex.Message}");
            }
        }

        public async Task<string> LoginAsync(LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                throw new ArgumentException("Email and Password cannot be empty.");
            }

            var session = await _client.Auth.SignInWithPassword(model.Email, model.Password);

            if (session?.AccessToken == null)
            {
                throw new Exception("Login failed. Please check your credentials.");
            }

            return session.AccessToken;
        }
    }
}
