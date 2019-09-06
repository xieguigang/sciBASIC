Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Levenshtein
Imports VB = Microsoft.VisualBasic.CommandLine.InteropService.SharedORM.VisualBasic

Namespace CommandLine

    ''' <summary>
    ''' Executive implementation of <see cref="Interpreter"/>
    ''' </summary>
    Module ExecuteImpl

        <Extension>
        Friend Function CreateCLIPipelineFile(cli_app As Interpreter, args As CommandLine) As Integer
            Dim namespace$ = args("/namespace") Or "CLI"
            Dim vb$ = New VB(App:=cli_app, [namespace]:=[namespace]).GetSourceCode
            Dim dev As TextWriter

            If args("---echo") Then
                dev = App.StdOut
            Else
                dev = ($"{App.HOME}/{cli_app.Type.Assembly.CodeBase.BaseName}.vb") _
                    .Open(, doClear:=True) _
                    .DoCall(Function(file)
                                Return New StreamWriter(file, Encodings.UTF8WithoutBOM.CodePage)
                            End Function)
            End If

            Call dev.WriteLine(vb)
            Call dev.Flush()
            Call dev.Close()

            Return 0
        End Function

        <Extension>
        Friend Function ListingRelatedCommands(cli_app As Interpreter, query$) As String()
            Dim key As New LevenshteinString(query.ToLower)
            Dim LQuery = From x As String
                         In cli_app.__API_table.Keys.AsParallel
                         Let compare = key Like x
                         Where Not compare Is Nothing AndAlso
                             compare.Score > 0.3
                         Select compare.Score,
                             x
                         Order By Score Descending

            Dim levenshteins = LQuery _
                .Select(Function(x) x.x) _
                .AsList

            levenshteins += cli_app.__API_table _
                .Keys _
                .Where(Function(s)
                           Return InStr(s, query, CompareMethod.Text) > 0 OrElse
                                  InStr(query, s, CompareMethod.Text) > 0
                       End Function)

            Return levenshteins _
                .Distinct _
                .Select(Function(name)
                            Return cli_app.__API_table(name).Name
                        End Function) _
                .ToArray
        End Function

        Friend Sub PrintVariables()
            Dim vars = App.GetAppVariables

            Call Console.WriteLine()
            Call Console.WriteLine(PS1.Fedora12.ToString)
            Call Console.WriteLine()
            Call Console.WriteLine($"Print environment variables for {GetType(App).FullName}:")
            Call Console.WriteLine(ConfigEngine.Prints(vars))
        End Sub

        Friend Sub HandleShellHistory(args As CommandLine)
            Dim logs$ = (App.LogErrDIR.ParentPath & "/.shells.log")

            If args.Parameters.IsNullOrEmpty Then
                Call Console.WriteLine()
                Call logs.ReadAllText.EchoLine
                Call Console.WriteLine()
            Else
                With args.ParameterList.First
                    Select Case .Name.ToLower
                        Case "/clear"
                            Call New Byte() {}.FlushStream(logs)
                        Case "/search"

                            Dim term$ = .Value

                            Call Console.WriteLine()
                            Call logs.IterateAllLines _
                                    .Where(Function(line)
                                               Return InStr(line, term, CompareMethod.Text) > 0
                                           End Function) _
                                    .JoinBy(vbCrLf) _
                                    .EchoLine
                            Call Console.WriteLine()

                        Case Else
                            Console.WriteLine("Unknown command!")
                    End Select
                End With
            End If
        End Sub

        Friend Sub HandleProgramManual(cli_app As Interpreter, args As CommandLine)
            ' 默认是分段打印帮助信息，假若加上了  --print参数的话，则才会一次性的打印所有的信息出来
            Dim doc As String = cli_app.SDKdocs()
            Dim output$ = args("/out") Or "./"

            If Not args("--file") Then
                If args("--print") Then
                    Call Console.WriteLine(doc)
                Else
                    Call SDKManual.LaunchManual(CLI:=cli_app)
                End If
            Else
                ' 只会写文件而不会在终端打开帮助窗口
            End If

            Call ($"{output}/{App.AssemblyName}.md") _
                .DoCall(Sub(md)
                            Call doc.SaveTo(md, Encoding.UTF8)
                        End Sub)
        End Sub
    End Module
End Namespace