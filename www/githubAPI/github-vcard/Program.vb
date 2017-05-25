Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Webservices.Github.Visualizer
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/write",
               Info:="Draw user github vcard.",
               Usage:="/write /user <userName, example: xieguigang> [/schema$ <default=YlGnBu:c8> /out <vcard.png>]")>
    <Argument("/user", Description:="The user github account name.")>
    <Argument("/schema", Description:="The color schema name of the user contributions 3D plot data.")>
    <Argument("/out", Description:="The png image output path.")>
    Public Function vcard(args As CommandLine) As Integer
        Dim user$ = args <= "/user"
        Dim schema$ = args.GetValue("/schema", "YlGnBu:c8")
        Dim out$ = args.GetValue("/out", $"./{user}_github-vcard.png")

        Return IsometricContributions.Plot(
            user.GetUserContributions,
            schema:=schema,
            user:=Users.GetUserData(user)) _
            .Save(out) _
            .CLICode
    End Function
End Module
