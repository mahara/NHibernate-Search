﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using NHibernate.Cfg;
using NHibernate.Search.Store;
using NUnit.Framework;

namespace NHibernate.Search.Tests.DirectoryProvider
{
    using System.Threading.Tasks;
    [TestFixture]
    public class FSSlaveAndMasterDPTestAsync : MultiplySessionFactoriesTestCase
    {
        protected override IEnumerable<string> Mappings
        {
            get { return new string[] { "DirectoryProvider.SnowStorm.hbm.xml" }; }
        }

        protected override int NumberOfSessionFactories
        {
            get { return 2; }
        }

        /// <summary>
        /// Verify that copies of the master get properly copied to the slaves.
        /// </summary>
        [Test]
        public async Task ProperCopyAsync()
        {
            // Assert that the slave index is empty
            IFullTextSession fullTextSession = Search.CreateFullTextSession(GetSlaveSession());
            ITransaction tx = fullTextSession.BeginTransaction();
            QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "id", new StopAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
            IList result = await (fullTextSession.CreateFullTextQuery(parser.Parse("Location:texas")).ListAsync());
            Assert.AreEqual(0, result.Count, "No copy yet, fresh index expected");
            await (tx.CommitAsync());
            fullTextSession.Close();

            // create an entity on the master and persist it in order to index it
            ISession session = CreateSession(0);
            tx = session.BeginTransaction();
            SnowStorm sn = new SnowStorm();
            sn.DateTime = DateTime.Now;
            sn.Location = ("Dallas, TX, USA");
            await (session.PersistAsync(sn));
            await (tx.CommitAsync());
            session.Close();

            int waitPeriodMilli = 2*1*1000 + 10; //wait a bit more than 2 refresh (one master / one slave)
            await (Task.Delay(waitPeriodMilli));

            // assert that the master has indexed the snowstorm
            fullTextSession = Search.CreateFullTextSession(GetMasterSession());
            tx = fullTextSession.BeginTransaction();
            result = await (fullTextSession.CreateFullTextQuery(parser.Parse("Location:dallas")).ListAsync());
            Assert.AreEqual(1, result.Count, "Original should get one");
            await (tx.CommitAsync());
            fullTextSession.Close();

            // assert that index got copied to the slave as well
            fullTextSession = Search.CreateFullTextSession(GetSlaveSession());
            tx = fullTextSession.BeginTransaction();
            result = await (fullTextSession.CreateFullTextQuery(parser.Parse("Location:dallas")).ListAsync());
            Assert.AreEqual(1, result.Count, "First copy did not work out");
            await (tx.CommitAsync());
            fullTextSession.Close();

            // add a new snowstorm to the master
            session = GetMasterSession();
            tx = session.BeginTransaction();
            sn = new SnowStorm();
            sn.DateTime = DateTime.Now;
            sn.Location = ("Chennai, India");
            await (session.PersistAsync(sn));
            await (tx.CommitAsync());
            session.Close();

            await (Task.Delay(waitPeriodMilli)); //wait a bit more than 2 refresh (one master / one slave)

            // assert that the new snowstorm made it into the slave
            fullTextSession = Search.CreateFullTextSession(GetSlaveSession());
            tx = fullTextSession.BeginTransaction();
            result = await (fullTextSession.CreateFullTextQuery(parser.Parse("Location:chennai")).ListAsync());
            Assert.AreEqual(1, result.Count, "Second copy did not work out");
            await (tx.CommitAsync());
            fullTextSession.Close();

            session = GetMasterSession();
            tx = session.BeginTransaction();
            sn = new SnowStorm();
            sn.DateTime = DateTime.Now;
            sn.Location = ("Melbourne, Australia");
            await (session.PersistAsync(sn));
            await (tx.CommitAsync());
            session.Close();

            await (Task.Delay(waitPeriodMilli)); //wait a bit more than 2 refresh (one master / one slave)

            // once more - assert that the new snowstorm made it into the slave
            fullTextSession = Search.CreateFullTextSession(GetSlaveSession());
            tx = fullTextSession.BeginTransaction();
            result = await (fullTextSession.CreateFullTextQuery(parser.Parse("Location:melbourne")).ListAsync());
            Assert.AreEqual(1, result.Count, "Third copy did not work out");
            await (tx.CommitAsync());
            fullTextSession.Close();
        }

        #region Helper methods

        public override void FixtureSetUp()
        {
            ZapLuceneStore();

            base.FixtureSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            ZapLuceneStore();
        }

        protected override void Configure(IList<Configuration> cfg)
        {
            // master
            cfg[0].SetProperty("hibernate.search.default.sourceBase", $"./lucenedirs/{nameof(FSSlaveAndMasterDPTestAsync)}/master/copy");
            cfg[0].SetProperty("hibernate.search.default.indexBase", $"./lucenedirs/{nameof(FSSlaveAndMasterDPTestAsync)}/master/main");
            cfg[0].SetProperty("hibernate.search.default.refresh", "1"); // 1 sec
            cfg[0].SetProperty("hibernate.search.default.directory_provider", typeof(FSMasterDirectoryProvider).AssemblyQualifiedName);

            // slave(s)
            cfg[1].SetProperty("hibernate.search.default.sourceBase", $"./lucenedirs/{nameof(FSSlaveAndMasterDPTestAsync)}/master/copy");
            cfg[1].SetProperty("hibernate.search.default.indexBase", $"./lucenedirs/{nameof(FSSlaveAndMasterDPTestAsync)}/slave");
            cfg[1].SetProperty("hibernate.search.default.refresh", "1"); // 1sec
            cfg[1].SetProperty("hibernate.search.default.directory_provider", typeof(FSSlaveDirectoryProvider).AssemblyQualifiedName);
        }

        private ISession GetMasterSession()
        {
            return CreateSession(0);
        }

        private ISession GetSlaveSession()
        {
            return CreateSession(1);
        }

        private ISession CreateSession(int sessionFactoryNumber)
        {
            return SessionFactories[sessionFactoryNumber].OpenSession();
        }

        private void ZapLuceneStore()
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    if (Directory.Exists("./lucenedirs/"))
                    {
                        Directory.Delete("./lucenedirs/", true);
                    }
                }
                catch (IOException)
                {
                    // Wait for it to wind down for a while
                    Thread.Sleep(1000);
                }
            }
        }

        #endregion
    }
}