// See https://aka.ms/new-console-template for more information

using System.Reflection;

using var writer = new StreamWriter(Console.OpenStandardOutput());
var visitor = new TypeStructureVisitor.TypeStructureVisitor(typeof(Assembly));
visitor.Visit(writer);