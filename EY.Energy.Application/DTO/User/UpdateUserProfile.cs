using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO.User
{
    public class UpdateUserProfile
    {
        [Required(ErrorMessage = "Le numéro du télephone est requis")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 digits long")]
        public string Phone { get; set; } = string.Empty;
    }
}
