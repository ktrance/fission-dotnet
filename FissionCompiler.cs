using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Fission.DotNetCore.Compiler
{
    class FissionCompiler
    {
        public static Function Compile(string code, out List<string> errors) {
            errors = new List<string>();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)
            };
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => 
                        diagnostic.IsWarningAsError || 
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        errors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var type = assembly.GetType("FissionLoader");
                    var info = type.GetMember("Run").First() as MethodInfo;
                    return new Function(info);
                }
            }
            return null;
        }
    }
    class Function
    {
        private readonly MethodInfo _info;
        public Function(MethodInfo info)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));
            _info = info;
        }

        public object Invoke(){
            return _info.Invoke(null, null);
        }
    }
}