namespace SeleniumTests

open System
open System.IO
open System.Reflection
open FsUnit
open Xunit
open Xunit.Sdk
open OpenQA.Selenium
open OpenQA.Selenium.Chrome

type WikipediaTestDataAttribute() =
    inherit DataAttribute()
    override this.GetData (_ : MethodInfo) : obj[] seq =
        File.ReadAllLines "data.txt" |> Seq.map (fun x -> [| x |])

module Tests =

    [<Trait("Category", "Automation")>]
    [<Theory>]
    [<WikipediaTestData>]
    let ``Search in Wikipedia`` (searchTerm : string) =
        let options = ChromeOptions()
        options.AddArgument("--no-sandbox")
        use driver = new ChromeDriver(options)

        let navigation : INavigation = driver.Navigate()
        navigation.GoToUrl("https://www.google.pl/")

        let element : IWebElement = driver.FindElementByCssSelector @"input[name=""q""]"
        element.SendKeys (sprintf "%s Wikipedia" searchTerm)
        element.SendKeys Keys.Enter

        let element : IWebElement = driver.FindElementByCssSelector "a > h3"
        element.Click()

        let text = driver.FindElementsByTagName "p" |> Seq.map (fun x -> x.Text) |> Seq.toArray |> String.concat "\n\n"
        let path = sprintf "%s\%s.txt" (Environment.GetFolderPath Environment.SpecialFolder.Desktop) searchTerm
        File.WriteAllText(path, text)        

        File.Exists path |> should be True
