using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Rosalind.Core.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;

namespace Rosalind.Core.Services
{
    public class SqlService
    {
        private readonly Setting _setting;

        public SqlService(IServiceProvider service)
        {
            _setting = service.GetRequiredService<Setting>();
        }

        /// <summary>
        /// 길드 테이블에 새로운 유저 추가
        /// </summary>
        /// <param name="guildId">길드 아이디</param>
        /// <param name="userId">추가할 유저 아이디</param>
        public User AddNewUser(ulong guildId, ulong userId)
        {
            var _connection = new MySqlConnection(_setting.Config.ConnectionString);
            var _db = new QueryFactory(_connection, new MySqlCompiler());

            if (_db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
                AddNewGuild(guildId);

            var user = _db.Query($"TABLE_{guildId}")
                .AsInsert(new User() { UserId = userId })
                .First<User>();

            _connection.Close();

            return user;
        }

        /// <summary>
        /// 길드를 데이터베이스에 새로 추가합니다.
        /// </summary>
        /// <param name="guildid">길드 아이디</param>
        public void AddNewGuild(ulong guildid)
        {
            var _connection = new MySqlConnection(_setting.Config.ConnectionString);
            var _db = new QueryFactory(_connection, new MySqlCompiler());

            _db.Statement($"CREATE TABLE TABLE_{guildid} (ID INT PRIMARY KEY AUTO_INCREMENT, USERID BIGINT NOT NULL, COIN BIGINT NOT NULL DEFAULT 0) ENGINE = INNODB;");

            _connection.Close();
        }

        /// <summary>
        /// 랭킹을 데이터베이스로 부터 가져옵니다.
        /// </summary>
        /// <param name="guildId">길드 아이디</param>
        /// <param name="limit">랭킹 유저의 수</param>
        /// <returns></returns>
        public List<User> GetRanking(ulong guildId, int limit)
        {
            var _connection = new MySqlConnection(_setting.Config.ConnectionString);
            var _db = new QueryFactory(_connection, new MySqlCompiler());

            if (_db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
                AddNewGuild(guildId);

            var users = _db.Query($"TABLE_{guildId}")
                            .Where("MONEY", "NOT 0")
                            .OrderBy("MONEY DESC")
                            .Limit(limit)
                            .Get<User>();

            _connection.Close();

            return new List<User>(users);
        }

        /// <summary>
        /// 유저를 데이터베이스에서 가져와 객체로 반환합니다.
        /// </summary>
        /// <param name="guildId">길드의 아이디</param>
        /// <param name="userId">유저의 아이디</param>
        /// <returns>유저 객체를 반환합니다.</returns>
        public User GetUser(ulong guildId, ulong userId)
        {
            var _connection = new MySqlConnection(_setting.Config.ConnectionString);
            var _db = new QueryFactory(_connection, new MySqlCompiler());

            if (_db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
                AddNewGuild(guildId);

            if (_db.Query($"TABLE_{guildId}").Where("USERID", userId).Exists() != true)
                AddNewUser(guildId, userId);

            var user = _db.Query($"TABLE_{guildId}")
                        .Select()
                        .Where("USERID", userId)
                        .First<User>();

            _connection.Close();

            return user;
        }

        /// <summary>
        /// 유저에게 코인을 추가합니다.
        /// </summary>
        /// <param name="guildId">길드 아이디</param>
        /// <param name="userId">유저 아이디</param>
        /// <param name="coin">추가할 코인의 양</param>
        public void AddUserCoin(ulong guildId, ulong userId, ulong coin)
        {
            var _connection = new MySqlConnection(_setting.Config.ConnectionString);
            var _db = new QueryFactory(_connection, new MySqlCompiler());

            if (_db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
                AddNewGuild(guildId);

            if (_db.Query($"TABLE_{guildId}").Where("USERID", userId).Exists() != true)
                AddNewUser(guildId, userId);

            var user = GetUser(guildId, userId);

            _db.Query($"TABLE_{guildId}")
                .Where("USERID", userId)
                .Update(new User() { Coin = user.Coin + coin });

            _connection.Close();
        }

        /// <summary>
        /// 유저에게서 코인을 가져갑니다.
        /// </summary>
        /// <param name="guildId">길드 아이디</param>
        /// <param name="userId">유저 아이디</param>
        /// <param name="coin">추가할 코인의 량</param>
        public void SubUserCoin(ulong guildId, ulong userId, ulong coin)
        {
            var _connection = new MySqlConnection(_setting.Config.ConnectionString);
            var _db = new QueryFactory(_connection, new MySqlCompiler());

            if (_db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
                AddNewGuild(guildId);

            if (_db.Query($"TABLE_{guildId}").Where("USERID", userId).Exists() != true)
                AddNewUser(guildId, userId);

            var user = GetUser(guildId, userId);

            _db.Query($"TABLE_{guildId}")
                .Where("USERID", userId)
                .Update(new User() { Coin = user.Coin - coin });

            _connection.Close();
        }
    }
}