using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Models
{
    internal class Vault
    {
        public static string VaultCsvPath { get; set; } = "../../../../resources/db/vault.csv";

        [Name("user_id")]
        public string? UserId { get; set; }

        [Name("username")]
        public string? Username { get; set; } = string.Empty;

        [Name("password")]
        public string? Password { get; set; }

        [Name("website")]
        public string? Website { get; set; }

        [Ignore]
        public User? User
        {
            get
            {
                if (User.UserCsvPath == null) return null;
                using StreamReader reader = new(VaultCsvPath);
                using CsvReader csv = new(
                    reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<User>()
                    .Where(item => item.Username == UserId)
                    .FirstOrDefault();
            }
        }
    }
}
