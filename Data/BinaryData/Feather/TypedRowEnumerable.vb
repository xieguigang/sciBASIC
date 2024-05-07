Imports System.Collections
Imports System.Collections.Generic

Namespace FeatherDotNet
    ''' <summary>
    ''' Allocation free enumerator for a typed row.
    ''' </summary>
    Public Class TypedRowEnumerator(Of TRow)
        Implements IEnumerator(Of TRow)

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT).CurrentProp"/>
        ''' </summary>
        Private _CurrentProp As TRow
        Private Parent As TypedDataFrameBase(Of TRow)
        Private Index As Long

        Public Property CurrentProp As TRow Implements IEnumerator(Of TRow).Current
            Get
                Return _CurrentProp
            End Get
            Private Set(value As TRow)
                _CurrentProp = value
            End Set
        End Property

        Private ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                Return CurrentProp
            End Get
        End Property

        Friend Sub New(parent As TypedDataFrameBase(Of TRow))
            CurrentProp = Nothing
            Me.Parent = parent
            Index = -1
        End Sub

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT)"/>
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Parent = Nothing
        End Sub

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT)"/>
        ''' </summary>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Index += 1

            Dim row As TRow
            If Not Parent.TryGetRowTranslated(Index, row) Then Return False

            CurrentProp = row
            Return True
        End Function

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT)"/>
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            Index = -1
        End Sub
    End Class

    ''' <summary>
    ''' Allocation free enumerable for a typed row.
    ''' </summary>
    Public Class TypedRowEnumerable(Of TRow)
        Implements IEnumerable(Of TRow)
        Private Parent As TypedDataFrameBase(Of TRow)

        Friend Sub New(parent As TypedDataFrameBase(Of TRow))
            Me.Parent = parent
        End Sub

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerable(OfT).GetEnumerator"/>
        ''' </summary>
        Public Function GetEnumerator() As TypedRowEnumerator(Of TRow)
            Return New TypedRowEnumerator(Of TRow)(Parent)
        End Function

        Private Function GetEnumerator1() As IEnumerator(Of TRow) Implements IEnumerable(Of TRow).GetEnumerator
            Return GetEnumerator()
        End Function

        Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace
