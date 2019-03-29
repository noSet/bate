using Contact.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contact.API.Data
{
    public class ContactContext
    {
        private IMongoDatabase _database;
        private AppSettings _appSettings;

        public ContactContext(IOptionsSnapshot<AppSettings> settings)
        {
            _appSettings = settings.Value;
            var client = new MongoClient(_appSettings.MongoContactConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(_appSettings.MongoContactDatabase);
            }
        }

        private void CheckAndCreateCollection(string collectionName)
        {
            var collectionList = _database.ListCollections().ToList();
            var collectionNames = new List<string>();

            collectionList.ForEach(b => collectionNames.Add(b["name"].AsString));
            if (!collectionNames.Contains(collectionName))
            {
                _database.CreateCollection(collectionName);
            }
        }

        /// <summary>
        /// 用户通讯录
        /// </summary>
        public IMongoCollection<ContactBook> ContactBooks
        {
            get
            {
                CheckAndCreateCollection(nameof(ContactBooks));
                return _database.GetCollection<ContactBook>(nameof(ContactBooks));
            }
        }

        /// <summary>
        /// 好友申请请求记录
        /// </summary>
        public IMongoCollection<ContactApplyRequest> ContactApplyRequests
        {
            get
            {
                CheckAndCreateCollection(nameof(ContactBooks));
                return _database.GetCollection<ContactApplyRequest>(nameof(ContactApplyRequests));
            }
        }
    }
}
