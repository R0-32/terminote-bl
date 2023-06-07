using System;
using System.Collections.Generic;
using Octokit;

namespace CodespacesBlankProgram
{
    class MenuItem
    {
        public int Number { get; set; }
        public string Title { get; set; }
    }

    class Program
    {
        static GitHubClient client;
        static string currentUsername;
        static TokenManager tokenManager;

        static void Main(string[] args)
        {
            client = new GitHubClient(new ProductHeaderValue("terminote"));
            currentUsername = "";
            tokenManager = new TokenManager();

            bool exit = false;
            int selectedOption = 0;

            var options = new List<MenuItem>
            {
                new MenuItem { Number = 1, Title = "Вход в GitHub" },
                new MenuItem { Number = 2, Title = "Просмотреть и удалить репозиторий" },
                new MenuItem { Number = 3, Title = "Закрыть приложение" }
            };

            do
            {
                Console.Clear();
                Console.WriteLine("Главное меню:");

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.Write("-> ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("   ");
                    }

                    Console.WriteLine($"{options[i].Number}. {options[i].Title}");
                }

                var keyInfo = Console.ReadKey(intercept: true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedOption = (selectedOption - 1 + options.Count) % options.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedOption = (selectedOption + 1) % options.Count;
                        break;
                    case ConsoleKey.Enter:
                        ProcessSelectedOption(options[selectedOption]);
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                    case ConsoleKey.Tab:
                        selectedOption = (selectedOption + 1) % options.Count;
                        break;
                }

            } while (!exit);
        }

        static void ProcessSelectedOption(MenuItem option)
        {
            switch (option.Number)
            {
                case 1:
                    Login();
                    break;
                case 2:
                    ShowAndDeleteRepository();
                    break;
                case 3:
                    Environment.Exit(0);
                    break;
            }
        }

        static void Login()
        {
            Console.WriteLine("Подключение к GitHub");

            if (!string.IsNullOrEmpty(currentUsername))
            {
                Console.WriteLine($"Вы уже подключены к аккаунту {currentUsername}");
                Console.WriteLine("1. Ввести другой токен");
                Console.WriteLine("2. Назад");

                var options = new List<MenuItem>
                {
                    new MenuItem { Number = 1, Title = "Ввести другой токен" },
                    new MenuItem { Number = 2, Title = "Назад" }
                };

                var choice = GetArrowSelection(options);

                if (choice.Number == 1)
                {
                    currentUsername = "";
                }
                else if (choice.Number == 2)
                {
                    Console.Clear();
                    return;
                }
            }

            string accessToken = tokenManager.GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            client.Credentials = new Credentials(accessToken);

            if (ValidateCredentials())
            {
                try
                {
                    var user = client.User.Current().Result;

                    currentUsername = user.Login;

                    Console.WriteLine("Успешная аутентификация!");
                    Console.WriteLine($"Добро пожаловать, {currentUsername}!");
                    Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении информации о пользователе: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Ошибка: Неправильный токен доступа!");
                Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                Console.ReadKey();
            }
        }

static void ShowAndDeleteRepository()
{
    if (string.IsNullOrEmpty(currentUsername))
    {
        Console.WriteLine("Ошибка: Вы не подключены к аккаунту GitHub!");
        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
        Console.ReadKey();
        return;
    }

    bool exit = false;
    int selectedOption = 0;
    List<Repository> repositories = null;

    do
    {
        try
        {
            if (repositories == null)
            {
                repositories = client.Repository.GetAllForUser(currentUsername).Result;
            }

            Console.Clear();
            Console.WriteLine($"Репозитории пользователя {currentUsername}:");
            Console.WriteLine();

            for (int i = 0; i < repositories.Count; i++)
            {
                if (i == selectedOption)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("-> ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("   ");
                }

                Console.WriteLine(repositories[i].Name);
            }

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption = (selectedOption - 1 + repositories.Count) % repositories.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedOption = (selectedOption + 1) % repositories.Count;
                    break;
                case ConsoleKey.Enter:
                    var selectedRepository = repositories[selectedOption];
                    DeleteRepository(selectedRepository);
                    repositories = null; // Refresh the repository list after deletion
                    break;
                case ConsoleKey.Escape:
                    exit = true;
                    break;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении или удалении репозитория: {ex.Message}");
            Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
            Console.ReadKey();
            exit = true;
        }
    } while (!exit);
}

static void DeleteRepository(Repository repository)
{
    try
    {
        var confirmationMessage = $"Вы уверены, что хотите удалить репозиторий \"{repository.Name}\"? (y/n): ";
        Console.Write(confirmationMessage);
        var confirmation = Console.ReadLine();

        if (confirmation?.ToLower() == "y")
        {
            client.Repository.Delete(repository.Owner.Login, repository.Name).Wait();
            Console.WriteLine($"Репозиторий \"{repository.Name}\" успешно удален!");
        }
        else
        {
            Console.WriteLine("Удаление репозитория отменено.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при удалении репозитория: {ex.Message}");
    }

    Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
    Console.ReadKey();
}

        static bool ValidateCredentials()
        {
            try
            {
                if (string.IsNullOrEmpty(currentUsername))
                {
                    var user = client.User.Current().Result;
                    currentUsername = user.Login;
                }

                // Если запрос прошел успешно, токен считается валидным
                return true;
            }
            catch
            {
                return false;
            }
        }

        static MenuItem GetArrowSelection(List<MenuItem> options)
        {
            int selectedOption = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Выберите опцию с помощью стрелок:");

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.Write("-> ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("   ");
                    }

                    Console.WriteLine($"{options[i].Number}. {options[i].Title}");
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedOption = (selectedOption - 1 + options.Count) % options.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedOption = (selectedOption + 1) % options.Count;
                        break;
                }

            } while (key != ConsoleKey.Enter);

            return options[selectedOption];
        }
    }

    class TokenManager
    {
        public string GetAccessToken()
        {
            Console.Write("Токен доступа: ");
            string accessToken = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Console.WriteLine("Ошибка: Необходимо ввести токен!");
                Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                Console.ReadKey();
                return null;
            }
            return accessToken;
        }
    }
}
