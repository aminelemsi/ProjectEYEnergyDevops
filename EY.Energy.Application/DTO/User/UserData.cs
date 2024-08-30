using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO.User
{
    public class UserData
    {
        public string UserId { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}
