﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using Lucene.Net.Util;

namespace NHibernate.Search.Tests.Session
{
    using System.Collections;

    using Impl;

    using Lucene.Net.Analysis;
    using Lucene.Net.QueryParsers;

    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    public class OptimizeTestAsync : PhysicalTestCase
    {
        protected override IEnumerable<string> Mappings
        {
            get { return new string[] { "Session.Email.hbm.xml" }; }
        }

        [Test]
        public async Task OptimizeAsync()
        {
            IFullTextSession s = Search.CreateFullTextSession(OpenSession());
            ITransaction tx = s.BeginTransaction();
            int loop = 2000;
            for (int i = 0; i < loop; i++)
            {
                await (s.PersistAsync(new Email(i + 1, "JBoss World Berlin", "Meet the guys who wrote the software")));
            }

            await (tx.CommitAsync());
            s.Close();

            s = Search.CreateFullTextSession(OpenSession());
            tx = s.BeginTransaction();
            s.SearchFactory.Optimize(typeof(Email));
            await (tx.CommitAsync());
            s.Close();

            // Check non-indexed object get indexed by s.index;
            s = new FullTextSessionImpl(OpenSession());
            tx = s.BeginTransaction();
            QueryParser parser = new QueryParser(Version.LUCENE_30, "id", new StopAnalyzer(Version.LUCENE_30));
            int result = (await (s.CreateFullTextQuery(parser.Parse("Body:wrote")).ListAsync())).Count;
            Assert.AreEqual(2000, result);

            await (s.DeleteAsync("from System.Object"));
            await (tx.CommitAsync());
            s.Close();
        }
    }
}