namespace CLI.UI;

public class Command
{
    public required string Description { init; get; }
    public required string Syntax { init; get; }
    public required int args { init; get; }
}