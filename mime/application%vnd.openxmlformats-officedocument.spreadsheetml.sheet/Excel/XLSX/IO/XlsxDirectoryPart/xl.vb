#Region "Microsoft.VisualBasic::e12ca993c0920d4337fc54ad970561fa, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\XlsxDirectoryPart\xl.vb"

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

    '   Total Lines: 82
    '    Code Lines: 53 (64.63%)
    ' Comment Lines: 17 (20.73%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 12 (14.63%)
    '     File Size: 3.31 KB


    '     Class xl
    ' 
    '         Properties: _rels, sharedStrings, styles, workbook, worksheets
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: _name, Exists, GetTableData, GetWorksheet, GetWorksheetByIndex
    ' 
    '         Sub: _loadContents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl.worksheets
Imports csv = Microsoft.VisualBasic.Data.Framework.IO.File

Namespace XLSX.Model.Directory

    Public Class xl : Inherits XlsxDirectoryPart

        Public Property workbook As workbook
        Public Property styles As styles
        Public Property sharedStrings As sharedStrings
        Public Property worksheets As worksheets
        Public Property _rels As _rels

        Friend Sub New(data As ZipPackage)
            Call MyBase.New(data.data)
        End Sub

        ''' <summary>
        ''' 使用表名称来判断目标工作簿是否存在？
        ''' </summary>
        ''' <param name="worksheet$">The sheet name</param>
        ''' <returns></returns>
        Public Function Exists(worksheet$) As Boolean
            Return Not workbook _
                .GetSheetIDByName(worksheet) _
                .StringEmpty
        End Function

        ''' <summary>
        ''' Get <see cref="Xmlns.worksheet"/> by name.
        ''' </summary>
        ''' <param name="name$">如果表名称不存在的话，则这个函数是会返回一个空值的</param>
        ''' <returns></returns>
        Public Function GetWorksheet(name$) As XML.xl.worksheets.worksheet
            Dim sheetID$ = workbook.GetSheetIDByName(name)

            ' rId to sheetName by using rels file
            If sheetID.StringEmpty Then
                Return Nothing
            Else
                Dim key$ = _rels _
                    .workbook _
                    .Target(sheetID) _
                    .Target _
                    .BaseName
                Return worksheets.GetWorksheet(key)
            End If
        End Function

        ''' <summary>
        ''' 因为这个是直接通过编号来查找的，所以应该不会存在不存在名称的问题
        ''' 直接返回就好了
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetWorksheetByIndex(index As Integer) As XML.xl.worksheets.worksheet
            Return worksheets.GetWorksheet(sheetID:=workbook.GetSheetIDByIndex(index))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetTableData(worksheet As XML.xl.worksheets.worksheet) As csv
            Return worksheet.ToTableFrame(sharedStrings)
        End Function

        Protected Overrides Sub _loadContents()
            sharedStrings = ReadInternalFileText("/sharedStrings.xml").LoadFromXml(Of sharedStrings)(throwEx:=False) Or New sharedStrings().AsDefault
            workbook = ReadInternalFileText("/workbook.xml").LoadFromXml(Of workbook)(throwEx:=False) Or New workbook().AsDefault
            worksheets = New worksheets(fs, subdir)
            _rels = New _rels(fs, subdir)
        End Sub

        Protected Overrides Function _name() As String
            Return NameOf(xl)
        End Function
    End Class

End Namespace
