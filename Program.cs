
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncBoekOpdracht
{
    class Boek
    {
        public string Titel { get; set; }
        public string Auteur { get; set; }
        // public float AIScore {
        //     get {
        //         // Deze 'berekening' is eigenlijk een ingewikkeld AI algoritme.
        //         // Pas de volgende vier regels niet aan.
        //         double ret = 1.0f;
        //         for (int i = 0; i < 10000000; i++)
        //             for (int j = 0; j < 10; j++)
        //                 ret = (ret + Willekeurig.Random.NextDouble()) % 1.0;
        //         return (float)ret;
        //     }
        // }

        public async Task<Tuple<float, Boek>> AIScore()
        {
            // Deze 'berekening' is eigenlijk een ingewikkeld AI algoritme.
            // Pas de volgende vier regels niet aan.
            Console.WriteLine("Calling AIScore");
            var task = AIScoreRandom();
            await task;
            Console.WriteLine("AIScore call ended");        
            return Tuple.Create(task.Result,this);
        }

        public async Task<float> AIScoreRandom()
        {
            var result = await Task.Run(() => {
                double ret = 1.0f;
                for (int i = 0; i < 10000000; i++)
                    for (int j = 0; j < 10; j++)
                        ret = (ret + Willekeurig.Random.NextDouble()) % 1.0;
                return (float)ret;
            });
            return result;
                   
        }
    }
    static class Willekeurig
    {
        public static Random Random = new Random();
        public static async Task Vertraging(int MinAantalMs = 500, int MaxAantalMs = 1000)
        {
            await Task.Delay(Random.Next(MinAantalMs, MaxAantalMs));
        }
    }
    static class Database
    {
        private static List<Boek> lijst = new List<Boek>();
        public static async Task VoegToe(Boek b)
        {
            await Willekeurig.Vertraging(); // INSERT INTO ...
            lijst.Add(b);
        }
        public static async Task<List<Boek>> HaalLijstOp()
        {
            await Willekeurig.Vertraging(); // SELECT * FROM ...
            return lijst;
        }
        public static async Task Logboek(string melding)
        {
            await Willekeurig.Vertraging(); // schrijf naar logbestand
        }
    }
    class Program
    {
        static async Task VoegBoekToe() {
            Console.WriteLine("Geef de titel op: ");
            var titel = Console.ReadLine();
            Console.WriteLine("Geef de auteur op: ");
            var auteur = Console.ReadLine();
            await Database.VoegToe(new Boek {Titel = titel, Auteur = auteur});
            Database.Logboek("Er is een nieuw boek!");
            Console.WriteLine("De huidige lijst met boeken is: ");
            foreach (var boek in await Database.HaalLijstOp()) {
                Console.WriteLine(boek.Titel);
            }   
            
        }
        static async Task ZoekBoek() {
            Console.WriteLine("Waar gaat het boek over?");
            var beschrijving = Console.ReadLine();
            Tuple<float, Boek> beste = null;
            var tasklist = new List<Task<Tuple<float,Boek>>>();
            foreach (var boek in await Database.HaalLijstOp())
                tasklist.Add(boek.AIScore());
            foreach(var x in await (Task.WhenAll(tasklist))){
                if (beste == null || x.Item1 > beste.Item1)
                    beste = x;
            }
            Console.WriteLine("Het boek dat het beste overeenkomt met de beschrijving is: ");
            Console.WriteLine(beste.Item2.Titel);
        }
        static bool Backupping = false;
        // "Backup" kan lang duren. We willen dat de gebruiker niet hoeft te wachten,
        // en direct daarna boeken kan toevoegen en zoeken.
        static async Task Backup() {
            if (Backupping)
                return;
            Backupping = true;
            await Willekeurig.Vertraging(2000, 3000);
            Backupping = false;
        }
        static async Task Main(string[] args)
        {
            await Seed();
            Console.WriteLine("Welkom bij de boeken administratie!");
            string key = null;
            while (key != "q") {
                Console.WriteLine("+) Boek toevoegen");
                Console.WriteLine("z) Boek zoeken");
                Console.WriteLine("b) Backup maken van de boeken");
                Console.WriteLine("q) Quit");
                key = Console.ReadLine();
                if (key == "+")
                    await VoegBoekToe();
                else if (key == "z")
                    await ZoekBoek();
                else if (key == "b")
                    Backup();
                else if (key != "q")
                    Console.WriteLine("Ongeldige invoer!");
            }
        }
        public static async Task Seed()
        {
            List<Task> tasks = new List<Task>();
            Parallel.For(0, 20, x => {
                Console.WriteLine("Boek " + x + " wordt aangemaakt.");
                tasks.Add(Database.VoegToe(new Boek{Auteur = "Auteur " + x, Titel = "Titel " + x}));
                Console.WriteLine("Boek " + x + " is klaar.");
            });
            await Task.WhenAll(tasks);
            Console.WriteLine("Seed is klaar");
        }
    }
}
