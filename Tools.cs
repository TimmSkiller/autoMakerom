using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace autoMakeromCLI
{
    internal static class Tools
    {
        public static bool IsDLC(string productId)
        {
            return productId.Split('-')[1] switch
            {
                "P" => false,
                "U" => false,
                "M" => true,
                "N" => false,
                _ => false
            };
        }

        public static string GetCIARegion(string productId)
        {
            string result = productId.Split('-')[2][3] switch
            {
                'P' => "(E)",
                'Z' => "(E)",
                'F' => "(E)",
                'X' => "(E)",
                'V' => "(E)",
                'Y' => "(E)",
                'D' => "(E)",
                'E' => "(U)",
                'J' => "(J)",
                'K' => "(K)",
                'W' => "(CN)",
                'A' => "(W)",
                _ => "(UNK)",
            };
            return result;
        }

        //reads any file from a given start and end hex offset
        public static string ReadHexUTF16(string path, Int32 startOffset, Int32 endOffset, bool decode)
        {
            if (!File.Exists(path)) { throw new FileNotFoundException($"Could not find {path}"); }

            string result = "";

            List<byte> bytes = new List<byte>();
            BinaryReader reader = new BinaryReader(File.OpenRead(path));
            reader.BaseStream.Position = startOffset;

            for (int i = startOffset; i < endOffset - 0x3F; i++)
            {
                bytes.Add(reader.ReadByte());
            }

            byte[] c = bytes.ToArray();

            if (decode)
            {
                for (int i = 0; i < c.Length - 1; i += 2)
                {
                    if (c[i] == 0 && c[i + 1] == 0)
                    {
                        break;
                    }
                    result += BitConverter.ToChar(c, i);
                }
            }
            else
            {
                foreach (byte b in bytes)
                {
                    result += b.ToString("X2");
                }
            }
            reader.Close();
            return result;
        }

        public static string ReadHexUTF8(string path, Int32 startOffset, Int32 endOffset, bool decode)
        {
            if (!File.Exists(path)) { throw new FileNotFoundException($"Could not find {path}"); }

            string result = "";

            List<byte> bytes = new List<byte>();
            BinaryReader reader = new BinaryReader(File.OpenRead(path));
            reader.BaseStream.Position = startOffset;

            for (int i = startOffset; i < endOffset; i++)
            {
                bytes.Add(reader.ReadByte());
            }

            if (decode)
            {
                result = Encoding.UTF8.GetString(bytes.ToArray());
            }
            else
            {
                foreach (byte b in bytes)
                {
                    result += b.ToString("X2");
                }
            }
            reader.Close();
            return result;
        }

        public static void KillNinfs()
        {
            foreach (Process prs in Process.GetProcessesByName("ninfs"))
            {
                prs.Kill();
            }
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string GetContentIndex(string contentDir)
        {
            string output = "";
            string indexOne = contentDir.Split('.')[0];
            string indexTwo = contentDir.Split('.')[1];

            if (contentDir == "0000.00000000")
            {
                return "0:0";
            }

            for (int i = 0; i < indexOne.Length; i++)
            {
                if (indexOne == "0000")
                {
                    output += "0x0";
                    break;
                }

                if (indexOne[i] != '0')
                {
                    string currentIndex = indexOne.Substring(i);

                    output += $"0x{currentIndex}";
                    break;
                }
            }

            output += ":";

            for (int i = 0; i < indexTwo.Length; i++)
            {
                if (indexTwo == "00000000")
                {
                    output += "0x0";
                    break;
                }

                if (indexTwo[i] != '0')
                {
                    string currentIndex = indexTwo.Substring(i);

                    output += $"0x{currentIndex}";
                    break;
                }
            }
            return output;
        }

        public static bool IsValidDecKey(string decKey) => Regex.IsMatch(decKey, @"\A\b[0-9a-fA-F]+\b\Z") && decKey.Length == 32;

        public static string[] GetValidCdnContentFolders(string path)
        {
            string[] dirs = Directory.GetDirectories(path);
            List<string> validOutputContentDirs;

            if (dirs.Length < 1 || !dirs.All(c => new DirectoryInfo(c).Name.StartsWith("0004")))
            {
                throw new ArgumentException("The path that was entered does not contain any valid CDN Content subdirectories.");
            }

            validOutputContentDirs = new List<string>();

            foreach (string contentDir in dirs)
            {
                if (File.Exists($"{contentDir}/tmd"))
                {
                    validOutputContentDirs.Add(contentDir);
                }
            }

            return validOutputContentDirs.ToArray();
        }

        public static List<DecKeyModel> ReadDecKeyDatabase(string path)
        {
            if (!File.Exists(path)) { throw new FileNotFoundException($"Could not find {path}"); }

            string[] lines = File.ReadAllLines(path);
            List<DecKeyModel> keys;

            if (lines.Any(c => c.Split(',').Length - 1 > 2))
            {
                throw new ArgumentException("The specified database file has lines that contain more than two commas and therefore can not be used. Please either fix the file (remove unneccessary commas) or use a good file.");
            }

            keys = new List<DecKeyModel>();

            foreach (string line in lines)
            {
                string[] key = line.Split(',');
                if (IsValidDecKey(key[1]))
                {
                    keys.Add(new DecKeyModel(key[0], key[1]));
                }
            }

            return keys;
        }

        public static string StripDoubleQuotes(string input) => input.Replace("\"", "");
    }
}