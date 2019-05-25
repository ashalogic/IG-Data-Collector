using IG_Data_Collector.Instagram;
using System;
using System.Threading.Tasks;

namespace IG_Data_Collector
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Bot bot = new Bot();

            Helper.Print_Welcome();

            bool exit = false;

            while (!exit)
            {
                Console.Write(" How can i help you ? ");

                string[] cmd = Console.ReadLine().Trim().ToLower().Split(' ');
                switch (cmd[0])
                {
                    case "help":
                        Helper.Print_Help();
                        break;
                    case "setup":
                        await bot.Setup(configfile: "./Config/Bots_List.csv");
                        break;
                    case "info":
                        Helper.Print_AppInfo();
                        break;
                    case "collect":

                        //Check if Bot is ready 
                        if (!bot.IsBotReady)
                        {
                            Helper.Print_Msg_Err("Sorry please setup your bots frist type see 'help' for more informaiton");
                            break;
                        }

                        if (cmd.Length > 1)
                        {
                            switch (cmd[1])
                            {
                                case "hashtag":
                                    bot.Collect_By_Hashtag("./Config/Hashtags_List.csv");
                                    break;
                                case "profile":
                                    bot.Collect_By_Profile("./Config/Usernames_List.csv");
                                    break;
                            }
                        }
                        else
                        {
                            Helper.Print_Msg_Err("Sorry please set your collection type see help for more informaiton");
                            break;
                        }
                        break;
                    case "exit":
                        exit = true;
                        break;
                    default:
                        Helper.Print_Msg_Err(model: 1);
                        break;
                }
            }
        }
    }
}
