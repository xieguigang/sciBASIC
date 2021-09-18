#Region "Microsoft.VisualBasic::a7c0e148ee88dd8b7dd40ddb3f543a37, Data\DataFrame\IO\Generic\DataSet.vb"

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

'     Class DataSet
' 
'         Properties: ID, MyHashCode, Vector
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Append, Copy, (+2 Overloads) LoadDataSet, SubSet, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Text

Namespace IO

    ''' <summary>
    ''' The numeric dataset, <see cref="DynamicPropertyBase(Of Double)"/>, <see cref="Double"/>.
    ''' (数值类型的数据集合，每一个数据实体对象都有自己的编号以及数据属性)
    ''' </summary>
    Public Class DataSet : Inherits DynamicPropertyBase(Of Double)
        Implements INamedValue

        ''' <summary>
        ''' 当前的这条数据记录在整个数据集之中的唯一标记符
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 20191031
        ''' 重写这个属性会造成<see cref="FileFormat.SolveDataSetIDMapping(String, String, Boolean?, Encoding)"/>失效
        ''' </remarks>
        Public Overridable Property ID As String Implements INamedValue.Key

        Protected Overrides ReadOnly Property MyHashCode As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ID.GetHashCode
            End Get
        End Property

        ''' <summary>
        ''' equivalent to <see cref="Properties"/>.Values.ToArray
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 在进行序列化为csv表格文件的时候，这个属性将会被忽略掉
        ''' </remarks>
        <Ignored>
        Public ReadOnly Property Vector As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.propertyTable.Values.ToArray
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(id$)
            Me.ID = id
            Me.Properties = New Dictionary(Of String, Double)
        End Sub

        Shared ReadOnly replace As New [Default](Of Func(Of Double, Double, Double))(Function(previous, now) now)

        ''' <summary>
        ''' 将一系列数据添加进入当前的数据集对象实例之中
        ''' </summary>
        ''' <param name="data">数据系列</param>
        ''' <param name="duplicated"></param>
        ''' <returns></returns>
        Public Function Append(data As [Property](Of Double), Optional duplicated As Func(Of Double, Double, Double) = Nothing) As DataSet
            duplicated = duplicated Or DataSet.replace

            For Each item In data.Properties
                If Me.HasProperty(item.Key) Then
                    Me(item.Key) = duplicated(Me(item.Key), item.Value)
                Else
                    Me.Properties.Add(item.Key, item.Value)
                End If
            Next

            Return Me
        End Function

        ''' <summary>
        ''' Copy prop[erty value
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Copy() As DataSet
            Return New DataSet With {
                .ID = ID,
                .Properties = New Dictionary(Of String, Double)(Properties)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{ID} has ({Properties.Keys.Take(5).JoinBy(", ")}...) {MyBase.ToString}"
        End Function

        ''' <summary>
        ''' 直接使用<paramref name="labels"/>取出<see cref="Properties"/>之中的一个子集
        ''' 对于不存在的属性，默认值为零
        ''' </summary>
        ''' <param name="labels"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SubSet(labels As IEnumerable(Of String)) As DataSet
            Return New DataSet With {
                .ID = ID,
                .Properties = labels _
                    .ToDictionary(Function(x) x,
                                  Function(x)
                                      Return Me(x)
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' The dataset for this table loader should be in format like:
        ''' 
        ''' + First column should be a string value column for indicate the dataset row uniquely.
        ''' + If the first column is not the rows' unique id, then <paramref name="uidMap"/> parameter should be provided for specific the which column is your datasets' uid column
        ''' + Then all of the other column will be treated as the numeric property data
        ''' 
        ''' <paramref name="uidMap"/>一般情况下会自动进行判断，不需要具体的设置
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="uidMap">
        ''' 默认是使用csv文件的第一行第一个单元格中的内容作为标识符，但是有时候可能标识符不是在第一列的，则这个时候就需要对这个参数进行赋值了
        ''' </param>
        ''' <param name="fieldNameMaps">
        ''' [name_in_file => name_after_load]
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDataSet(path$,
                                           Optional uidMap$ = Nothing,
                                           Optional fieldNameMaps As Dictionary(Of String, String) = Nothing,
                                           Optional tsv As Boolean = False,
                                           Optional encoding As Encoding = Nothing,
                                           Optional silent As Boolean = False) As IEnumerable(Of DataSet)

            Return EntityObject.LoadDataSet(
                path:=path,
                uidMap:=uidMap,
                fieldNameMaps:=fieldNameMaps,
                tsv:=tsv,
                encoding:=encoding,
                silent:=silent
            ).AsDataSet
        End Function

        ''' <summary>
        ''' 这个函数可以处理csv以及tsv数据格式
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path$"></param>
        ''' <param name="uidMap$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Shared Function LoadDataSet(Of T As DataSet)(path$,
                                                            Optional uidMap$ = Nothing,
                                                            Optional encoding As Encoding = Nothing,
                                                            Optional isTsv As Boolean = False) As IEnumerable(Of T)

            Dim mapFrom$ = FileFormat.SolveDataSetIDMapping(path, uidMap, isTsv, encoding)

            If isTsv Then
                Return path.LoadTsv(Of T)(encoding, {{mapFrom, NameOf(DataSet.ID)}})
            Else
                Return path.LoadCsv(Of T)(
                    explicit:=False,
                    maps:={{mapFrom, NameOf(DataSet.ID)}},
                    encoding:=encoding
                )
            End If
        End Function

        ''' <summary>
        ''' 加载一个矩阵数据：单元格全是数字类型，但是缺少第一列ID数据
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="isTsv"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Shared Iterator Function LoadMatrix(path As String, Optional isTsv As Boolean = False, Optional encoding As Encoding = Nothing) As IEnumerable(Of DataSet)
            Using reader As New StreamReader(path.Open(FileMode.Open, doClear:=False, [readOnly]:=True), encoding)
                Dim line As Value(Of String) = reader.ReadLine
                Dim deli As Char = If(isTsv, ASCII.TAB, ","c)
                Dim headers As String() = Tokenizer.CharsParser(line, deli).ToArray
                Dim vector As Double()
                Dim data As Dictionary(Of String, Double)
                Dim i As i32 = 1

                Do While Not line = reader.ReadLine Is Nothing
                    vector = Tokenizer _
                        .CharsParser(line, deli) _
                        .Select(AddressOf Val) _
                        .ToArray
                    data = New Dictionary(Of String, Double)

                    For j As Integer = 0 To headers.Length - 1
                        data(headers(j)) = vector(j)
                    Next

                    Yield New DataSet With {
                        .ID = ++i,
                        .Properties = data
                    }
                Loop
            End Using
        End Function
    End Class
End Namespace
