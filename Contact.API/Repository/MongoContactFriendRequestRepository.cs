﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contact.API.Data;
using Contact.API.Data.Enum;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Contact.API.Repository
{
    public class MongoContactFriendRequestRepository : IContactFriendRequestRepository
    {
        private readonly MongoContactDbContext _dbContext;

        public MongoContactFriendRequestRepository(MongoContactDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<FriendRequest>> GetFriendRequestListAsync(int userId)
        {
            var friendRequests = await _dbContext.FriendRequestCollection.FindSync(x => x.UserId == userId).ToListAsync();
            return friendRequests;
        }

        public async Task AddFriendAsync(FriendRequest request)
        {
            var friendRequests = await _dbContext.FriendRequestCollection.FindSync(x => x.UserId == request.UserId && x.AppliedUserId == request.AppliedUserId).ToListAsync();
            if (!friendRequests.Any())
            {
                await _dbContext.FriendRequestCollection.InsertOneAsync(request);
            }
            else
            {
                await _dbContext.FriendRequestCollection.UpdateOneAsync(
                    x => x.UserId == request.UserId && x.AppliedUserId == request.AppliedUserId,
                    Builders<FriendRequest>.Update.Set(x => x.ApplyDateTime, DateTime.Now));
            }
        }

        public async Task PassFriendRequestAsync(int userId, int appliedUserId)
        {
            BsonDocumentUpdateDefinition<FriendRequest> updateDefinition =
                new BsonDocumentUpdateDefinition<FriendRequest>(new BsonDocument(new Dictionary<string, object>()
                {
                    {"ApplyStatus",(int)ApplyStatus.Passed },
                    {"UpdateTime",DateTime.Now }
                }));
            await _dbContext.FriendRequestCollection.UpdateOneAsync(
                x => x.UserId == userId && x.AppliedUserId == appliedUserId,
                updateDefinition);
        }

        public async Task RejectFriendRequestAsync(int userId, int appliedUserId)
        {
            BsonDocumentUpdateDefinition<FriendRequest> updateDefinition =
                new BsonDocumentUpdateDefinition<FriendRequest>(new BsonDocument(new Dictionary<string, object>()
                {
                    {"ApplyStatus",(int)ApplyStatus.Reject },
                    {"UpdateTime",DateTime.Now }
                }));
            await _dbContext.FriendRequestCollection.UpdateOneAsync(
                x => x.UserId == userId && x.AppliedUserId == appliedUserId,
                updateDefinition);
        }
    }
}