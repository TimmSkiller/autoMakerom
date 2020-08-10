using System;
using System.Diagnostics;
using System.IO;

namespace autoMakeromCLI
{
    public class Content
    {
        public string FileName { get; set; }
        public string[] ContentDirectories { get; set; }
        public bool ContentIsDlc { get; set; }
        public string TitleId { get; set; }
        public string ProductCode { get; set; }
        public string Region { get; set; }
        public string ShortName { get; set; }
        public string FullFileName { get; set; }
        public bool IsDsiWare { get; set; }

        public Content(string fileName, string[] contentDirectories, bool contentIsDlc, string titleId, string productCode, string region, string shortName, string fullFileName, bool isDsiWare)
        {
            FileName = fileName;
            ContentDirectories = contentDirectories;
            ContentIsDlc = contentIsDlc;
            TitleId = titleId;
            ProductCode = productCode;
            Region = region;
            ShortName = shortName;
            FullFileName = fullFileName;
            IsDsiWare = isDsiWare;
        }

        public Content()
        {
            FileName = "na";
            ContentDirectories = Array.Empty<string>();
            ContentIsDlc = false;
            TitleId = "na";
            ProductCode = "XXX-X-XXXX - XXX-X-XXXX-XX";
            Region = "Unknown (UNK)";
            ShortName = "Invalid File Name - Please Recheck";
            FullFileName = "na";
            IsDsiWare = false;
        }

        //reading the CIA
        public static Content ReadContent(string path, string decKey)
        {
            if (!Tools.IsValidDecKey(decKey))
            {
                throw new ArgumentException($"The provided titlekey for current title {Directory.GetParent(path).Name.ToUpper()} is invalid.");
            }

            string mountPoint = $"{Environment.CurrentDirectory}/ninfs_temp";
            Process ninfs = new Process();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ninfs.StartInfo.FileName = $"{Environment.CurrentDirectory}/tools/ninfs.exe";
            }
            else
            {
                ninfs.StartInfo.FileName = "mount_cdn";
            }

            ninfs.StartInfo.Arguments = $"cdn -f \"{path}\" \"{mountPoint}\" --dec-key {decKey}";

            ninfs.Start();

            Console.WriteLine($"Running ninfs...\nMounting {path}...\n");

            while (true)
            {
                if (File.Exists($"{mountPoint}/tmd.bin"))
                {
                    break;
                }
            }

            
            Content c = new Content();
            string firstContentDir;

            try
            {
                firstContentDir = Directory.GetDirectories(mountPoint)[0];
                Console.WriteLine("Successfully mounted CDN Content!");
            }
            catch (Exception)
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\failed_mounts\\nokey");
                File.Create($"{Environment.CurrentDirectory}\\failed_mounts\\nokey\\{Directory.GetParent(path).Name.ToUpper()}").Close();
                throw new ArgumentException($"The provided titlekey for current title {Directory.GetParent(path).Name.ToUpper()} is invalid.");
            }

            c.ContentDirectories = Directory.GetDirectories(mountPoint);

            string tmdPath = $"{mountPoint}\\tmd.bin";
            string NcchPath = $"{firstContentDir}\\ncch.bin";
            string IconPath = $"{firstContentDir}/exefs/icon.bin";

            //dsiware only
            string bannerPath = $"{firstContentDir}\\banner.bin";

            c.TitleId = $"{Tools.ReadHexUTF8(tmdPath, 0x18C, 0x194, false).Replace('\x00', ' ').Trim()}";
            if (c.TitleId.StartsWith("00048"))
            {
                c.ProductCode = $"TWL-N-{Tools.ReadHexUTF8(tmdPath, 0x190, 0x194, true)}";
                try
                {
                    c.ShortName = Tools.ReadHexUTF16(bannerPath, 0x240, 0x340, true).Split('\n')[0].Replace('\u0000', ' ').Replace(":", " -").Replace('\\', '-').Replace('/', '-').Replace('?', '-').Trim();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace + "\n");
                }
                c.Region = $"{Tools.GetCIARegion(c.ProductCode).Replace('\x00', ' ').Trim()}";
                c.IsDsiWare = true;
            }
            else
            {
                c.ProductCode = Tools.ReadHexUTF8(NcchPath, 0x150, 0x160, true).Replace('\x00', ' ').Trim();
                c.ShortName = Tools.ReadHexUTF16(IconPath, 0x208, 0x287, true).Replace('\x00', ' ').Replace(":", " -").Replace('\\', '-').Replace('/','-').Replace('?', '-').Trim();
                c.Region = $"{Tools.GetCIARegion(c.ProductCode).Replace('\x00', ' ').Trim()}";
                c.IsDsiWare = false;
            }

            c.FullFileName = $"{c.TitleId} {c.ShortName} ({c.ProductCode}) {c.Region}.cia";

            Console.WriteLine($"Title ID: {c.TitleId}\nProduct Code: {c.ProductCode}\nShort Name: {c.ShortName}\n");

            return c;
        }
    }
}