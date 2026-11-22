Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.ComponentModel

''' <summary>
''' A minimal v1 CLI container. Each subcommand takes a single <c>args As CommandLine</c>
''' and reads values manually. Metadata comes from &lt;Argument&gt;, &lt;Usage&gt; and
''' &lt;Description&gt; attributes.
''' </summary>
<CLI>
Public Module Program

    <ExportAPI("/hello")>
    <Description("Print a greeting to the console.")>
    <Usage("/hello --name &lt;value&gt; [--loud]")>
    <Argument("--name", False, CLITypes.String, Description:="The name of the person to greet.")>
    <Argument("--loud", True, CLITypes.Boolean, Description:="Print the greeting in upper case.")>
    Public Function Hello(args As CommandLine) As Integer
        Dim name$ = args("--name")
        Dim loud As Boolean = args("--loud")

        Dim message$ = $"Hello, {name}!"
        If loud Then
            message = message.ToUpper
        End If

        Call Console.WriteLine(message)
        Return 0
    End Function

    <ExportAPI("/add")>
    <Description("Add two integers and print the result.")>
    <Usage("/add --a &lt;integer&gt; --b &lt;integer&gt;")>
    <Argument("--a", False, CLITypes.Integer, AcceptTypes:={GetType(Integer)}, Description:="The first operand.")>
    <Argument("--b", False, CLITypes.Integer, AcceptTypes:={GetType(Integer)}, Description:="The second operand.")>
    Public Function Add(args As CommandLine) As Integer
        Dim a% = args.GetValue(Of Integer)("--a", 0)
        Dim b% = args.GetValue(Of Integer)("--b", 0)
        Call Console.WriteLine($"{a} + {b} = {a + b}")
        Return 0
    End Function

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

End Module
