using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using PasswordManager.Models;

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

        rootCommand.SetHandler(async (workdir) => await SetWorkdir(workdir), workDirOption);

        registerCommand.SetHandler(async (username, password, email, firstname, lastname) => 
        {
            await RegisterUser(username, password, email, firstname, lastname);
        }, usernameOption, passwordOption, emailOption, firstnameOption, lastnameOption);









        return await rootCommand.InvokeAsync(args);
    }

    internal static async Task SetWorkdir(string Workdir)
    {
        if (Workdir != null)
        {
            User.UserCsvPath = Workdir + "user.csv";
            Vault.VaultCsvPath = Workdir + "vault.csv";
            Console.WriteLine("Workdir set");
        }
    }

    internal static async Task RegisterUser(string username, string password, string email, string firstname, string lastname)
    {
        using (StreamWriter writer = new(User.UserCsvPath, append: true))
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            await using CsvWriter csv = new(writer, config);
            csv.WriteRecords(new User[]
            {
                    new User()
                    {
                        Username= username,
                        Password= password,
                        Email= email,
                        Firstname= firstname,
                        Lastname= lastname
                    }
            });
            Console.WriteLine("User registered!");
        }
    }
}