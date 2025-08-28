#Region "Microsoft.VisualBasic::5f35155bbb98ca4d1cddfa0cc07cb3ee, Data\DataFrame\IO\CSVText\CSVFile\StreamIO.vb"

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

    '   Total Lines: 139
    '    Code Lines: 86 (61.87%)
    ' Comment Lines: 35 (25.18%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 18 (12.95%)
    '     File Size: 5.88 KB


    '     Module StreamIO
    ' 
    '         Function: (+2 Overloads) [TypeOf], HeaderMatchScore, (+2 Overloads) SaveDataFrame
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace IO.CSVFile

    Public Module StreamIO

        ''' <summary>
        ''' 根据文件的头部的定义，从<paramref name="types"/>之中选取得到最合适的类型的定义
        ''' </summary>
        ''' <param name="csv"></param>
        ''' <param name="types"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [TypeOf](csv As File, ParamArray types As Type()) As Type
            Return csv.Headers.TypeOf(types)
        End Function

        <Extension>
        Public Function HeaderMatchScore(type As Type, headers As Index(Of String)) As (hits As Integer, total As Integer)
            Dim schema As SchemaProvider = SchemaProvider.CreateObjectInternal(type)
            Dim allNames$() = schema.Properties _
                .Select(Function(pi)
                            ' 20241126
                            ' get mapping field name or the property name
                            ' if the mapping file attribute tag is missing
                            Return pi.Name
                        End Function) _
                .ToArray
            Dim matches = Aggregate p As String
                In allNames
                Where headers.IndexOf(p) > -1
                Into Count

            Return (matches, allNames.Length)
        End Function

        ''' <summary>
        ''' 根据文件的头部的定义，从<paramref name="types"/>之中选取得到最合适的类型的定义
        ''' </summary>
        ''' <param name="header"></param>
        ''' <param name="types">A candidate type list</param>
        ''' <returns>
        ''' 一个也都没有匹配上, 则这个函数会返回空值
        ''' </returns>
        <Extension>
        Public Function [TypeOf](header As RowObject, ParamArray types As Type()) As Type
            Dim scores As New List(Of (Type, Integer, Integer))
            Dim headers As New Index(Of String)(header)

            For Each schema As Type In types
                With schema.HeaderMatchScore(headers)
                    scores += (schema.DeclaringType, .hits, .total)
                End With
            Next

            Dim desc = From score As (type As Type, score%, allNames%)
                       In scores
                       Select type = score.Item1, Value = score.Item2 / score.Item3
                       Order By Value Descending
            Dim topFirst = desc.FirstOrDefault

            If topFirst.Value <= 0 Then
                ' 零分表示一个属性都没有匹配上
                Return Nothing
            Else
                Return topFirst.type
            End If
        End Function

        Const NullLocationRef$ = "Sorry, the ``path`` reference to a null location!"

        ''' <summary>
        ''' Save this csv document into a specific file location <paramref name="path"/>.
        ''' </summary>
        ''' <param name="path">
        ''' 假若路径是指向一个已经存在的文件，则原有的文件数据将会被清空覆盖
        ''' </param>
        ''' <remarks>当目标保存路径不存在的时候，会自动创建文件夹</remarks>
        <Extension>
        Public Function SaveDataFrame(csv As IEnumerable(Of RowObject),
                                      Optional path$ = "",
                                      Optional encoding As Encoding = Nothing,
                                      Optional tsv As Boolean = False,
                                      Optional silent As Boolean = False) As Boolean
            If path.StringEmpty Then
                Throw New NullReferenceException(NullLocationRef)
            Else
                Return csv.SaveDataFrame(path.Open(FileMode.OpenOrCreate, doClear:=True), encoding, tsv, silent)
            End If
        End Function

        ''' <summary>
        ''' Save this csv document into a specific <paramref name="file"/> stream
        ''' </summary>
        ''' <remarks>当目标保存路径不存在的时候，会自动创建文件夹</remarks>
        <Extension>
        Public Function SaveDataFrame(csv As IEnumerable(Of RowObject),
                                      file As Stream,
                                      Optional encoding As Encoding = Nothing,
                                      Optional tsv As Boolean = False,
                                      Optional silent As Boolean = False,
                                      Optional autoCloseFile As Boolean = True) As Boolean

            Dim stopwatch As Stopwatch = Stopwatch.StartNew
            Dim del As Char = ","c Or ASCII.TAB.AsDefault(Function() tsv)
            Dim out As New StreamWriter(file, encoding Or UTF8)

            For Each line$ In csv.Select(Function(r) r.AsLine(del))
                Call out.WriteLine(line)
            Next

            ' 20221005
            '
            ' data must be flush at here
            ' or the file stream still leaves
            ' empty if the table file is
            ' small in size
            Call out.Flush()

            If autoCloseFile Then
                Call out.Dispose()
            End If

            If Not silent Then
                Call $"Generate csv file document using time {stopwatch.ElapsedMilliseconds} ms.".info
            End If

            Return True
        End Function
    End Module
End Namespace
