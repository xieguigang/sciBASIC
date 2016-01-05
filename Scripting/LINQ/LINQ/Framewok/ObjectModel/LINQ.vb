Namespace Framework.ObjectModel

    ''' <summary>
    ''' LINQ查询表达式的对象模型
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class LINQ : Implements System.IDisposable

        Protected Friend StatementInstance As Object
        Protected Friend Test As System.Func(Of Boolean)
        Protected Friend SetObject As System.Func(Of Object, Boolean)
        Protected Friend SelectConstruct As System.Func(Of Object)
        Protected Friend Statement As Global.LINQ.Statements.LINQStatement
        Protected Friend ObjectCollection As Object()
        Protected Friend FrameworkRuntime As Global.LINQ.Script.I_DynamicsRuntime

        Sub New(Statement As Global.LINQ.Statements.LINQStatement, Runtime As Global.LINQ.Script.I_DynamicsRuntime)
            Me.StatementInstance = Statement.CreateInstance  'Create a instance for the LINQ entity and intialzie the components
            Me.Test = Function() Statement.ConditionTest.TestMethod.Invoke(StatementInstance, Nothing) 'Construct the Lambda expression
            Me.SetObject = Function(p As Object) Statement.Object.SetObject.Invoke(StatementInstance, {p})
            Me.SelectConstruct = Function() Statement.SelectConstruct.SelectMethod.Invoke(StatementInstance, Nothing)
            Me.ObjectCollection = LINQ.GetCollection(Statement, Runtime)
            Me.Statement = Statement
            Me.FrameworkRuntime = Runtime
        End Sub

        Protected Friend Shared Function GetCollection(Statement As Global.LINQ.Statements.LINQStatement, Runtime As Global.LINQ.Script.I_DynamicsRuntime) As Object()
            If Statement.Collection.Type = Statements.Tokens.ObjectCollection.CollectionTypes.File Then
                Return Statement.Collection.ILINQCollection.GetCollection(Statement.Collection.Value)
            Else
                '返回运行时环境中的对象集合
                Return Runtime.GetCollection(Statement.Collection)
            End If
        End Function

        Public Overridable Function EXEC() As Object()
            Dim LQuery = From [Object] As Object In ObjectCollection
                         Let f As Boolean = SetObject([Object])
                         Where True = Test()
                         Let t As Object = SelectConstruct()
                         Select t     'Build a LINQ query object model using the constructed elements
            Return LQuery.ToArray 'Return the query result
        End Function

        Public Overrides Function ToString() As String
            Return Statement.ToString
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO:  释放托管状态(托管对象)。
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace