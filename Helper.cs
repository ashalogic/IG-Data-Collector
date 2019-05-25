using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Console = Colorful.Console;

namespace IG_Data_Collector
{
    static class LinqExtensions
    {
        public static List<List<T>> Split<T>(this List<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.ToList();
            return splits.ToList();
        }
    }

    public static class Helper
    {

        public static void Print_Table_With_TF_Style(System.Collections.Generic.List<string[]> rows)
        {
            Console.WriteLine();

            var table = new ConsoleTables.ConsoleTable();
            table.Options.EnableCount = false;

            table.AddColumn(rows[0]);
            for (int i = 1; i < rows.Count; i++)
            {
                table.AddRow(rows[i]);
            }

            Colorful.StyleSheet _Temp1 = new Colorful.StyleSheet(System.Drawing.Color.White);
            _Temp1.AddStyle("True[a-z]*", System.Drawing.Color.Green);
            _Temp1.AddStyle("False[a-z]*", System.Drawing.Color.Red);
            Colorful.Console.WriteLineStyled(table.ToMarkDownString(), _Temp1);

        }
        public static void Print_Help()
        {
            Console.WriteLine(" Data Collector ver ");
            Console.WriteLine(" ================================================================================");
            Console.WriteLine(" Type 'info' to check app summary.", Color.Gray);
            Console.WriteLine(" Type 'help' to close app.", Color.White);
            Console.WriteLine(" Type 'setup' to setup bots.", Color.Orange);
            Console.WriteLine(" Type 'hashtag' to close app.", Color.Orange);
            Console.WriteLine(" Type 'profile' to close app.", Color.Orange);
            Console.WriteLine(" Type 'exit' to close app.", Color.Red);
            Console.WriteLine(" ================================================================================");
            Console.WriteLine();
        }
        public static void Print_Welcome()
        {
            Console.Title = "IG Data Collector ";
            Console.WindowWidth = 140;
            Console.WriteAscii("DATACOLLECTOR", Colorful.FigletFont.Load("./Fonts/big.flf"), System.Drawing.Color.Cyan);
            Console.WriteLine(" Welcome to the data collector.");
            Console.WriteLine(" ================================================================================");
            Console.WriteLine(" Datacollector is mini app for collecting data from IG so feel free to use it but on your own also you can help me in git.");
            Console.WriteLine(" Danke :)");
            Console.WriteLine(" Ach,so if you are new Type 'help' for more information.");
            Console.WriteLine(" Have good time !");
            Console.WriteLine(" ================================================================================");
            Console.WriteLine();
        }
        public static void Print_AppInfo()
        {
            var table = new ConsoleTables.ConsoleTable(new ConsoleTables.ConsoleTableOptions()
            {
                EnableCount = false,
                NumberAlignment = ConsoleTables.Alignment.Left
            });

            //Print out Application information
            table.Columns.Add("App name");
            table.Columns.Add("Data Collector");
            table.AddRow("Version", "0.1");
            table.AddRow("Author", "Alireza Keshavarz");
            table.AddRow("Date", "05/10/2019");
            table.Write(ConsoleTables.Format.Alternative);

            table.Rows.Clear(); table.Columns.Clear();

            //Print out Platforms Status
            table.Columns.Add("Platform");
            table.Columns.Add("Ready?");
            table.AddRow("Instagram", "Yes");
            table.AddRow("Twitter", "No");
            table.AddRow("Telegram", "No");
            Colorful.StyleSheet styleSheet = new Colorful.StyleSheet(System.Drawing.Color.White);
            styleSheet.AddStyle("Yes[a-z]*", System.Drawing.Color.Green);
            styleSheet.AddStyle("No[a-z]*", System.Drawing.Color.Red);
            Colorful.Console.WriteLineStyled(table.ToMarkDownString(), styleSheet);
        }
        public static void Print_Msg(string msg = "", int model = 0)
        {

        }
        public static void Print_Msg_Err(string msg = "", int model = 0)
        {
            switch (model)
            {
                case 0:
                    Console.WriteLine(" " + msg + "\n", Color.Green);
                    break;
                case 1:
                    Console.WriteLine(" Sorry i did't catch that type 'help' for more information or 'exit' to quit\n", Color.Red);
                    break;
            }
        }
        public static void Print_Msg_Success(string msg = "", int model = 0)
        {
            switch (model)
            {
                case 0:
                    Console.WriteLine(" " + msg + "\n", Color.Green);
                    break;
                case 1:
                    Console.WriteLine(" Sorry i did't catch that type 'help' for more information or 'exit' to quit\n", Color.Red);
                    break;
            }
        }
    }
}
