#Region "Microsoft.VisualBasic::a7b1a2f885324a2d6b9115d61df76f51, Data\BinaryData\SQLite3\Writer\Sqlite3DataRow.vb"

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

    '   Total Lines: 52
    '    Code Lines: 36 (69.23%)
    ' Comment Lines: 3 (5.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (25.00%)
    '     File Size: 1.64 KB


    '     Class Sqlite3DataRow
    ' 
    '         Properties: RowId, Table, Values
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IsDBNull, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace Writer

    ''' <summary>
    ''' 写入模块的内存数据行, 取值语义与读取侧 <c>Sqlite3Row</c> 保持一致(可按序号或列名访问)。
    ''' </summary>
    Public Class Sqlite3DataRow

        Public ReadOnly Property Table As Sqlite3TableWriter
        Public ReadOnly Property RowId As Long
        Public ReadOnly Property Values As Object()

        Friend Sub New(table As Sqlite3TableWriter, rowId As Long, values As Object())
            Me.Table = table
            Me.RowId = rowId
            Me.Values = values
        End Sub

        Default Public ReadOnly Property Item(index As Integer) As Object
            Get
                If Values Is Nothing OrElse index < 0 OrElse index >= Values.Length Then
                    Return Nothing
                End If

                Return Values(index)
            End Get
        End Property

        Default Public ReadOnly Property Item(name As String) As Object
            Get
                Return Me(Table.GetOrdinal(name))
            End Get
        End Property

        Public Function IsDBNull(index As Integer) As Boolean
            Return Me(index) Is Nothing
        End Function

        Public Overrides Function ToString() As String
            Dim parts As New List(Of String)()

            For Each v As Object In Values
                parts.Add(If(v Is Nothing, "<NULL>", v.ToString()))
            Next

            Return "#" & RowId & " = [" & String.Join(", ", parts) & "]"
        End Function

    End Class

End Namespace

