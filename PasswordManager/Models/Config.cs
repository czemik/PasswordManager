using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Models
{
    internal class Config
    {
        public string? WorkDir { get; set; } 

        public User? LoggedInUser { get; set; }

        public static string ConfigPath { get; } = AppDomain.CurrentDomain.BaseDirectory + "../../../config.json";

        private static Config? instance = null;
        private Config() { }

        public static Config Instance { 
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }
                return instance;
            }
        }
    }
}
