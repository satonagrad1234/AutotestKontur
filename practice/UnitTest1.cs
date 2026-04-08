using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;

namespace practice;


public class Tests
{
// 1. Структура теста — есть Setup и Teardown, авторизация вынесена в отдельный метод
// 2. Переиспользование кода — повторяющиеся блоки вынесены в отдельные методы
// 3. Нет лишних UI-действий — например, используем переход по URL вместо клика по кнопкам меню,  если этого не требуется для проверки в рамках теста
// 4. Понятные сообщения в ассертах — при падении теста сразу ясно, что пошло не так
// 5. Человекочитаемые названия тестов — проверяющий понимает, что именно тестируется
// 6. Уникальные локаторы — используются там, где это возможно
// 7. Явные или неявные ожидания — тесты не падают из-за гонки с интерфейсом


    public IWebDriver driver;
    public WebDriverWait wait;

    public string discussionsURL =
        "https://staff-testing.testkontur.ru/communities/612a7485-7f49-48c9-8fe1-ee49b4435111?tab=discussions&id=66892117-a81f-4b3a-9e64-e09cedc18dc2";

    public string baseURL = "https://staff-testing.testkontur.ru/";
    public string newsURL = "https://staff-testing.testkontur.ru/news";
    public string commentsURL = "https://staff-testing.testkontur.ru/comments";

    [SetUp]
    public void Setup() // Выполняется перед каждым из тестов
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Size = new System.Drawing.Size(1080, 1080);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(5000);
        driver.Navigate().GoToUrl(baseURL);
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
    }

    [TearDown]
    public void Teardown() // Выполняется после завершения каждого из тестов
    {
        driver.Quit();
        driver.Dispose();
        Thread.Sleep(3000);
    }

    public void Authorization() //авторизация на сайте
    {
        var login = driver.FindElement(By.Id("Username"));
        login.SendKeys(Credentials.Login);

        var password = driver.FindElement(By.Id("Password"));
        password.SendKeys(Credentials.Password);

        var button = driver.FindElement(By.Name("button"));
        button.Click();

        wait.Until(ExpectedConditions.UrlToBe(newsURL));
    }

    [Test]
    public void AuthorizationTest() //тестирование авторизации
    {
        Authorization();

        Assert.That(driver.Url, Is.EqualTo(newsURL),
            $"Адрес в поисковой строке не поменялся на '{newsURL}' - авторизация не прошла");
    }

    [Test]
    public void NavigateToCommentsPageTest() //тестирование открытия раздела "Комментарии"
    {
        Authorization();
        wait.Until(ExpectedConditions.TitleIs("Новости"));

        var burgerButton = driver.FindElement(By.CssSelector("[data-tid='SidebarMenuButton']"));
        burgerButton.Click();

        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SidePage__container']")));

        var sidebar = driver.FindElement(By.CssSelector("[data-tid='SidePageBody']"));
        var comments = sidebar.FindElement(By.CssSelector("[data-tid='Comments']"));
        comments.Click();

        wait.Until(ExpectedConditions.UrlToBe(commentsURL));
        var pageTitle = driver.FindElement(By.CssSelector("[data-tid='Title']"));

        Assert.That(pageTitle.Text, Does.Contain("Комментарии"),
            "Заголовок раздела не 'Комментарии' - переход на страницу не удался");
    }

    [Test]
    public void SearchTest() //тестирование поисковой строки
    {
        Authorization();
        wait.Until(ExpectedConditions.TitleIs("Новости"));
        var searchBar = driver.FindElement(By.CssSelector("[data-tid='SearchBar']"));
        searchBar.Click();
        var searchInput =
            driver.FindElement(
                By.CssSelector("[placeholder='Поиск сотрудника, подразделения, сообщества, мероприятия']"));
        searchInput.SendKeys("Воронцов Егор Алексеевич");

        Assert.That(searchInput.GetDomAttribute("value").Contains("Воронцов Егор Алексеевич"),
            "Поле поиска должно содержать введеный текст");
    }

    [Test]
    public void ProfileOpen() // Тестирование, открытия меню профиля
    {
        Authorization();

        // Открытие выпадающего меню профилья
        var profileMenuButton =
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='ProfileMenu'] button")));
        profileMenuButton.Click();

        // Переход на страницу "Профиль"
        var settingsItem = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='Profile']")));
        settingsItem.Click();

        // Проверка, что открыт профиль нужного пользователя
        var fioElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='EmployeeName']")));
        Assert.That(fioElement.Text, Is.EqualTo("Воронцов Егор Алексеевич"), "Открыт профиль другого пользователя");
    }
    
    [Test]
    public void SendCommentCommunity() // Тестирование отправки комментария в обсуждении "Для домашки DevTools"
    {
        Authorization();

        var expectedCommentText = "текст для проверки";
        driver.Navigate().GoToUrl(discussionsURL);

        // Нажимаем кнопку добавления комментария
        var addCommentButton = driver.FindElement(By.CssSelector("[data-tid='AddComment']"));
        addCommentButton.Click();

        // Вводим текст комментария
        var commentTextField = driver.FindElement(By.CssSelector("[placeholder='Комментировать...']"));
        commentTextField.SendKeys(expectedCommentText);

        // Отправляем комментарий (Tab + Enter)
        new Actions(driver).SendKeys(Keys.Tab).SendKeys(Keys.Enter).Perform();

        // Находим последний комментарий в списке
        var commentsContainer = driver.FindElement(By.CssSelector("[data-tid='CommentsList']"));
        var allComments = commentsContainer.FindElements(By.CssSelector("[data-tid='TextComment']"));
        var lastComment = allComments.Last();

        // Проверяем, что комментарий отправился с правильным текстом
        Assert.That(lastComment.Text, Does.Contain(expectedCommentText),
            $"Вместо введенного текста: '{expectedCommentText}'. Отображается: '{lastComment.Text}'");
    }
}