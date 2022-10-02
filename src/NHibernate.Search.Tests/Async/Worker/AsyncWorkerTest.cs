﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Search.Tests.Worker
{
    using System.Threading.Tasks;
    [TestFixture]
    public class AsyncWorkerTestAsync : WorkerTestCaseAsync
    {
        protected override void Configure(Configuration configuration)
        {
            base.Configure(configuration);
            configuration.SetProperty("hibernate.search.default.directory_provider", typeof(Store.RAMDirectoryProvider).AssemblyQualifiedName);
            configuration.SetProperty(Environment.AnalyzerClass, typeof(Lucene.Net.Analysis.StopAnalyzer).AssemblyQualifiedName);
            configuration.SetProperty(Environment.WorkerScope, "transaction");
            configuration.SetProperty(Environment.WorkerExecution, "async");
            configuration.SetProperty(Environment.WorkerThreadPoolSize, "1");
            configuration.SetProperty(Environment.WorkerThreadPoolSize, "10");
        }
    }
}