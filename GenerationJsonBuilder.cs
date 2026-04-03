using fast_json_oim.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fast_json_oim
{
    public static class GenerationJsonBuilder
    {
        public static string BuildPolicyJson(List<string> policyNumbers)
        {
            var (startDate, endDate) = GetCurrentMonthRange();

            // ✅ Carrega configuração de teste
            var config = TestConfiguration.Load();

            // ✅ Aplica limite de apólices se especificado no arquivo de configuração
            var finalPolicyNumbers = policyNumbers;
            if (config.MaxPolicyNumbers.HasValue && config.MaxPolicyNumbers.Value > 0)
            {
                finalPolicyNumbers = policyNumbers.Take(config.MaxPolicyNumbers.Value).ToList();
                Console.WriteLine($"📋 Limite de apólices aplicado: {finalPolicyNumbers.Count} de {policyNumbers.Count} (máximo: {config.MaxPolicyNumbers.Value})");
            }

            // ✅ Usa LotSize do arquivo de configuração, senão otimiza automaticamente
            int lotSize;
            if (config.LotSize.HasValue && config.LotSize.Value > 0)
            {
                lotSize = config.LotSize.Value;
                Console.WriteLine($"⚙️ LotSize do arquivo de configuração: {lotSize}");
            }
            else
            {
                lotSize = LotSizeOptimizer.OptimizeLotSize(finalPolicyNumbers.Count);
                Console.WriteLine($"⚙️ LotSize otimizado automaticamente: {lotSize} (baseado em {finalPolicyNumbers.Count} apólices)");
            }

            var payload = new Root
            {
                Generations = new List<Generation>
                {
                    new Generation
                    {
                        Description = "Policy files generation - Normal processing (no batching)",
                        Services = DefaultServices(),
                        StartDate = startDate,
                        EndDate = endDate,
                        PolicyNumbers = finalPolicyNumbers,
                        ClaimNumbers = new List<string>(),
                        TreatyCodes = new List<string>(),
                        BorderauxNumbers = new List<string>(),
                        EndorsementNumbers = new List<string>(),
                        PolicyNumbersNotExported = new List<string>(),
                        OutputDirectory = @"\\itgappprod\ITG_Arquivos\OIMx\_FilaDeProcessamento\",
                        PorItem = false,
                        LotSize = lotSize
                    }
                }
            };

            return Serialize(payload);
        }

        /// <summary>
        /// Gera múltiplos arquivos JSON, dividindo as apólices em lotes de MaxPolicyNumbers.
        /// Cada arquivo JSON terá no máximo MaxPolicyNumbers de apólices.
        /// </summary>
        public static List<(string Json, int PolicyCount, int FileIndex)> BuildMultiplePolicyJsonFiles(List<string> policyNumbers)
        {
            var config = TestConfiguration.Load();
            var results = new List<(string Json, int PolicyCount, int FileIndex)>();

            // Se MaxPolicyNumbers não estiver configurado, retorna um único arquivo
            if (!config.MaxPolicyNumbers.HasValue || config.MaxPolicyNumbers.Value <= 0)
            {
                var singleJson = BuildPolicyJson(policyNumbers);
                results.Add((singleJson, policyNumbers.Count, 1));
                return results;
            }

            int maxPerFile = config.MaxPolicyNumbers.Value;
            int totalFiles = (int)Math.Ceiling((double)policyNumbers.Count / maxPerFile);

            Console.WriteLine($"📦 Dividindo {policyNumbers.Count} apólices em {totalFiles} arquivo(s) (máximo {maxPerFile} por arquivo)");

            // ✅ Usa LotSize do arquivo de configuração
            int lotSize;
            if (config.LotSize.HasValue && config.LotSize.Value > 0)
            {
                lotSize = config.LotSize.Value;
                Console.WriteLine($"⚙️ LotSize do arquivo de configuração: {lotSize}");
            }
            else
            {
                // Se não tiver LotSize configurado, usa o tamanho do lote para cada arquivo
                lotSize = LotSizeOptimizer.OptimizeLotSize(maxPerFile);
                Console.WriteLine($"⚙️ LotSize otimizado automaticamente: {lotSize} (baseado em {maxPerFile} apólices por arquivo)");
            }

            var (startDate, endDate) = GetCurrentMonthRange();

            for (int i = 0; i < totalFiles; i++)
            {
                var batch = policyNumbers.Skip(i * maxPerFile).Take(maxPerFile).ToList();
                
                var payload = new Root
                {
                    Generations = new List<Generation>
                    {
                        new Generation
                        {
                            Description = "Policy files generation - Normal processing (no batching)",
                            Services = DefaultServices(),
                            StartDate = startDate,
                            EndDate = endDate,
                            PolicyNumbers = batch,
                            ClaimNumbers = new List<string>(),
                            TreatyCodes = new List<string>(),
                            BorderauxNumbers = new List<string>(),
                            EndorsementNumbers = new List<string>(),
                            PolicyNumbersNotExported = new List<string>(),
                            OutputDirectory = @"\\itgappprod\ITG_Arquivos\OIMx\_FilaDeProcessamento\",
                            PorItem = false,
                            LotSize = lotSize
                        }
                    }
                };

                var json = Serialize(payload);
                results.Add((json, batch.Count, i + 1));
            }

            return results;
        }

        public static string BuildEndorsementJson(List<string> endorsementNumbers)
        {
            var (startDate, endDate) = GetCurrentMonthRange();

            // ✅ Carrega configuração de teste
            var config = TestConfiguration.Load();

            // ✅ Aplica limite de endossos se especificado no arquivo de configuração
            var finalEndorsementNumbers = endorsementNumbers;
            if (config.MaxEndorsementNumbers.HasValue && config.MaxEndorsementNumbers.Value > 0)
            {
                finalEndorsementNumbers = endorsementNumbers.Take(config.MaxEndorsementNumbers.Value).ToList();
                Console.WriteLine($"📋 Limite de endossos aplicado: {finalEndorsementNumbers.Count} de {endorsementNumbers.Count} (máximo: {config.MaxEndorsementNumbers.Value})");
            }

            // ✅ Usa LotSize do arquivo de configuração, senão otimiza automaticamente
            int lotSize;
            if (config.LotSize.HasValue && config.LotSize.Value > 0)
            {
                lotSize = config.LotSize.Value;
                Console.WriteLine($"⚙️ LotSize do arquivo de configuração: {lotSize}");
            }
            else
            {
                lotSize = LotSizeOptimizer.OptimizeLotSize(finalEndorsementNumbers.Count);
                Console.WriteLine($"⚙️ LotSize otimizado automaticamente: {lotSize} (baseado em {finalEndorsementNumbers.Count} endossos)");
            }

            var payload = new Root
            {
                Generations = new List<Generation>
                {
                    new Generation
                    {
                        Description = "Policy files generation - Normal processing (no batching)",
                        Services = DefaultServices(),
                        StartDate = startDate,
                        EndDate = endDate,
                        PolicyNumbers = new List<string>(),
                        ClaimNumbers = new List<string>(),
                        TreatyCodes = new List<string>(),
                        BorderauxNumbers = new List<string>(),
                        EndorsementNumbers = finalEndorsementNumbers,
                        PolicyNumbersNotExported = new List<string>(),
                        OutputDirectory = @"\\itgappprod\ITG_Arquivos\OIMx\_FilaDeProcessamento\",
                        PorItem = false,
                        LotSize = lotSize
                    }
                }
            };

            return Serialize(payload);
        }

        /// <summary>
        /// Gera múltiplos arquivos JSON, dividindo os endossos em lotes de MaxEndorsementNumbers.
        /// Cada arquivo JSON terá no máximo MaxEndorsementNumbers de endossos.
        /// </summary>
        public static List<(string Json, int EndorsementCount, int FileIndex)> BuildMultipleEndorsementJsonFiles(List<string> endorsementNumbers)
        {
            var config = TestConfiguration.Load();
            var results = new List<(string Json, int EndorsementCount, int FileIndex)>();

            // Se MaxEndorsementNumbers não estiver configurado, retorna um único arquivo
            if (!config.MaxEndorsementNumbers.HasValue || config.MaxEndorsementNumbers.Value <= 0)
            {
                var singleJson = BuildEndorsementJson(endorsementNumbers);
                results.Add((singleJson, endorsementNumbers.Count, 1));
                return results;
            }

            int maxPerFile = config.MaxEndorsementNumbers.Value;
            int totalFiles = (int)Math.Ceiling((double)endorsementNumbers.Count / maxPerFile);

            Console.WriteLine($"📦 Dividindo {endorsementNumbers.Count} endossos em {totalFiles} arquivo(s) (máximo {maxPerFile} por arquivo)");

            // ✅ Usa LotSize do arquivo de configuração
            int lotSize;
            if (config.LotSize.HasValue && config.LotSize.Value > 0)
            {
                lotSize = config.LotSize.Value;
                Console.WriteLine($"⚙️ LotSize do arquivo de configuração: {lotSize}");
            }
            else
            {
                // Se não tiver LotSize configurado, usa o tamanho do lote para cada arquivo
                lotSize = LotSizeOptimizer.OptimizeLotSize(maxPerFile);
                Console.WriteLine($"⚙️ LotSize otimizado automaticamente: {lotSize} (baseado em {maxPerFile} endossos por arquivo)");
            }

            var (startDate, endDate) = GetCurrentMonthRange();

            for (int i = 0; i < totalFiles; i++)
            {
                var batch = endorsementNumbers.Skip(i * maxPerFile).Take(maxPerFile).ToList();
                
                var payload = new Root
                {
                    Generations = new List<Generation>
                    {
                        new Generation
                        {
                            Description = "Policy files generation - Normal processing (no batching)",
                            Services = DefaultServices(),
                            StartDate = startDate,
                            EndDate = endDate,
                            PolicyNumbers = new List<string>(),
                            ClaimNumbers = new List<string>(),
                            TreatyCodes = new List<string>(),
                            BorderauxNumbers = new List<string>(),
                            EndorsementNumbers = batch,
                            PolicyNumbersNotExported = new List<string>(),
                            OutputDirectory = @"\\itgappprod\ITG_Arquivos\OIMx\_FilaDeProcessamento\",
                            PorItem = false,
                            LotSize = lotSize
                        }
                    }
                };

                var json = Serialize(payload);
                results.Add((json, batch.Count, i + 1));
            }

            return results;
        }

        private static (DateTime start, DateTime end) GetCurrentMonthRange()
        {
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
            var end = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);
            return (start, end);
        }

        private static List<string> DefaultServices() => new()
        {
            "apl01","parc01","cms01","cced01","cob01","itcomp01",
            "itauto01","rsg01","franq01","pess01","itemb01"
        };

        private static string Serialize(Root payload)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            return JsonSerializer.Serialize(payload, options);
        }
    }
}
