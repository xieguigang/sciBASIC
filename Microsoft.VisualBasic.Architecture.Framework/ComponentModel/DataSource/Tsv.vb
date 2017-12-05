#Region "Microsoft.VisualBasic::c2e1068f6c75fb91a48c15ca92911c90, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Tsv.vb"

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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports FieldTuple = System.Collections.Generic.KeyValuePair(Of String, System.Reflection.PropertyInfo)
Imports RowTokens = System.Collections.Generic.IEnumerable(Of System.String)

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' 将文件读取出来然后对每一行数据进行分割，由于没有使用自定义属性来标记列的名称，所以这个很简单的tsv加载器要求属性的名称与列名称要完全一致。
    ''' 而且，还不能够为非初始数据类型，这个模块之中提供了简单的数据类型转换操作，这个只是一个简单的内置TSv文件读取模块
    ''' </summary>
    ''' <remarks></remarks>
    Public Module TsvFileIO

        ''' <summary>
        ''' Columns indexing by title.(自动将tsv文件数据之中的行解析反序列化加载为一个Class对象)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="Path"></param>
        ''' <returns></returns>
        Public Iterator Function Load(Of T As Class)(path$, Optional encoding As Encodings = Encodings.UTF8) As IEnumerable(Of T)
            Dim data As IEnumerable(Of RowTokens) = TsvFileIO.LoadFile(path, encoding.CodePage, skipFirstLine:=True)
            Dim tableSchema = DataFramework.Schema(Of T)(PropertyAccess.ReadWrite, True)
            Dim type As Type = GetType(T)
            Dim schemaOrdinals As Index(Of String) =
                path _
                .OpenReader(encoding.CodePage) _
                .GetTsvHeader(False)
            Dim typers = tableSchema.ToDictionary(
                Function(m) m.Key,
                Function(p) p.Value.PropertyType)

            For Each line As String() In data.Select(Function(r) DirectCast(r, String()))
                Dim o As Object = Activator.CreateInstance(type)

                For Each field As FieldTuple In tableSchema
                    With field
                        Dim index As Integer = schemaOrdinals(.Key)
                        Dim s$ = line(index)
                        Dim value As Object = Scripting.CTypeDynamic(s, typers(.Key))

                        Call .Value.SetValue(o, value)
                    End With
                Next

                Yield DirectCast(o, T)
            Next
        End Function

        ''' <summary>
        ''' Columns indexing by position.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Iterator Function LoadByIndex(Of T As Class)(path$, Optional encoding As Encodings = Encodings.UTF8) As IEnumerable(Of T)
            Dim data As IEnumerable(Of RowTokens) = TsvFileIO.LoadFile(path, encoding.CodePage, skipFirstLine:=False)
            Dim type As Type = GetType(T)
            Dim index = DataFrameColumnAttribute _
                .LoadMapping(type, mapsAll:=False) _
                .Values _
                .OrderBy(Function(field) field.Field.Index) _
                .ToArray

            For Each line As RowTokens In data
                Dim o = Activator.CreateInstance(type)

                For Each col As SeqValue(Of String) In line.SeqIterator
                    With index(col)
                        Call .SetValue(o, Scripting.CTypeDynamic(col.value, .Type))
                    End With
                Next

                Yield DirectCast(o, T)
            Next
        End Function

        ''' <summary>
        ''' Returns the source string without any processing
        ''' </summary>
        ReadOnly noProcess As New DefaultValue(Of Func(Of String, String))(Function(str) str)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <param name="lower"></param>
        ''' <param name="process"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Linux平台上面的mono这里有bug，为什么<see cref="StreamReader.ReadLine()"/>一直都输出空值？
        ''' </remarks>
        <Extension>
        Public Function GetTsvHeader(stream As StreamReader,
                                     Optional lower As Boolean = False,
                                     Optional process As Func(Of String, String) = Nothing) As Index(Of String)

            Dim line$ = stream.ReadLine
            Dim headers$() = line _
                .Split(ASCII.TAB) _
                .Select(selector:=process Or noProcess) _
                .ToArray

            If lower Then
                Return headers _
                    .Select(AddressOf Strings.LCase) _
                    .Indexing
            Else
                Return New Index(Of String)(headers)
            End If
        End Function

        ''' <summary>
        ''' 读取文件并且按照TAb进行分割
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="skipFirstLine">The first line of the text document maybe is the title headers, skip this line?</param>
        ''' <returns></returns>
        Private Function LoadFile(path$, Optional encoding As Encoding = Nothing, Optional skipFirstLine As Boolean = False) As IEnumerable(Of RowTokens)
            Dim lines As String() = TextDoc.ReadAllLines(path, encoding Or UTF8)
            Dim LQuery = LinqAPI.Exec(Of RowTokens) _
 _
                () <= From strLine As String
                      In lines
                      Let t As String() = Strings.Split(strLine, vbTab) ' 跳过标题行
                      Select DirectCast(t, RowTokens)

            If skipFirstLine Then
                Return LQuery.Skip(1)
            Else
                Return LQuery
            End If
        End Function
    End Module
End Namespace
