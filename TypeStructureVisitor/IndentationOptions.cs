namespace TypeStructureVisitor;

public sealed class IndentationOption
{
    public string IndentationString { get; init; } = " ";
    public uint Repeat { get; init; } = 4;
    
    public static IndentationOption Default { get; } = new IndentationOption();
}