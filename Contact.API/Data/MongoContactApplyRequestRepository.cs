using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Models;
using MongoDB.Driver;

namespace Contact.API.Data
{
    public class MongoContactApplyRequestRepository : IContacaApplyRequestRepository
    {
        private readonly ContactContext _contactContext;
        public MongoContactApplyRequestRepository(ContactContext contactContext, CancellationToken cancellationToken)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {            
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == request.UserId && r.ApplierId == request.ApplierId);

            if (await _contactContext.ContactApplyRequests.CountAsync(filter) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update.Set(r => r.ApplyTime, DateTime.Now);

                var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
                return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
            }

            await _contactContext.ContactApplyRequests.InsertOneAsync(request, null, cancellationToken);
            return true;
        }

        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == userId && r.ApplierId == applierId);
            var update = Builders<ContactApplyRequest>.Update
                .Set(r => r.Approvaled, 1)
                .Set(r => r.ApplyTime, DateTime.Now);

            var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);

            return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
        }

        public async Task<List<ContactApplyRequest>> GetRequestListAsync(int userId, CancellationToken cancellationToken)
        {
            return (await _contactContext.ContactApplyRequests.FindAsync(r => r.UserId == userId)).ToList(cancellationToken);
        }
    }
}
