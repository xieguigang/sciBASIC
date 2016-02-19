Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization

Namespace Linq

    Public Module IteratorExtensions

        <Extension> Public Iterator Function SeqIterator(Of T)(source As IEnumerable(Of T), Optional offset As Integer = 0) As IEnumerable(Of SeqValue(Of T))
            If Not source.IsNullOrEmpty Then
                Dim idx As Integer = offset

                For Each x As T In source
                    Yield New SeqValue(Of T)(idx, x)
                    idx += 1
                Next
            End If
        End Function

        Public Structure SeqValue(Of T) : Implements IAddressHandle

            Public Property Pos As Integer
            Public Property obj As T

            Private Property AddrHwnd As Long Implements IAddressHandle.AddrHwnd
                Get
                    Return CLng(Pos)
                End Get
                Set(value As Long)
                    Pos = CInt(value)
                End Set
            End Property

            Sub New(i As Integer, x As T)
                Pos = i
                obj = x
            End Sub

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function

            Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As T
                Return x.obj
            End Operator

            Public Shared Narrowing Operator CType(x As SeqValue(Of T)) As Integer
                Return x.Pos
            End Operator

            Public Sub Dispose() Implements IDisposable.Dispose
            End Sub
        End Structure
    End Module
End Namespace