using ChartSightAI.DTO_S.DB;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;
using Client = Supabase.Client;
using Preferences = ChartSightAI.MVVM.Models.Preferences;

public class UserPreferenceRepo
{
    private readonly Client _client;
    public UserPreferenceRepo(Client client) => _client = client;

    public async Task<Preferences> GetAsync(Guid userId)
    {
        var resp = await _client.From<UserPreferenceRow>()
                                    .Where(x => x.UserId == userId)
                                    .Get();
        var preferenceRow = resp.Models.FirstOrDefault();
        return preferenceRow!.ToDomain();
    }

    public Task UpdateUserNameAsync(Guid userId, string? userName) =>
     _client.From<UserPreferenceRow>()
            .Filter(nameof(UserPreferenceRow.UserId), Constants.Operator.Equals, userId)
            .Set(x => x.UserName!, userName ?? "")
            .Update();

    public Task UpsertUserNameAsync(Guid userId, string? userName) =>
        _client.From<UserPreferenceRow>()
               .Upsert(new UserPreferenceRow
               {
                   UserId = userId,
                   UserName = userName ?? ""
               });

}
