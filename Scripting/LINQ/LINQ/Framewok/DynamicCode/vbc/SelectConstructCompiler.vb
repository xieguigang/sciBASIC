Namespace Framework.DynamicCode.VBC

    Public Class SelectConstructCompiler : Inherits TokenCompiler

        Public Const SelectMethodName As String = "SelectMethod"

        Dim Statement As LINQ.Statements.LINQStatement

        Sub New(Statement As LINQ.Statements.LINQStatement)
            Me.Statement = Statement
        End Sub

        Public Function Compile() As CodeDom.CodeMemberMethod
            Dim AssignStatement = New CodeDom.CodeAssignStatement(New CodeDom.CodeVariableReferenceExpression("rval"), Statement.SelectConstruct.Expression)  '生成赋值语句，设置函数返回值
            Dim [Function] As CodeDom.CodeMemberMethod = DynamicCode.VBC.DynamicCompiler.DeclareFunction(
                                SelectMethodName, "System.Object", New CodeDom.CodeStatementCollection From {AssignStatement})
            [Function].Attributes = CodeDom.MemberAttributes.Public
            Return [Function]
        End Function
    End Class

    Public MustInherit Class TokenCompiler

        Public Shared Function DeclareType(Name As String, [Object] As Statements.Tokens.ObjectDeclaration, ReadOnlyObjects As Statements.Tokens.ReadOnlyObject()) As CodeDom.CodeTypeDeclaration
            Dim [Module] As CodeDom.CodeTypeDeclaration = New CodeDom.CodeTypeDeclaration(Name)
            Call [Module].Members.Add([Object].ToFieldDeclaration)  '声明模块变量，然后在后面的条件测试函数中进行引用
            For Each ReadOnlyObject In ReadOnlyObjects
                Call [Module].Members.Add(ReadOnlyObject.ToFieldDeclaration)  '添加只读对象
            Next

            Return [Module]
        End Function
    End Class
End Namespace