Imports System.Text
Imports System.Text.RegularExpressions

Namespace Statements.Tokens

    Public Class WhereCondition : Inherits LINQ.Statements.Tokens.Token

        Friend Expression As CodeDom.CodeExpression
        Friend TestMethod As System.Reflection.MethodInfo

        Sub New(Statement As LINQ.Statements.LINQStatement)
            Me.Statement = Statement

            Dim Parser As Parser.Parser = New Parser.Parser
            Dim str = LINQ.Statements.Tokens.ReadOnlyObject.Parser.GetStatement(Statement._OriginalCommand, New String() {"where", "let"}, False)
            If String.IsNullOrEmpty(str) Then
                str = LINQ.Statements.Tokens.ReadOnlyObject.Parser.GetStatement(Statement._OriginalCommand, New String() {"where", "select"}, False)
            End If

            Expression = Parser.ParseExpression(str)
        End Sub

        Public Sub Initialize()
            Me.TestMethod = Framework.DynamicCode.DynamicInvoke.GetMethod(Statement.ILINQProgram, Framework.DynamicCode.VBC.WhereConditionTestCompiler.FunctionName)
        End Sub
    End Class
End Namespace