﻿using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace WebApiClientCore.Benchmarks.Requests
{
    /// <summary> 
    /// 跳过真实的http请求环节的模拟Post表单请求
    /// </summary>
    [MemoryDiagnoser]
    public class PutFormBenchmark : Benchmark
    {
        /// <summary>
        /// 使用WebApiClientCore请求
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public async Task<Model> WebApiClientCore_PutFormAsync()
        {
            using var scope = this.ServiceProvider.CreateScope();
            var benchmarkApi = scope.ServiceProvider.GetRequiredService<IWebApiClientCoreApi>();
            var input = new Model { A = "a" };
            return await benchmarkApi.PutFormAsync("id001", input);
        }


        [Benchmark]
        public async Task<Model> Refit_PutFormAsync()
        {
            using var scope = this.ServiceProvider.CreateScope();
            var benchmarkApi = scope.ServiceProvider.GetRequiredService<IRefitApi>();
            var input = new Model { A = "a" };
            return await benchmarkApi.PutFormAsync("id001", input);
        }
    }
}
