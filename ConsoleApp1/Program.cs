using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Net.Http.Headers;
using System.Net;

namespace Seleniumtest_zubkov2
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }
    }

    public class Seleniumtest
    {
        private IWebDriver driver;

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArguments("--no-sandbox", "--window-size=970,873", "--disable-extensions");
            driver = new ChromeDriver(options);
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
        
        public bool IsElementPresent(By locator)
        {
            try
            {
                driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        
        
        [Test]
        [TestCase("zubkoff.m@yandex.ru", "Userzubkov1!", "https://staff-testing.testkontur.ru/")]
        [TestCase("user", "1q2w3e4r8T", "https://yandex.ru/")]
        [TestCase("user", "1q2w3e4r8T", "https://kontur.ru/")]
        [TestCase("test", "test", "https://staff-testing.testkontur.ru/")]
        
        public void Authorisation(string username, string password, string url)
        {
            driver.Navigate().GoToUrl(url);
            
            try
            {
                IWebElement logins = driver.FindElement(By.Id("Username"), 5);
                if (logins != null)
                {
                    logins.SendKeys(username);
                }
            }
            catch (WebDriverTimeoutException)
            {
                // Проверка авторизации и вывод новости если yandex.ru
                if (url.Contains("yandex.ru"))
                {
                    try
                    {
                        IWebElement titleElement = driver.FindElement(By.CssSelector("span.card-news-story__text-3F"), 5);
                        string title = titleElement.Text;
                        throw new InvalidOperationException($"Прекрасная новость: {title}");
                    }
                    catch (WebDriverTimeoutException)
                    {
                        throw new InvalidOperationException("Страница не загружена. Заголовок новости не найден.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Страница не загружена/Неверный URL");
                }
            }
            

            IWebElement passwords = driver.FindElement(By.Id("Password"));
            passwords.SendKeys(password);

            IWebElement button = driver.FindElement(By.Name("button"));
            button.Click();

            // Проверка правильности введенных логина и пароля
            IWebElement[] errorauth = driver.FindElements(By.CssSelector("li")).ToArray();

            // Если массив пустой, значит элемент не найден
            if (errorauth.Length != 0)
            {
                throw new InvalidOperationException("Неверный логин или пароль");
            }
            
            // Работа с кнопкой "Меню"
            try
            {
                IWebElement menubutton = driver.FindElement(By.CssSelector("button[data-tid='SidebarMenuButton']"), 5);
                if (menubutton != null)
                {
                    Console.WriteLine("Кнопка 'Меню' найдена");
                    menubutton.Click();
                }
            }
            catch (WebDriverTimeoutException)
            {
                try
                {
                    // Поиск другой кнопки, если первая не найдена
                    IWebElement alternativeMenuButton = driver.FindElement(By.CssSelector("button.sc-kLojOw.gkKHtQ"), 5);
                    if (alternativeMenuButton != null)
                    {
                        Console.WriteLine("Альтернативная кнопка 'Меню' найдена");
                        alternativeMenuButton.Click();
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    // Если и вторая кнопка не найдена, ошибка
                    throw new InvalidOperationException("Кнопка 'Меню' не найдена");
                }
            }
            // Проверяем существование кнопки "Выйти"
            if (IsElementPresent(By.CssSelector("button[data-tid='LogoutButton']")))
                try
                {
                    IWebElement logoutbutton = driver.FindElement(By.CssSelector("button[data-tid='LogoutButton']"), 5);
                    if (logoutbutton != null)
                    {
                        Console.WriteLine("Кнопка 'Выйти' найдена");
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    throw new InvalidOperationException("Кнопка 'Выйти' не найдена");
                }
            
            // Извлечение значения ключа "token" из локального хранилища
            string token = ((IJavaScriptExecutor)driver).ExecuteScript("return localStorage.getItem('token');").ToString();

            // Проверяем, что значение ключа "token" было извлечено успешно
            if (!string.IsNullOrEmpty(token))
            {
                // Отправка GET-запроса для проверки авторизации
                using (HttpClient client = new HttpClient())
                {
                    // Устанавливаем заголовок Authorization
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    
                    HttpResponseMessage response = client.GetAsync("https://staff-testing.testkontur.ru/api/v1/communities").Result;
                    
                    HttpStatusCode statusCode = response.StatusCode;
                    
                    
                    if (response.IsSuccessStatusCode)
                    {
                        // Получаем содержимое ответа
                        string responseBody = response.Content.ReadAsStringAsync().Result;
                      
                        // Результаты отправки запроса
                        Console.WriteLine("Код ответа сервера:" + statusCode +(int)statusCode);
                        Console.WriteLine("Значение token: " + token);
                        Console.WriteLine("Ответ сервера: " + responseBody);
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при выполнении запроса: " + response.StatusCode);
                    }
                }
            }
            else
            {
                // Если токен не был извлечен
                Console.WriteLine("Значение token не найдено в локальном хранилище.");
            }
            
            Thread.Sleep(5000);
            
            
        }
        
        
    
    }
}








