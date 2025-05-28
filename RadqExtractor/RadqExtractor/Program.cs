using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System;

namespace RadqExtractor
{
    internal class Program
    {
        public static string[] Files { get; set; } = [];

        static async Task Main(string[] args)
        {
            var files = args.Where(File.Exists);
            var tasks = files.Select(ProcessFile);
            await Task.WhenAll(tasks);
            Console.WriteLine($"Finished processing {files.Count()} file(s), press any key to exit.");
            Console.ReadKey();
        }

        public static async Task ProcessFile(string file)
        {
            Console.WriteLine($"Processing file: {file}");
            try
            {
                byte[] inputBytes = await File.ReadAllBytesAsync(file);
                if (inputBytes.Length == 0)
                {
                    Console.WriteLine($"Skipping empty file: {file}");
                }
                else
                {
                    await DumpCleanedExtendedRadq(inputBytes, file);
                    Console.WriteLine($"File finished: {file}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File {file} failed:\n{ex}");
            }
        }

        public static byte[] CleanExtendedRadq(byte[] raw)
        {
            if (raw[0] == 80 && raw[1] == 75 && raw[2] == 3)
            {
                return raw;
            }
            for (int i40 = 0; i40 < raw.Length; ++i40)
            {
                if (raw[i40] == 75)
                {
                    raw[i40] = 85;
                }
                else if (raw[i40] == 85)
                {
                    raw[i40] = 75;
                }

                if (raw[i40] == 36)
                {
                    raw[i40] = 64;
                }
                else if (raw[i40] == 64)
                {
                    raw[i40] = 36;
                }

                if (raw[i40] == 53)
                {
                    raw[i40] = 19;
                }
                else if (raw[i40] == 19)
                {
                    raw[i40] = 53;
                }

                if (raw[i40] == 21)
                {
                    raw[i40] = 44;
                }
                else if (raw[i40] == 44)
                {
                    raw[i40] = 21;
                }

                if (raw[i40] == 59)
                {
                    raw[i40] = 72;
                }
                else if (raw[i40] == 72)
                {
                    raw[i40] = 59;
                }

                if (raw[i40] == 11)
                {
                    raw[i40] = 49;
                }
                else if (raw[i40] == 49)
                {
                    raw[i40] = 11;
                }

                if (raw[i40] == 13)
                {
                    raw[i40] = 68;
                }
                else if (raw[i40] == 68)
                {
                    raw[i40] = 13;
                }
            }
            return raw;
        }

        public static async Task DumpCleanedExtendedRadq(byte[] raw, string file)
        {
            await File.WriteAllBytesAsync($"{file}.zip", CleanExtendedRadq(raw));
        }

        public static async Task ExtractExtendedRadq(byte[] raw, string file)
        {
            var guid = Guid.NewGuid().ToString();
            var fileDirectory = $"{Path.GetFileName(file)}_{guid}".Replace(".", "_");

            ZipInputStream zipInputStream = new ZipInputStream(new MemoryStream(CleanExtendedRadq(raw)));

            for (ZipEntry zipentry = zipInputStream.GetNextEntry(); zipentry != null; zipentry = zipInputStream.GetNextEntry())
            {
                byte[] result_byte = new byte[zipentry.Size];
                await zipInputStream.ReadAsync(result_byte);

                string entryName = zipentry.Name;
                entryName = String.IsNullOrEmpty(entryName) ? guid : entryName;

                string directory = Path.Combine("output", fileDirectory);
                string outputPath = Path.Combine("output", fileDirectory, entryName);
                Directory.CreateDirectory(directory);
                Console.WriteLine($"Entry: {zipentry.Name} Output: {outputPath}");

                await File.WriteAllBytesAsync(outputPath, result_byte);
            }
            zipInputStream.Close();
        }
    }
}
