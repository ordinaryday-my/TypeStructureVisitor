using System.Reflection;
using System.Text;

namespace TypeStructureVisitor;

// TODO: 看与ai的聊天改进代码
// TODO: 消除警告
public sealed class TypeStructureVisitor
{
    private readonly Type _visitType;
    private readonly FieldInfo[] _typeFieldsInfos;
    private readonly MethodInfo[] _typeMethodsInfos;
    private readonly Dictionary<MethodInfo, ParameterInfo[]> _methodsParametersInfos;
    private readonly PropertyInfo[] _typePropertiesInfos;
    private readonly uint _indentationLevel = 0;
    private readonly string _indentation;
    private readonly IndentationOption _option;
    private readonly HashSet<Type> _visitedTypes;
    private int _recursionDepthLimit = -1;

    public TypeStructureVisitor(Type visitType, IndentationOption? option = null)
    {
        _option = option ?? IndentationOption.Default;
        _visitType = visitType;
        _typeFieldsInfos = visitType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                               BindingFlags.NonPublic);
        _typeMethodsInfos = visitType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                 BindingFlags.NonPublic);
        _methodsParametersInfos = [];
        foreach(var method in _typeMethodsInfos)
        {
            _methodsParametersInfos[method] = method.GetParameters(); // TODO: 过滤自动生成方法
        }
        _typePropertiesInfos = visitType.GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic);
        _visitedTypes = new HashSet<Type>();
        _indentation = CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel);
    }

    public TypeStructureVisitor UseRecursionDepthLimit(int limit)
    {
        _recursionDepthLimit = limit;
        return this;
    }

    private TypeStructureVisitor(Type visitType, IndentationOption option, uint indentationLevel, HashSet<Type> visited)
    {
        _option = option;
        _visitType = visitType;
        _typeFieldsInfos = visitType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                               BindingFlags.NonPublic);
        _typeMethodsInfos = visitType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                 BindingFlags.NonPublic);
        _methodsParametersInfos = [];
        foreach(var method in _typeMethodsInfos)  // TODO: 过滤自动生成方法
        {
            _methodsParametersInfos[method] = method.GetParameters();
        }
        _typePropertiesInfos = visitType.GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic);
        _indentationLevel = indentationLevel;
        _indentation = CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel);
        _visitedTypes = visited;
    }

    private static string CalculateIndentation(string indentationString, uint repeat, uint indentationLevel) =>
        new(
            Enumerable.Repeat(
                    indentationString,
                    Convert.ToInt32(indentationLevel * repeat))
                .SelectMany(s => s)
                .ToArray()
        );


    // 改进：添加 ref TextWriter 参数，结果直接输出到 TextWriter
    public void Visit(TextWriter writer)
    {
        // 校验 TextWriter 不为 null
        if (writer == null)
            throw new ArgumentNullException(nameof(writer), "TextWriter 不能为 null");

        // 递归深度限制判断：超过限制输出省略标记
        if (_recursionDepthLimit <= _indentationLevel && _recursionDepthLimit >= 0)
        {
            writer.WriteLine($"{_indentation}...");
            return;
        }

        // 避免循环引用（如 A 包含 B、B 包含 A）
        if (!_visitedTypes.Add(_visitType))
        {
            writer.WriteLine($"{_indentation}Type {_visitType.FullName} Has Been Visited.");
            return;
        }

        try
        {
            var typeName = _visitType.FullName;
            var deeperIndentation =
                CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 1);

            // 输出字段统计信息
            writer.WriteLine($"{_indentation}Type {typeName} Has {_typeFieldsInfos.Length} Fields");
            if (_typeFieldsInfos.Length != 0)
            {
                writer.WriteLine($"{_indentation}{{");
                VisitFields(deeperIndentation, writer);
                writer.WriteLine($"{_indentation}}}");
            }

            // 输出属性统计信息
            writer.WriteLine($"{_indentation}Type {typeName} Has {_typePropertiesInfos.Length} Properties");
            if (_typePropertiesInfos.Length != 0)
            {
                writer.WriteLine($"{_indentation}{{");
                VisitProperties(deeperIndentation, writer);
                writer.WriteLine($"{_indentation}}}");
            }
            
            // 输出方法统计信息
            writer.WriteLine($"{_indentation}Type {typeName} Has {_typePropertiesInfos.Length} Methods");
            if (_typeMethodsInfos.Length != 0)
            {
                writer.WriteLine($"{_indentation}{{");
                VisitMethods(deeperIndentation, writer);
                writer.WriteLine($"{_indentation}}}");
            }


            // TODO: 写访问方法的代码（可参考字段/属性逻辑，添加 VisitMethods 方法并适配 TextWriter）
            // TODO: 访问构造函数（需通过 Type.GetConstructors 获取构造函数信息，添加 VisitConstructors 方法）
            // TODO: 访问事件（通过 Type.GetEvents 获取事件信息，添加 VisitEvents 方法）
            // TODO: 访问嵌套类型（通过 Type.GetNestedTypes 获取嵌套类型信息，递归调用 Visit）
        }
        finally
        {
            // 无论是否异常，都移除当前类型（避免影响其他分支的访问逻辑）
            _visitedTypes.Remove(_visitType);
        }
    }

