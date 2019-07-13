#Region "Microsoft.VisualBasic::509c595b181e32feb6f26159ff36302d, tutorials\PipelineTest\Program.vb"

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

    ' Module Program
    ' 
    '     Function: CreateException, JustStdDevice, Main, OnlySupportsFile, SupportsBothFileAndPipeline
    '               TestException
    ' 
    '     Sub: call1, call2, call3, call4, call5
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON

<ExceptionHelp>
<CLI>
Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/file", Usage:="/file /in <file.txt> [/out <out.txt>]")>
    Public Function OnlySupportsFile(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out As String = args.GetValue("/out", [in].TrimSuffix & ".output.json")
        Return [in].ReadAllLines.GetJson.SaveTo(out).CLICode
    End Function

    <ExportAPI("/std", Usage:="/std <input> <output>")>
    Public Function JustStdDevice() As Integer
        Using input = Console.OpenStandardInput, output = New StreamWriter(Console.OpenStandardOutput)
            Call output.Write(New StreamReader(input).ReadToEnd.LineTokens.GetJson)
        End Using

        Return 0
    End Function

    <ExportAPI("/pipe.Test", Usage:="/pipe.Test /in <file.txt/std_in> [/out <out.txt/std_out>]")>
    Public Function SupportsBothFileAndPipeline(args As CommandLine) As Integer
        Using out = args.OpenStreamOutput("/out")
            Dim inData As String() = args.OpenStreamInput("/in").ReadToEnd.LineTokens
            Call out.Write(inData.GetJson)
        End Using

        Return 0
    End Function

    ''' <summary>
    ''' Child throw exception
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/throw.ex")>
    <Usage("/throw.ex /type <name> /message <text>")>
    Public Function CreateException(args As CommandLine) As Integer
        Dim type$ = args("/type")
        Dim message$ = args("/message")
        Dim ex As Exception = Activator.CreateInstance(System.Type.GetType(type, throwOnError:=True), {message})

        App.JoinVariable("pause.disable", True)

        Throw ex
    End Function

    Sub call1()
        Throw New NotImplementedException("afsdfsdffsdfsd")
    End Sub

    Sub call2()
        call1()
    End Sub

    Sub call3()
        call2()
    End Sub

    Sub call4()
        call3()
    End Sub

    Sub call5()
        call4()
    End Sub

    ''' <summary>
    ''' Catch child exception
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/test.catch")>
    <Usage("/test.catch")>
    Public Function TestException(args As CommandLine) As Integer

        Try
            '     call5()
        Catch ex As Exception
            Call LogException(New Exception("helllllll", ex))
        End Try


        Dim app As New InteropService(Microsoft.VisualBasic.App.ExecutablePath)
        Dim child = app.RunDotNetApp($"/throw.ex /type {GetType(NotImplementedException).FullName} /message ""Hello world!""")

        Call child.Run()

        Dim err = InteropService.GetLastError(child)

        Pause()

        Throw New Exception(err.ToString)
    End Function
End Module
