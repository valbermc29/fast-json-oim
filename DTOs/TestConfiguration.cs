using System;
using System.IO;
using System.Text.Json;

namespace fast_json_oim.DTOs
{
    /// <summary>
    /// Classe para gerenciar configurações de teste.
    /// Lê o arquivo test-config.json para definir limites e LotSize.
    /// Se os valores estiverem vazios ou null, usa os valores padrão.
    /// </summary>
    public class TestConfiguration
    {
        private static TestConfiguration? _instance;
        private static readonly object _lock = new object();
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-config.json");

        public int? MaxPolicyNumbers { get; set; }
        public int? MaxEndorsementNumbers { get; set; }
        public int? LotSize { get; set; }

        /// <summary>
        /// Carrega a configuração do arquivo test-config.json.
        /// Se o arquivo não existir, retorna uma instância com valores padrão (null).
        /// </summary>
        public static TestConfiguration Load()
        {
            if (_instance != null)
                return _instance;

            lock (_lock)
            {
                if (_instance != null)
                    return _instance;

                _instance = new TestConfiguration();

                if (!File.Exists(ConfigFilePath))
                {
                    // Cria arquivo padrão se não existir
                    _instance.Save();
                    return _instance;
                }

                try
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<TestConfigurationDto>(json);

                    if (config != null)
                    {
                        // Converte strings vazias ou null para null (int?)
                        _instance.MaxPolicyNumbers = ParseIntOrNull(config.MaxPolicyNumbers);
                        _instance.MaxEndorsementNumbers = ParseIntOrNull(config.MaxEndorsementNumbers);
                        _instance.LotSize = ParseIntOrNull(config.LotSize);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erro ao carregar configuração: {ex.Message}. Usando valores padrão.");
                }

                return _instance;
            }
        }

        /// <summary>
        /// Salva a configuração atual no arquivo test-config.json.
        /// </summary>
        public void Save()
        {
            try
            {
                var dto = new TestConfigurationDto
                {
                    MaxPolicyNumbers = MaxPolicyNumbers?.ToString() ?? "",
                    MaxEndorsementNumbers = MaxEndorsementNumbers?.ToString() ?? "",
                    LotSize = LotSize?.ToString() ?? ""
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(dto, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro ao salvar configuração: {ex.Message}");
            }
        }

        /// <summary>
        /// Reseta a instância (útil para testes).
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        private static int? ParseIntOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(value, out int result) && result > 0)
                return result;

            return null;
        }

        private class TestConfigurationDto
        {
            public string? MaxPolicyNumbers { get; set; }
            public string? MaxEndorsementNumbers { get; set; }
            public string? LotSize { get; set; }
        }
    }
}
