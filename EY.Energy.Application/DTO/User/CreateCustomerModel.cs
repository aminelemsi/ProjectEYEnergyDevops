using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO.User
{
    public class CreateCustomerModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "The email is in the wrong format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le numéro du télephone est requis")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 digits long")]
        public string Phone { get; set; } = string.Empty;



        [Required(ErrorMessage = "NameCompany is required")]
        [MinLength(2, ErrorMessage = "The NameCompany must be at least 6 characters long")]
        public string NameCompany { get; set; } = string.Empty;

        



    }
}
