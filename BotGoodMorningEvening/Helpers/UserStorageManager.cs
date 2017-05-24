using BotGoodMorningEvening.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotGoodMorningEvening.Helpers
{
    public static class UserStorageManager
    {
        const string botGmePartitionKey = "botGmeTableKey";

        private static CloudTableClient tableClient { get; set; }
        private static CloudTable UserTable { get; set; }

        static UserStorageManager()
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            tableClient = storageAccount.CreateCloudTableClient();

            // Create User Table
            UserTable = tableClient.GetTableReference("User");

            // Create the table if it doesn't exist.
            UserTable.CreateIfNotExistsAsync();
        }

        public static void AddOrUpdateUser(string userId, string userName, string botId, string botName,
            string serviceUrl, int? gmtPlus, string channelId)
        {
            try
            {
                //if current user exist in table storage, we update user's data esle we create the new user
                var currentUser = GetUser(userId);
                if(currentUser == null)
                {
                    // Create a user entity.
                    currentUser = new UserEntity(botGmePartitionKey, userId);
                    currentUser.UserId = userId;
                    currentUser.UserName = userName;
                    currentUser.BotId = botId;
                    currentUser.BotName = botName;
                    currentUser.ServiceURL = serviceUrl;
                    currentUser.Gmtplus = gmtPlus;
                    currentUser.ChannelId = channelId;
                    currentUser.CardsCache = string.Empty;
                    currentUser.ResumptionCookie = string.Empty;

                    // insert user or replace
                    TableOperation insertOperation = TableOperation.Insert(currentUser);
                    UserTable.Execute(insertOperation);
                }
                else
                {
                    currentUser.UserId = userId;
                    currentUser.UserName = userName;
                    currentUser.BotId = botId;
                    currentUser.BotName = botName;
                    currentUser.ServiceURL = serviceUrl;
                    currentUser.Gmtplus = gmtPlus;
                    currentUser.ChannelId = channelId;

                    UpdateUser(currentUser);
                }
                
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static List<UserEntity> GetUsers()
        {
            try
            {
                //get All users from "botGmePartitionKey" partition Key
                TableQuery<UserEntity> query = new TableQuery<UserEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, botGmePartitionKey));
                return UserTable.ExecuteQuery(query).ToList<UserEntity>();

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static UserEntity GetUser(string userId)
        {
            try
            {
                // Create a retrieve operation that takes a User entity.
                TableOperation retrieveOperation = TableOperation.Retrieve<UserEntity>(botGmePartitionKey,userId);

                // Execute the retrieve operation.
                TableResult retrievedResult = UserTable.Execute(retrieveOperation);

                // Print the phone number of the result.
                if (retrievedResult.Result != null)
                {
                    return retrievedResult.Result as UserEntity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static void UpdateUser (UserEntity user)
        {
            try
            {
                if (user != null)
                {
                    // Create the update TableOperation.
                    TableOperation updateOperation = TableOperation.Replace(user);

                    // Execute the operation.
                    UserTable.Execute(updateOperation);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            
        }
    }
}