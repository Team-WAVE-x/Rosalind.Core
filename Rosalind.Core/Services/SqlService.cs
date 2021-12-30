using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Rosalind.Core.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;

namespace Rosalind.Core.Services;

/// <summary>
/// SQL 작업을 수행하는 서비스입니다.
/// </summary>
public class SqlService
{
    private readonly Setting _setting;

    /// <summary>
    /// 서비스의 진입점입니다. SQL 연결에 사용할 설정을 불러옵니다.
    /// </summary>
    /// <param name="service"></param>
    public SqlService(IServiceProvider service)
    {
        _setting = service.GetRequiredService<Setting>();
    }

    /// <summary>
    /// 길드 테이블에 새로운 유저 추가합니다.
    /// </summary>
    /// <param name="guildId">길드 아이디</param>
    /// <param name="userId">추가할 유저 아이디</param>
    private void AddNewUser(ulong guildId, ulong userId)
    {
        var connection = new MySqlConnection(_setting.Config.ConnectionString);
        var db = new QueryFactory(connection, new MySqlCompiler());

        if (db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
            AddNewGuild(guildId);

        db.Query($"TABLE_{guildId}")
            .AsInsert(new User { UserId = userId })
            .First<User>();

        connection.Close();
    }

    /// <summary>
    /// 길드를 데이터베이스에 새로 추가합니다.
    /// </summary>
    /// <param name="guildid">길드 아이디</param>
    private void AddNewGuild(ulong guildid)
    {
        var connection = new MySqlConnection(_setting.Config.ConnectionString);
        var db = new QueryFactory(connection, new MySqlCompiler());

        db.Statement($"CREATE TABLE TABLE_{guildid} (ID INT PRIMARY KEY AUTO_INCREMENT, USERID BIGINT NOT NULL, COIN BIGINT NOT NULL DEFAULT 0) ENGINE = INNODB;");

        connection.Close();
    }

    /// <summary>
    /// 랭킹을 데이터베이스로 부터 가져옵니다.
    /// </summary>
    /// <param name="guildId">길드 아이디</param>
    /// <param name="limit">랭킹 유저의 수</param>
    /// <returns>유저 리스트를 반환합니다.</returns>
    public List<User> GetRanking(ulong guildId, int limit)
    {
        var connection = new MySqlConnection(_setting.Config.ConnectionString);
        var db = new QueryFactory(connection, new MySqlCompiler());

        if (db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
            AddNewGuild(guildId);

        var users = db.Query($"TABLE_{guildId}")
            .Where("MONEY", "NOT 0")
            .OrderBy("MONEY DESC")
            .Limit(limit)
            .Get<User>();

        connection.Close();

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
        var connection = new MySqlConnection(_setting.Config.ConnectionString);
        var db = new QueryFactory(connection, new MySqlCompiler());

        if (db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
            AddNewGuild(guildId);

        if (db.Query($"TABLE_{guildId}").Where("USERID", userId).Exists() != true)
            AddNewUser(guildId, userId);

        var user = db.Query($"TABLE_{guildId}")
            .Select()
            .Where("USERID", userId)
            .First<User>();

        connection.Close();

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
        var connection = new MySqlConnection(_setting.Config.ConnectionString);
        var db = new QueryFactory(connection, new MySqlCompiler());

        if (db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
            AddNewGuild(guildId);

        if (db.Query($"TABLE_{guildId}").Where("USERID", userId).Exists() != true)
            AddNewUser(guildId, userId);

        var user = GetUser(guildId, userId);

        db.Query($"TABLE_{guildId}")
            .Where("USERID", userId)
            .Update(new User { Coin = user.Coin + coin });

        connection.Close();
    }

    /// <summary>
    /// 유저에게서 코인을 가져갑니다.
    /// </summary>
    /// <param name="guildId">길드 아이디</param>
    /// <param name="userId">유저 아이디</param>
    /// <param name="coin">추가할 코인의 량</param>
    public void SubUserCoin(ulong guildId, ulong userId, ulong coin)
    {
        var connection = new MySqlConnection(_setting.Config.ConnectionString);
        var db = new QueryFactory(connection, new MySqlCompiler());

        if (db.Query("information_schema.tables").Where("table_schema", "stonks_db").Where("table_name", $"TABLE_{guildId}").Count<int>() == 0)
            AddNewGuild(guildId);

        if (db.Query($"TABLE_{guildId}").Where("USERID", userId).Exists() != true)
            AddNewUser(guildId, userId);

        var user = GetUser(guildId, userId);

        db.Query($"TABLE_{guildId}")
            .Where("USERID", userId)
            .Update(new User { Coin = user.Coin - coin });

        connection.Close();
    }
}