using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Firefox;
using System.IO;
using System.Threading;
using System.Reflection;

namespace UpdateCourseDatabase
{
    class Program


    {
        static void Main(string[] args)
        {
            //Get HTML
            getHTML();
            //Format HTML
            //Thread.Sleep(30000);
            formatHTML();
            //Write to database

            Console.ReadLine();
        }

        private static void formatHTML()
        {
            //CONVERTING HTML TO DATATABLE

            Console.WriteLine("Starting to Parse HTML");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = System.IO.Path.Combine(currentDirectory, "Data", "Courses.txt");

            string htmlCode = System.IO.File.ReadAllText(filePath);

            doc.LoadHtml(htmlCode);


            //Remove uneeded html
            foreach (var row in doc.DocumentNode.SelectNodes("//table[contains(@class, 'plaintable' )]"))
            {
                row.Remove();
            }

            foreach (var row in doc.DocumentNode.SelectNodes("//table[contains(@class, 'infotexttable' )]"))
            {
                row.Remove();
            }




            var headers = doc.DocumentNode.SelectNodes("//tr/th");
            DataTable table = new DataTable();


            foreach (HtmlNode header in headers)
                try
                {
                    if (table.Columns.Count <= 18)
                    {

                        if (header.InnerText.Length < 11)
                        {
                            table.Columns.Add(header.InnerText); // create columns from th
                                                                 // select rows with td elements

                        }
                    }

                }
                catch (Exception ex)
                {

                }
            foreach (var row in doc.DocumentNode.SelectNodes("//tr[td]"))
            {

                table.Rows.Add(row.SelectNodes("td").Select(td => td.InnerText).ToArray());


            }




            //MODIFYING DATATABLE

            for (int row = 0; row <= table.Rows.Count - 1; row++)
            {

                for (int col = 0; col <= table.Columns.Count - 1; col++)
                {



                    //if ((col == 0) && (table.Rows[row][0].ToString() == " "))
                    int n;
                    bool isNumeric = int.TryParse(table.Rows[row][0].ToString(), out n);
                    if ((col == 0) && !isNumeric)
                        {
                        int currentRow = row;

                        //update previous record that conatinas the CRN

                        //day
                        table.Rows[currentRow - 1][7] = table.Rows[currentRow - 1][7].ToString() + "*" + table.Rows[currentRow][7].ToString();
                        //time
                        table.Rows[currentRow - 1][8] = table.Rows[currentRow - 1][8].ToString() + "*" + table.Rows[currentRow][8].ToString();
                        //instructor
                        table.Rows[currentRow - 1][14] = table.Rows[currentRow - 1][14].ToString() + "*" + table.Rows[currentRow][14].ToString();
                        //date
                        table.Rows[currentRow - 1][15] = table.Rows[currentRow - 1][15].ToString() + "*" + table.Rows[currentRow][15].ToString();
                        //location
                        table.Rows[currentRow - 1][17] = table.Rows[currentRow - 1][17].ToString() + "*" + table.Rows[currentRow][17].ToString();

                        table.Rows[row].Delete();
                        //go back a row since currentRow gets deleted
                        row--;

                    }
                }

            }
            int count = 0;
            foreach (DataRow row in table.Rows)
            {
                count++;
                Console.WriteLine("--- Row ---");
                foreach (var item in row.ItemArray)
                {
                    Console.Write("Item: "); // Print label.
                    Console.WriteLine(item);
                }
  
                //if (count > 500)
                //{
                //   break;
                //}
            }
            Console.WriteLine("Finished Parsing HTML");
            Console.ReadLine();

        }
    

        private static void getHTML()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = System.IO.Path.Combine(currentDirectory, "Data", "courses.txt");

            File.WriteAllText(filePath, "");

            Console.WriteLine("Starting Selenium");


            string driverPath = System.IO.Path.Combine(currentDirectory, "Drivers");

            IWebDriver driver = new ChromeDriver(driverPath)
            {
                Url = "https://seanet.uncw.edu/TEAL/twbkwbis.P_GenMenu?name=homepage"
                
            };

            IWebElement link;

            link = driver.FindElement(By.LinkText("Search for Courses"));

            link.Click();

            //////////////////////////////////////////////////////////

            IWebElement mySelectElement = driver.FindElement(By.Name("term_in"));

            SelectElement dropDown = new SelectElement(mySelectElement);

            dropDown.SelectByText("Fall 2018");


            IWebElement submitButton;

            submitButton = driver.FindElement(By.TagName("form"));

            submitButton.Submit();

            //////////////////////////////////////////////////////////

            IWebElement mySelectElement2 = driver.FindElement(By.Id("subj_id"));

            SelectElement dropDown2 = new SelectElement(mySelectElement2);

            IList<IWebElement> options2 = dropDown2.Options;

            foreach (IWebElement we in options2)
            {
                var str = we.Text;
                dropDown2.SelectByText(str);
            }

            //dropDown2.SelectByText("Chemistry"); use this for testing purposes

            IWebElement submitButton2;

            submitButton2 = driver.FindElement(By.TagName("form"));


            try
            {
                submitButton2.Submit();
            }
            catch
            {
                Console.WriteLine("help");
            }
            //////////////////////////////////////////////////////////

            var html = driver.PageSource;

            //StreamReader sr = new StreamReader(html);
            //StreamWriter sw = new StreamWriter(@"C:\Users\ctr20\Projects\VisualStudio Projects\ReadHTML\ReadHTML\TextFile1.txt");

            //var line = "";

            Thread.Sleep(5000);

            File.WriteAllText(filePath, html);

            Console.WriteLine("Finished Selenium");

        }
    }
}
