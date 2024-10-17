﻿
using StackExchange.Redis;
using Store.Repository.Basket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Store.Repository.Basket
{
    public class BasketRepository : IBasketRepository
    {

        private readonly IDatabase _database;
        public BasketRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public Task<bool> DeleteCustomerAsync(string basketId)
            => _database.KeyDeleteAsync(basketId);

        public async Task<CustomerBasket> GetCustomerAsync(string basketId)
        {
            var basket = await _database.StringGetAsync(basketId);

            return basket.IsNullOrEmpty ? null : JsonSerializer.Deserialize<CustomerBasket>(basket);
        }

        public async Task<CustomerBasket> UpdateCustomerAsync(CustomerBasket basket)
        {
            var isCreated = await _database.StringSetAsync(basket.Id, JsonSerializer.Serialize(basket), TimeSpan.FromDays(30));

            if (!isCreated)
                return null;

            return await GetCustomerAsync(basket.Id);

        }
    }
}
