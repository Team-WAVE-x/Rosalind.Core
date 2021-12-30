using Discord;

namespace Rosalind.Core.Models;

public class Button
{
    public int Row { get; }
    public string Label { get; }
    public string CustomId { get; }
    public IEmote Emote { get; }
    public ButtonStyle Style { get; }

    public Button(string label, string customId, IEmote emote = null, int row = 0, ButtonStyle style = ButtonStyle.Primary)
    {
        this.Row = row;
        this.Label = label;
        this.CustomId = customId;
        this.Emote = emote;
        this.Style = style;
    }
}