#Region "Microsoft.VisualBasic::ee531c649a540cf95a155ede211caae1, Data\DataFrame\Extensions\Write_csv.vb"

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

    '   Total Lines: 144
    '    Code Lines: 98 (68.06%)
    ' Comment Lines: 31 (21.53%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (10.42%)
    '     File Size: 6.23 KB


    ' Module Write_csv
    ' 
    '     Function: (+2 Overloads) SaveTo
    '     Class Arguments
    ' 
    '         Properties: encoding, layout, maps, metaBlank, nonParallel
    '                     reorderKeys, silent, strict, transpose, tsv
    ' 
    '         Function: ToCsv
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions

''' <summary>
''' write csv file method that call in clr environment
''' </summary>
Public Module Write_csv

    Public Class Arguments

        Public Property strict As Boolean = False
        Public Property encoding As Encoding = Nothing
        Public Property metaBlank As String = ""
        Public Property nonParallel As Boolean = False
        Public Property maps As Dictionary(Of String, String) = Nothing
        Public Property reorderKeys As Integer = 0
        Public Property layout As Dictionary(Of String, Integer) = Nothing
        Public Property tsv As Boolean = False
        Public Property transpose As Boolean = False
        Public Property silent As Boolean = False

        Public Function ToCsv(Of T)(objSeq As Object) As IEnumerable(Of RowObject)
            Dim csv As IEnumerable(Of RowObject) = Reflector.GetsRowData(
                source:=objSeq,
                type:=GetType(T),
                strict:=strict,
                maps:=maps,
                parallel:=Not nonParallel,
                metaBlank:=metaBlank,
                reorderKeys:=reorderKeys,
                layout:=layout
            )

            If transpose Then
                Return csv _
                    .Select(Function(r) r.ToArray) _
                    .MatrixTranspose _
                    .Select(Function(r) New RowObject(r)) _
                    .ToArray
            Else
                Return csv
            End If
        End Function

    End Class

    ''' <summary>
    ''' Save the object collection data dump into a csv file.
    ''' (将一个对象数组之中的对象保存至一个Csv文件之中，请注意:
    ''' + 这个方法仅仅会保存简单的基本数据类型的属性值
    ''' + 并且这个方法仅适用于小型数据集, 如果需要保存大型数据集, 请使用Linq版本的拓展函数)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source">应该是List, Array或者Collection, 不应该是一个Linq拓展表达式</param>
    ''' <param name="path"></param>
    ''' <param name="strict">
    ''' If true then all of the simple data type property its value will be save to the data file,
    ''' if not then only save the property with the <see cref="ColumnAttribute"></see>
    ''' </param>
    ''' <param name="encoding"></param>
    ''' <param name="maps">``{meta_define -> custom}``</param>
    ''' <param name="layout">可以通过这个参数来进行列顺序的重排，值越小表示排在越前面</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function SaveTo(Of T)(source As IEnumerable(Of T), path As String,
                                 Optional strict As Boolean = False,
                                 Optional encoding As Encoding = Nothing,
                                 Optional metaBlank As String = "",
                                 Optional nonParallel As Boolean = False,
                                 Optional maps As Dictionary(Of String, String) = Nothing,
                                 Optional reorderKeys As Integer = 0,
                                 Optional layout As Dictionary(Of String, Integer) = Nothing,
                                 Optional tsv As Boolean = False,
                                 Optional transpose As Boolean = False,
                                 Optional silent As Boolean = False) As Boolean

        Dim argv As New Arguments With {
            .encoding = encoding,
            .layout = layout,
            .maps = maps,
            .metaBlank = metaBlank,
            .nonParallel = nonParallel,
            .reorderKeys = reorderKeys,
            .silent = silent,
            .strict = strict,
            .transpose = transpose,
            .tsv = tsv
        }

        Try
            path = FileIO.FileSystem.GetFileInfo(path).FullName
        Catch ex As Exception
            Throw New Exception("Probably invalid path value: " & path, ex)
        End Try

        Using file As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False, verbose:=Not silent)
            Return source.SaveTo(file, args:=argv)
        End Using
    End Function

    ''' <summary>
    ''' Save the object collection data dump into a csv file.
    ''' (将一个对象数组之中的对象保存至一个Csv文件之中，请注意:
    ''' + 这个方法仅仅会保存简单的基本数据类型的属性值
    ''' + 并且这个方法仅适用于小型数据集, 如果需要保存大型数据集, 请使用Linq版本的拓展函数)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source">应该是List, Array或者Collection, 不应该是一个Linq拓展表达式</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function SaveTo(Of T)(source As IEnumerable(Of T), file As Stream, args As Arguments) As Boolean
        Dim objSeq As Object() = source _
            .Select(Function(o) DirectCast(o, Object)) _
            .ToArray

        If Not args.silent Then
            Call EchoLine($"[CSV.Reflector::{GetType(T).FullName}]")
            Call EchoLine($"Save data to file:///{file.ToString}")
            Call EchoLine($"[CSV.Reflector] Reflector have {objSeq.Length} lines of data to write.")
        End If

        Dim success = args.ToCsv(Of T)(objSeq).SaveDataFrame(
            file:=file,
            encoding:=args.encoding,
            tsv:=args.tsv,
            silent:=args.silent
        )

        If success AndAlso Not args.silent Then
            Call "CSV saved!".EchoLine
        End If

        Return success
    End Function
End Module
