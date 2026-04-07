#Region "Microsoft.VisualBasic::352d24f811b04d6c1ecf9c3b2c4c9b9b, Data\BinaryData\SQLite3\Tables\Sqlite3Row.vb"

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

    '   Total Lines: 120
    '    Code Lines: 94 (78.33%)
    ' Comment Lines: 1 (0.83%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 25 (20.83%)
    '     File Size: 3.70 KB


    '     Class Sqlite3Row
    ' 
    '         Properties: ColumnData, ReadIndex, RowId, Table
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBoolean, GetDouble, GetInt32, GetOrdinal, GetString
    '                   IsDBNull, ToString, (+2 Overloads) TryGetOrdinal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
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

        Default Public ReadOnly Property Item(field As String) As Object
            Get
                Return Me(field:=Table.schema.GetOrdinal(field))
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOrdinal(name As String) As Integer
            Return Table.schema.GetOrdinal(name)
        End Function

        Public Function GetBoolean(i As Integer) As Boolean
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(i, tmp) Then
                Return False
            Else
                Return CBool(tmp)
            End If
        End Function

        Public Function GetDouble(i As Integer) As Double
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(i, tmp) Then
                Return False
            Else
                Return CDbl(tmp)
            End If
        End Function

        Public Function GetInt32(i As Integer) As Integer
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(i, tmp) Then
                Return False
            Else
                Return CInt(tmp)
            End If
        End Function

        Public Function GetString(i As Integer) As String
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(i, tmp) Then
                Return False
            Else
                Return CStr(tmp)
            End If
        End Function

        Public Function IsDBNull(i As Integer) As Boolean
            Dim tmp As Object = Nothing

            If Not TryGetOrdinal(i, tmp) Then
                Return True
            Else
                Return tmp Is Nothing
            End If
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
