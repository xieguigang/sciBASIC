Imports System.Collections
Imports System.Collections.Generic

Namespace FeatherDotNet
    ''' <summary>
    ''' Allocation free enumerable for the columns in a dataframe.
    ''' </summary>
    Public Class ColumnEnumerable
        Implements IEnumerable(Of Column)
        Private Parent As DataFrame

        Friend Sub New(parent As DataFrame)
            Me.Parent = parent
        End Sub

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerable(OfT).GetEnumerator"/>
        ''' </summary>
        Public Function GetEnumerator() As ColumnEnumerator
            Return New ColumnEnumerator(Parent)
        End Function

        Private Function GetEnumerator1() As IEnumerator(Of Column) Implements IEnumerable(Of Column).GetEnumerator
            Return GetEnumerator()
        End Function

        Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class

    ''' <summary>
    ''' Allocation free enumerator for the columns in a dataframe.
    ''' </summary>
    Public Class ColumnEnumerator
        Implements IEnumerator(Of Column)

        ''' <summary>
        ''' <seecref="System.Collections.Generic.IEnumerator(OfT).CurrentProp"/>
        ''' </summary>
        Private _CurrentProp As FeatherDotNet.Column
        Private Parent As DataFrame
        Private Index As Long

        Public Property CurrentProp As Column Implements IEnumerator(Of Column).Current
            Get
                Return _CurrentProp
            End Get
            Private Set(value As Column)
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

            Dim nextColumn As Column = Nothing
            If Not Parent.TryGetColumnTranslated(Index, nextColumn) Then Return False

            CurrentProp = nextColumn
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
