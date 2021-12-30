using Newtonsoft.Json;

namespace Rosalind.Core.Models;

/// <summary>
/// 설정 객체입니다.
/// </summary>
public class Setting
{
    /// <summary>
    /// 프로그램의 설정입니다.
    /// </summary>
    [JsonProperty("config")]
    public Config Config { get; set; }

    /// <summary>
    /// 봇의 명령어 도움말 리스트입니다.
    /// </summary>
    [JsonProperty("commandGroup")]
    public CommandGroup[] CommandGroup { get; set; }
}

/// <summary>
/// 명령어 리스트입니다.
/// </summary>
public class CommandGroup
{
    /// <summary>
    /// 명령어 그룹의 이름입니다.
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// 명령어 그룹의 설명입니다.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// 도움말 명령어에서 이 그룹이 어떤 색상으로 표시되야 할지 설정합니다.
    /// </summary>
    [JsonProperty("color")]
    public string Color { get; set; }

    /// <summary>
    /// 이 명령어 그룹에 포함되어 있는 명령어를 설정합니다.
    /// </summary>
    [JsonProperty("commands")]
    public Command[] Commands { get; set; }
}

public class Command
{
    /// <summary>
    /// 명령어의 이름입니다.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 명령어의 설명입니다.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// 명령어의 인수입니다.
    /// </summary>
    [JsonProperty("args")]
    public string[] Args { get; set; }
}

public class Config
{
    /// <summary>
    /// 프로그램이 사용할 디스코드 토큰입니다.
    /// </summary>
    [JsonProperty("token")]
    public string Token { get; set; }

    /// <summary>
    /// 전적 조회에 사용할 TRN API 토큰입니다.
    /// </summary>
    [JsonProperty("trnToken")]
    public string TrnToken { get; set; }

    /// <summary>
    /// KoreanBots에 상태를 주기적으로 업로드하기 위한 토큰입니다.
    /// </summary>
    [JsonProperty("koreanbotsToken")]
    public string KoreanbotsToken { get; set; }

    /// <summary>
    /// SQL 연결에 사용되는 문자열입니다.
    /// </summary>
    [JsonProperty("connectionString")]
    public string ConnectionString { get; set; }

    /// <summary>
    /// 개발자의 디스코드 아이디입니다.
    /// </summary>
    [JsonProperty("developerId")]
    public ulong DeveloperId { get; set; }

    /// <summary>
    /// 어디에 썼던건지 까먹었습니다.
    /// </summary>
    [JsonProperty("koreanbotsId")]
    public ulong KoreanbotsId { get; set; }

    /// <summary>
    /// 명령어에 사용될 접두사입니다.
    /// </summary>
    [JsonProperty("prefix")]
    public string Prefix { get; set; }
}