using System;

namespace fast_json_oim.DTOs
{
    /// <summary>
    /// Classe para otimizar o tamanho do lote (LotSize) baseado no número total de apólices.
    /// Segue as recomendações de produção: 500-600 para volumes de 10k, 20k, 25k+ apólices.
    /// </summary>
    public static class LotSizeOptimizer
    {
        /// <summary>
        /// Tamanhos de lote recomendados para produção (em ordem de prioridade).
        /// </summary>
        private static readonly int[] RecommendedLotSizes = { 600, 500 };

        /// <summary>
        /// Tamanhos de lote alternativos para desenvolvimento/homologação.
        /// </summary>
        private static readonly int[] DevelopmentLotSizes = { 200, 250, 400 };

        /// <summary>
        /// Tamanhos de lote para alto volume de produção.
        /// </summary>
        private static readonly int[] HighVolumeLotSizes = { 1000 };

        /// <summary>
        /// Otimiza o tamanho do lote baseado no número total de apólices.
        /// Retorna o tamanho otimizado seguindo as recomendações de produção.
        /// </summary>
        /// <param name="totalPolicies">Número total de apólices a processar</param>
        /// <param name="environment">Ambiente: Production (padrão), Development, HighVolume</param>
        /// <returns>Tamanho do lote otimizado (recomendado: 500 ou 600 para produção)</returns>
        public static int OptimizeLotSize(int totalPolicies, OptimizationEnvironment environment = OptimizationEnvironment.Production)
        {
            if (totalPolicies <= 0)
                return RecommendedLotSizes[1]; // Retorna 500 como padrão mínimo

            int[] lotSizes = environment switch
            {
                OptimizationEnvironment.Development => DevelopmentLotSizes,
                OptimizationEnvironment.HighVolume => HighVolumeLotSizes,
                _ => RecommendedLotSizes // Production
            };

            // Usa a lógica do BatchPlanner para encontrar o melhor tamanho
            var bestPlan = BatchPlanner.RecommendPlan(totalPolicies, lotSizes);
            return bestPlan.LotSize;
        }

        /// <summary>
        /// Obtém informações detalhadas sobre o plano de otimização.
        /// </summary>
        /// <param name="totalPolicies">Número total de apólices a processar</param>
        /// <param name="environment">Ambiente: Production (padrão), Development, HighVolume</param>
        /// <returns>Plano completo com informações sobre arquivos gerados, último arquivo, etc.</returns>
        public static BatchPlanner.BatchPlan GetOptimizedPlan(int totalPolicies, OptimizationEnvironment environment = OptimizationEnvironment.Production)
        {
            if (totalPolicies <= 0)
            {
                return BatchPlanner.CreatePlan(0, RecommendedLotSizes[1]);
            }

            int[] lotSizes = environment switch
            {
                OptimizationEnvironment.Development => DevelopmentLotSizes,
                OptimizationEnvironment.HighVolume => HighVolumeLotSizes,
                _ => RecommendedLotSizes // Production
            };

            return BatchPlanner.RecommendPlan(totalPolicies, lotSizes);
        }

        /// <summary>
        /// Ambiente de otimização.
        /// </summary>
        public enum OptimizationEnvironment
        {
            /// <summary>
            /// Ambiente de produção - usa tamanhos recomendados (500-600)
            /// </summary>
            Production,

            /// <summary>
            /// Ambiente de desenvolvimento/testes - usa tamanhos menores (200-400)
            /// </summary>
            Development,

            /// <summary>
            /// Alto volume de produção - usa tamanhos maiores (1000)
            /// </summary>
            HighVolume
        }
    }
}
