using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;

namespace inlamning4
{
    public class VaccinationSettings
    {
        public int AvailableDoses;
        public bool VaccinateChildren = false;
    }
    public class FileSettings
    {
        //added standard path for files
        public string InputFilePath = "C:\\Windows\\Temp\\People.csv";
        public string OutputFilePath = "C:\\Windows\\Temp\\Vaccinations.csv";
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
             string[] vaccinationUnOrdered = ReadFromCSV(fileSettings.InputFilePath);
             string[] vaccinationOrdered = CreateVaccinationOrder(vaccinationUnOrdered, vaccinationSettings.AvailableDoses, vaccinationSettings.VaccinateChildren);
             SaveToCSV(fileSettings.OutputFilePath, vaccinationOrdered);
            }
            else if (menuOption == 1)
            {
               ChangeAvailableDoses();
                    Console.Clear();
            }
            else if (menuOption == 2)
            {
               ChangeVaccinateChildren();
                    Console.Clear();
                }
            else if (menuOption == 3)
            {
             ChangeInputFile();
                    Console.Clear();
                }
            else if (menuOption == 4)
            {
             ChangeOutputFile();
                    Console.Clear();
                }
            else if (menuOption == 5)
            {
                Environment.Exit(0);
            }
           
            }
        }

        static void ChangeAvailableDoses()
        {
            Console.WriteLine("Ändra antal vaccindoser");
            Console.WriteLine("-----------------------");

            while (true)
            {
                Console.Write("Ange nytt antal doser: ");

                try
                {

                    if (int.TryParse(Console.ReadLine(), out int doses))
                    {
                        vaccinationSettings.AvailableDoses = doses;
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Använd enbart giltliga siffror för inmatning, 0-9");
                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("Ett fel uppstod. Försök igen");
                }
            }
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
            
            List<Person> people = PeopleAdder(input);
            foreach (Person person in people)
            {
                string birthdateString = person.Age.ToString();
                int birthYear = int.Parse(birthdateString.Substring(0, 4));
                person.Age = DateTime.Now.Year - birthYear;
               
            }

            var prioritizedPeople = people.OrderByDescending(p => p.IsHealthcareWorker)
                .ThenByDescending(p => p.Age > 65)
                .ThenByDescending(p => p.Age)
                .ThenByDescending(p => p.IsInDanger)
                .ThenByDescending(p => (vaccinateChildren && p.Age <= 18 ))
                .Select(p => $"{p.PersonNummer},{p.LastName}, {p.FirstName}, {p.Age}")
                .ToArray();

           
            return prioritizedPeople;
        }

        public static List<Person> PeopleAdder(string[] input)
        {
            List<Person> people = new List<Person>();
            foreach (var line in input)
            {
                string[] parts = line.Split(',');

                if (parts.Length >= 6)
                {
                    var personNummer = parts[0].Replace("-", "");
                    var person = new Person()
                    {
                        PersonNummer = personNummer,
                        LastName = parts[1],
                        FirstName = parts[2],
                        IsHealthcareWorker = (int.Parse(parts[3]) == 1) ? true : false,
                        IsInDanger = (int.Parse(parts[4]) == 1) ? true : false,
                        Infected = (int.Parse(parts[5]) == 1) ? true : false,
                    };

                    if (personNummer.Length == 12)
                    {
                        int birthYear = int.Parse(personNummer.Substring(0, 4));
                        int birthMonth = int.Parse(personNummer.Substring(4, 2));
                        int birthDay = int.Parse(personNummer.Substring(6, 2));

                       
                        DateTime birthdate = new DateTime(birthYear, birthMonth, birthDay);
                        person.Age = CalculateAge(birthdate);
                    }
                    else if (personNummer.Length == 10)
                    {
                        int yearPrefix = int.Parse(personNummer.Substring(0, 2));
                        int birthMonth = int.Parse(personNummer.Substring(2, 2));
                        int birthDay = int.Parse(personNummer.Substring(4, 2));

                        int birthYear = (yearPrefix >= 0 && yearPrefix <= 18) ? 20 + yearPrefix : 19 + yearPrefix;

                        DateTime birthdate = new DateTime(birthYear, birthMonth, birthDay);
                        person.Age = CalculateAge(birthdate);
                    }
                    people.Add(person);
                }
            } 
            return people;
        }
        
      public static int CalculateAge(DateTime birthdate) 
        { 
            DateTime now = DateTime.Now;

            int age = now.Year - birthdate.Year;

            if (now.Month < birthdate.Month || (now.Month == birthdate.Month && now.Day < birthdate.Day))
            {
                age--; 
            }
            return age;
        }
        public static string[] ReadFromCSV(string inputFilePath) 
      
        {
            string[] lines = File.ReadAllLines(inputFilePath);

            return lines;
        }
        public static void SaveToCSV(string prioritizedPeople, string[] data)
        {
            File.WriteAllLines(prioritizedPeople, data);
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
            // Arrange
            string[] input =
            {
                "19720906-1111,Elba,Idris,0,0,1",
                "8102032222,Efternamnsson,Eva,1,1,0"
            };
            int doses = 10;
            bool vaccinateChildren = false;

            // Act
            string[] output = Program.CreateVaccinationOrder(input, doses, vaccinateChildren);

            // Assert
            Assert.AreEqual(output.Length, 2);
            Assert.AreEqual("19810203-2222,Efternamnsson,Eva,2", output[0]);
            Assert.AreEqual("19720906-1111,Elba,Idris,1", output[1]);
        }
    }
}
/*
Random list of people added by chatgpt
19720906-1111,Elba,Idris,0,0,1
8102032222,Efternamnsson,Eva,1,1,0
19850304-1234,Andersson,Lena,1,0,1
8906021555,Karlsson,Per,0,1,0
7608093211,Persson,Maria,1,1,1
20010718-5678,Johansson,David,0,0,1
8412131122,Gustafsson,Anna,1,0,0
9307108910,Nyqvist,Peter,0,1,1
8805021111,Svensson,Karin,1,1,0
6503224444,Bergman,Anders,0,0,1
19780317-6543,Lindgren,Sara,1,0,1
8102249876,Eriksson,Johan,0,1,0
6907313333,Larsson,Linda,1,1,1
9504202222,Nilsson,Martin,0,0,1
7811027890,Holmberg,Emma,1,0,0
8808091234,Jönsson,Jonas,0,1,1
6412150000,Sjöberg,Maria,1,1,0
8706021111,Ekström,Mikael,0,0,1
8304052222,Åkesson,Helena,1,0,1
20030519-8765,Hallberg,Simon,0,1,0
7410234444,Lindqvist,Annika,1,1,1
9007155555,Jonsson,Kristoffer,0,0,1
7508229876,Nordin,Jessica,1,0,0
7904103333,Karlström,Fredrik,0,1,1
8712202222,Söderlund,Katrin,1,1,0
6307091111,Wikström,Erik,0,0,1
7205249876,Göransson,Caroline,1,0,1
9108283333,Palm,Kristina,0,1,0
19750414-4444,Björk,Lars,1,1,1
8406031111,Henriksson,Diana,0,0,1
8804209876,Forsberg,Magnus,1,0,0
6712163333,Knutsson,Kerstin,0,1,1
7611022222,Hagström,Patrik,1,1,0
9509271111,Sandberg,Camilla,0,0,1
8510309876,Mårtensson,Thomas,1,0,1
6707153333,Almgren,Elin,0,1,0
9304092222,Nordström,Jörgen,1,1,1
7506219876,Blomqvist,Emelie,0,0,1
8409181111,Westerberg,Joakim,1,0,0
8808023333,Falk,Hanna,0,1,1
6507032222,Hägg,Andreas,1,1,0
20011027-4444,Hermansson,Anneli,0,0,1
7710169876,Lindblom,Peter,1,0,1
8407031111,Klasson,Carolina,0,1,0
8703052222,Andersson,Daniel,1,1,1
9106041111,Sundberg,Sofia,0,0,1
7512229876,Kjellberg,Lennart,1,0,0
7604283333,Edström,Katja,0,1,1
9209211111,Lilja,Jörgen,1,1,0
7408012222,Nielsen,Ann,0,0,1
8509209876,Lindell,Stefan,1,0,1
20020802-3333,Sjöstedt,Kerstin,0,1,0
8106141111,Engström,Oscar,1,1,1
9210259876,Åberg,Linda,0,0,1
8001103333,Wallin,Peter,1,0,0
6512201111,Löfgren,Isabella,0,1,1
7805172222,Ekman,Kristian,1,1,0
9107189876,Forsgren,Lotta,0,0,1
8412193333,Lindholm,Joel,1,0,1
7505101111,Pettersson,Anna,0,1,0
7203279876,Håkansson,Mikael,1,1,1
9105022222,Gerhardsson,Susanne,0,0,1
8507179876,Liljegren,Niklas,1,0,0
7808103333,Sandström,Elisabeth,0,1,1
8707131111,Leijon,Andreas,1,1,0
9302122222,Åkerberg,Jenny,0,0,1
6709269876,Eliasson,Karin,1,0,1
9405031111,Öberg,Jimmy,0,1,0
8901173333,Källström,Anna,1,1,1
8712052222,Göransson,Elin,0,0,1
7607149876,Hellström,Peter,1,0,0
8409273333,Lind,Anna,0,1,1
9508271111,Edman,Mattias,1,1,0
7109122222,Borg,Jessica,0,0,1
8805219876,Olsson,Mikael,1,0,1
7812301111,Strömberg,Marie,0,1,0
7204093333,Ekdahl,Lars,1,1,1
9402202222,Johannesson,Elin,0,0,1
8110299876,Sjöholm,Stefan,1,0,0
8309033333,Brolin,Erika,0,1,1
7902251111,Isaksson,Andreas,1,1,0
8709102222,Wik,Kristina,0,0,1
6906199876,Österberg,Lars,1,0,1
9104223333,Hjelm,Madeleine,0,1,0
7506081111,Sjöstrand,Anders,1,1,1
8405312222,Magnusson,Maria,0,0,1
8609069876,Öhman,Magnus,1,0,0
*/