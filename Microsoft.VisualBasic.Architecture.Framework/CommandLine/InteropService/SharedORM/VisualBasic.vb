Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    Public Class VisualBasic : Inherits CodeGenerator

        Dim namespace$

        Public Sub New(CLI As Type, namespace$)
            MyBase.New(CLI)
            Me.namespace = [namespace]
        End Sub

        Public Overrides Function GetSourceCode() As String
            Dim vb As New StringBuilder

            Call vb.AppendLine("Imports " & GetType(IIORedirectAbstract).Namespace)
            Call vb.AppendLine("Imports " & GetType(InteropService).Namespace)
            Call vb.AppendLine()
            Call vb.AppendLine("' Microsoft VisualBasic CommandLine Code AutoGenerator")
            Call vb.AppendLine("' assembly: " & App.Type.Assembly.Location.GetFullPath)
            Call vb.AppendLine()
            Call vb.AppendLine("Namespace " & [namespace])
            Call vb.AppendLine()
            Call vb.AppendLine(__xmlComments(App.Type.NamespaceEntry.Description))
            Call vb.AppendLine($"Public Class {MyBase.exe} : Inherits {GetType(InteropService).Name}")
            Call vb.AppendLine()
            Call vb.AppendLine()
            Call vb.AppendLine("Sub New(App$)")
            Call vb.AppendLine($"MyBase.{NameOf(InteropService._executableAssembly)} = App$")
            Call vb.AppendLine("End Sub")

            For Each API In Me.EnumeratesAPI
                Call __calls(vb, API)
            Next

            Call vb.AppendLine("End Class")
            Call vb.AppendLine("End Namespace")

            Return vb.ToString
        End Function

        Private Shared Function __xmlComments(description$) As String
            Return $"
''' <summary>
''' {description}
''' </summary>
'''"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vb"></param>
        ''' <param name="API"></param>
        Private Sub __calls(vb As StringBuilder, API As NamedValue(Of CommandLine))
            ' Public Function CommandName(args$,....Optional args$....) As Integer
            ' Dim CLI$ = "commandname arguments"
            ' Dim proc As IIORedirectAbstract = RunDotNetApp(CLI$)
            ' Return proc.Run()
            ' End Function
            Dim func$ = __normalizedAsIdentifier(API.Name).Trim("_"c)
            Dim xmlComments$ = __xmlComments(API.Description)
            Dim params$()

            Try
                params = __vbParameters(API.Value)
            Catch ex As Exception
                ex = New Exception("Check for your CLI Usage definition: " & API.Value.ToString, ex)
                Throw ex
            End Try

            Call vb.AppendLine(xmlComments)
            Call vb.AppendLine($"Public Function {func}({params.JoinBy(", ")}) As Integer")
            Call vb.AppendLine($"Dim CLI$ = $""{__CLI(API.Value)}""")
            Call vb.AppendLine($"Dim proc As {NameOf(IIORedirectAbstract)} = {NameOf(InteropService.RunDotNetApp)}(CLI$)")
            Call vb.AppendLine($"Return proc.{NameOf(IIORedirectAbstract.Run)}()")
            Call vb.AppendLine("End Function")
        End Sub

        Private Shared Function __vbParameters(API As CommandLine) As String()
            Dim out As New List(Of String)
            Dim param$

            For Each arg As NamedValue(Of String) In API.ParameterList
                param = $"{VisualBasic.__normalizedAsIdentifier(arg.Name)} As String"

                If Not arg.Description.StringEmpty Then
                    ' 可选参数
                    param = $"Optional {param} = ""{__defaultValue(arg.Value)}"""
                End If

                out += param
            Next
            For Each bool In API.BoolFlags
                out += $"Optional {VisualBasic.__normalizedAsIdentifier(bool)} As Boolean = False"
            Next

            Return out
        End Function

        Private Shared Function __defaultValue(value$) As String
            value = value.GetStackValue("<", ">")
            If InStr(value, "default=") > 0 Then
                value = Strings.Split(value, "default=").Last
            End If
            Return value
        End Function

        Private Shared Function __CLI(API As CommandLine) As String
            Dim CLI$ = $"{API.Name} "
            Dim args As New List(Of String)

            For Each param In API.ParameterList
                args += $"{param.Name} """"{"{" & __normalizedAsIdentifier(param.Name) & "}"}"""""
            Next
            For Each b In API.BoolFlags
                args += "{" & $"If({__normalizedAsIdentifier(b)}, ""{b}"", """")" & "}"
            Next
            CLI &= args.JoinBy(" ")

            Return CLI
        End Function

        Private Shared Function __normalizedAsIdentifier(arg$) As String
            Dim s As Char() = arg.ToArray
            Dim upper As Char() = arg.ToUpper.ToArray
            Dim c As Char

            If s.First = "<"c OrElse s.Last = ">"c Then
                Throw New SyntaxErrorException("'<' or '>' is using for the IO redirect in your terminal, unavailable for your commandline argument name!")
            End If

            For i As Integer = 0 To s.Length - 1
                c = upper(i)

                If (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "0"c AndAlso c <= "9"c) OrElse (c = "_") Then
                    ' 合法的字符，不做处理
                Else
                    ' 非法字符串都被替换为下划线
                    s(i) = "_"c
                End If
            Next

            Return New String(s)
        End Function
    End Class
End Namespace