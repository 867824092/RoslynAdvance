using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Text;

string str = @"
using System;
namespace Test {
  public class Student {
     public void Run() {
      Console.WriteLine(123456);
     }
  }
}
";

SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(str, encoding: Encoding.UTF8);
CSharpCompilation compilation = CSharpCompilation.Create("StringCompiler",
    syntaxTrees: new[] { syntaxTree },
    references: new[] {
       MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
       MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Console")).Location),
       MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location)
    },
    options:new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
using var memory = new MemoryStream();
EmitResult result = compilation.Emit(memory);
foreach (var item in result.Diagnostics) {
    Console.WriteLine(item.ToString());
}
if (!result.Success) return;
memory.Seek(0, SeekOrigin.Begin);
Assembly assembly = Assembly.Load(memory.ToArray());
Type type = assembly.GetType("Test.Student") ?? throw new Exception("未找到Student类型");
MethodInfo method =  type.GetMethod("Run", BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("未找到Run方法");
object obj = Activator.CreateInstance(type);
method.Invoke(obj, null); 


CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
Console.WriteLine($"The tree is a {root.Kind()} node.");
Console.WriteLine($"The tree has {root.Members.Count} elements in it.");
Console.WriteLine($"The tree has {root.Usings.Count} using statements. They are:");
foreach (UsingDirectiveSyntax element in root.Usings)
    Console.WriteLine($"\t{element.Name}");

MemberDeclarationSyntax firstMember = root.Members[0];
Console.WriteLine($"The first member is a {firstMember.Kind()}.");

