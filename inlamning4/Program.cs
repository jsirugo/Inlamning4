using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace inlamning4
{
    public class VaccinationSettings
    {
        public int AvailableDoses;
        public bool VaccinateChildren = true;
    }
    public class FileSettings
    {
        public string InputFilePath = "";
        public string OutputFilePath = "";
    }

    public class Person
    {
        public string PersonNummer;
        public string LastName;
        public string FirstName;
        public int Age;
        public bool IsHealthcareWorker;
        public bool IsInDanger;
        public bool Infected;
    }
    public class Program
    {
        
        static VaccinationSettings vaccinationSettings = new VaccinationSettings();
        static FileSettings fileSettings = new FileSettings();

        public static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            
           ShowMainMenu();
        }
        public static void ShowMainMenu()
        {
            while(true) { 
            Console.WriteLine("Huvudmeny");
            Console.WriteLine("Antal tillgängliga vaccindoser: " + vaccinationSettings.AvailableDoses);
            Console.WriteLine("Nuvarande indatafil: "+ fileSettings.InputFilePath);
            Console.WriteLine("Nuvarande utdatafil: " + fileSettings.OutputFilePath);
            Console.WriteLine("Vaccinering under 18 år: "+ (vaccinationSettings.VaccinateChildren ? "ja" : "nej"));
            Console.WriteLine("------------------");

            int menuOption = ShowMenu("Vad vill du göra?", new[]
             {
                "Skapa prioritetsordning",
                "Ändra antal vaccindoser",
                "Ändra åldersgräns",
                "Ändra indatafil",
                "Ändra utdatafil",
                "Avsluta" 
            });
            if (menuOption == 0)
            {
             //   string[] vaccinationUnOrdered = ReadFromCSV();
              //  string[] vaccinationOrdered = CreateVaccinationOrder();
                //SaveToCSV(fileSettings.OutputFilePath, vaccinationOrdered);
            }
            else if (menuOption == 1)
            {
               ChangeAvailableDoses();
            }
            else if (menuOption == 2)
            {
               ChangeVaccinateChildren();
            }
            else if (menuOption == 3)
            {
             ChangeInputFile();
            }
            else if (menuOption == 4)
            {
             ChangeOutputFile();
            }
            else if (menuOption == 5)
            {
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Ogiltigt val. Försök igen.");
            }

            }
        }

        static void ChangeAvailableDoses()
        {
            Console.WriteLine("Ändra antal vaccindoser");
            Console.WriteLine("-----------------------");
            Console.Write("Ange nytt antal doser: ");
            vaccinationSettings.AvailableDoses = int.Parse(Console.ReadLine());
        }
        static void ChangeVaccinateChildren()
        {

            int option = ShowMenu("Ska barn vaccineras?", new[]
                  {
                    "ja",
                    "nej",

                });
            if (option == 0)
            {
                vaccinationSettings.VaccinateChildren = true;
            }
            else
            {
                vaccinationSettings.VaccinateChildren = false;
            }
        }
        static void ChangeInputFile()
        {
            Console.WriteLine("Nuvarande indatafilväg: " + fileSettings.InputFilePath);
            Console.Write("Ange sökväg till ny indatafil: ");
            fileSettings.InputFilePath = Console.ReadLine();

        }
        static void ChangeOutputFile()
        {
            Console.Write("Ange sökväg till ny utdatafil: ");
            fileSettings.OutputFilePath = Console.ReadLine();
        }
        public static string[] CreateVaccinationOrder(string[] input, int doses, bool vaccinateChildren)
        {
            // Replace with your own code.
            return new string[0];
        }


        public static string[] ReadFromCSV(string inputFilePath) 
      
        {
            string[] lines = {};

            return lines;
        }
        public static void SaveToCSV(string fileName, string[] data)
        {

        }

        public static int ShowMenu(string prompt, IEnumerable<string> options)
        {
            if (options == null || options.Count() == 0)
            {
                throw new ArgumentException("Cannot show a menu for an empty list of options.");
            }

            Console.WriteLine(prompt);

            // Hide the cursor that will blink after calling ReadKey.
            Console.CursorVisible = false;

            // Calculate the width of the widest option so we can make them all the same width later.
            int width = options.Max(option => option.Length);

            int selected = 0;
            int top = Console.CursorTop;
            for (int i = 0; i < options.Count(); i++)
            {
                // Start by highlighting the first option.
                if (i == 0)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.White;
                }

                var option = options.ElementAt(i);
                // Pad every option to make them the same width, so the highlight is equally wide everywhere.
                Console.WriteLine("- " + option.PadRight(width));

                Console.ResetColor();
            }
            Console.CursorLeft = 0;
            Console.CursorTop = top - 1;

            ConsoleKey? key = null;
            while (key != ConsoleKey.Enter)
            {
                key = Console.ReadKey(intercept: true).Key;

                // First restore the previously selected option so it's not highlighted anymore.
                Console.CursorTop = top + selected;
                string oldOption = options.ElementAt(selected);
                Console.Write("- " + oldOption.PadRight(width));
                Console.CursorLeft = 0;
                Console.ResetColor();

                // Then find the new selected option.
                if (key == ConsoleKey.DownArrow)
                {
                    selected = Math.Min(selected + 1, options.Count() - 1);
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    selected = Math.Max(selected - 1, 0);
                }

                // Finally highlight the new selected option.
                Console.CursorTop = top + selected;
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                string newOption = options.ElementAt(selected);
                Console.Write("- " + newOption.PadRight(width));
                Console.CursorLeft = 0;
                // Place the cursor one step above the new selected option so that we can scroll and also see the option above.
                Console.CursorTop = top + selected - 1;
                Console.ResetColor();
            }

            // Afterwards, place the cursor below the menu so we can see whatever comes next.
            Console.CursorTop = top + options.Count();

            // Show the cursor again and return the selected option.
            Console.CursorVisible = true;
            return selected;
        }
    }

    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void ExampleTest()
        {
            using FakeConsole console = new FakeConsole("First input", "Second input");
            Program.Main();
            Assert.AreEqual("Hello!", console.Output);
        }
    }
}
