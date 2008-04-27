using System;

namespace NHibernate.Search.Attributes {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndexedAttribute : Attribute {
        private string index = null;

        public string Index {
            get { return index; }
            set { index = value; }
        }
    }
}