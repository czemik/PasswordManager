using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Models
{
    internal class User
    {
        public static string UserCsvPath { get; set; } = "../resources/db/user.csv";

        [Name("username")]
        public string? Username { get; set; }

        [Name("password")]
        public string? Password { get; set; }

        [Name("email")]
        public string? Email { get; set; }

        [Name("firstname")]
        public string? Firstname { get; set; }

        [Name("lastname")]
        public string? Lastname { get; set; }
    }
}
