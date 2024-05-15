#Region "Microsoft.VisualBasic::34eb303dc126b0c43747c9800ad89fd4, Data\BinaryData\Feather\TypedRowEnumerable.vb"

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
    '     File Size: 2.70 KB


    ' Class TypedRowEnumerator
    ' 
    '     Properties: Current, CurrentProp
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MoveNext
    ' 
    '     Sub: Dispose, Reset
    ' 
    ' Class TypedRowEnumerable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetEnumerator, GetEnumerator1, GetEnumerator2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections
Imports System.Collections.Generic

''' <summary>
''' Allocation free enumerator for a typed row.
''' </summary>
Public Class TypedRowEnumerator(Of TRow)
    Implements IEnumerator(Of TRow)

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T).Current"/>
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

        Dim row As TRow
        If Not Parent.TryGetRowTranslated(Index, row) Then Return False

        CurrentProp = row
        Return True
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
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
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
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

