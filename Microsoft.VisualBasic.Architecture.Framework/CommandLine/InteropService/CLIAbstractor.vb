Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language

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

        <Extension> Public Function CLICaller(api As MethodInfo, args As CommandLine) As Integer
            Dim paramValues As List(Of Object)
            Dim names As Dictionary(Of String, String)
            Dim parameters = api.GetParameters

            If parameters.Length = 1 AndAlso parameters(Scan0).ParameterType Is GetType(CommandLine) Then
                Return DirectCast(api.Invoke(Nothing, {args}), Integer)
            Else
                paramValues = New List(Of Object)
            End If

            names = args _
                .Keys _
                .ToDictionary(Function(k)
                                  Return k.Trim("/"c, "-"c).Trim.ToLower
                              End Function)

            For Each param As ParameterInfo In api.GetParameters
                Dim name$ = param.Name.ToLower

                If param.ParameterType Is GetType(Boolean) Then
                    paramValues += names.ContainsKey(name)
                Else
                    paramValues += Scripting.CTypeDynamic(
                        args(names(name)),
                        param.ParameterType)
                End If
            Next

            Dim result% = DirectCast(api.Invoke(Nothing, paramValues.ToArray), Integer)
            Return result
        End Function
    End Module
End Namespace