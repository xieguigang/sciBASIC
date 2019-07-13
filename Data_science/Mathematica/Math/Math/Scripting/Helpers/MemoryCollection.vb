#Region "Microsoft.VisualBasic::a6f371b5ef3cea111c7ee005d0afc189, Data_science\Mathematica\Math\Math\Scripting\Helpers\MemoryCollection.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class MemoryCollection
    ' 
    '         Properties: DictData, Objects
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Add, GetEnumerator, GetEnumerator1
    ' 
    '         Sub: __buildCache
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting

    Public MustInherit Class MemoryCollection(Of T) : Implements IEnumerable(Of KeyValuePair(Of String, T))

        Protected ReadOnly objTable As Dictionary(Of String, T) = New Dictionary(Of String, T)
        Protected ReadOnly __engine As Expression

        Dim __caches As String()

        Public Sub New(engine As Expression)
            __engine = engine
        End Sub

        Public ReadOnly Property Objects As String()
            Get
                Return __caches
            End Get
        End Property

        Public ReadOnly Property DictData As Dictionary(Of String, T)
            Get
                Return objTable
            End Get
        End Property

        Protected Sub __buildCache()
            __caches = (From strName As String
                        In objTable.Keys
                        Select strName
                        Order By Len(strName) Descending).ToArray
        End Sub

        ''' <summary>
        ''' 名称的大小写不敏感
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Protected Function Add(Name As String, value As T, cache As Boolean, sensitive As Boolean) As Integer
            If Not sensitive Then
                Name = Name.ToLower
            End If

            Name = Name.Trim

            If objTable.ContainsKey(Name) Then
                Call objTable.Remove(Name)
            End If

            Call objTable.Add(Name, value)
            If cache Then
                Call __buildCache()
            End If

            Return 0
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, T)) Implements IEnumerable(Of KeyValuePair(Of String, T)).GetEnumerator
            For Each item As KeyValuePair(Of String, T) In objTable
                Yield item
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