private void VisitMethods(string deeperIndentation, TextWriter writer)
{
    foreach (var methodInfo in _typeMethodsInfos)
    {
        // 方法基本信息 - 使用统一缩进
        writer.WriteLine($"{_indentation}Method: {methodInfo.Name}");
        
        // 方法返回类型 - 增加一级缩进
        string returnTypeIndent = CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 1);
        writer.WriteLine($"{returnTypeIndent}Return Type: {methodInfo.ReturnType.FullName}");
        
        // 递归访问返回类型
        var returnTypeVisitor = new TypeStructureVisitor(methodInfo.ReturnType, _option, _indentationLevel + 2, _visitedTypes);
        returnTypeVisitor.Visit(writer);
        
        // 方法参数信息
        var parameters = _methodsParametersInfos[methodInfo];
        writer.WriteLine($"{returnTypeIndent}Parameters ({parameters.Length}):");
        
        if (parameters.Length > 0)
        {
            // 参数缩进比方法信息多两级
            string parameterIndent = CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 2);
            VisitMethodParameters(parameterIndent, parameters, writer);
        }
        
        // 方法间添加分隔线提高可读性
        writer.WriteLine();
    }
}

private void VisitMethodParameters(string parameterIndent, ParameterInfo[] parameters, TextWriter writer)
{
    foreach (var parameterInfo in parameters)
    {
        // 处理ref/out参数的显示
        string parameterModifier = string.Empty;
        if (parameterInfo.IsOut)
            parameterModifier = "out ";
        else if (parameterInfo.ParameterType.IsByRef && !parameterInfo.IsOut)
            parameterModifier = "ref ";
        
        // 参数基本信息
        writer.WriteLine($"{parameterIndent}Parameter: {parameterInfo.Name}");
        
        // 参数类型信息 - 再增加一级缩进
        var paramTypeIndent = CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 3);
        // 安全获取参数类型名称，处理可能的null值
        string typeName = "unknown"; // 默认值，确保不会为null
        Type paramType = parameterInfo.ParameterType;

        {
            // 处理引用类型的"&"后缀
            string rawTypeName = paramType.FullName ?? paramType.Name; // 优先用FullName，缺失则用Name
            typeName = paramType.IsByRef ? rawTypeName.TrimEnd('&') : rawTypeName;
        }

        writer.WriteLine($"{paramTypeIndent}Type: {parameterModifier}{typeName}");

        
        // 处理params参数标记
        if (parameterInfo.IsDefined(typeof(ParamArrayAttribute), false))
        {
            writer.WriteLine($"{paramTypeIndent}Attribute: params");
        }
        
        // 递归访问参数类型
        // 安全获取参数的实际类型，避免空引用异常
        var actualType = parameterInfo.ParameterType;
        if (actualType.IsByRef)
        {
            // 先获取元素类型，若为null则使用原始类型作为备选
            actualType = actualType.GetElementType() ?? actualType;
        }
        var paramTypeVisitor = new TypeStructureVisitor(actualType, _option, _indentationLevel + 3, _visitedTypes);
        paramTypeVisitor.Visit(writer);
        
        // 参数间添加空行分隔
        if (parameterInfo != parameters.Last())
        {
            writer.WriteLine();
        }
    }
}


    // 改进：适配 TextWriter 输出，移除 StringBuilder 拼接
    private void VisitProperties(string deeperIndentation, TextWriter writer)
    {
        foreach (var propertyInfo in _typePropertiesInfos)
        {
            // 输出属性名称和类型
            writer.WriteLine(
                $"{deeperIndentation}Name={propertyInfo.Name} Has Value Type={propertyInfo.PropertyType.FullName}");

            // 递归访问属性的类型

            var insideVisitor = new TypeStructureVisitor(propertyInfo.PropertyType, _option, _indentationLevel + 1,
                _visitedTypes);
            insideVisitor.Visit(writer); // 递归调用改进后的 Visit 方法
            writer.WriteLine();
        }
    }

    // 改进：适配 TextWriter 输出，移除 StringBuilder 拼接
    private void VisitFields(string deeperIndentation, TextWriter writer)
    {
        foreach (var fieldInfo in _typeFieldsInfos)
        {
            // 输出字段名称和类型 TODO: 为泛型类型做特化
            writer.WriteLine($"{deeperIndentation}Name={fieldInfo.Name} HasType={fieldInfo.FieldType.FullName}");

            // 递归访问字段的类型
            var insideVisitor =
                new TypeStructureVisitor(fieldInfo.FieldType, _option, _indentationLevel + 1, _visitedTypes);
            insideVisitor.Visit(writer); // 递归调用改进后的 Visit 方法
            writer.WriteLine();
        }
    }
}