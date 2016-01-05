Imports System.Text.RegularExpressions

Namespace Statements.Tokens

    Public Class SelectConstruct : Inherits Tokens.Token
        Friend Expression As CodeDom.CodeExpression
        Friend SelectMethod As System.Reflection.MethodInfo

        Sub New(Statement As LINQStatement)
            MyBase.Statement = Statement
            Call Me.TryParse()
        End Sub

        Private Sub TryParse()
            Dim str = Regex.Match(Statement._OriginalCommand, " select .+", RegexOptions.IgnoreCase).Value
            For Each key In Options.OptionList
                str = Regex.Split(str, String.Format(" {0}\s?", key), RegexOptions.IgnoreCase).First
            Next
            str = Mid(str, 9)
            MyBase._OriginalCommand = str
            If String.IsNullOrEmpty(str) Then
                Throw New SyntaxErrorException("Not SELECT statement token, can not procedure the query operation!")
            End If
            Me.Expression = New Parser.Parser().ParseExpression(str)
        End Sub

        Public Sub Initialzie()
            SelectMethod = Framework.DynamicCode.DynamicInvoke.GetMethod(MyBase.Statement.ILINQProgram, Framework.DynamicCode.VBC.SelectConstructCompiler.SelectMethodName)
        End Sub
    End Class
End Namespace