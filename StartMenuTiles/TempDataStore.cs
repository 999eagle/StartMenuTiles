using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartMenuTiles
{
    class TempDataStore
    {
        Dictionary<int, object> m_store;
        private TempDataStore()
        {
            m_store = new Dictionary<int, object>();
        }

        public int StoreObject(object data)
        {
            int hash = data.GetHashCode();
            m_store.Add(hash, data);
            return hash;
        }

        public object GetObject(int key)
        {
            return m_store[key];
        }

        static TempDataStore m_instance;
        public static TempDataStore GetInstance()
        {
            return m_instance ?? (m_instance = new TempDataStore());
        }
    }
}
