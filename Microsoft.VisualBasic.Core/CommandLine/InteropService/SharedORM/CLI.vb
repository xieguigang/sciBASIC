#Region "Microsoft.VisualBasic::fbedf0252652f4d03e94a8db319af729, Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\CLI.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class CLIAttribute
    ' 
    '         Function: ParseAssembly
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 创建一个自定义标记来表示这个模块是一个CLI命令接口模块
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class)>
    Public Class CLIAttribute : Inherits Attribute

        Public Shared Function ParseAssembly(dll$) As Type
            Dim assembly As Assembly = Assembly.LoadFile(dll)
            Dim type As Type = LinqAPI.DefaultFirst(Of Type) _
 _
                () <= From t As Type
                      In EmitReflection.GetTypesHelper(assembly)
                      Where Not t.GetCustomAttribute(Of CLIAttribute) Is Nothing
                      Select t ' 

            Return type
        End Function
    End Class
End Namespace
