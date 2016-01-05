Namespace Framework.DynamicCode.VBC

    Public Class WhereConditionTestCompiler : Inherits TokenCompiler

        Dim Statement As Statements.LINQStatement

        Public Const FunctionName As String = "Test"

        Public Shared Function [True]() As Boolean
            Return True
        End Function

        Sub New(Statement As LINQ.Statements.LINQStatement)
            Me.Statement = Statement
        End Sub

        Public Function Compile() As CodeDom.CodeTypeMember
            Dim StatementCollection As CodeDom.CodeStatementCollection = Nothing

            If Not Statement.ConditionTest.Expression Is Nothing Then
                StatementCollection = New CodeDom.CodeStatementCollection
                StatementCollection.Add(New CodeDom.CodeAssignStatement(New CodeDom.CodeVariableReferenceExpression("rval"), Statement.ConditionTest.Expression))
            End If
            Dim [Function] As CodeDom.CodeMemberMethod = DynamicCode.VBC.DynamicCompiler.DeclareFunction(FunctionName, "System.Boolean", StatementCollection)
            [Function].Attributes = CodeDom.MemberAttributes.Public
            Return [Function]
        End Function

        Public Overrides Function ToString() As String
            Return Statement.ConditionTest.ToString
        End Function
    End Class
End Namespace