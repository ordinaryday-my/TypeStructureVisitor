// TODO: 根据命令行参数指定类型

using System.Reflection;
using TypeStructureVisitor;

if (args.Length is not (>= 1 and <= 2))
{
    Console.Error.WriteLine("Usage: TypeStructureVisitor <TypeName> <TreeDepthLimit=-1>");
    return;
}

var typeName = args[0];
Type? type = null;

// 尝试直接获取类型（支持全名+程序集格式）
type = Type.GetType(typeName);

// 尝试从已加载的程序集中查找
if (type == null)
{
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        type = assembly.GetType(typeName);
        if (type != null) break;
    }
}

// 尝试加载指定路径的程序集并查找类型
if (type == null && File.Exists(typeName))
{
    try
    {
        var assembly = Assembly.LoadFrom(typeName);
        // 假设类型名可能是简单名称，尝试从加载的程序集中查找
        type = assembly.GetType(Path.GetFileNameWithoutExtension(typeName));
        if (type == null)
        {
            // 如果找不到，尝试遍历程序集中的所有类型
            type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to load assembly: {ex.Message}");
    }
}

var treeDepthLimit = -1;
if (args.Length > 1)
{
    if (!int.TryParse(args[1], out var limit))
    {
        Console.Error.WriteLine($"Invalid TreeDepthLimit: '{args[1]}' is not a valid integer");
        Console.Error.WriteLine("Usage: TypeStructureVisitor <TypeName> <TreeDepthLimit=-1>");
        return;
    }
    treeDepthLimit = limit;
}

if (type == null)
{
    Console.Error.WriteLine($"Type '{typeName}' not found");
    return;
}

using (var writer = Console.Out) // 使用using确保资源释放（即使Console.Out不需要，也为未来扩展兼容）
{
    var visitor = new TypeStructureVisitor.TypeStructureVisitor(
        type, 
        new IndentationOption { IndentationString = " ", Repeat = 2 }
    ).UseRecursionDepthLimit(treeDepthLimit);
    visitor.Visit(writer);
}