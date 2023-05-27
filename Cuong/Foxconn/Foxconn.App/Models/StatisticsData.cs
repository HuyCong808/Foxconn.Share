using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Foxconn.App.Models
{
    public class StatisticsData
    {
        public BsonObjectId _id { get; set; }
        public string ModelName { get; set; }
        public int Total { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public List<string> History { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateCreated { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateModified { get; set; }

        public StatisticsData()
        {
            ModelName = string.Empty;
            Total = 0;
            Pass = 0;
            Fail = 0;
            History = new List<string>();
            DateCreated = DateTime.Now.Date;
            DateModified = DateTime.Now;
        }

        public StatisticsData Clone()
        {
            return new StatisticsData()
            {
                ModelName = ModelName,
                Total = Total,
                Pass = Pass,
                Fail = Fail,
                History = History != null ? new List<string>() { null } : null,
                DateCreated = DateCreated,
                DateModified = DateModified,
            };
        }
    }
}
