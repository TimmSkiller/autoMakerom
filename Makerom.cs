using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace autoMakeromCLI
{
    public static class Makerom
    {
        public static void Run(Content content)
        {
            Process makerom = new Process();
            Regex r = new Regex("[\u3040-\u30ff\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff\uff66-\uff9f]");

            makerom.StartInfo.FileName = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? $"{Environment.CurrentDirectory}/tools/makerom.exe"
                : $"{Environment.CurrentDirectory}/tools/makerom";

            if (content.IsDsiWare)
            {
                string tempCiaName = null;

                if (r.IsMatch(content.FullFileName))
                {
                    tempCiaName = "tempName.cia";
                    makerom.StartInfo.Arguments = $"-f cia -o \"{tempCiaName}\" -srl \"{Directory.GetFiles($"{Environment.CurrentDirectory}\\ninfs_temp", "*.nds")[0]}\"";
                }
                else
                {
                    makerom.StartInfo.Arguments = $"-f cia -o \"{content.FullFileName}\" -srl \"{Directory.GetFiles($"{Environment.CurrentDirectory}\\ninfs_temp", "*.nds")[0]}\"";
                }

                makerom.Start();
                Console.WriteLine($"Building CIA for title {content.TitleId}...\n");
                makerom.WaitForExit();

                if (tempCiaName != null)
                {
                    File.Move(tempCiaName, content.FullFileName);
                }
            }
            else
            {
                makerom.StartInfo.Arguments += $"-f cia -o \"{content.FullFileName}\" ";
                foreach (string directory in content.ContentDirectories)
                {
                    string index = Tools.GetContentIndex(Path.GetFileName(directory));
                    string decryptedType = File.Exists($"{directory}/decrypted.cxi") ? "decrypted.cxi" : "decrypted.cfa";

                    makerom.StartInfo.Arguments += $"-i {directory}/{decryptedType}:{index} ";
                    Console.WriteLine($"Content {Path.GetFileName(directory)} | Index: {index} | NCCH: {decryptedType}");
                }

                Console.WriteLine();

                if (content.ContentIsDlc)
                {
                    makerom.StartInfo.Arguments += "-dlc ";
                }

                makerom.StartInfo.Arguments += "-ignoresign";

                Console.WriteLine($"Building CIA for title {content.TitleId}...\n");
                makerom.Start();
                makerom.WaitForExit();
            }

            if (File.Exists($"{Environment.CurrentDirectory}/{content.FullFileName}"))
            {
                Console.WriteLine($"Successfully built {content.FullFileName}!");
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\success_builds");
                File.Move($"{Environment.CurrentDirectory}/{content.FullFileName}", $"{Environment.CurrentDirectory}\\success_builds\\{content.FullFileName}");
            }
            else
            {
                Console.WriteLine($"An error occured when makerom was ran to build {content.TitleId}. The CIA was not built.");
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\failed_mounts\\failed_makerom_build");
                File.Create($"{Environment.CurrentDirectory}\\failed_mounts\\failed_makerom_build\\{content.TitleId.ToUpper()}").Close();
            }

            Tools.KillNinfs();
        }
    }
}