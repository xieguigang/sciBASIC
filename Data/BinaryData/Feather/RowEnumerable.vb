Imports System.Collections
Imports System.Collections.Generic

Namespace FeatherDotNet
    ''' <summary>
    ''' Allocation free enumerable for a row.
    ''' </summary>
    Public Class RowEnumerable
        Implements IEnumerable(Of Row)
        Private Parent As DataFrame

        Friend Sub New(parent As DataFrame)
            Me.Parent = parent
        End Sub

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerable(OfT).GetEnumerator"/>
        ''' </summary>
        Public Function GetEnumerator() As RowEnumerator
            Return New RowEnumerator(Parent)
        End Function

        Private Function GetEnumerator1() As IEnumerator(Of Row) Implements IEnumerable(Of Row).GetEnumerator
            Return GetEnumerator()
        End Function

        Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class

    ''' <summary>
    ''' Allocation free enumerator for a row.
    ''' </summary>
    Public Class RowEnumerator
        Implements IEnumerator(Of Row)

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT).CurrentProp"/>
        ''' </summary>
        Private _CurrentProp As FeatherDotNet.Row
        Private Parent As DataFrame
        Private Index As Long

        Public Property CurrentProp As Row Implements IEnumerator(Of Row).Current
            Get
                Return _CurrentProp
            End Get
            Private Set(value As Row)
                _CurrentProp = value
            End Set
        End Property

        Friend Sub New(parent As DataFrame)
            CurrentProp = Nothing
            Me.Parent = parent
            Index = -1
        End Sub

        Private ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                Return CurrentProp
            End Get
        End Property

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

            Dim nextRow As Row
            If Not Parent.TryGetRowTranslated(Index, nextRow) Then Return False

            CurrentProp = nextRow
            Return True
        End Function

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT)"/>
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            Index = -1
        End Sub
    End Class
End Namespace
