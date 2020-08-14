using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace autoMakeromCLI
{
    internal class App
    {
        private static void Main(string[] cliArgs)
        {
            Console.WriteLine("autoMakerom 1.1 - Made by TimmSkiller || Credit goes to ihaveamac for ninfs, and the contributors to Project_CTR for makerom.");

            if (cliArgs.Length < 2 || !File.Exists(cliArgs[1]) || !Directory.Exists(cliArgs[0]))
            {
                Console.WriteLine("Usage:\nautoMakerom.exe CDN_FOLDER TITLEKEY_DATABASE_CSV\nThe title key database must be a CSV file that is structured like this: TITLEID,DECRYPTION_KEY");
                Environment.Exit(1);
            }

            string[] validCdnContentDirs = Tools.GetValidCdnContentFolders(cliArgs[0]);
            List<DecKeyModel> decKeys = Tools.ReadDecKeyDatabase(cliArgs[1]);
            List<string> successfulBuilds = new List<string>();
            List<string> failedBuidls = new List<string>();
            Content currentContent = new Content();
            DecKeyModel currentKey = new DecKeyModel();

            foreach (string dir in validCdnContentDirs)
            {
                Console.WriteLine();
                try
                {
                    currentKey = decKeys.Find(c => c.TitleId.ToUpper() == Path.GetFileName(dir).ToUpper());
                    currentContent = Content.ReadContent($"{dir}\\tmd", currentKey.DecKey);
                    Makerom.Run(currentContent);
                }
                catch (NullReferenceException)
                {
                    Directory.CreateDirectory($"{Environment.CurrentDirectory}\\failed_mounts\\nokey");
                    File.Create($"{Environment.CurrentDirectory}\\failed_mounts\\nokey\\{Path.GetFileName(dir).ToUpper()}").Close();
                    Console.WriteLine($"Titlekey was not found for title {Path.GetFileName(dir).ToUpper()}");
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Tools.KillNinfs();
                    continue;
                }
                
            }
        }
    }
}
