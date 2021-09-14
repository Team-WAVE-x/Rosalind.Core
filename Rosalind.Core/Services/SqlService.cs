using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Rosalind.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

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
        /// <param name="guildid">길드 아이디</param>
        /// <param name="userid">추가할 유저 아이디</param>
        public User AddNewUser(ulong guildId, ulong userId)
        {
            User user = null;

            try
            {
                long insertedId = 0;

                using (MySqlConnection _connection = new MySqlConnection(_setting.Config.ConnectionString))
                {
                    _connection.Open();

                    using (MySqlCommand sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = _connection;
                        sqlCom.CommandText = $"INSERT INTO TABLE_{guildId} (USERID, MONEY) VALUES(@USERID, 0)";
                        sqlCom.Parameters.AddWithValue("@USERID", userId);
                        sqlCom.CommandType = CommandType.Text;
                        sqlCom.ExecuteNonQuery();

                        insertedId = sqlCom.LastInsertedId;
                    }

                    _connection.Close();
                }

                user = new User(
                    id: Convert.ToUInt64(insertedId),
                    guildId: guildId,
                    userId: userId,
                    coin: 0
                );
            }
            catch (MySqlException ex)
            {
                switch ((MySqlErrorCode)ex.Number)
                {
                    case MySqlErrorCode.NoSuchTable:
                        AddNewGuild(guildId);
                        user = AddNewUser(guildId, userId);
                        break;

                    default:
                        throw new Exception($"처리되지 못한 예외가 발생하였습니다. ({ex.Number}, {ex.Message})");
                }
            }

            return user;
        }

        /// <summary>
        /// 길드를 데이터베이스에 새로 추가합니다.
        /// </summary>
        /// <param name="guildid">길드 아이디</param>
        public void AddNewGuild(ulong guildid)
        {
            try
            {
                using (MySqlConnection _connection = new MySqlConnection(_setting.Config.ConnectionString))
                {
                    _connection.Open();

                    using (MySqlCommand sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = _connection;
                        sqlCom.CommandText = $"CREATE TABLE TABLE_{guildid} (_ID INT PRIMARY KEY AUTO_INCREMENT, USERID BIGINT NOT NULL, MONEY BIGINT NOT NULL) ENGINE = INNODB;";
                        sqlCom.CommandType = CommandType.Text;
                        sqlCom.ExecuteNonQuery();
                    }

                    _connection.Close();
                }
            }
            catch (MySqlException ex)
            {
                switch ((MySqlErrorCode)ex.Number)
                {
                    case MySqlErrorCode.TableExists:
                        throw new Exception("길드가 이미 존재합니다.");

                    default:
                        throw new Exception($"처리되지 못한 예외가 발생하였습니다. ({ex.Number}, {ex.Message})");
                }
            }
        }

        /// <summary>
        /// 랭킹을 데이터베이스로 부터 가져옵니다.
        /// </summary>
        /// <param name="guildid">길드 아이디</param>
        /// <param name="limit">랭킹 유저의 수</param>
        /// <returns></returns>
        public List<User> GetRanking(ulong guildId, int limit)
        {
            List<User> users = new List<User>();

            try
            {
                using (MySqlConnection _connection = new MySqlConnection(_setting.Config.ConnectionString))
                {
                    _connection.Open();

                    using (MySqlCommand sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = _connection;
                        sqlCom.CommandText = $"SELECT * FROM TABLE_{guildId} WHERE NOT MONEY=0 ORDER BY MONEY DESC LIMIT @LIMIT;";
                        sqlCom.Parameters.AddWithValue("@LIMIT", limit);
                        sqlCom.CommandType = CommandType.Text;

                        using (MySqlDataReader reader = sqlCom.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User(
                                    id: Convert.ToUInt64(reader["_ID"]),
                                    guildId: guildId,
                                    userId: Convert.ToUInt64(reader["USERID"]),
                                    coin: Convert.ToUInt64(reader["MONEY"])
                                ));
                            }
                        }
                    }

                    _connection.Close();
                }
            }
            catch (MySqlException ex)
            {
                switch ((MySqlErrorCode)ex.Number)
                {
                    case MySqlErrorCode.NoSuchTable:
                        AddNewGuild(guildId);
                        users = GetRanking(guildId, limit);
                        break;

                    default:
                        throw new Exception($"처리되지 못한 예외가 발생하였습니다. ({ex.Number}, {ex.Message})");
                }
            }

            return users;
        }

        /// <summary>
        /// 유저를 데이터베이스에서 가져와 객체로 반환합니다.
        /// </summary>
        /// <param name="guildId">길드의 아이디</param>
        /// <param name="userId">유저의 아이디</param>
        /// <returns></returns>
        public User GetUser(ulong guildId, ulong userId)
        {
            User user = null;

            try
            {
                using (MySqlConnection _connection = new MySqlConnection(_setting.Config.ConnectionString))
                {
                    _connection.Open();

                    using (MySqlCommand sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = _connection;
                        sqlCom.CommandText = $"SELECT * FROM TABLE_{guildId} WHERE USERID=@ID LIMIT 1;";
                        sqlCom.Parameters.AddWithValue("@ID", userId);
                        sqlCom.CommandType = CommandType.Text;

                        using (MySqlDataReader reader = sqlCom.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    user = new User(
                                        id: Convert.ToUInt64(reader["_ID"]),
                                        guildId: guildId,
                                        userId: Convert.ToUInt64(reader["USERID"]),
                                        coin: Convert.ToUInt64(reader["MONEY"])
                                    );
                                }
                            }
                            else
                            {
                                user = AddNewUser(guildId, userId);
                            }
                        }
                    }

                    _connection.Close();
                }
            }
            catch (MySqlException ex)
            {
                switch ((MySqlErrorCode)ex.Number)
                {
                    case MySqlErrorCode.NoSuchTable:
                        AddNewGuild(guildId);
                        user = GetUser(guildId, userId);
                        break;

                    default:
                        throw new Exception($"처리되지 못한 예외가 발생하였습니다. ({ex.Number}, {ex.Message})");
                }
            }

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
            try
            {
                using (MySqlConnection _connection = new MySqlConnection(_setting.Config.ConnectionString))
                {
                    _connection.Open();

                    using (MySqlCommand sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = _connection;
                        sqlCom.CommandText = $"UPDATE TABLE_{guildId} SET MONEY=MONEY+@MONEY WHERE USERID=@ID";
                        sqlCom.Parameters.AddWithValue("@MONEY", coin);
                        sqlCom.Parameters.AddWithValue("@ID", userId);
                        sqlCom.CommandType = CommandType.Text;

                        int numberOfRecords = sqlCom.ExecuteNonQuery();

                        if (numberOfRecords == 0)
                        {
                            AddNewUser(guildId, userId);
                            AddUserCoin(guildId, userId, coin);
                        }
                    }

                    _connection.Close();
                }
            }
            catch (MySqlException ex)
            {
                switch ((MySqlErrorCode)ex.Number)
                {
                    case MySqlErrorCode.NoSuchTable: //ER_NO_SUCH_TABLE
                        AddNewGuild(guildId);
                        break;

                    default:
                        throw new Exception($"처리되지 못한 예외가 발생하였습니다. ({ex.Number}, {ex.Message})");
                }
            }
        }

        /// <summary>
        /// 유저에게서 코인을 가져갑니다.
        /// </summary>
        /// <param name="guildId">길드 아이디</param>
        /// <param name="userId">유저 아이디</param>
        /// <param name="coin">추가할 코인의 량</param>
        public void SubUserCoin(ulong guildId, ulong userId, ulong coin)
        {
            try
            {
                using (MySqlConnection _connection = new MySqlConnection(_setting.Config.ConnectionString))
                {
                    _connection.Open();

                    using (MySqlCommand sqlCom = new MySqlCommand())
                    {
                        sqlCom.Connection = _connection;
                        sqlCom.CommandText = $"UPDATE TABLE_{guildId} SET MONEY=MONEY-@MONEY WHERE USERID=@ID";
                        sqlCom.Parameters.AddWithValue("@MONEY", coin);
                        sqlCom.Parameters.AddWithValue("@ID", userId);
                        sqlCom.CommandType = CommandType.Text;

                        int numberOfRecords = sqlCom.ExecuteNonQuery();

                        if (numberOfRecords == 0)
                        {
                            AddNewUser(guildId, userId);
                            AddUserCoin(guildId, userId, coin);
                        }
                    }

                    _connection.Close();
                }
            }
            catch (MySqlException ex)
            {
                switch ((MySqlErrorCode)ex.Number)
                {
                    case MySqlErrorCode.NoSuchTable:
                        AddNewGuild(guildId);
                        break;

                    default:
                        throw new Exception($"처리되지 못한 예외가 발생하였습니다. ({ex.Number}, {ex.Message})");
                }
            }
        }
    }
}
