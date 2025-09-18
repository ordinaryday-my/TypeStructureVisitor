using System.Reflection;
using System.Runtime.CompilerServices;

namespace TypeStructureVisitor;

// TODO: 消除警告
// TODO: 将一部分构造放入Builder中
public sealed class TypeStructureVisitor
{
    private readonly Type _visitType;
    private readonly FieldInfo[] _typeFieldsInfos;
    private readonly MethodInfo[] _typeMethodsInfos;
    private readonly Dictionary<MethodInfo, ParameterInfo[]> _methodsParametersInfos;
    private readonly PropertyInfo[] _typePropertiesInfos;
    private readonly ConstructorInfo[] _typeConstructorsInfos;
    private readonly Dictionary<ConstructorInfo, ParameterInfo[]> _constructorsParametersInfos;
    private readonly uint _indentationLevel;
    private readonly string _indentation;
    private readonly IndentationOption _option;
    private readonly HashSet<Type> _visitedTypes;
    private readonly bool _isRoot = true;
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
        foreach (var method in _typeMethodsInfos)
        {
            if(!method.GetCustomAttributes().OfType<CompilerGeneratedAttribute>().Any())
                _methodsParametersInfos[method] = method.GetParameters();
        }

        _typePropertiesInfos = visitType.GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic);
        _typeConstructorsInfos = visitType.GetConstructors(BindingFlags.Instance | BindingFlags.Static |
                                                           BindingFlags.Public | BindingFlags.NonPublic);
        _constructorsParametersInfos = [];
        foreach (var constructor in _typeConstructorsInfos)
        {
            _constructorsParametersInfos[constructor] = constructor.GetParameters();
        }
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
        _isRoot = false;
        _option = option;
        _visitType = visitType;
        _typeFieldsInfos = visitType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                               BindingFlags.NonPublic);
        _typeMethodsInfos = visitType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                 BindingFlags.NonPublic);
        _methodsParametersInfos = [];
        foreach (var method in _typeMethodsInfos) 
        {
            if(!method.GetCustomAttributes().OfType<CompilerGeneratedAttribute>().Any())
                _methodsParametersInfos[method] = method.GetParameters();
        }

        _typePropertiesInfos = visitType.GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic);
        _typeConstructorsInfos = visitType.GetConstructors(BindingFlags.Instance | BindingFlags.Static |
                                                           BindingFlags.Public | BindingFlags.NonPublic);
        _constructorsParametersInfos = [];
        foreach (var constructor in _typeConstructorsInfos)
        {
            _constructorsParametersInfos[constructor] = constructor.GetParameters();
        }
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
            writer.WriteLine($"{_indentation}Type {typeName} Has {_typeMethodsInfos.Length} Methods");
            if (_typeMethodsInfos.Length != 0)
            {
                writer.WriteLine($"{_indentation}{{");
                VisitMethods(deeperIndentation, writer);
                writer.WriteLine($"{_indentation}}}");
            }
            
            writer.WriteLine($"{_indentation} Type {typeName} Has {_typeConstructorsInfos.Length} Constructors.");
            if (_typeConstructorsInfos.Length != 0)
            {
                writer.WriteLine($"{_indentation}{{");
                VisitConstructors(deeperIndentation, writer);
                writer.WriteLine($"{_indentation}}}");
            }
            
            // TODO: 访问事件（通过 Type.GetEvents 获取事件信息，添加 VisitEvents 方法）
            // TODO: 访问嵌套类型（通过 Type.GetNestedTypes 获取嵌套类型信息，递归调用 Visit）
        }
        finally
        {
            // 无论是否异常，都移除当前类型（避免影响其他分支的访问逻辑）
            if(_isRoot)
            {
                _visitedTypes.Clear();
            }
            
        }
    }

    private void VisitConstructors(string deeperIndentation, TextWriter writer)
    {
        foreach (var ((constructorInfo, parameters), i) in _constructorsParametersInfos.Zip(Enumerable.Range(0,_constructorsParametersInfos.Count)))
        {
            // 构造函数基本信息
            writer.WriteLine($"{deeperIndentation}Constructor <{i}> Has {parameters.Length} Parameters");

            if (parameters.Length > 0)
            {
                writer.WriteLine($"{deeperIndentation}{{");
                string parameterIndent =
                    CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 2);
                VisitMethodParameters(parameterIndent, parameters, writer);
                writer.WriteLine($"{deeperIndentation}}}");
            }

            // 仅在不是最后一个构造函数时添加分隔空行
            if (i != _constructorsParametersInfos.Count - 1)
            {
                writer.WriteLine();
            }
        }
        
    }

    // 修改 VisitMethods 方法，移除方法间多余空行
    private void VisitMethods(string indentation, TextWriter writer)
    {
        foreach (var ((methodInfo, parameters), i) in _methodsParametersInfos.Zip(Enumerable.Range(0, _methodsParametersInfos.Count)))
        {
            // 方法基本信息
            writer.Write($"{indentation}Method {methodInfo.Name}");

            // 方法返回类型
            writer.WriteLine($" Has Return Type: {methodInfo.ReturnType.FullName}");

            // 递归访问返回类型
            var returnTypeVisitor =
                new TypeStructureVisitor(methodInfo.ReturnType, _option, _indentationLevel + 1, _visitedTypes)
                    .UseRecursionDepthLimit(_recursionDepthLimit);
            returnTypeVisitor.Visit(writer);

            // 方法参数信息
            writer.WriteLine($"{indentation}Has {parameters.Length} Parameters");

            if (parameters.Length > 0)
            {
                writer.WriteLine($"{indentation}{{");
                string parameterIndent =
                    CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 2);
                VisitMethodParameters(parameterIndent, parameters, writer);
                writer.WriteLine($"{indentation}}}");
            }

            // 仅在不是最后一个方法时添加分隔空行
            if (i != _methodsParametersInfos.Count - 1)
            {
                writer.WriteLine();
            }
        }
    }

