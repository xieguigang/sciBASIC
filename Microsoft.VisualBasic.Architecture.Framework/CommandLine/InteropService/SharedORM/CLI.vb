Imports System.Reflection
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    <AttributeUsage(AttributeTargets.Class)>
    Public Class CLIAttribute : Inherits Attribute

        Public Shared Function ParseAssembly(dll$) As Type
            Dim assembly As Assembly = Assembly.LoadFile(dll)
            Dim type As Type = LinqAPI.DefaultFirst(Of Type) <=
 _
                From t As Type
                In assembly.GetTypes
                Where Not t.GetCustomAttribute(Of CLIAttribute) Is Nothing
                Select t ' 

            Return type
        End Function
    End Class
End Namespace