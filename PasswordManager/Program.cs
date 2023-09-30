using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using PasswordManager.Models;
using PasswordManager;

namespace scl;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var workDirOption = new Option<string>(
            name: "--workdir",
            description: "Sets the working directory for the application");

        var usernameOption = new Option<string>(
            name: "--username",
            description: "Username");

        var passwordOption = new Option<string>(
            name: "--password",
            description: "Password");

        var emailOption = new Option<string>(
            name: "--email",
            description: "Email");

        var firstnameOption = new Option<string>(
            name: "--firstname",
            description: "Firstname");

        var lastnameOption = new Option<string>(
            name: "--lastname",
            description: "Lastname");
        
        var websiteOption = new Option<string>(
            name: "--website",
            description: "Website");

        var registerCommand = new Command("register", "Registers the user")
        {
            usernameOption,
            passwordOption,
            emailOption,
            firstnameOption,
            lastnameOption,
        };

        var loginCommand = new Command("login", "Logs in the user")
        {
            usernameOption,
            passwordOption,
        };

        var listCommand = new Command("list", "Lists the users saved passwords");

        var addCommand = new Command("add", "Adds a new password to db")
        {
            usernameOption,
            passwordOption,
            websiteOption
        };

        var rootCommand = new RootCommand("Sample app for System.CommandLine")
        {
            workDirOption,
            registerCommand,
            loginCommand,
            listCommand,
            addCommand,
        };

        rootCommand.SetHandler((workdir) => SetWorkdir(workdir), workDirOption);

        registerCommand.SetHandler((username, password, email, firstname, lastname) => Register(username, password, email, firstname, lastname), usernameOption, passwordOption, emailOption, firstnameOption, lastnameOption);

        loginCommand.SetHandler((username, password) => Login(username, password), usernameOption, passwordOption);

        




        return await rootCommand.InvokeAsync(args);
    }

    internal static void SetWorkdir(string Workdir)
    {
        if (Workdir != null && Workdir != "")
        {
            User.UserCsvPath = Workdir + "user.csv";
            Vault.VaultCsvPath = Workdir + "vault.csv";
            Console.WriteLine("Workdir set");
        }
    }

    internal static void Register(string username, string password, string email, string? firstname, string? lastname)
    {
        if (username == null)
        {
            Console.WriteLine("Username must be given");
            return;
        }

        if (password == null)
        {
            Console.WriteLine("Password must be given");
            return;
        }

        if (email == null)
        {
            Console.WriteLine("Email must be given");
            return;
        }

        using (StreamReader reader = new(User.UserCsvPath))
        {
            using CsvReader csv = new(
                reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<User>().ToList();

            foreach (var record in records)
            {
                if (record.Username == username)
                {
                    Console.WriteLine("Username already in use");
                    return;
                } else if (record.Email == email)
                {
                    Console.WriteLine("Email already in use");
                    return;
                }
            }
        }

        using (StreamWriter writer = new(User.UserCsvPath, append: true))
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            using CsvWriter csv = new(writer, config);
            csv.WriteRecords(new User[]
            {
                    new User()
                    {
                        Username = username,
                        Password = Encrypter.Encrypt(email, password),
                        Email = email,
                        Firstname = firstname,
                        Lastname = lastname
                    }
            });
            Console.WriteLine("User registered!");
        }
    }

    internal static void Login(string username, string password)
    {
        if(username == null && username != "")
        {
            Console.WriteLine("Username must be given");
            return;
        }

        if (password == null && password != "")
        {
            Console.WriteLine("Password must be given");
            return;
        }

        using (StreamReader reader = new(User.UserCsvPath))
        {
            using CsvReader csv = new(
                reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<User>().ToList();
            
            foreach (var record in records)
            {
                if (record.Username == username && password == Encrypter.Decrypt(record.Email!, record.Password!)) 
                {
                    User.loggedInUser = record;
                    Console.WriteLine("Successfuly logged in");
                    return;
                }
            }
            Console.WriteLine("Username and password didn't match");
        }
    }
}