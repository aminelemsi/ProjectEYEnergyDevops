using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO.Chat
{
    public class UserRoomConnection
    {
        public string User { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public List<string> InvitedUsers { get; set; } = new List<string>();
    }
}
