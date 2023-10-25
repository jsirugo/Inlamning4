using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.ComponentModel.Design;
using System.Threading;
using System.Runtime.CompilerServices;

namespace inlamning4
{
    public class VaccinationSettings
    {
        public int AvailableDoses = 0;
        public bool VaccinateChildren = false;
    }
    public class FileSettings
    {

        public string InputFilePath = "C:\\Windows\\Temp\\People.csv";
        public string OutputFilePath = "C:\\Windows\\Temp\\Vaccinations.csv";
    }

    public class Person
    {
        public string PersonNummer;
        public string LastName;
        public string FirstName;
        public int Doses = 0;
        public int Age;
        public DateTime BirthDate;
        public bool IsHealthcareWorker;
        public bool RiskGroup;
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
            while (true)
            {
                Console.WriteLine("Huvudmeny");
                Console.WriteLine("-----------------");
                Console.WriteLine("Antal tillgängliga vaccindoser: " + vaccinationSettings.AvailableDoses);
                Console.WriteLine("Nuvarande indatafil: " + fileSettings.InputFilePath);
                Console.WriteLine("Nuvarande utdatafil: " + fileSettings.OutputFilePath);
                Console.WriteLine("Vaccinering under 18 år: " + (vaccinationSettings.VaccinateChildren ? "ja" : "nej"));
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
                    string[] vaccinationOrdered = CreateVaccinationOrder(vaccinationUnOrdered, vaccinationSettings);
                    SaveToCSV(vaccinationOrdered);
                    Console.Clear();
                    Console.WriteLine("Resultatet har sparats i:" + fileSettings.OutputFilePath);
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
                int doses = 0;
                Console.Write("Ange nytt antal doser: ");

                try
                {
                    doses = int.Parse(Console.ReadLine());

                    if (doses >= 0)
                    {
                        vaccinationSettings.AvailableDoses = doses;
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Antal doser kan inte vara mindre än 0.");
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
            bool validPath = false;
            string newPath = "";
            while (!validPath)
            {
                try
                {
                    Console.WriteLine("Nuvarande indatafilväg: " + fileSettings.InputFilePath);
                    Console.Write("Ange sökväg till ny indatafil: ");

                    newPath = Console.ReadLine();

                    if (!File.Exists(newPath))
                    {
                        Console.WriteLine("Filen du har angett existerar inte. Ange en giltig filväg.");
                    }
                    else
                    {
                        validPath = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ett oväntat fel uppstod: " + ex.Message);
                }
            }
            fileSettings.InputFilePath = newPath;

        }
        //för framtida referens till dokumentationen tog detta mig väldigt lång tid att få till
        //Jag fick leta länge för att hitta en lösning där programmet ignorerar allt efter en giltlig sökväg
        //Då Directory.Exists i sig tillåter ofullständiga sökvägar. Lösningen kom genom att bryta 
        //ut IsPathValid till en separat metod
        static void ChangeOutputFile()
        {
            while (true)
            {
                Console.Write("Ange väg till utdatafil: ");
                string input = Console.ReadLine();
                string path = Path.GetDirectoryName(input);
                string filename = Path.GetFileName(input);

                if (IsPathValid(path))
                {

                    if (string.IsNullOrEmpty(filename))
                    {
                        fileSettings.OutputFilePath = "C:\\Windows\\Temp\\Vaccinations.csv";
                        Console.WriteLine("Utdata felaktigt inmatad och ställd till standardvärde");
                    }

                    else if (!input.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {

                        input += ".csv";
                        fileSettings.OutputFilePath = input;
                        break;

                    }
                    else
                    {
                        fileSettings.OutputFilePath = input;
                        break;
                    }

                }

                Console.WriteLine("Felaktig sökväg");
            }
        }

        static bool IsPathValid(string path)
        {
            string directory = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(directory) && Directory.Exists(directory);
        }
        public static string[] CreateVaccinationOrder(string[] input, VaccinationSettings vaccinationSettings)
        {

            List<Person> people = PeopleAdder(input);
            List<Person> filteredPeople = people.Where(person => vaccinationSettings.VaccinateChildren || person.Age >= 18).ToList();

            var prioritizedPeople = filteredPeople
             .OrderByDescending(p => p.IsHealthcareWorker)
             .ThenByDescending(p => p.Age >= 65)
             .ThenByDescending(p => p.RiskGroup)
             .ThenByDescending(p => p.Age)
             .ThenBy(p => p.BirthDate.Month)
             .ThenBy(p => p.BirthDate.Day)
             .ToList();

            DoseDistributionSystem(prioritizedPeople, vaccinationSettings);


            var result = prioritizedPeople
                .Select(p => $"{p.PersonNummer},{p.LastName},{p.FirstName},{p.Doses}")
                .ToArray();

            return result;

        }
        public static void DoseDistributionSystem(List<Person> prioritizedPeople, VaccinationSettings vaccinationSettings)
        {

            int availableDoses = vaccinationSettings.AvailableDoses;

            foreach (var person in prioritizedPeople)
            {
                if (person.Infected && availableDoses > 0)
                {
                    person.Doses++;
                    availableDoses--;
                }
                else if (!person.Infected && availableDoses >= 2)
                {
                    person.Doses += 2;
                    availableDoses -= 2;
                }
                else if (!person.Infected && availableDoses == 1 && person.Doses == 0)
                {
                    Console.WriteLine("Finns ej tillräckligt med doser för fullständig vaccinering.");
                }

                if (availableDoses <= 0)
                {
                    break;
                }

            }
        }

        public static List<Person> PeopleAdder(string[] input)
        {
            List<Person> people = new List<Person>();
            if (input == null)
            {
                Console.WriteLine("Fel i inläsning av data");
                return null;
            }
            else
            {

                foreach (var line in input)
                {
                    string[] parts = line.Split(',');


                    if (parts.Length >= 6)
                    {
                        var personNummer = parts[0];
                        if (personNummer.ElementAt(personNummer.Length - 5) != '-')
                        {
                            personNummer = personNummer.Insert(personNummer.Length - 4, "-");
                        }
                        var person = new Person()
                        {
                            PersonNummer = personNummer,
                            LastName = parts[1],
                            FirstName = parts[2],
                            IsHealthcareWorker = (int.Parse(parts[3]) == 1) ? true : false,
                            RiskGroup = (int.Parse(parts[4]) == 1) ? true : false,
                            Infected = (int.Parse(parts[5]) == 1) ? true : false,
                        };

                        if (personNummer.Length == 13)
                        {
                            int birthYear = int.Parse(personNummer.Substring(0, 4));
                            int birthMonth = int.Parse(personNummer.Substring(4, 2));
                            int birthDay = int.Parse(personNummer.Substring(6, 2));

                            DateTime birthdate = new DateTime(birthYear, birthMonth, birthDay);
                            person.Age = CalculateAge(birthdate);
                            person.BirthDate = birthdate;

                        }
                        else if (personNummer.Length == 11)
                        {
                            int yearPrefix = int.Parse(personNummer.Substring(0, 2));
                            int birthMonth = int.Parse(personNummer.Substring(2, 2));
                            int birthDay = int.Parse(personNummer.Substring(4, 2));

                            int birthYear = (yearPrefix >= 0 && yearPrefix <= 18) ? 2000 + yearPrefix : 1900 + yearPrefix;

                            DateTime birthdate = new DateTime(birthYear, birthMonth, birthDay);
                            person.BirthDate = birthdate;
                            person.Age = CalculateAge(birthdate);

                            if (!personNummer.StartsWith("19") || !personNummer.StartsWith("20"))
                            {
                                person.PersonNummer = birthYear.ToString() + personNummer.Substring(2, 9);

                            } // fick bli här nere då åldersberäkningen sker först här
                        }
                        people.Add(person);
                    }
                }
                return people;
            }
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
            int errorCounter = 0;

            for (int currentLine = 0; currentLine < lines.Length; currentLine++)
            {
                string line = lines[currentLine];
                string[] parts = line.Split(',');


                if (parts[0].Length > 13 || parts[0].Length < 10)
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ": ogiltigt personnummer, för långt eller för kort.");
                    errorCounter++;
                }

                if (ContainsLetter(parts[0]))
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ": Personnumret innehåller en bokstav.");
                    errorCounter++;
                }


                if (IsDigitsOrEmpty(parts[1]))
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ": Efternamn innehåller en siffra eller är tom.");
                    errorCounter++;
                }


