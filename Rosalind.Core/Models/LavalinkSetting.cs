using Newtonsoft.Json;

namespace Rosalind.Core.Models;

/// <summary>
/// 라바링크 설정 객체입니다.
/// </summary>
public class LavalinkSetting
{
    /// <summary>
    /// 봇이 음성 채널에 접속하고 헤드폰을 끌지 설정합니다.
    /// </summary>
    [JsonProperty("selfDeaf")]
    public bool SelfDeaf { get; set; }

    /// <summary>
    /// 라바링크 노드들을 설정합니다.
    /// </summary>
    [JsonProperty("nodes")]
    public Node[] Nodes { get; set; }
}

/// <summary>
/// 라바링크의 노드를 의미합니다.
/// </summary>
public class Node
{
    /// <summary>
    /// 라바링크 노드의 주소를 설정합니다.
    /// </summary>
    [JsonProperty("hostname")]
    public string Hostname { get; set; }

    /// <summary>
    /// 라바링크 노드의 포트를 설정합니다.
    /// </summary>
    [JsonProperty("port")]
    public ushort Port { get; set; }

    /// <summary>
    /// 라바링크 노드의 비밀번호를 설정합니다.
    /// </summary>
    [JsonProperty("password")]
    public string Password { get; set; }
}