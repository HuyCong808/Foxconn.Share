using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.Editor
{
    public class DataBase
    {
        private List<Data> _data { get; set; }

        public List<Data> Data
        {
            get => _data;
            set => _data = value;
        }

        public DataBase()
        {
            _data = new List<Data>();
        }


        //      public static DataBase Current => __current;
        //      private static DataBase __current = new DataBase();
        ////      public DataBase() { }
        //      static DataBase() { }

        public void Save()
        {
            try
            {
                string _filepath = @"params\database.json";
                string data = JsonConvert.SerializeObject(this);
                File.WriteAllText(_filepath, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //public void Load()
        //{
        //    try
        //    {
        //        DataBase _database = null;
        //        string _filepath = @"params\database.json";
        //        if(File.Exists(_filepath))
        //        {
        //            _database = JsonConvert.DeserializeObject<DataBase>(File.ReadAllText(_filepath));
        //        }
        //        else
        //        {
        //            _database = new DataBase();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        public void AddData(string sn)
        {
            int num = Data.Count;
            Data item = new Data()
            {
                SN = sn,
            };
            _data.Add(item);
        }

        public void RemoveData(Data sn)
        {
            _data.Remove(sn);
        }

    }

    public class Data
    {
        public string SN { get; set; }
        public Data()
        {
            SN = "";
        }
    }
}
