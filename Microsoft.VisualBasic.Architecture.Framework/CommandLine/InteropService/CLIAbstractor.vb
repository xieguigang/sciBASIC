Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Debugging

Namespace CommandLine.InteropService

    ''' <summary>
    ''' 将函数的定义抽象为一个命令行，从而能够提供更好的命令行编写服务
    ''' </summary>
    Public Module CLIAbstractor

        ''' <summary>
        ''' Abstract the function declare as a CLI usage declare. This tool not support the generic function method.
        ''' (不支持泛型函数)
        ''' </summary>
        ''' <param name="api"></param>
        ''' <param name="prefix$"></param>
        ''' <returns></returns>
        Public Function CLIUsage(api As MethodInfo, Optional prefix$ = "/") As String
            Dim isGeneric = (Not api.GetGenericArguments.IsNullOrEmpty) Or die($"Function abstract tool not working on a generic method: {api.DeclaringType.FullName}::{api.Name}!")
            Dim name$ = prefix & api.Name
            Dim args As New List(Of ParameterInfo)(api.GetParameters)
            Dim optionalArguments = args.Where(Function(param) param.IsOptional).ToArray
            Dim requiredArguments = args - optionalArguments
            Dim usage As New StringBuilder
            Dim required$ = requiredArguments _
                .Select(Function(param)
                            Return $"{prefix}{param.Name} <value:={param.Name}>"
                        End Function) _
                .JoinBy(" ")
            Dim optionals = optionalArguments _
                .Select(Function(param)
                            If param.ParameterType Is GetType(Boolean) Then
                                Return $"{prefix}{param.Name}"
                            Else
                                Dim default$ = $"<default={Scripting.CStrSafe(param.DefaultValue)}>"
                                Return $"{prefix}{param.Name} {[default]}"
                            End If
                        End Function) _
                .JoinBy(" ")

            Call usage.Append(name & " ")
            Call usage.Append(required)
            Call usage.Append(" ")
            Call usage.Append($"[{optionals}]")

            Return usage.ToString.Trim
        End Function
    End Module
End Namespace