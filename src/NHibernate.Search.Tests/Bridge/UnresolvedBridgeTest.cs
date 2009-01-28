using System.Collections;
using NHibernate.Cfg;
using NHibernate.Search.Impl;
using NHibernate.Search.Store;
using NUnit.Framework;

namespace NHibernate.Search.Tests.Bridge
{
    [TestFixture]
    public class UnresolvedBridgeTest : SearchTestCase
    {
        protected override IList Mappings
        {
            get { return new string[] {}; }
        }

        [Test, ExpectedException(typeof(HibernateException), ExpectedMessage = "Unable to guess IFieldBridge for Id")]
        public void SystemTypeForDocumentId()
        {
            Configuration tempCfg = new Configuration();
            tempCfg.Configure();
            tempCfg.SetProperty("hibernate.search.default.directory_provider", typeof(RAMDirectoryProvider).AssemblyQualifiedName);
            tempCfg.AddClass(typeof(Gangster));
            tempCfg.BuildSessionFactory();
            Assert.Fail("Undefined bridge went through (Gangster.Id's type is System.Type, which shouldn't be recognized)");
        }
    }
}