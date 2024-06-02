﻿using BenchmarkDotNet.Attributes;
using System;
using WebApiClientCore.Internals;

namespace WebApiClientCore.Benchmarks.CreateInstances
{
    [InProcess]
    public class Benchmarks : IBenchmark
    {
        private readonly Func<int, Model> ctor = LambdaUtil.CreateCtorFunc<int, Model>(typeof(Model));

        [Benchmark]
        public void ActivatorCreate()
        {
            typeof(Model).CreateInstance<Model>(1);
        }

        [Benchmark]
        public void LabdaCreate()
        {
            ctor.Invoke(1);
        }

        public class Model
        {
            private readonly int value;

            public Model(int value)
            {
                this.value = value;
            }
        }
    }
}
