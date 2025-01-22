using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace WiseMaestroRBAC.Models
{
    [Table("users")]
    public class UserModel : BaseModel
    {
        [Column("id")]
        [PrimaryKey("id")]
        public string? Id { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("role")]
        public string? Role {  get; set; }

        [Column("created_at")]
        public DateTime Created_at { get; set; }
    }
}
