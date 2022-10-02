﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace NHibernate.Search.Tests.Inheritance
{
    using System.Threading.Tasks;
    [TestFixture]
    public class ProxyObjectInheritanceTestAsync : SearchTestCase
    {
        protected override IEnumerable<string> Mappings
        {
            get
            {
                return new string[]
                           {
                               "Inheritance.Animal.hbm.xml",
                               "Inheritance.Mammal.hbm.xml",
                           };
            }
        }

        [Test]
        public async Task ProxyObjectInheritanceAsync()
        {
            // This will test subclassed proxy objects and make sure that they are index correctly.
            IFullTextSession s = Search.CreateFullTextSession(OpenSession());
            ITransaction tx = s.BeginTransaction();

            // Create an object in db
            Mammal temp = new Mammal();
            temp.NumberOfLegs = (4);
            temp.Name = ("Some Mammal Name Here");
            await (s.SaveAsync(temp));

            await (tx.CommitAsync()); //post commit events for lucene

            // Clear object from cache by clearing session
            s.Clear();

            // This should return a proxied object
            Mammal mammal = await (s.LoadAsync<Mammal>(temp.Id));

            // Index the proxied object
            s.Index(mammal);

            // Build an index reader
            var reader = s.SearchFactory.ReaderProvider.OpenReader(s.SearchFactory.GetDirectoryProviders(typeof(Mammal)));

            // Get the last document indexed
            var Document = reader.Document(reader.MaxDoc - 1);

            // get the class name field from the document
            string classTypeThatWasIndex = Document.Get(NHibernate.Search.Engine.DocumentBuilder.CLASS_FIELDNAME);

            // get the expected lucene type name (this should be equivilent to 
            // the static method of NHibernate.Search.Util.TypeHelper.LuceneTypeName
            string expectedLuceneTypeName = typeof(Mammal).FullName + ", " + typeof(Mammal).Assembly.GetName().Name;

            Assert.AreEqual(expectedLuceneTypeName, classTypeThatWasIndex);

            // Tidyup
            tx = s.BeginTransaction();
            await (s.DeleteAsync("from System.Object"));
            await (tx.CommitAsync());
            s.Close();
        }
    }
}