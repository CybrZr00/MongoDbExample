using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Models
{
    public class ApplicationUser : IdentityUser
    {
        private string email;
        public string DisplayName { get; set; } = "Default Name";
        public string GravitarUrl { get; private set; }
        [EmailAddress]
        public override string Email
        {
            get => email;
            set
            {
                email = value;
                if (!string.IsNullOrEmpty(value))
                {
                    GravitarUrl = $"https://www.gravatar.com/avatar/{Hashing.CalculateMd5Hash(value)}?s={128}&d=identicon&r=PG";
                }
            }
        }
        [NotMapped]
        public List<string> Roles { get; set; } = new List<string>();
        [NotMapped]
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
    /// <summary>
    /// Used to calculate the hash
    /// </summary>
    public static class Hashing
    {
        /// <summary>
        ///     Calculate the hash based on this MSDN post:
        ///     http://blogs.msdn.com/b/csharpfaq/archive/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string_3f00_.aspx
        /// </summary>
        /// <param name="input">The input to hash</param>
        /// <returns>The hashed and lowered string</returns>
        public static string CalculateMd5Hash(string input)
        {
            if (input == null)
                throw new InvalidOperationException("The input parameter is required.");
            // step 1, calculate MD5 hash from input
#pragma warning disable SCS0006 // Weak hashing function.
            using var md5 = MD5.Create();
#pragma warning restore SCS0006 // Weak hashing function.
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var t in hash) sb.Append(t.ToString("X2"));
            return sb.ToString().ToLower();
        }
    }
    public class ApplicationUserCreateVM
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Claims { get; set; } = new List<string>();
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
    public class ApplicationUserEditVM
    {
        [Required]
        public string Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Enable User")]
        public bool EnableUser { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Claims { get; set; } = new List<string>();

    }
}
