using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Foxconn.App.Models
{
    public class StorageData
    {
        public class StorageDataCustom
        {
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime DateTime { get; set; }
            public string String { get; set; }

            public StorageDataCustom()
            {
                DateTime = DateTime.Now;
                String = null;
            }

            public StorageDataCustom Clone()
            {
                return new StorageDataCustom()
                {
                    DateTime = DateTime,
                    String = String,
                };
            }
        }

        public BsonObjectId _id { get; set; }
        public string ModelName { get; set; }
        public List<StorageDataCustom> Data { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateCreated { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateModified { get; set; }

        public StorageData()
        {
            ModelName = string.Empty;
            Data = new List<StorageDataCustom>();
            DateCreated = DateTime.Now.Date;
            DateModified = DateTime.Now;
        }

        public StorageData Clone()
        {
            return new StorageData()
            {
                ModelName = ModelName,
                Data = Data != null ? new List<StorageDataCustom>() { new StorageDataCustom().Clone() } : null,
                DateCreated = DateCreated,
                DateModified = DateModified,
            };
        }
    }
}
