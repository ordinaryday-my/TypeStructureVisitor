
# TypeStructureVisitor

TypeStructureVisitor 是一个用于深度分析和展示 .NET 类型结构的工具，能够递归遍历指定类型的字段、属性、方法、构造函数、事件及嵌套类型，并以结构化方式输出这些信息，帮助开发者快速理解类型的内部组成。

## 功能特点

- **全面解析**：递归获取类型的字段、属性、方法、构造函数、事件及嵌套类型的详细信息

- **深度控制**：支持设置递归深度限制，避免因复杂类型层级导致输出过于庞大

- **循环引用处理**：自动检测并处理循环引用，防止无限递归

- **多目标输出**：通过 `MultiTextWriter` 支持同时输出到多个文本流（如控制台和文件）

- **缩进定制**：可通过 `IndentationOption` 自定义输出的缩进字符和层级重复次数

- **详细信息展示**：包括成员名称、类型、参数（含 ref/out 修饰符）、返回值等信息

## 安装与使用

### 前提条件

- .NET 6.0 或更高版本

### 编译项目

1. 克隆仓库到本地

2. 使用 .NET CLI 编译：

```bash

dotnet build
```

### 命令行使用

#### 基本语法



```
TypeStructureVisitor <TypeName> [TreeDepthLimit=-1]
```

#### 参数说明



* `TypeName`：必选，要分析的类型全名（格式：`Namespace.TypeName` 或 `TypeName,Assembly.dll`）

* `TreeDepthLimit`：可选，递归深度限制（-1 表示无限制，默认值为 -1）

#### 使用示例



```
# 分析 System.String 类型

TypeStructureVisitor System.String

# 分析自定义类型 MyNamespace.MyClass，递归深度限制为 3

TypeStructureVisitor MyNamespace.MyClass 3

# 分析指定程序集中的类型

TypeStructureVisitor MyClass,MyAssembly.dll
```

## 编程接口示例

### 基本用法



```
using TypeStructureVisitor;

using System.IO;

// 分析 List\<int> 类型，限制递归深度为 2

var type = typeof(System.Collections.Generic.List\<int>);

var visitor = new TypeStructureVisitor(type)

   .UseRecursionDepthLimit(2);

// 输出到文件

using (var writer = new StreamWriter("output.txt"))

{

   visitor.Visit(writer);

}
```

### 多目标输出



```
// 同时输出到控制台和日志文件

var writers = new List\<TextWriter> { Console.Out, new StreamWriter("log.txt") };

using (var multiWriter = new MultiTextWriter(writers))

{

   visitor.Visit(multiWriter);

}
```

### 自定义缩进格式



```
// 使用两个空格作为缩进，每层重复一次

var options = new IndentationOption&#x20;


   IndentationString = "  ",

   Repeat = 1&#x20;

};

var visitor = new TypeStructureVisitor(type, options);
```

## 输出格式说明

程序按以下结构输出类型信息，每个部分先显示成员数量，存在成员时展开详细信息：



1. **字段信息**：名称及字段类型，递归展示字段类型的结构

2. **属性信息**：名称及属性类型，递归展示属性类型的结构

3. **方法信息**：名称、返回类型（递归展示）及参数详情（含 ref/out 修饰符）

4. **构造函数信息**：参数详情（含 ref/out 修饰符）

5. **事件信息**：名称及事件处理程序类型，递归展示事件类型的结构

6. **嵌套类型**：递归展示嵌套类型的完整结构

## 主要类说明



* `TypeStructureVisitor`：核心类，负责遍历和输出类型结构，支持设置递归深度限制

* `IndentationOption`：缩进选项配置类，可自定义缩进字符和每层重复次数

* `MultiTextWriter`：多目标文本写入器，支持同时向多个 `TextWriter` 写入内容

* `StringExtension`：字符串扩展方法类，提供字符串包裹引号的功能

## 许可证

[MIT](LICENSE)

