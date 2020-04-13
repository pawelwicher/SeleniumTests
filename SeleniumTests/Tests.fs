namespace SeleniumTests

open System
open System.IO
open System.Reflection
open FsUnit
open Xunit
open Xunit.Sdk
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type WikipediaTestDataAttribute() =
    inherit DataAttribute()
    override this.GetData (_ : MethodInfo) : obj[] seq =
        File.ReadAllLines "data.txt" |> Seq.map (fun x -> [| x |])

module Tests =

    let private findBySelector (driver : IWebDriver) (selector : string) : IWebElement =
        let wait = WebDriverWait(driver, TimeSpan.FromSeconds(10.0))
        wait.Until(fun x -> selector |> By.CssSelector |> x.FindElement)

    let private findAllBySelector (driver : IWebDriver) (selector : string) : IWebElement list =
        let wait = WebDriverWait(driver, TimeSpan.FromSeconds(10.0))
        wait.Until(fun x -> selector |> By.CssSelector |> x.FindElements) |> Seq.toList

    [<Trait("Category", "Automation")>]
    [<Theory>]
    [<WikipediaTestData>]
    let ``Search in Wikipedia`` (searchTerm : string) =
        let options = ChromeOptions()
        options.AddArgument("--no-sandbox")
        options.AddArgument("--start-maximized")

        use driver = new ChromeDriver(options)

        let navigation = driver.Navigate()
        navigation.GoToUrl("https://www.google.pl/")

        let element = findBySelector driver @"input[name=""q""]"
        element.SendKeys (sprintf "%s Wikipedia" searchTerm)
        element.SendKeys Keys.Enter

        let element = findBySelector driver "a > h3"
        element.Click()

        let desktopPath = Environment.GetFolderPath Environment.SpecialFolder.Desktop
        let screenshotFilePath = sprintf "%s\%s.png" desktopPath searchTerm
        let textFilePath = sprintf "%s\%s.txt" desktopPath searchTerm

        let screenshotMaker = driver :> ITakesScreenshot
        screenshotMaker.GetScreenshot().SaveAsFile(screenshotFilePath, ScreenshotImageFormat.Png)

        let text = findAllBySelector driver "p" |> Seq.map (fun x -> x.Text) |> Seq.toArray |> String.concat "\n\n"
        File.WriteAllText(textFilePath, text)        

        File.Exists textFilePath |> should be True