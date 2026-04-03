using System;
using System.IO;
using System.Threading.Tasks;

namespace fast_json_oim
{
    internal class Program
    {
        private static readonly string WatchFolder = @"C:\IMPORTACIONS-OIM";
        private static readonly string OutputFolder = Path.Combine(WatchFolder, "JsonFiles");

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to fast JSON");

            // Create directories if they don't exist
            if (!Directory.Exists(WatchFolder))
            {
                Directory.CreateDirectory(WatchFolder);
                Console.WriteLine($"📁 Created directory: {WatchFolder}");
            }
            else
            {
                Console.WriteLine($"📁 Directory already exists: {WatchFolder}");
            }

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                Console.WriteLine($"📁 Created directory: {OutputFolder}");
            }
            else
            {
                Console.WriteLine($"📁 Directory already exists: {OutputFolder}");
            }

            using var watcher = new FileSystemWatcher(WatchFolder)
            {
                Filter = "*.txt",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            watcher.Created += (_, e) => HandleNewFile(e.FullPath);
            watcher.Renamed += (_, e) => HandleNewFile(e.FullPath);

            Console.WriteLine($"Monitoring folder: {WatchFolder}");
            Console.WriteLine("Drop a .txt file there to generate JSON automatically.");
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static void HandleNewFile(string filePath)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"Detected file: {filePath}");

                    // ✅ espera o arquivo estar pronto (parar de ser copiado)
                    FileProcessor.WaitUntilFileIsReady(filePath);

                    // ✅ extrai as 2 listas (Policies e Endossos)
                    var (policies, endorsements) = FileProcessor.ExtractPoliciesAndEndorsements(filePath);

                    var baseName = Path.GetFileNameWithoutExtension(filePath);

                    // ✅ gera múltiplos arquivos JSON para Policies (divididos por MaxPolicyNumbers)
                    var policyJsonFiles = GenerationJsonBuilder.BuildMultiplePolicyJsonFiles(policies);
                    for (int i = 0; i < policyJsonFiles.Count; i++)
                    {
                        var (json, count, fileIndex) = policyJsonFiles[i];
                        var fileName = policyJsonFiles.Count > 1 
                            ? $"generation_policies_{baseName}_part{fileIndex:D3}.json"
                            : $"generation_policies_{baseName}.json";
                        var outputPath = Path.Combine(OutputFolder, fileName);
                        File.WriteAllText(outputPath, json);
                        Console.WriteLine($"✅ Policies JSON #{fileIndex} generated: {count} items");
                        Console.WriteLine(outputPath);
                    }

                    // ✅ gera múltiplos arquivos JSON para Endorsements (divididos por MaxEndorsementNumbers)
                    var endorsementJsonFiles = GenerationJsonBuilder.BuildMultipleEndorsementJsonFiles(endorsements);
                    for (int i = 0; i < endorsementJsonFiles.Count; i++)
                    {
                        var (json, count, fileIndex) = endorsementJsonFiles[i];
                        var fileName = endorsementJsonFiles.Count > 1 
                            ? $"generation_endorsements_{baseName}_part{fileIndex:D3}.json"
                            : $"generation_endorsements_{baseName}.json";
                        var outputPath = Path.Combine(OutputFolder, fileName);
                        File.WriteAllText(outputPath, json);
                        Console.WriteLine($"✅ Endorsements JSON #{fileIndex} generated: {count} items");
                        Console.WriteLine(outputPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing {filePath}: {ex.Message}");
                }
            });
        }
    }
}
