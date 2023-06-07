using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Terminal.Gui;

namespace terminote_bl
{
    class Program
    {
        private static GitHubClient client;
        private static string currentUsername;
        private static TokenManager tokenManager;

        static async Task Main(string[] args)
        {
            client = new GitHubClient(new ProductHeaderValue("terminote"));
            currentUsername = "";
            tokenManager = new TokenManager();

            // Выполняем авторизацию
            await GhLogin();

            // Выводим список репозиториев
            await ShowRepositories();

            Console.WriteLine("Нажмите любую клавишу, чтобы выйти...");
            Console.ReadKey();
        }

        public static async Task GhLogin()
        {
            Console.WriteLine("Подключение к GitHub");

            if (!string.IsNullOrEmpty(currentUsername))
            {
                Console.WriteLine($"Вы уже подключены к аккаунту {currentUsername}");
                Console.WriteLine("1. Ввести другой токен");
                Console.WriteLine("2. Продолжить с текущим токеном");

                var options = new List<MenuItem>
                {
                    new MenuItem { Number = 1, Title = "Ввести другой токен" },
                    new MenuItem { Number = 2, Title = "Продолжить с текущим токеном" }
                };

                var choice = GetArrowSelection(options);

                if (choice.Number == 1)
                {
                    currentUsername = "";
                }
            }

            string accessToken = tokenManager.GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            client.Credentials = new Credentials(accessToken);

            if (await ValidateCredentials())
            {
                try
                {
                    var user = await client.User.Current();

                    currentUsername = user.Login;

                    Console.WriteLine("Успешная аутентификация!");
                    Console.WriteLine($"Добро пожаловать, {currentUsername}!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении информации о пользователе: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Ошибка: Неправильный токен доступа!");
            }
        }

        public static async Task ShowRepositories()
        {
            try
            {
                var repositories = await client.Repository.GetAllForUser(currentUsername);

                Console.WriteLine($"Репозитории пользователя {currentUsername}:");
                Console.WriteLine();

                foreach (var repository in repositories)
                {
                    Console.WriteLine(repository.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении репозиториев: {ex.Message}");
            }
        }

        private static async Task<bool> ValidateCredentials()
        {
            try
            {
                if (string.IsNullOrEmpty(currentUsername))
                {
                    var user = await client.User.Current();
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

        private static MenuItem GetArrowSelection(List<MenuItem> options)
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

    class MenuItem
    {
        public int Number { get; set; }
        public string Title { get; set; }
    }
}
