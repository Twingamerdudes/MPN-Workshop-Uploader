using System;
using Steamworks;
using System.Threading.Tasks;
using Ugc = Steamworks.Ugc;
using Steamworks.Data;
using System.Xml.Linq;

namespace MPNWorkshopUploader
{
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
					Console.Write("Icon path (absolute): ");
					string iconPath = Console.ReadLine();
					Console.Write("Content Path (absolute): ");
					string path = Console.ReadLine();
					Console.Write("Tags (common seperated): ");
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

					//Cool little Madness Combat themed text
					Console.WriteLine("Upload Process running.");
					await Task.Delay(100);
					Console.WriteLine("Improbability Drive found");
					await Task.Delay(50);
					Console.WriteLine("Starting upload");
					Console.WriteLine("Current server: Other_World_001");

					await upload(item);
				}
				else
				{
					//Getting mod info
					Console.Write("Workshop item ID: ");
					ulong IDInt = 0;
					PublishedFileId ID = new PublishedFileId();
					bool parsing = true;


					//Get the ID until a valid one is provided
					while (parsing)
					{
						string id = Console.ReadLine();
						if (ulong.TryParse(id, out var parsedID))
						{
							IDInt = parsedID;
							parsing = false;
						}
						else
						{
							Console.WriteLine("INVALID ID");
							Console.Write("Workshop item ID: ");
						}
					}

					ID.Value = IDInt;

					Console.Write("Mod description (leave blank to not change): ");
					string description = Console.ReadLine();
					Console.Write("Icon path (absolute, leave blank to not change): ");
					string iconPath = Console.ReadLine();
					Console.Write("Content Path (absolute, leave blank to not change): ");
					string path = Console.ReadLine();
					Console.Write("Tags (common seperated, leave blank to not change): ");
					string tags = Console.ReadLine();
					Console.Write("Update notes: ");
					string updateNotes = Console.ReadLine();


					//Get mod based on ID
					var item = new Ugc.Editor(ID)
						.WithChangeLog(updateNotes);
					
					//Update it with the new info
					if(description != "")
					{
						item.WithDescription(description);
					}

					if(iconPath != "")
					{
						item.WithPreviewFile(iconPath);
					}

					if(path != "")
					{
						item.WithContent(path);
					}

					//Tag parsing
					foreach (string tag in tags.Split(','))
					{
						item.WithTag(tag);
					}

					Console.Clear();
					Console.ForegroundColor = ConsoleColor.Red;

					//Cool little Madness Combat themed text
					Console.WriteLine("Upload Process running.");
					await Task.Delay(100);
					Console.WriteLine("Improbability Drive found");
					await Task.Delay(50);
					Console.WriteLine("Starting upload");
					Console.WriteLine("Current server: Other_World_001");

					await upload(item);
				}

				//Cleanup steam
				SteamClient.Shutdown();
			}
			catch (Exception e)
			{
				Console.WriteLine("Well shit");
				Console.WriteLine(e.Message);
			}


			Console.WriteLine("Upload success. Press key to exit");
			Console.ReadLine();
			Environment.Exit(0);
		}

		public static async Task upload(Ugc.Editor item)
		{
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
