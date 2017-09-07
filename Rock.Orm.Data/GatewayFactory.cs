using System;
using System.Collections.Generic;
using System.Data.Common;
namespace Rock.Orm.Data
{
    public static class GatewayFactory
    {
        static Dictionary<string, Gateway> _gateways = new Dictionary<string, Gateway>();
        //private static Dictionary<string, DbTransaction> _transDictionary = new Dictionary<string, DbTransaction>();
        //private static Dictionary<string, DateTime> _transTime = new Dictionary<string, DateTime>();
        private static Dictionary<string, string> _connStr = new Dictionary<string, string>();

        public static Gateway Default
        {
            get
            {
                return GetGateway("Default");
            }
        }

        public static Gateway Base
        {
            get
            {
                return GetGateway("Base");
            }
        }

        public static string DefaultConnectionString
        {
            get
            {
                string connectionString;
                if (!_connStr.TryGetValue("Default",out connectionString))
                {
                    connectionString = DbProviderFactory.CreateConnectionString("Default");
                    _connStr.Add("Default", connectionString);
                }

                return connectionString;

            }
        }

        public static Gateway GetGateway(string connStrName)
        {
            Gateway gateway = null;
            if (!_gateways.TryGetValue(connStrName, out gateway))
            {
                gateway = new Gateway(connStrName);
                _gateways.Add(connStrName, gateway);
            }
            return gateway;
        }

        //public static void SetGateway(string connStrName, Gateway gateway)
        //{

        //    ////写法1
        //    //if (!_gateways.ContainsKey(connStrName))
        //    //{
        //    //    _gateways.Add(connStrName, gateway);
        //    //}
        //    //else
        //    //{
        //    //    _gateways[connStrName] = gateway;
        //    //}

        //    //写法2
        //    _gateways[connStrName] = gateway;
        //}

        //public static void BeginTransaction(string key)
        //{
        //    //Gateway gateway = null;
        //    //if (!_gateways.TryGetValue("Default", out gateway))
        //    //{
        //    //    _gateways.Add("Default", new Gateway("Default"));
        //    //}

        //    //DbTransaction trans = _gateways["Default"].BeginTransaction();
        //    //_transDictionary.Add(key, trans);
        //    //_transTime.Add(key, DateTime.Now);
        //    BeginTransaction(key, "Default");
        //}

        //public static void BeginTransaction(string key, string connStrName)
        //{
        //    //DbTransaction trans = GetGateway(connStrName).BeginTransaction();
        //    //_transDictionary.Add(key, trans);
        //    //_transTime.Add(key, DateTime.Now);
        //}

        //public static DbTransaction GetTransaction(string key)
        //{
        //    //DbTransaction value = null;
        //    //if (_transDictionary.TryGetValue(key, out value))
        //    //{
        //    //    return value;
        //    //}
        //    //else
        //    //{
        //    //    return null;
        //    //}
        //}

        //public static DbTransaction GetTransaction()
        //{
        //    return _gateways["Default"].BeginTransaction();
        //}

        //public static void Commit(string key)
        //{
        //    //DbTransaction value = null;
        //    //if (_transDictionary.TryGetValue(key, out value))
        //    //{
        //    //    _transDictionary[key].Commit();
        //    //    _transDictionary[key].Dispose();
        //    //    _transDictionary.Remove(key);
        //    //}

        //    ////顺便清理过期的事务
        //    //CleanTransaction();
        //}

        //public static void Rollback(string key)
        //{
        //    //DbTransaction value = null;
        //    //if (_transDictionary.TryGetValue(key,out value))
        //    //{
        //    //    _transDictionary[key].Rollback();
        //    //    _transDictionary[key].Dispose();
        //    //    _transDictionary.Remove(key);
        //    //}
        //}

        //public static void CleanTransaction()
        //{
        //    foreach (var key in _transTime.Keys)
        //    {
        //        TimeSpan ts = DateTime.Now - _transTime[key];
        //        if (ts.Minutes > 10)
        //        {
        //            Rollback(key);
        //            _transDictionary[key].Dispose();
        //            _transDictionary.Remove(key);
        //        }
        //    }
        //}
    }
}
