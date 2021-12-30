namespace Rosalind.Core.Models;

/// <summary>
/// SQL 데이터베이스의 유저 객체입니다.
/// </summary>
public class User
{
    /// <summary>
    /// 내부적으로 관리에 사용되는 아이디입니다.
    /// </summary>
    public ulong Id { get; init; }

    /// <summary>
    /// 유저의 디스코드 아이디입니다.
    /// </summary>
    public ulong UserId { get; init; }

    /// <summary>
    /// 유저가 소지하고 있는 금액입니다.
    /// </summary>
    public ulong Coin { get; init; }
}