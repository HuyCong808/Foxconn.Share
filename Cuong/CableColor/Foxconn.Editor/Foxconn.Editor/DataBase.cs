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
            var item = _data.Find(x => x.SN == sn);
            if (item == null)
            {
                int num = Data.Count;
                Data data = new Data()
                {
                    SN = sn,
                };
                _data.Add(data);
            }
        }

        public void RemoveData(Data sn)
        {
            _data.Remove(sn);
        }
    }

    public class Data
    {
        public string SN { get; set; }
        public bool IsStep1 { get; set; }
        public bool IsStep2 { get; set; }
        public Data()
        {
            SN = "";
            IsStep1 = false;
            IsStep2 = false;

        }
    }
}
