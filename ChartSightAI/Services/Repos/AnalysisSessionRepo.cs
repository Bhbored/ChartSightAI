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
    public class AnalysisSessionRepo
    {
        private readonly Client _client;
        public AnalysisSessionRepo(Client client) => _client = client;

        public async Task<List<AnalysisSession>> GetByDateRange(Guid userId, DateTime? from = null, DateTime? to = null, int limit = 200)
        {
            var resp = await _client.From<AnalysisSessionRow>()
                                    .Where(x => x.UserId == userId)
                                    .Order(x => x.CreatedAt, Ordering.Descending)
                                    .Get();

            var rows = resp.Models.AsEnumerable();

            if (from.HasValue) rows = rows.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue) rows = rows.Where(r => r.CreatedAt <= to.Value);
            if (limit > 0) rows = rows.Take(limit);

            return rows.Select(m => m.ToDomain()).ToList();
        }

        public async Task<int> InsertAsync(Guid userId, AnalysisSession s)
        {
            var dto = AnalysisSessionRow.FromDomain(s, userId);
            var res = await _client.From<AnalysisSessionRow>().Insert(dto);
            return res.Models.First().Id;
        }

        public async Task<int> UpdateAsync(Guid userId, AnalysisSession s)
        {
            var dto = AnalysisSessionRow.FromDomain(s, userId);
            var res = await _client.From<AnalysisSessionRow>().Update(dto);
            return res.Models.First().Id;
        }

        public Task DeleteAsync(int id) =>
            _client.From<AnalysisSessionRow>()
                   .Where(x => x.Id == id)
                   .Delete();
    }
}
