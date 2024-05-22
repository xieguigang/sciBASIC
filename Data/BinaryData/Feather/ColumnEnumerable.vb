#Region "Microsoft.VisualBasic::ebfc179a2953377680e9dbf0a653d107, Data\BinaryData\Feather\ColumnEnumerable.vb"

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

    '   Total Lines: 88
    '    Code Lines: 53 (60.23%)
    ' Comment Lines: 21 (23.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (15.91%)
    '     File Size: 2.60 KB


    ' Class ColumnEnumerable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetEnumerator, GetEnumerator1, GetEnumerator2
    ' 
    ' Class ColumnEnumerator
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
    ''' <see cref="System.Collections.Generic.IEnumerable(Of T).GetEnumerator"/>
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
    ''' <see cref="System.Collections.Generic.IEnumerator(Of Column).Current"/>
    ''' </summary>
    Private _CurrentProp As Column
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

        Dim nextColumn As Column = Nothing
        If Not Parent.TryGetColumnTranslated(Index, nextColumn) Then Return False

        CurrentProp = nextColumn
        Return True
    End Function

    ''' <summary>
    ''' <see cref="System.Collections.Generic.IEnumerator(Of T)"/>
    ''' </summary>
    Public Sub Reset() Implements IEnumerator.Reset
        Index = -1
    End Sub
End Class
