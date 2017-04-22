#Region "Microsoft.VisualBasic::70b4bdbec7f15739a3eae52c7ec56370, ..\sciBASIC#\Data\DataFrame\IO\MetaData\GenericMeta.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace IO

    Public Module GenericMeta

        ''' <summary>
        ''' 通用的meta元数据的解析函数
        ''' </summary>
        ''' <param name="file$"></param>
        ''' <param name="prefix$"></param>
        ''' <returns></returns>
        Public Function GetMetaDataRows(file$, Optional prefix$ = "##") As String()
            Dim out As New List(Of String)

            For Each line$ In file.IterateAllLines
                If InStr(line, prefix, CompareMethod.Text) = 1 Then
                    out += line
                Else
                    ' 已经找不到起始字符串了，则不会是元数据了
                    Exit For
                End If
            Next

            Return out.ToArray
        End Function

        Public Function TryParseMetaDataRows(file$, Optional delimiter$ = "=", Optional prefix$ = "##") As Dictionary(Of String, String())
            Dim rows$() = GetMetaDataRows(file, prefix)
            Dim pl% = prefix.Length
            Dim tagsData As NamedValue(Of String)() = rows _
                .Select(Function(s) Mid(s, pl + 1)) _
                .Select(Function(s) s.GetTagValue(delimiter, trim:=True)) _
                .ToArray

            Return tagsData.GroupBy(Function(o) o.Name) _
                .ToDictionary(Function(k) k.Key,
                              Function(s) s.Select(Function(v) v.Value).ToArray)
        End Function
    End Module
End Namespace
