using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO.User
{
    public class AssignRoleModel
    {

        [Required(ErrorMessage = "User  is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;
    }
}
