#Region "Microsoft.VisualBasic::067f9367f812385e656985211acdf33d, Data\DataFrame\IO\MetaData\GenericMeta.vb"

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

    '   Total Lines: 46
    '    Code Lines: 32 (69.57%)
    ' Comment Lines: 7 (15.22%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 7 (15.22%)
    '     File Size: 1.75 KB


    '     Module GenericMeta
    ' 
    '         Function: GetMetaDataRows, TryParseMetaDataRows
    ' 
    ' 
    ' /********************************************************************************/

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
                .Select(Function(s)
                            Return s.GetTagValue(delimiter, trim:=True)
                        End Function) _
                .ToArray

            Return tagsData.GroupBy(Function(o) o.Name) _
                .ToDictionary(Function(k) k.Key,
                              Function(s)
                                  Return s.Select(Function(v) v.Value).ToArray
                              End Function)
        End Function
    End Module
End Namespace
