using System;
using System.Collections.Generic;

namespace fast_json_oim.DTOs
{
    public static class BatchPlanner
    {
        public sealed class BatchPlan
        {
            public int TotalPolicies { get; init; }
            public int LotSize { get; init; }
            public int FilesGenerated { get; init; }
            public int PoliciesPerFile { get; init; }
            public int LastFilePolicies { get; init; }
            public IReadOnlyList<int> BatchSizes { get; init; } = Array.Empty<int>();
        }

        /// <summary>
        /// Gera o "DE/PARA" (quantidade total -> quantos lotes/arquivos e tamanho do último lote).
        /// </summary>
        public static BatchPlan CreatePlan(int totalPolicies, int lotSize)
        {
            if (totalPolicies < 0) throw new ArgumentOutOfRangeException(nameof(totalPolicies));
            if (lotSize <= 0) throw new ArgumentOutOfRangeException(nameof(lotSize));
            if (totalPolicies == 0)
            {
                return new BatchPlan
                {
                    TotalPolicies = 0,
                    LotSize = lotSize,
                    FilesGenerated = 0,
                    PoliciesPerFile = lotSize,
                    LastFilePolicies = 0,
                    BatchSizes = Array.Empty<int>()
                };
            }

            int files = (int)Math.Ceiling(totalPolicies / (double)lotSize);
            int remainder = totalPolicies % lotSize;
            int last = remainder == 0 ? lotSize : remainder;

            var batches = BuildBatchSizes(totalPolicies, lotSize);

            return new BatchPlan
            {
                TotalPolicies = totalPolicies,
                LotSize = lotSize,
                FilesGenerated = files,
                PoliciesPerFile = lotSize,
                LastFilePolicies = last,
                BatchSizes = batches
            };
        }

        /// <summary>
        /// Recomendação automática (produção): escolhe entre 500 e 600 (ou lista customizada).
        /// Regra eficiente: minimiza quantidade de arquivos e deixa o último lote o maior possível.
        /// </summary>
        public static BatchPlan RecommendPlan(
            int totalPolicies,
            int[]? preferredLotSizes = null)
        {
            preferredLotSizes ??= new[] { 600, 500 }; // prioriza 600, depois 500

            if (preferredLotSizes.Length == 0)
                throw new ArgumentException("preferredLotSizes cannot be empty.");

            BatchPlan? best = null;

            foreach (var size in preferredLotSizes)
            {
                var plan = CreatePlan(totalPolicies, size);

                if (best is null)
                {
                    best = plan;
                    continue;
                }

                // 1) Menos arquivos é melhor
                if (plan.FilesGenerated < best.FilesGenerated)
                {
                    best = plan;
                    continue;
                }

                // 2) Se empatar, último lote maior é melhor (menos "arquivo pequeno")
                if (plan.FilesGenerated == best.FilesGenerated && plan.LastFilePolicies > best.LastFilePolicies)
                {
                    best = plan;
                    continue;
                }

                // 3) Se ainda empatar, lote maior (tende a ser mais "padrão produção")
                if (plan.FilesGenerated == best.FilesGenerated &&
                    plan.LastFilePolicies == best.LastFilePolicies &&
                    plan.LotSize > best.LotSize)
                {
                    best = plan;
                }
            }

            return best!;
        }

        /// <summary>
        /// Retorna a sequência de tamanhos dos lotes. Ex:
        /// total=20000, lot=600 => [600 x33, 200]
        /// </summary>
        public static IReadOnlyList<int> BuildBatchSizes(int totalPolicies, int lotSize)
        {
            var list = new List<int>();
            if (totalPolicies <= 0) return list;

            int remaining = totalPolicies;
            while (remaining > 0)
            {
                int batch = Math.Min(lotSize, remaining);
                list.Add(batch);
                remaining -= batch;
            }

            return list;
        }

        /// <summary>
        /// Divide uma lista em lotes (útil para gerar N JSONs).
        /// </summary>
        public static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> items, int lotSize)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (lotSize <= 0) throw new ArgumentOutOfRangeException(nameof(lotSize));

            for (int i = 0; i < items.Count; i += lotSize)
            {
                int size = Math.Min(lotSize, items.Count - i);
                var chunk = new List<T>(size);
                for (int j = 0; j < size; j++)
                    chunk.Add(items[i + j]);

                yield return chunk;
            }
        }
    }
}
