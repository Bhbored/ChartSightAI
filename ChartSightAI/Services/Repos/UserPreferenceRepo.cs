using ChartSightAI.DTO_S.DB;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSightAI.Services.Repos
{
    public class UserPreferenceRepo
    {
        private readonly Client _client;
        public UserPreferenceRepo(Client client) => _client = client;

        public Task UpdateUserNameAsync(Guid userId, string? userName) =>
            _client.From<UserPreferenceRow>()
                   .Where(x => x.UserId == userId)
                   .Set(x => x.UserName, userName ?? "")
                   .Update();
    }
}
