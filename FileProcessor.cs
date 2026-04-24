using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fast_json_oim
{
    public class FileProcessor
    {
        // Mantive seu método (usado se você ainda quiser gerar só Policies)
        public static List<string> GetPolicyNumbersWhereLastSixIsZero(string filePath)
        {
            return File.ReadLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line =>
                {
                    var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) return null;

                    var firstValue = parts[0].Trim();
                    var lastSix = NormalizeSixDigits(parts[1].Trim());

                    return lastSix == "000000" ? firstValue : null;
                })
                .Where(x => x != null)
                .Distinct()
                .ToList()!;
        }

        // ✅ NOVO: retorna 2 listas a partir do TXT original
        // Policies: lastSix == 000000 => guarda apenas o firstValue (remove zeros)
        // Endorsements: lastSix != 000000 => guarda firstValue + lastSix (concatenado completo)
        public static (List<string> Policies, List<string> Endorsements) ExtractPoliciesAndEndorsements(string filePath)
        {
            var policies = new HashSet<string>(StringComparer.Ordinal);
            var endorsements = new HashSet<string>(StringComparer.Ordinal);

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                var firstValue = parts[0].Trim();
                var lastSix = NormalizeSixDigits(parts[1].Trim());

                if (lastSix == "000000")
                {
                    // ✅ PolicyNumbers recebe somente o base
                    policies.Add(firstValue);
                }
                else
                {
                    // ✅ EndorsementNumbers recebe o número completo concatenado
                    endorsements.Add(firstValue + lastSix);
                }
            }

            return (policies.ToList(), endorsements.ToList());
        }

        /// <summary>
        /// TXT do fluxo BAIXAS-AP: um número de apólice por linha (sem tab).
        /// </summary>
        public static List<string> ReadBaixasApPolicyNumbers(string filePath)
        {
            return File.ReadLines(filePath)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();
        }

        // ✅ garante que o sufixo tem exatamente 6 dígitos
        private static string NormalizeSixDigits(string value)
        {
            var v = value.Trim();

            if (v.Length < 6) v = v.PadLeft(6, '0');
            if (v.Length > 6) v = v[^6..]; // pega últimos 6 se vier maior por algum motivo

            return v;
        }

        public static void WaitUntilFileIsReady(string filePath)
        {
            for (int i = 0; i < 30; i++) // ~15s
            {
                try
                {
                    using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    return;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            throw new IOException("File is still locked after waiting. Maybe it is still being copied.");
        }
    }
}
