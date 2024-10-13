#Region "Microsoft.VisualBasic::5bfda7e867546bc0ef6fa9c95e39b00f, Data\DataFrame\IO\Generic\Table.vb"

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

    '   Total Lines: 128
    '    Code Lines: 81 (63.28%)
    ' Comment Lines: 31 (24.22%)
    '    - Xml Docs: 70.97%
    ' 
    '   Blank Lines: 16 (12.50%)
    '     File Size: 5.11 KB


    '     Class Table
    ' 
    ' 
    ' 
    '     Module FileFormat
    ' 
    '         Function: ContainsIDField, GetIDList, IsTsvFile, readHeaders, SolveDataSetIDMapping
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace IO

    ''' <summary>
    ''' 没有名称属性的表抽象对象
    ''' </summary>
    Public Class Table : Inherits DynamicPropertyBase(Of String)
    End Class

    Public Module FileFormat

        Public Function IsTsvFile(path As String) As Boolean
            Dim headers = path.ReadFirstLine
            Dim comma = headers.Count(","c)
            Dim tab = headers.Count(ASCII.TAB)

            Return tab > comma
        End Function

        Friend Function SolveDataSetIDMapping(file$, uidMap$, tsv As Boolean?, encoding As Encoding) As String
            If tsv Is Nothing Then
                tsv = IsTsvFile(path:=file)
            End If

            If uidMap.StringEmpty Then
                If ContainsIDField(file, CBool(tsv), encoding, uidMap) Then
                    uidMap = NameOf(EntityObject.ID)
                Else
                    ' 使用第一列作为ID
                    ' 因为再函数之中已经通过ByRef返回来了，所以do nothing
                End If
            Else
                ' 使用用户自定义的列作为ID
                ' 在这里do nothing
            End If

            Return uidMap
        End Function

        ''' <summary>
        ''' 获取数据集之中的被映射为ID列的值列表
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="uidMap$"></param>
        ''' <param name="tsv"></param>
        ''' <param name="ignoreMapErrors"></param>
        ''' <returns></returns>
        Public Function GetIDList(path$,
                                  Optional uidMap$ = Nothing,
                                  Optional tsv As Boolean = False,
                                  Optional ignoreMapErrors As Boolean = False,
                                  Optional encoding As Encodings = Encodings.UTF8) As String()

            Dim table As File = If(tsv, File.LoadTsv(path, encoding), File.Load(path, encoding.CodePage))
            Dim getIDsDefault = Function()
                                    Return table.Columns _
                                        .First _
                                        .Skip(1) _
                                        .ToArray
                                End Function

            If uidMap.StringEmpty Then
                ' 第一列的数据就是所需要的编号数据
                Return getIDsDefault()
            Else
                With table.Headers.IndexOf(uidMap)
                    If .ByRef = -1 AndAlso ignoreMapErrors Then
                        Return getIDsDefault()
                    Else
                        ' 当不忽略错误的时候，不存在的uidMap其index位置会出现越界的错误直接在这里报错
                        Return table.Columns(.ByRef) _
                            .Skip(1) _
                            .ToArray
                    End If
                End With
            End If
        End Function

        ''' <summary>
        ''' 使用这个函数来判断目标文件之中是否存在ID列
        ''' （ID列可能不在第一列）
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="tsv"></param>
        ''' <param name="encoding"></param>
        ''' <param name="FirstColumn">
        ''' 函数总是会从这一个参数返回第一列的标题，如果不存在ID列的话可以用这一列来作为ID（可能会出现意想不到的错误）
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ContainsIDField(path$,
                                        Optional tsv As Boolean = False,
                                        Optional encoding As Encoding = Nothing,
                                        Optional ByRef firstColumn$ = Nothing) As Boolean
            Return readHeaders(
                    path,
                    tsv,
                    encoding,
                    firstColumn
                ).Any(Function(s) s = NameOf(EntityObject.ID))
        End Function

        Friend Function readHeaders(path$, tsv As Boolean, encoding As Encoding, ByRef firstColumn$) As String()
            Dim headers$()

            ' 从文件的第一行数据之中得到列标题列表
            ' 即表头字符串集合
            If Not tsv Then
                headers = New RowObject(path.ReadFirstLine(encoding)).ToArray
            Else
                headers = path _
                    .ReadFirstLine(encoding) _
                    .Split(ASCII.TAB)
            End If

            firstColumn = headers(Scan0)

            Return headers
        End Function
    End Module
End Namespace
