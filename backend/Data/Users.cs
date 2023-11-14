using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("Users")]
    public class Users : IdentityUser
    {
        public int Age { get; set; }

        public int Score { get; set; }
    }
}