// 修改 VisitMethodParameters 方法，移除参数间多余空行
    private void VisitMethodParameters(string parameterIndent, ParameterInfo[] parameters, TextWriter writer)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];
            string parameterModifier = string.Empty;
            if (parameterInfo.IsOut)
                parameterModifier = "out ";
            else if (parameterInfo.ParameterType.IsByRef && !parameterInfo.IsOut)
                parameterModifier = "ref ";

            writer.Write($"{parameterIndent}Parameter {parameterInfo.Name}");
            
            string typeName = "unknown";
            Type paramType = parameterInfo.ParameterType;

            {
                string rawTypeName = paramType.FullName ?? paramType.Name;
                typeName = paramType.IsByRef ? rawTypeName.TrimEnd('&') : rawTypeName;
            }

            writer.WriteLine($" Has Type: {parameterModifier}{typeName}");

            var actualType = parameterInfo.ParameterType;
            if (actualType.IsByRef)
            {
                actualType = actualType.GetElementType() ?? actualType;
            }

            var paramTypeVisitor = new TypeStructureVisitor(actualType, _option, _indentationLevel + 2, _visitedTypes)
                .UseRecursionDepthLimit(_recursionDepthLimit);
            paramTypeVisitor.Visit(writer);

            // 仅在不是最后一个参数时添加分隔空行
            if (i != parameters.Length - 1)
            {
                writer.WriteLine();
            }
        }
    }

// 修改 VisitProperties 方法，移除属性间多余空行
    private void VisitProperties(string deeperIndentation, TextWriter writer)
    {
        for (int i = 0; i < _typePropertiesInfos.Length; i++)
        {
            var propertyInfo = _typePropertiesInfos[i];
            writer.WriteLine(
                $"{deeperIndentation}Name={propertyInfo.Name} Has Value Type={propertyInfo.PropertyType.FullName}");

            var insideVisitor = new TypeStructureVisitor(propertyInfo.PropertyType, _option, _indentationLevel + 1,
                _visitedTypes)
                .UseRecursionDepthLimit(_recursionDepthLimit);
            insideVisitor.Visit(writer);

            // 仅在不是最后一个属性时添加分隔空行
            if (i != _typePropertiesInfos.Length - 1)
            {
                writer.WriteLine();
            }
        }
    }

// 修改 VisitFields 方法，移除字段间多余空行
    private void VisitFields(string deeperIndentation, TextWriter writer)
    {
        for (int i = 0; i < _typeFieldsInfos.Length; i++)
        {
            var fieldInfo = _typeFieldsInfos[i];
            writer.WriteLine($"{deeperIndentation}Name={fieldInfo.Name} HasType={fieldInfo.FieldType.FullName}");

            var insideVisitor =
                new TypeStructureVisitor(fieldInfo.FieldType, _option, _indentationLevel + 1, _visitedTypes)
                    .UseRecursionDepthLimit(_recursionDepthLimit);
            insideVisitor.Visit(writer);

            // 仅在不是最后一个字段时添加分隔空行
            if (i != _typeFieldsInfos.Length - 1)
            {
                writer.WriteLine();
            }
        }
    }
}