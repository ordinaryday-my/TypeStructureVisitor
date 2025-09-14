// See https://aka.ms/new-console-template for more information

using System.Reflection;

var visitor = new TypeStructureVisitor.TypeStructureVisitor(typeof(int)).UseRecursionDepthLimit(2);
Console.WriteLine(visitor.Visit());