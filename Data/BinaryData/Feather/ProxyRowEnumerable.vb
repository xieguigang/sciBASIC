#Region "Microsoft.VisualBasic::74c1deb684a914ee14d53ac451e54955, Data\BinaryData\Feather\ProxyRowEnumerable.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 91
    '    Code Lines: 55
    ' Comment Lines: 21
    '   Blank Lines: 15
    '     File Size: 2.80 KB


    ' Class ProxyRowEnumerable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetEnumerator, GetEnumerator1, GetEnumerator2
    ' 
    ' Class ProxyRowEnumerator
    ' 
    '     Properties: Current, CurrentProp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MoveNext
    ' 
    '     Sub: Dispose, Reset
    ' 
    ' /********************************************************************************/

#End Region

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
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
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
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T).Current"/>
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
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Parent = Nothing
    End Sub

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        Index += 1

        Dim nextRow As TProxyType
        If Not Parent.TryGetRowTranslated(Index, nextRow) Then Return False

        CurrentProp = nextRow
        Return True
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Reset() Implements IEnumerator.Reset
        Index = -1
    End Sub
End Class

