#Region "Microsoft.VisualBasic::9faf9e3e6de9036dababf6420bd15464, Data\BinaryData\SQLite3\Tables\Sqlite3Row.vb"

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

    '   Total Lines: 59
    '    Code Lines: 44 (74.58%)
    ' Comment Lines: 1 (1.69%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (23.73%)
    '     File Size: 1.88 KB


    '     Class Sqlite3Row
    ' 
    '         Properties: ColumnData, ReadIndex, RowId, Table
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, (+2 Overloads) TryGetOrdinal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ManagedSqlite.Core.Tables

    Public Class Sqlite3Row

        Public ReadOnly Property Table As Sqlite3Table
        Public ReadOnly Property RowId As Long
        Public ReadOnly Property ReadIndex As Long
        Public ReadOnly Property ColumnData As Object()

        Default Public ReadOnly Property Item(field As Integer) As Object
            Get
                Dim value As Object = Nothing
                Call TryGetOrdinal(field, value)
                Return value
            End Get
        End Property


        Friend Sub New(index&, table As Sqlite3Table, rowId As Long, columnData As Object())
            Me.Table = table
            Me.RowId = rowId
            Me.ColumnData = columnData
            Me.ReadIndex = index
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{ReadIndex} [{RowId}] = {ColumnData.Select(AddressOf Scripting.ToString).GetJson}"
        End Function

        Public Function TryGetOrdinal(index As UShort, ByRef value As Object) As Boolean
            value = Nothing

            If ColumnData.Length > index Then
                value = ColumnData(index)
                Return True
            End If

            Return False
        End Function

        Public Function TryGetOrdinal(Of T)(index As UShort, Optional ByRef value As T = Nothing) As Boolean
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(index, tmp) Then
                Return False
            End If

            ' TODO: Is null a success case?
            If tmp Is Nothing Then
                Return False
            End If

            value = DirectCast(Convert.ChangeType(tmp, GetType(T)), T)
            Return True
        End Function
    End Class
End Namespace
