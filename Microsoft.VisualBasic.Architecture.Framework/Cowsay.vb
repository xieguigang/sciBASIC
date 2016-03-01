Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("Cowsay",
                  Description:="Cowsay tricks for showing your message more friendly.",
                  Revision:=21,
                  Publisher:="<a href=""mailto://xie.guigang@live.com"">xie.guigang@live.com</a>",
                  Url:="http://gcmodeller.org",
                  Category:=APICategories.UtilityTools)>
Public Module CowsayTricks

    Public ReadOnly Property NormalCow As String =
<COW>          |
          |    ^__^
           --  (oo)\_______
               (__)\       )\/\
                   ||----W |
                   ||     ||
</COW>

    Public ReadOnly Property DeadCow As String =
<COW>          |
          |    ^__^
           --  (XX)\_______
               (__)\       )\/\
                   ||----W |
                   ||     ||
</COW>

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("Cowsay", Info:="Show cowsay with a specific input message on your console screen. you can using /dead to change its face.")>
    Public Function Cowsay(msg As String, Optional isDead As Boolean = False) As String
        If isDead Then
            msg = __msgbox(msg) & DeadCow
        Else
            msg = __msgbox(msg) & NormalCow
        End If

        Call Console.WriteLine(msg)

        Return msg
    End Function

    Private Function __msgbox(msg As String) As String
        Dim l = Len(msg)
        Dim offset As String = New String(" ", 8)
        Dim sBuilder As StringBuilder = New StringBuilder(vbCrLf, 1024)
        Call sBuilder.AppendLine(offset & " " & New String("_", l + 4) & " ")
        Call sBuilder.AppendLine(offset & String.Format("<  {0}  >", msg))
        Call sBuilder.AppendLine(offset & " " & New String("-", l + 4) & " ")

        Return sBuilder.ToString
    End Function
End Module