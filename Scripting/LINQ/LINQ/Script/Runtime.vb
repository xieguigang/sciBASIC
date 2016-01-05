Namespace Script

    ''' <summary>
    ''' LINQ脚本查询环境
    ''' </summary>
    ''' <remarks></remarks>
    Public Class I_DynamicsRuntime : Inherits Dynamic.DynamicObject
        Implements System.IDisposable

        Dim Variables As List(Of Variable) = New List(Of Variable)

        Friend Class Variable
            Public Property Name As String
            Public Property Data As Object()

            Public Overrides Function ToString() As String
                Return Name
            End Function

            Public Shared Widening Operator CType(v As Object()) As Variable
                Return New Variable With {.Name = v(0).ToString, .Data = v(1)}
            End Operator
        End Class

        Dim _TypeLibrary As Global.LINQ.Framework.TypeRegistry

        Public Function Evaluate(script As String) As Object()
            Throw New NotImplementedException
        End Function

        Public Function Initialize() As Boolean
            Return True
        End Function

        Public Function SetVariable(var As String, value As Object) As Boolean
            Throw New NotImplementedException
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region


        Public Function GetCollection(CollectionReference As LINQ.Statements.Tokens.ObjectCollection) As Object()
            Dim LQuery = From Item In Variables Where String.Equals(CollectionReference.Value, Item.Name, StringComparison.OrdinalIgnoreCase) Select Item.Data  '
            Dim Result = LQuery.ToArray
            If Result.Count = 0 Then
                Return New Object() {}
            Else
                Return Result.First
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Collection"></param>
        ''' <remarks></remarks>
        Public Function SetObject(Name As String, Collection As Object()) As Boolean
            Dim LQuery = From Item In Variables Where String.Equals(Name, Item.Name, StringComparison.OrdinalIgnoreCase) Select Item '
            Dim Result = LQuery.ToArray
            If Result.Count = 0 Then '添加新纪录
                Call Variables.Add({Name, Collection})
            Else
                Result.First.Data = Collection
            End If

            Throw New NotImplementedException
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0} variables in the LINQ runtime.", Variables.Count)
        End Function

        ''' <summary>
        ''' 执行一个LINQ查询脚本文件
        ''' </summary>
        ''' <param name="FilePath">LINQ脚本文件的文件路径</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 脚本要求：
        ''' 
        ''' </remarks>
        Public Function Source(FilePath As String) As Boolean
            Throw New NotImplementedException
        End Function
    End Class
End Namespace