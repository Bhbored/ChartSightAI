using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Preferences = ChartSightAI.MVVM.Models.Preferences;
namespace ChartSightAI.DTO_S.DB
{
    [Table("user_preferences")]
    public sealed class UserPreferenceRow : BaseDto
    {
    
        [Column("user_name")]
        public string? UserName { get; set; }
        public Preferences ToDomain() => new()
        {
            Id = Id,
            UserName = UserName
        };

        public static UserPreferenceRow FromDomain(Preferences p, Guid userId) => new()
        {
            Id = p.Id,                
            UserId = userId,
            UserName = p.UserName
        };
    }
}