                if (IsDigitsOrEmpty(parts[2]))
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ":Förnamnet innehåller antingen en siffra, eller är tom.");
                    errorCounter++;
                }

                if (parts[3] != "0" && parts[3] != "1")
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ": värdet är annat än 1 eller 0.");
                    errorCounter++;
                }
                if (parts[4] != "0" && parts[4] != "1")
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ": värdet är annat än 1 eller 0.");
                    errorCounter++;
                }
                if (parts[5] != "0" && parts[5] != "1")
                {
                    Console.WriteLine("Fel på rad: " + (currentLine + 1) + ": värdet är annat än 1 eller 0.");
                    errorCounter++;
                }
            }

            if (errorCounter > 0)
            {
                Console.WriteLine("Antal fel funna: " + errorCounter);
                int option = ShowMenu("Felaktigt format på rader, återgår till huvudmeny.", new[]
                  {
                    "Tryck enter för att återgå till huvudmeny",
                });
                if (option == 0)
                {

                    Console.Clear();
                    ShowMainMenu();
                }
            }
            return lines;
        }

        public static bool ContainsLetter(string input)
        {

            return input.Any(char.IsLetter);
        }

        public static bool IsDigitsOrEmpty(string input)
        {

            return string.IsNullOrWhiteSpace(input) || input.All(char.IsDigit);
        }
        public static void SaveToCSV(string[] data)
        {
            if (File.Exists(fileSettings.OutputFilePath))
            {
                int option = ShowMenu("Filen existerar redan, vill du skriva över den?", new[]
                  {
                    "ja",
                    "nej",
                });
                if (option == 0)
                {
                    File.WriteAllLines(fileSettings.OutputFilePath, data);
                }
                else
                {
                    Console.WriteLine("Användare valt att ej skriva över fil, återgår till meny");
                    return;
                }

            }
            else { File.WriteAllLines(fileSettings.OutputFilePath, data); }

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
        public void KidTesterVaccinateOff()
        {
            // Arrange
            VaccinationSettings vaccinationSettings = new VaccinationSettings();
            vaccinationSettings.AvailableDoses = 10;
            vaccinationSettings.VaccinateChildren = false;

            string[] input =
            {
            "19720906-1111,Elba,Idris,0,0,1",
            "8102032222,Efternamnsson,Eva,1,1,0",
            "200607160039,Skrikapansson,Bob,0,0,0"
        };

            // Act
            string[] output = Program.CreateVaccinationOrder(input, vaccinationSettings);

            // Assert
            Assert.AreEqual(2, output.Length);
            Assert.AreEqual("19810203-2222,Efternamnsson,Eva,2", output[0]);
            Assert.AreEqual("19720906-1111,Elba,Idris,1", output[1]);
        }

        [TestMethod]
        public void KidTesterVaccinateON()
        {
            VaccinationSettings vaccinationSettings = new VaccinationSettings();
            vaccinationSettings.AvailableDoses = 10;
            vaccinationSettings.VaccinateChildren = true;
            string[] input =
            {
                "19720906-1111,Elba,Idris,0,0,1",
                "8102032222,Efternamnsson,Eva,1,1,0",
                "200807160039,Skrikapansson,Bob,0,0,0"
            };


            // Act
            string[] output = Program.CreateVaccinationOrder(input, vaccinationSettings);

            // Assert
            Assert.AreEqual(output.Length, 3);
            Assert.AreEqual("19810203-2222,Efternamnsson,Eva,2", output[0]);
            Assert.AreEqual("19720906-1111,Elba,Idris,1", output[1]);
            Assert.AreEqual("20080716-0039,Skrikapansson,Bob,2", output[2]);

        }
        [TestMethod]
        public void NotEnoughDoses()
        {
            VaccinationSettings vaccinationSettings = new VaccinationSettings();
            vaccinationSettings.AvailableDoses = 6; // bör bli att det finns en över när bob ska ha två och då får han 0
            vaccinationSettings.VaccinateChildren = true;
            string[] input =
            {
                "19720906-1111,Elba,Idris,0,0,1",
                "8102032222,Efternamnsson,Eva,1,1,0",
                "200807160039,Skrikapansson,Bob,0,0,0",
                "20010718-5678,Johansson,David,0,0,0"
            };


            // Act
            string[] output = Program.CreateVaccinationOrder(input, vaccinationSettings);

            // Assert
            Assert.AreEqual(output.Length, 4);
            Assert.AreEqual("19810203-2222,Efternamnsson,Eva,2", output[0]);
            Assert.AreEqual("19720906-1111,Elba,Idris,1", output[1]);
            Assert.AreEqual("20010718-5678,Johansson,David,2", output[2]);
            Assert.AreEqual("20080716-0039,Skrikapansson,Bob,0", output[3]);

        }
        [TestMethod]
        public void VaccinationOrderAllVariablesNoChildren()
        {
            // Arrange
            VaccinationSettings vaccinationSettings = new VaccinationSettings();

            vaccinationSettings.VaccinateChildren = false;

            string[] input =
            {
            "19880808-8888,Elba,VårdEjRisk,1,0,0",
            "19880807-8888,Efternamnsson,VårdRisk,1,1,0",
            "19880806-8888,Andersson,EjVårdRisk,0,1,0",
            "19880805-8888,Karlsson,EjRiskEjVård,0,0,0",
            "19200808-8888,Persson,GammalEjVårdEjRisk,0,0,0",
            "20200808-8888,Johansson,UngEjVårdEjRisk,0,0,0",
            "200808-8888,Gustafsson,GammalRisk,0,1,0",
            "20200807-8888,Nyqvist,UngRisk,0,1,0",
            "19200803-8888,Svensson,GammalInfekterad,0,0,1",
            "20200806-8888,Bergman,UngInfekterad,0,0,1",
            };

            // Act
            string[] output = Program.CreateVaccinationOrder(input, vaccinationSettings);

            // Assert
            Assert.AreEqual(7, output.Length);
            Assert.AreEqual("19880807-8888,Efternamnsson,VårdRisk,0", output[0]);
            Assert.AreEqual("19880808-8888,Elba,VårdEjRisk,0", output[1]);
            Assert.AreEqual("19200808-8888,Gustafsson,GammalRisk,0", output[2]);
            Assert.AreEqual("19200803-8888,Svensson,GammalInfekterad,0", output[3]);
            Assert.AreEqual("19200808-8888,Persson,GammalEjVårdEjRisk,0", output[4]);
            Assert.AreEqual("19880806-8888,Andersson,EjVårdRisk,0", output[5]);
            Assert.AreEqual("19880805-8888,Karlsson,EjRiskEjVård,0", output[6]);
        }
        [TestMethod]
        public void VaccinationOrderAllVariablesWithChildren()
        {
            // Arrange
            VaccinationSettings vaccinationSettings = new VaccinationSettings();

            vaccinationSettings.VaccinateChildren = true;

            string[] input =
            {
            "19880808-8888,Elba,VårdEjRisk,1,0,0",
            "19880807-8888,Efternamnsson,VårdRisk,1,1,0",
            "19880806-8888,Andersson,EjVårdRisk,0,1,0",
            "19880805-8888,Karlsson,EjRiskEjVård,0,0,0",
            "19200808-8888,Persson,GammalEjVårdEjRisk,0,0,0",
            "20200808-8888,Johansson,UngEjVårdEjRisk,0,0,0",
            "200808-8888,Gustafsson,GammalRisk,0,1,0",
            "20200807-8888,Nyqvist,UngRisk,0,1,0",
            "19200803-8888,Svensson,GammalInfekterad,0,0,1",
            "20200806-8888,Bergman,UngInfekterad,0,0,1",
            };

            // Act
            string[] output = Program.CreateVaccinationOrder(input, vaccinationSettings);

            // Assert
            Assert.AreEqual(10, output.Length);
            Assert.AreEqual("19880807-8888,Efternamnsson,VårdRisk,0", output[0]);
            Assert.AreEqual("19880808-8888,Elba,VårdEjRisk,0", output[1]);
            Assert.AreEqual("19200808-8888,Gustafsson,GammalRisk,0", output[2]);
            Assert.AreEqual("19200803-8888,Svensson,GammalInfekterad,0", output[3]);
            Assert.AreEqual("19200808-8888,Persson,GammalEjVårdEjRisk,0", output[4]);
            Assert.AreEqual("19880806-8888,Andersson,EjVårdRisk,0", output[5]);
            Assert.AreEqual("20200807-8888,Nyqvist,UngRisk,0", output[6]);
            Assert.AreEqual("19880805-8888,Karlsson,EjRiskEjVård,0", output[7]);
            Assert.AreEqual("20200806-8888,Bergman,UngInfekterad,0", output[8]);
            Assert.AreEqual("20200808-8888,Johansson,UngEjVårdEjRisk,0", output[9]);
        }
    }
}

