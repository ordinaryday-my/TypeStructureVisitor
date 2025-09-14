using System.Reflection;
using System.Text;

namespace TypeStructureVisitor;
// TODO: 看与ai的聊天改进代码
// TODO: 添加直接输出的选项
// TODO: 消除警告
public sealed class TypeStructureVisitor
{
    private readonly Type _visitType;
    private readonly FieldInfo[] _typeFieldsInfos;
    private readonly MethodInfo[] _typeMethodsInfos;
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
        _typeFieldsInfos = visitType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        _typeMethodsInfos = visitType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        _typePropertiesInfos = visitType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        _visitedTypes = new HashSet<Type>();
        _indentation = CalculateIndentation(_option.IndentationString,_option.Repeat, _indentationLevel);
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
        _typeFieldsInfos = visitType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        _typeMethodsInfos = visitType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        _typePropertiesInfos = visitType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
    

    public StringBuilder Visit()
    {
        if (_recursionDepthLimit <= _indentationLevel && _recursionDepthLimit >= 0)
        {
            return new StringBuilder("..."); 
        }
        _visitedTypes.Add(_visitType);
        var result = new StringBuilder();
        var typeName = _visitType.FullName;
        var deeperIndentation = CalculateIndentation(_option.IndentationString, _option.Repeat, _indentationLevel + 1);
        
        result.AppendLine($"{_indentation}Type {typeName} Has {_typeFieldsInfos.Length} Fields");
        
        result.AppendLine($"{_indentation}{'{'}");
        if (_typeFieldsInfos.Length != 0)
        {
            result.Append(VisitFields(deeperIndentation));
        }
        result.AppendLine($"{_indentation}{'}'}");

        result.AppendLine($"{_indentation}Type {typeName} Has {_typePropertiesInfos.Length} Properties");

        result.AppendLine($"{_indentation}{'{'}");
        if (_typePropertiesInfos.Length != 0)
        {
            result.Append(VisitProperties(deeperIndentation)); // TODO: 实现访问属性
        }
        result.AppendLine($"{_indentation}{'}'}");
        
        // TODO: 写访问方法的代码
        // TODO: 访问构造函数
        // TODO: 访问属性
        // TODO: 访问事件
        // TODO: 访问嵌套类型
        
        _visitedTypes.Remove(_visitType);
        
        return result;
    }

    private string VisitProperties(string deeperIndentation)
    {
        var result = new StringBuilder();
        foreach (var propertyInfo in _typePropertiesInfos)
        {
            result.Append($"{deeperIndentation}Name={propertyInfo.Name} ");
            var propertyType = propertyInfo.PropertyType;
            result.AppendLine($"Has Value Type={propertyType.FullName}");
            if (_visitedTypes.Add(propertyType))
            {
                var insideVisitor =
                    new TypeStructureVisitor(propertyType, _option, _indentationLevel + 1, _visitedTypes);
                result.AppendLine(insideVisitor.Visit().ToString());
                result.AppendLine($"{_indentation}{'}'}");
            }
            else
            {
                result.AppendLine($"{deeperIndentation}Type {propertyType.FullName} Has Been Visited.");
                result.AppendLine();
            }
        }
        return result.ToString();
    }

    private string VisitFields(string deeperIndentation)
    {
        var result = new StringBuilder();
        foreach (var fieldInfo in _typeFieldsInfos)
        {
            result.Append($"{deeperIndentation}Name={fieldInfo.Name} "); // TODO: 为泛型类型做特化
            var insideFieldType = fieldInfo.FieldType;
            result.AppendLine($"HasType={insideFieldType.FullName}");
            if (_visitedTypes.Add(insideFieldType))
            {
                var insideVisitor =
                    new TypeStructureVisitor(insideFieldType, _option, _indentationLevel + 1, _visitedTypes);
                result.AppendLine(insideVisitor.Visit().ToString());
                result.AppendLine();
            }
            else
            {
                result.AppendLine($"{deeperIndentation}Type {insideFieldType.FullName} Has Been Visited.");
                result.AppendLine();
            }
        }
        
        return result.ToString();
    }
}