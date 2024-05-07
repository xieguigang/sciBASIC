Imports System.Collections
Imports System.Collections.Generic

''' <summary>
''' Allocation free enumerable for a proxied row.
''' </summary>
Public Class ProxyRowEnumerable(Of TProxyType)
    Implements IEnumerable(Of TProxyType)
    Private Parent As ProxyDataFrame(Of TProxyType)

    Friend Sub New(parent As ProxyDataFrame(Of TProxyType))
        Me.Parent = parent
    End Sub

    ''' <summary>
    ''' <seecref="System.Collections.Generic.IEnumerable(OfT).GetEnumerator"/>
    ''' </summary>
    Public Function GetEnumerator() As ProxyRowEnumerator(Of TProxyType)
        Return New ProxyRowEnumerator(Of TProxyType)(Parent)
    End Function

    Private Function GetEnumerator1() As IEnumerator(Of TProxyType) Implements IEnumerable(Of TProxyType).GetEnumerator
        Return GetEnumerator()
    End Function

    Private Function GetEnumerator2() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class

''' <summary>
''' Allocation free enumerator for a proxied row.
''' </summary>
Public Class ProxyRowEnumerator(Of TProxyType)
    Implements IEnumerator(Of TProxyType)

    ''' <summary>
    ''' <seecref="System.Collections.Generic.IEnumerator(OfT).CurrentProp"/>
    ''' </summary>
    Private _CurrentProp As TProxyType
    Private Parent As ProxyDataFrame(Of TProxyType)
    Private Index As Long

    Public Property CurrentProp As TProxyType Implements IEnumerator(Of TProxyType).Current
        Get
            Return _CurrentProp
        End Get
        Private Set(value As TProxyType)
            _CurrentProp = value
        End Set
    End Property

    Friend Sub New(parent As ProxyDataFrame(Of TProxyType))
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

        Dim nextRow As TProxyType
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
