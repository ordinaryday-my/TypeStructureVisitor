using System.Text;
using TypeStructureVisitor;

var visitor = new TypeStructureVisitor.TypeStructureVisitor(typeof(System.Timers.Timer), IndentationOption.Default);
var writer = new StreamWriter("a.log", Encoding.UTF8, new FileStreamOptions()
{
    Mode = FileMode.Create,
    Access = FileAccess.Write,
    Share = FileShare.Read
});
visitor.Visit(writer);