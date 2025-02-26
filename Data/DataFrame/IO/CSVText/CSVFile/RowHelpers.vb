#Region "Microsoft.VisualBasic::ab36c0e037df054d06d851ec323f6f60, Data\DataFrame\IO\CSVText\CSVFile\RowHelpers.vb"

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

    '   Total Lines: 123
    '    Code Lines: 75 (60.98%)
    ' Comment Lines: 33 (26.83%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 15 (12.20%)
    '     File Size: 4.52 KB


    '     Module RowHelpers
    ' 
    '         Function: Distinct, doDelimiterMask, GetColumn, GetColumns, IsNullOrEmpty
    '                   Pointer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace IO.CSVFile

    Public Module RowHelpers

        ''' <summary>
        ''' 获取一个数据行对象的游标操作符对象
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Pointer(row As RowObject) As Pointer(Of String)
            Return New Pointer(Of String)(row)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s">The cell value text</param>
        ''' <param name="deli">The value row delimiter</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Friend Function doDelimiterMask(s$, deli As Char) As String
            If String.IsNullOrEmpty(s) Then
                Return ""
            ElseIf s.First = """"c AndAlso s.Last = """"c AndAlso s.Length > 1 Then
                ' string is already been wrapped by the quote
                ' returns the raw string
                Return s
            Else
                s = s.Replace("""", """""")
            End If

            ' 双引号可以转义换行
            If s.IndexOf(deli) > -1 OrElse
                s.IndexOf(ASCII.LF) > -1 OrElse
                s.IndexOf(ASCII.CR) > -1 Then

                Return $"""{s}"""
            Else
                Return s
            End If
        End Function

        ''' <summary>
        ''' 去除行集合中的重复的数据行
        ''' </summary>
        ''' <param name="rowList"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        Public Iterator Function Distinct(rowList As IEnumerable(Of RowObject)) As IEnumerable(Of RowObject)
            Dim source As IEnumerable(Of String) = From row In rowList
                                                   Let rowLine As String = CType(row, String)
                                                   Select rowLine
                                                   Distinct
                                                   Order By rowLine Ascending
            For Each line As String In source
                Yield New RowObject(line)
            Next
        End Function

        ''' <summary>
        ''' Is this row object contains any data?
        ''' </summary>
        ''' <param name="countAllEmpty">
        ''' All of the cell value which is empty string 
        ''' is also count as empty if this parameter is ``TRUE``.
        ''' </param>
        ''' <returns></returns>
        Public Function IsNullOrEmpty(row As RowObject, Optional countAllEmpty As Boolean = True) As Boolean
            If row Is Nothing OrElse row.buffer.Count = 0 Then
                Return True
            ElseIf Not countAllEmpty Then
                Return False
            End If

            ' all of the cell value which is empty string is also count as empty
            Dim LQuery = LinqAPI.DefaultFirst(Of Integer) _
                                                          _
                () <= From colum As String
                      In row.buffer
                      Where Len(Strings.Trim(colum)) > 0
                      Select 100

            Return Not LQuery > 50
        End Function

        <Extension>
        Public Iterator Function GetColumns(table As ICollection(Of RowObject)) As IEnumerable(Of String())
            Dim width As Integer = 0
            Dim index As Integer
            Dim col As IEnumerable(Of String)

            ' 20240306
            ' deal with the possible empty table data
            If table Is Nothing OrElse table.Count = 0 Then
                Return
            Else
                width = Aggregate row As RowObject
                        In table
                        Into Max(row.NumbersOfColumn)
            End If

            For offset As Integer = 0 To width - 1
                index = offset
                col = table.Select(Function(r) r(index))

                Yield col.ToArray
            Next
        End Function

        <Extension>
        Public Function GetColumn(table As ICollection(Of RowObject), offset As Integer) As String()
            Return table.Select(Function(r) r(offset)).ToArray
        End Function
    End Module
End Namespace
