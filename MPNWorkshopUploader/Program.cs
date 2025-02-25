using System;
using Steamworks;
using System.Threading.Tasks;
using Ugc = Steamworks.Ugc;
using Steamworks.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MPNWorkshopUploader
{
    public static class ImageClassifier
    {
	public enum ImageFormat
	{
	jpeg,
	gif,
	png,
	unknown
	}

	public static ImageFormat GetImageFormat(byte[] bytes)
	{
	    // see http://www.mikekunz.com/image_file_header.html  
	    var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
	    var png = new byte[] { 137, 80, 78, 71 };    // PNG
	    var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
	    var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg cano

	    if (gif.SequenceEqual(bytes.Take(gif.Length)))
	    return ImageFormat.gif;
   
	    if (png.SequenceEqual(bytes.Take(png.Length)))
    	    return ImageFormat.png;

	    if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
	    return ImageFormat.jpeg;

	    if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
	    return ImageFormat.jpeg;

	    return ImageFormat.unknown;
	}
    }
	//Thanks Facepunch for the code
    public class TrackUpload : IProgress<float>
    {
	float lastvalue = 0;

	public void Report(float value)
	{
	    if (lastvalue >= value) return;

	    lastvalue = value;

	    Console.WriteLine("Uploading progress: " + (value * 100).ToString() + "%");
	}
    }

    internal class Program
    {
        public static string CheckFilePathQuotes(string path)
        {
            string pattern = @"""(.*?)""";
            String[] Match = Regex.Split(path, pattern);
            if (Match.Length != 1)
            {
                path = Match[1];
            }
            return path;
        }

        public static string VerifyIconPath(string path)
	{
            if (File.Exists(path))
	    {
		if (ImageClassifier.GetImageFormat(File.ReadAllBytes(path)) == ImageClassifier.ImageFormat.unknown)
		{
		    return "File type is not valid!";
		} 
                else 
                {
		    return "";
		}
	    }
	    return "File path is not valid!";
	}
	
        public static string GetImagePath(bool updating = false)
        {
            string path = string.Empty;
	    while(true)
	    {
		path = Console.ReadLine();
                path = CheckFilePathQuotes(path);

                if (path == "" && updating)
		{
		    break;
		}
		string errorMessage = VerifyIconPath(path);
		if (errorMessage.Length != 0)
		{
		    Console.WriteLine(errorMessage);
		    Console.Write("Icon path (absolute, quotes are fine in your path): ");
		} 
                else 
                {
                    Console.WriteLine("Path chosen: {0}", path);
                    break;
		}
	    }
	return path;
	}

	public static string GetContentPath(bool updating = false)
	{
            string path = string.Empty;
            while (true)
            {
                path = Console.ReadLine();
                path = CheckFilePathQuotes(path);

                if (path == "" && updating)
                {
                    break;
                }
                if (Path.GetExtension(path) == string.Empty)
                {
                    Console.WriteLine("Folder path is invalid!");
                    Console.Write("Content path (absolute, quotes are fine in your path): ");
                } 
                else 
                {
                    Console.WriteLine("Path chosen: {0}", path);
                    break;
                }	
            }
            return path;
        }

	static async Task Main(string[] args)
	{
	    try
	    {
		//Steam init
		SteamClient.Init(488860, true);

		//Console Init
		Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;


                //Cool intro text
                Console.WriteLine("Madness: Project Nexus Workshop Uploader");
                Console.WriteLine("Copyrighted 1982-1996 2BDamned & Twingamerdudes");
                Console.WriteLine();

                Console.WriteLine("Are you updating or creating a new mod? (update/create)");
                string updateStr = Console.ReadLine();
                bool updating = updateStr.ToLower() == "update";


                if (!updating)
                {
                    //Getting mod info
                    Console.Write("Mod name: ");
		    string name = Console.ReadLine();
                    Console.Write("Mod description: ");
                    string description = Console.ReadLine();
                    Console.Write("Icon path (absolute, quotes are fine in your path): ");
                    string iconPath = GetImagePath();
                    Console.Write("Content path (absolute, quotes are fine in your path): ");
                    string path = GetContentPath();
                    Console.Write("Tags (comma separated): ");
                    string tags = Console.ReadLine();

                    Console.WriteLine("Are you sure you want to upload this? (y/n)");
                    string confirm = Console.ReadLine();
                    bool confirmed = confirm.ToLower() != "y";

                    if (confirmed)
		    {
                        Environment.Exit(0);
		    }

                    //Creating mod item
                    var item = Ugc.Editor.NewCommunityFile
		    .WithTitle(name)
		    .WithDescription(description)
		    .WithContent(path)
		    .WithPreviewFile(iconPath);

                    //Tag parsing
                    foreach (string tag in tags.Split(','))
		    {
			item.WithTag(tag);
		    }

		    Console.Clear();
		    Console.ForegroundColor = ConsoleColor.Red;

		    await upload(item);
		}
		else
		{
                    //Getting mod info
                    Console.Write("Workshop Item ID: ");
                    ulong IDInt = 0;
                    PublishedFileId ID = new PublishedFileId();


                    //Get the ID until a valid one is provided
                    while (true)
                    {
                        string id = Console.ReadLine();
                        if (ulong.TryParse(id, out var parsedID))
                        {
                            IDInt = parsedID;
                            ID.Value = IDInt;

                            var q = await Ugc.Query.Items.WithFileId(new PublishedFileId[] { ID }).WithType(UgcType.Items).GetPageAsync(1);
                            if (q.Value.Entries.First().Owner.Name != string.Empty)
                            {
                                break;
                            }
                        }
                        Console.WriteLine("INVALID ID");
                        Console.Write("Workshop Item ID: ");
                    }

                    Console.Write("Mod description (leave blank to not change): ");
                    string description = Console.ReadLine();
                    Console.Write("Icon path (absolute, quotes are fine in your path, leave blank to not change): ");
                    string iconPath = GetImagePath(true);
                    Console.Write("Content path (absolute, quotes are fine in your path, leave blank to not change content of your mod): ");
                    string path = GetContentPath(true);
                    Console.Write("Tags (comma separated, leave blank to not change): ");
                    string tags = Console.ReadLine();
                    Console.Write("Update notes: ");
                    string updateNotes = Console.ReadLine();


                    //Get mod based on ID
                    var item = new Ugc.Editor(ID)
                    .WithChangeLog(updateNotes);

                    var q2 = await Ugc.Query.Items.WithFileId(new PublishedFileId[] { ID }).WithType(UgcType.Items).GetPageAsync(1);
                    var itemData = q2.Value.Entries.First();

                    //Update it with the new info
                    if (description != "")
                    {
                        item.WithDescription(description);
                    }

                    if (iconPath != "")
                    {
                        item.WithPreviewFile(iconPath);
                    }

                    if (path != "")
                    {
                        item.WithContent(path);
                    }

                    //Tag parsing
                    if (tags.Split(',').Length > 0) {
                        foreach (string tag in tags.Split(','))
                        {
                            item.WithTag(tag);
                        }
                    }
                    else
                    {
                        if (itemData.Tags.Length > 0)
                        {
                            foreach (string tag in itemData.Tags)
                            {
                                item.WithTag(tag);
                            }
                        }
                    }

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;

                    await upload(item);
                }

                //Cleanup steam
                SteamClient.Shutdown();
            }
            catch (Exception e)
            {
                Console.WriteLine("Well shit");
                Console.WriteLine(e.Message);
                Console.ReadLine();
                Environment.Exit(0);
            }


            Console.WriteLine("Upload success. Press key to exit");
	    Console.ReadLine();
	    Environment.Exit(0);
        }

        public static async Task upload(Ugc.Editor item)
        {
            //Cool little Madness Combat themed text
            Console.WriteLine("Upload Process running.");
            await Task.Delay(100);
            Console.WriteLine("Improbability Drive found");
            await Task.Delay(50);
            Console.WriteLine("Starting upload");
            Console.WriteLine("Current server: Other_World_001");

            var result = await item.SubmitAsync(new TrackUpload());

            //Upload fails
            if (result.Result != Result.OK)
            {
                switch (result.Result)
                {
                    case Result.Fail:
                        Console.WriteLine("ERR: Upload failed, what the fuck did you do Jebus.");
                        break;
                    default:
                        Console.WriteLine("ERR: " + result.Result);
                        break;
                }
                Console.WriteLine("Press key to exit");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }
    }
}
