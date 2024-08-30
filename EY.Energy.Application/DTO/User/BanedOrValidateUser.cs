using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO.User
{
    public class BanedOrValidateUser
    {
        [Required(ErrorMessage = "User  is required")]
        public string Username { get; set; } = string.Empty;
    }
}
