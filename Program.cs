// Tüm yılları otomatik gezmek ve 2020'ye kadar olan tüm verileri MSSQL'e kaydetmek için
// güncellenmiş ve hatasız hale getirilmiş Program.cs dosyası

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MPDB;Integrated Security=True;";
using var connection = new SqlConnection(connectionString);
connection.Open();

var options = new ChromeOptions();
options.AddArgument("--disable-logging");
options.AddArgument("--log-level=3");
options.AddExcludedArgument("enable-automation");
options.AddAdditionalOption("useAutomationExtension", false);

using var driver = new ChromeDriver(options);

// Sayfaya git
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
driver.Navigate().GoToUrl("https://www.millipiyangoonline.com/cekilis-sonuclari/super-loto");

wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
Console.WriteLine("✅ Sayfa tamamen yüklendi.");

SelectElement aySelect = null;
SelectElement yilSelect = null;

try
{
    var aySelectElement = wait.Until(d => d.FindElement(By.XPath("//*[@id='draw-month']")));
    var yilSelectElement = wait.Until(d => d.FindElement(By.XPath("//*[@id='draw-year']")));
    aySelect = new SelectElement(aySelectElement);
    yilSelect = new SelectElement(yilSelectElement);
    Console.WriteLine("✅ Ay ve yıl select kutuları bulundu.");
}
catch (NoSuchElementException)
{
    Console.WriteLine("❌ Ay ve yıl select kutuları bulunamadı.");
    driver.Quit();
    return;
}

foreach (var yilOption in yilSelect.Options)
{
    if (!int.TryParse(yilOption.GetAttribute("value"), out int yilInt)) continue;
    if (yilInt > DateTime.Now.Year || yilInt < 2020) continue;

    yilSelect.SelectByValue(yilInt.ToString());
    Thread.Sleep(1000);

    foreach (var ayOption in aySelect.Options)
    {
        var ayValue = ayOption.GetAttribute("value");
        var isDisabled = ayOption.GetAttribute("disabled") != null;

        if (string.IsNullOrWhiteSpace(ayValue) || isDisabled || !int.TryParse(ayValue, out _))
            continue;

        aySelect.SelectByValue(ayValue);

        // ✅ Filtrele butonuna tıkla (güncellenmiş xpath)
        try
        {
            var filtreleBtn = wait.Until(d => d.FindElement(By.XPath("//*[@id='draws']/div[3]/div[6]/button")));
            filtreleBtn.Click();
            Thread.Sleep(3000); // verilerin yüklenmesini bekle
        }
        catch (Exception)
        {
            Console.WriteLine("⚠️ Filtrele butonu bulunamadı veya tıklanamadı.");
            continue;
        }

        try
        {
            var listeDiv = driver.FindElement(By.XPath("//*[@id='draws']/div[5]"));
            var kartlar = listeDiv.FindElements(By.ClassName("row"));

            foreach (var kart in kartlar)
            {
                try
                {
                    var tarihSpan = kart.FindElement(By.ClassName("draw_date"));
                    var tarihText = tarihSpan?.Text.Trim();

                    if (!DateTime.TryParse(tarihText, out DateTime tarih))
                    {
                        Console.WriteLine($"⚠️ Tarih parse edilemedi: {tarihText}");
                        continue;
                    }

                    var sayiElements = kart.FindElements(By.ClassName("numbers-purple")).FirstOrDefault()?.FindElements(By.TagName("div"));
                    if (sayiElements == null || sayiElements.Count != 6)
                    {
                        Console.WriteLine($"{tarih:dd.MM.yyyy} → ⚠️ 6 sayı bulunamadı.");
                        continue;
                    }

                    string sayilar = string.Join(" ", sayiElements.Select(e => e.Text.Trim()));

                    using var kontrolCmd = new SqlCommand("SELECT COUNT(*) FROM SuperLotoSonuclari WHERE Tarih = @Tarih", connection);
                    kontrolCmd.Parameters.AddWithValue("@Tarih", tarih);
                    int count = (int)kontrolCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        Console.WriteLine($"{tarih:dd.MM.yyyy} → {sayilar} → ℹ️ Zaten kayıtlı.");
                        continue;
                    }

                    using var insertCmd = new SqlCommand("INSERT INTO SuperLotoSonuclari (Tarih, Sayilar) VALUES (@Tarih, @Sayilar)", connection);
                    insertCmd.Parameters.AddWithValue("@Tarih", tarih);
                    insertCmd.Parameters.AddWithValue("@Sayilar", sayilar);
                    insertCmd.ExecuteNonQuery();

                    Console.WriteLine($"{tarih:dd.MM.yyyy} → {sayilar} → ✅ Kayıt edildi.");
                }
                catch (Exception kartEx)
                {
                    Console.WriteLine($"❌ Hata (çekiliş kartı): {kartEx.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Hata (veri içeriği): {ex.Message}");
        }
    }
}

connection.Close();
driver.Quit();
Console.WriteLine("\n🎯 Tüm işlem başarıyla tamamlandı.");
