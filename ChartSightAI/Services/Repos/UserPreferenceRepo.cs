using System;
using System.Linq;
using System.Threading.Tasks;
using ChartSightAI.DTO_S.DB;
using Supabase;
using Supabase.Postgrest;
using Client = Supabase.Client;

public class UserPreferenceRepo
{
    private readonly Client _client;
    public UserPreferenceRepo(Client client) => _client = client;

    public async Task<UserPreferenceRow?> GetAsync(Guid userId)
    {
        var resp = await _client.From<UserPreferenceRow>()
            .Filter(nameof(UserPreferenceRow.UserId), Constants.Operator.Equals, userId)
            .Get();
        return resp.Models.FirstOrDefault();
    }

    public Task UpdateUserNameAsync(Guid userId, string? userName) =>
        _client.From<UserPreferenceRow>()
               .Filter(nameof(UserPreferenceRow.UserId), Constants.Operator.Equals, userId)
               .Update(new UserPreferenceRow { UserName = userName ?? "" });

    public Task UpsertUserNameAsync(Guid userId, string? userName) =>
        _client.From<UserPreferenceRow>()
               .Upsert(new UserPreferenceRow { UserId = userId, UserName = userName ?? "" });
}
