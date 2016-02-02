Imports System.Threading
Imports System.Threading.Thread

Namespace ComponentModel.DataSourceModel

    Public Class Iterator : Implements IEnumerator

        ReadOnly _source As IEnumerable

        Sub New(source As IEnumerable)
            _source = source
            Reset()
        End Sub

        Public ReadOnly Property Current As Object Implements IEnumerator.Current

        Dim _read As Boolean = False

        Private Sub __moveNext()
            For Each x As Object In _source ' 单线程安全
                Do While _read
                    Call Sleep(1)
                Loop
                _Current = x
                _read = True
            Next

            _read = False
        End Sub

        Dim _forEach As Thread

        Public Sub Reset() Implements IEnumerator.Reset
            If Not _forEach Is Nothing Then  ' 终止这条线程然后再新建
                Call _forEach.Abort()
            End If

            _forEach = New Thread(AddressOf __moveNext)
            _forEach.Start()
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            _read = False
            Return True
        End Function
    End Class
End Namespace