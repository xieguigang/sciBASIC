Namespace Framework.ObjectModel

    ''' <summary>
    ''' 并行LINQ查询表达式的对象模型
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class ParallelLINQ : Inherits LINQ

        Sub New(Statement As Global.LINQ.Statements.LINQStatement, FrameworkRuntime As Global.LINQ.Script.I_DynamicsRuntime)
            Call MyBase.New(Statement, Runtime:=FrameworkRuntime)
        End Sub

        Public Overrides Function EXEC() As Object()
            Dim LQuery = From [Object] As Object In ObjectCollection.AsParallel
                         Let f As Boolean = SetObject([Object])
                         Where True = Test()
                         Let t As Object = SelectConstruct()
                         Select t     'Build a LINQ query object model using the constructed elements
            Return LQuery.ToArray 'Return the query result
        End Function
    End Class
End Namespace