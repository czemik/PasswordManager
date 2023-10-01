using CsvHelper.Configuration;
using CsvHelper;
using System.CommandLine;
using System.Globalization;
using PasswordManager.Models;
using PasswordManager;
using Newtonsoft.Json;

using static Newtonsoft.Json.JsonConvert;
using Newtonsoft.Json.Linq;

namespace scl;

class Program
{
    static async Task<int> Main(string[] args)
    {
        string text = File.ReadAllText(Config.ConfigPath);
        var config = JsonConvert.DeserializeObject<dynamic>(text);
        
        Config.Instance.WorkDir = config!.WorkDir;
        Config.Instance.LoggedInUser = config!.LoggedInUser.ToObject<User>();

        User.UserCsvPath = Config.Instance.WorkDir + "user.csv";
        Vault.VaultCsvPath = Config.Instance.WorkDir + "vault.csv";
        User.loggedInUser = Config.Instance.LoggedInUser;

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

        var logOutCommand = new Command("logout", "Log the user out");

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
            logOutCommand,
            loginCommand,
            listCommand,
            addCommand
        };

        rootCommand.SetHandler((workdir) => SetWorkdir(workdir), workDirOption);

        registerCommand.SetHandler((username, password, email, firstname, lastname) => Register(username, password, email, firstname, lastname), usernameOption, passwordOption, emailOption, firstnameOption, lastnameOption);
        
        logOutCommand.SetHandler(() => Logout());

        loginCommand.SetHandler((username, password) => Login(username, password), usernameOption, passwordOption);

        listCommand.SetHandler(() => List());

        addCommand.SetHandler((username, password, website) => Add(username, password, website), usernameOption, passwordOption, websiteOption);

        return await rootCommand.InvokeAsync(args);
    }

    internal static void SetWorkdir(string Workdir)
    {
        if (Workdir != null && Workdir != "")
        {
            User.UserCsvPath = Workdir + "user.csv";
            Vault.VaultCsvPath = Workdir + "vault.csv";

            Config.Instance.WorkDir = Workdir;
            var jsonString = JsonConvert.SerializeObject(Config.Instance);
            File.WriteAllText(Config.ConfigPath, jsonString);

            Console.WriteLine($"Workdir set to {Workdir}");
        }
    }

    internal static void Logout()
    {
        User.loggedInUser = null;
        Config.Instance.LoggedInUser = null;
        var jsonString = JsonConvert.SerializeObject(Config.Instance);
        File.WriteAllText(Config.ConfigPath, jsonString);
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
        try
        {
            using (StreamReader reader = new(User.UserCsvPath))
            {
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<User>().ToList();

                foreach (var record in records)
                {
                    if (record.Username == username)
                    {
                        Console.WriteLine("Username already in use");
                        return;
                    }
                    else if (record.Email == email)
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
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("Your workdir is not set correctly!");
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
        try
        {
            using (StreamReader reader = new(User.UserCsvPath))
            {
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<User>().ToList();

                foreach (var record in records)
                {
                    if (record.Username == username && password == Encrypter.Decrypt(record.Email!, record.Password!))
                    {
                        User.loggedInUser = record;
                        Config.Instance.LoggedInUser = record;

                        var jsonString = JsonConvert.SerializeObject(Config.Instance);
                        File.WriteAllText(Config.ConfigPath, jsonString);

                        Console.WriteLine("Successfuly logged in");
                        return;
                    }
                }
                Console.WriteLine("Username and password didn't match");
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("Your workdir is not set correctly!");
        }
    }

    internal static void List()
    {
        if (User.loggedInUser == null)
        {
            Console.WriteLine("You have to login first, use the login command beforehand");
            return;
        }
        try
        {


            using (StreamReader reader = new(Vault.VaultCsvPath))
            {
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<Vault>().Where(item => item.UserId == User.loggedInUser.Username).ToList();

                Console.WriteLine("Username\tPassword\tWebsite\t");
                foreach (var record in records)
                {
                    Console.WriteLine($"{record.Username}\t{Encrypter.Decrypt(record.User!.Email!, record.Password!)}\t{record.Website}\t");
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("Your workdir is not set correctly!");
        }
    }

    internal static void Add(string username, string password, string website)
    {
        if (User.loggedInUser == null)
        {
            Console.WriteLine("You have to login first, use the login command beforehand");
            return;
        }

        if (username == null)
        {
            Console.WriteLine("Username must be given!");
            return;
        }
        if (password == null)
        {
            Console.WriteLine("Password must be given!");
            return;
        }
        if (website == null)
        {
            Console.WriteLine("Website must be given!");
            return;
        }
        try
        {
            using (StreamWriter writer = new(Vault.VaultCsvPath, append: true))
            {
                CsvConfiguration config = new(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false
                };
                using CsvWriter csv = new(writer, config);
                csv.WriteRecords(new Vault[]
                {
                    new Vault()
                    {
                        UserId = User.loggedInUser.Username,
                        Username = username,
                        Password = Encrypter.Encrypt(User.loggedInUser.Email!, password),
                        Website = website
                    }
                });
                Console.WriteLine("New record added to vault!");
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("Your workdir is not set correctly!");
        }
    }

}