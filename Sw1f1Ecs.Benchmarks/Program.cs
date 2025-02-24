using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Sw1f1.Ecs.Benchmarks {
    class Program {
        static void Main(string[] args) {
            var config = ManualConfig.CreateMinimumViable()
                .WithOptions(ConfigOptions.DisableOptimizationsValidator);
            
            BenchmarkRunner.Run<Sw1f1EcsBanchmarks>(config);
        }
    }
}
