using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class GoogleToken
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; } // foreign key to User table
        public User User { get; set; } // navigation property to User table
    }
}
