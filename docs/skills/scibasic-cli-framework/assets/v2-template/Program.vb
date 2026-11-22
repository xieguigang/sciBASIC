Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine

''' <summary>
''' A minimal v2 CLI container. Mark the module with &lt;CLI&gt; and each subcommand
''' with &lt;ExportAPI("name")&gt;. XML doc comments become the generated help text.
''' </summary>
<CLI>
Public Module Program

    ''' <summary>
    ''' Print a greeting to the console.
    ''' </summary>
    ''' <param name="name">The name of the person to greet.</param>
    ''' <param name="loud">Print the greeting in upper case.</param>
    ''' <returns>Zero on success.</returns>
    <ExportAPI("/hello")>
    <Usage("/hello --name &lt;value&gt; [--loud]")>
    Public Function Hello(name As String,
                          Optional loud As Boolean = False) As Integer

        Dim message$ = $"Hello, {name}!"
        If loud Then
            message = message.ToUpper
        End If

        Call Console.WriteLine(message)
        Return 0
    End Function

    ''' <summary>
    ''' Add two integers and print the result.
    ''' </summary>
    ''' <param name="a">The first operand. CLI token: --a</param>
    ''' <param name="b">The second operand. CLI token: --b</param>
    ''' <returns>Zero on success.</returns>
    <ExportAPI("/add")>
    <Usage("/add --a &lt;integer&gt; --b &lt;integer&gt;")>
    Public Function Add(a As Integer, b As Integer) As Integer
        Call Console.WriteLine($"{a} + {b} = {a + b}")
        Return 0
    End Function

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

End Module
