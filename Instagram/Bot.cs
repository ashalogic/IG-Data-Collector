using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
namespace IG_Data_Collector.Instagram
{
    public class Bot
    {
        List<InstaSharper.API.IInstaApi> Bots = new List<InstaSharper.API.IInstaApi>();
        string StatusPath = "./Status/";
        public bool IsBotReady
        {
            get
            {
                if (Bots.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public async Task Setup(string configfile)
        {
            Bots.Clear();

            #region Read  Config
            Dictionary<string, string> userpass = new Dictionary<string, string>();
            if (File.Exists(configfile))
                using (StreamReader sr = new StreamReader(configfile))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] parts = sr.ReadLine().Trim().Split(",");
                        userpass.Add(parts[0], parts[1]);
                    }
                }
            else
            {
                Colorful.Console.WriteLine("No config file found", System.Drawing.Color.Red);
                return;
            }
            #endregion

            #region Create  Bots
            foreach (var up in userpass)
            {
                Bots.Add(InstaApiBuilder.CreateBuilder()
                    .SetUser(new InstaSharper.Classes.UserSessionData()
                    {
                        UserName = up.Key,
                        Password = up.Value
                    }).Build());
            }
            #endregion

            #region Initial Bots
            List<string[]> _TableLog = new List<string[]>();
            _TableLog.Add(new string[] { "Bot", "Username", "IsActive", "Err", "Login status" });

            var BotsUsernames = userpass.Keys.ToList();
            for (int i = 0; i < Bots.Count; i++)
            {
                if (File.Exists(StatusPath + BotsUsernames[i] + ".status"))
                {
                    Bots[i].LoadStateDataFromStream(new MemoryStream(File.ReadAllBytes(StatusPath + BotsUsernames[i] + ".status")));
                    _TableLog.Add(new string[] { (i + 1).ToString(), BotsUsernames[i], Bots[i].IsUserAuthenticated.ToString(), "No Err", "From ExSession" });
                }
                else
                {
                    var result = (await Bots[i].LoginAsync());
                    if (result.Succeeded)
                    {
                        Directory.CreateDirectory(StatusPath);
                        File.WriteAllBytes(StatusPath + BotsUsernames[i] + ".status", (Bots[i].GetStateDataAsStream() as MemoryStream).ToArray());
                        _TableLog.Add(new string[] { (i + 1).ToString(), BotsUsernames[i], Bots[i].IsUserAuthenticated.ToString(), "No Err", "New Session" });
                    }
                    else
                    {
                        _TableLog.Add(new string[] { (i + 1).ToString(), BotsUsernames[i], Bots[i].IsUserAuthenticated.ToString(), result.Info.Message, "" });
                        if (File.Exists(StatusPath + BotsUsernames[i] + ".status"))
                            File.Delete(StatusPath + BotsUsernames[i] + ".status");
                    }
                }

            }

            Helper.Print_Table_With_TF_Style(_TableLog);
            #endregion
        }
        public void Collect_By_Hashtag(string HashtagsList_Path, int count = 100, bool ranked = false, string Save_Path = "./Data/ByHashtag/")
        {
            string[] input_list = new StreamReader(HashtagsList_Path).ReadToEnd().Split("\r\n");
            int index = 0;
            List<List<string>> hashtags = input_list.GroupBy(s => index++ / Bots.Count).Select(g => g.ToList()).ToList();

            var stopwatch = Stopwatch.StartNew();

            List<Task<Task>> ddd = new List<Task<Task>>();

            for (int i = 0; i < Bots.Count; i++)
            {
                Func<object, Task> selector = async indexer =>
                {
                    int jj = int.Parse(indexer.ToString());

                    foreach (var hashtag in hashtags[jj])
                    {
                        string path = Save_Path + hashtag + "_" + DateTime.Now.ToFileTimeUtc();
                        Directory.CreateDirectory(path);

                        List<InstaMedia> medias = new List<InstaMedia>();
                        var FRes = await Bots[jj].GetTagFeedAsync(hashtag, PaginationParameters.MaxPagesToLoad(1));
                        medias.AddRange(FRes.Value.Medias);
                        string nextid = FRes.Value.NextId;
                        while (medias.Count <= count)
                        {
                            if (nextid != "")
                            {
                                var res = await Bots[jj].GetTagFeedAsync(hashtag, PaginationParameters.MaxPagesToLoad(1).StartFromId(nextid));
                                if (res.Succeeded)
                                    medias.AddRange(res.Value.Medias);
                                nextid = res.Value.NextId;
                            }
                            else
                            {
                                break;
                            }
                        }
                        using (StreamWriter sw = new StreamWriter(path + "/" + hashtag + "_RecentMedias.json"))
                        {
                            string jsonstring = JsonConvert.SerializeObject(medias, Formatting.Indented);
                            await sw.WriteAsync(jsonstring);
                        }
                    }

                };
                var jsase = new Task<Task>(selector, i);
                ddd.Add(jsase);
            }

            foreach (var d in ddd)
            {
                d.Start();
            }
            foreach (var d in ddd)
            {
                d.Wait();
            }
            foreach (var d in ddd)
            {
                d.Result.Wait();
            }

            Console.WriteLine("Done !");

            stopwatch.Stop();
        }
        public void Collect_By_Profile(string ProfilesList_Path, int count = 100, string Save_Path = "./Data/ByProfile/")
        {
            #region Read Profile File
            int index = 0;
            List<List<string>> Profiles = new StreamReader(ProfilesList_Path).ReadToEnd().Split("\r\n").GroupBy(s => index++ / Bots.Count).Select(g => g.ToList()).ToList();
            if (Profiles.Count() > Bots.Count)
            {
                Profiles.Last().ForEach(h => Profiles[Profiles.Count - 2].Add(h));
                Profiles.RemoveAt(Profiles.Count - 1);
            }
            index = 0;
            #endregion

            var spw = Stopwatch.StartNew();

            List<Task<Task>> TasksList = new List<Task<Task>>();

            for (; index < Bots.Count; index++)
            {
                TasksList.Add(new Task<Task>(new Func<object, Task>(async BotIndexOBJ =>
                {
                    int BotIndex = int.Parse(BotIndexOBJ.ToString());

                    #region Collect Profiles Medias
                    foreach (var profile in Profiles[BotIndex])
                    {
                        //Directory for instaMedias
                        string path = Save_Path + profile + "_" + DateTime.Now.ToFileTimeUtc();
                        Directory.CreateDirectory(path);

                        //Get instaMedias
                        InstaMediaList instaMedias = new InstaMediaList();
                        var FirstRequestResult = await Bots[BotIndex].GetUserMediaAsync(profile, PaginationParameters.MaxPagesToLoad(1));
                        instaMedias.AddRange(FirstRequestResult.Value);
                        string nextid = FirstRequestResult.Value.NextId;
                        while (instaMedias.Count <= count)
                        {
                            if (nextid != "")
                            {
                                var RequestResult = await Bots[BotIndex].GetUserMediaAsync(profile, PaginationParameters.MaxPagesToLoad(1).StartFromId(nextid));
                                if (RequestResult.Succeeded)
                                    instaMedias.AddRange(RequestResult.Value);
                                nextid = RequestResult.Value.NextId;
                            }
                            else
                            {
                                break;
                            }
                        }

                        //Save instaMedias
                        using (StreamWriter sw = new StreamWriter(path + "/" + profile + "_instaMedias.json"))
                        {
                            string jsonstring = JsonConvert.SerializeObject(instaMedias, Formatting.Indented);
                            await sw.WriteAsync(jsonstring);
                        }

                        //Download Images
                        Directory.CreateDirectory(path + "/Image");
                        Directory.CreateDirectory(path + "/Video");
                        Directory.CreateDirectory(path + "/Carousel");

                        using (WebClient client = new WebClient())
                        {
                            foreach (var media in instaMedias)
                            {
                                switch (media.MediaType)
                                {
                                    case InstaMediaType.Image:
                                        client.DownloadFile(new Uri(media.Images[0].URI), path + "/Image" + media.InstaIdentifier + ".jpg");
                                        break;
                                    case InstaMediaType.Video:
                                        client.DownloadFile(new Uri(media.Videos[0].Url), path + "/Video" + media.InstaIdentifier + ".mp4");
                                        break;
                                    case InstaMediaType.Carousel:
                                        for (int k = 0; k < media.Carousel.Count; k++)
                                        {
                                            if (media.Carousel[k].Images.Count > 0)
                                                client.DownloadFile(new Uri(media.Carousel[k].Images[0].URI), path + "/Carousel" + media.InstaIdentifier + "_" + k + "_" + ".jpg");
                                            if (media.Carousel[k].Videos.Count > 0)
                                                client.DownloadFile(new Uri(media.Carousel[k].Videos[0].Url), path + "/Carousel" + media.InstaIdentifier + "_" + k + "_" + ".mp4");
                                        }
                                        break;
                                }
                            }
                        }

                        Console.WriteLine($" Bot {BotIndex.ToString()} completed {Profiles[BotIndex].IndexOf(profile)} from {Profiles[BotIndex].Count}");
                    }
                    #endregion

                    Console.WriteLine($" Tasks for Bot {BotIndex.ToString()} completed after {spw.Elapsed}");
                }), index));
            }

            TasksList.ForEach(t => t.Start());        //Step A Start
            TasksList.ForEach(t => t.Wait());         //Step B Wait
            TasksList.ForEach(t => t.Result.Wait());  //Step C Result

            Console.WriteLine(" Done !");

            spw.Stop();
        }
    }
}
