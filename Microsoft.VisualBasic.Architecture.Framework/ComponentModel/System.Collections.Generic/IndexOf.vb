Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    ''' <summary>
    ''' Mappings of ``key -> index``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class IndexOf(Of T)

        Dim maps As New Dictionary(Of T, Integer)

        ''' <summary>
        ''' 请注意，这里的数据源请尽量使用Distinct的，否则对于重复的数据，只会记录下第一个位置
        ''' </summary>
        ''' <param name="source"></param>
        Sub New(source As IEnumerable(Of T))
            For Each x As SeqValue(Of T) In source.SeqIterator
                If Not maps.ContainsKey(x) Then
                    Call maps.Add(+x, x.i)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 不存在则返回-1
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Index(x As T) As Integer
            Get
                If maps.ContainsKey(x) Then
                    Return maps(x)
                Else
                    Return -1
                End If
            End Get
        End Property

        ''' <summary>
        ''' 这个函数是线程不安全的
        ''' </summary>
        ''' <param name="x"></param>
        Public Sub Add(x As T)
            If Not maps.ContainsKey(x) Then
                Call maps.Add(x, maps.Count)
            End If
        End Sub

        ''' <summary>
        ''' Display the input source sequence.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return maps.Keys _
                .Select(Function(x) x.ToString) _
                .ToArray _
                .GetJson
        End Function
    End Class
End Namespace