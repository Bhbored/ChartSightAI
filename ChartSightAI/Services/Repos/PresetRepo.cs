using ChartSightAI.DTO_S.DB;
using ChartSightAI.MVVM.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace ChartSightAI.Services.Repos
{
    public class PresetRepo
    {
        private readonly Client _client;
        public PresetRepo(Client client) => _client = client;

        public async Task<List<Preset>> GetAllAsync(Guid userId)
        {
            var resp = await _client.From<PresetRow>()
                                    .Where(x => x.UserId == userId)
                                    .Order(x => x.CreatedAt, Ordering.Descending)
                                    .Get();
            return resp.Models.Select(m => m.ToDomain()).ToList();
        }

        public async Task<int> InsertAsync(Guid userId, Preset p)
        {
            var dto = PresetRow.FromDomain(p, userId);
            var res = await _client.From<PresetRow>().Insert(dto);
            return res.Models.First().Id;
        }

        public async Task<int> UpdateAsync(Guid userId, Preset p)
        {
            var dto = PresetRow.FromDomain(p, userId);
            var res = await _client.From<PresetRow>().Update(dto);
            return res.Models.First().Id;
        }

        public Task DeleteAsync(int id) =>
            _client.From<PresetRow>()
                   .Where(x => x.Id == id)
                   .Delete();
    }
}
