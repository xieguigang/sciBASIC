#Region "Microsoft.VisualBasic::a1760ad1f550faa1677ff4e93da5f361, Data\DataFrame\IO\Generic\EntityObject.vb"

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

    '   Total Lines: 216
    '    Code Lines: 131 (60.65%)
    ' Comment Lines: 63 (29.17%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 22 (10.19%)
    '     File Size: 8.89 KB


    '     Class EntityObject
    ' 
    '         Properties: ID
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Copy, CreateFilter, GetPropertyNames, (+4 Overloads) LoadDataSet, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace IO

    ''' <summary>
    ''' The object entity, <see cref="DynamicPropertyBase(Of String)"/>, <see cref="String"/>.
    ''' </summary>
    ''' <remarks>
    ''' (有名称属性的表抽象对象)
    ''' </remarks>
    Public Class EntityObject : Inherits Table
        Implements INamedValue

        ''' <summary>
        ''' This object identifier
        ''' </summary>
        ''' <returns></returns>
        <Column("ID")>
        Public Overridable Property ID As String Implements INamedValue.Key

        ''' <summary>
        ''' 这个属性构建出与javascript之中的对象的属性读取类似的效果
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public Overrides Property ItemValue(name As String) As String
            Get
                If name = NameOf(ID) Then
                    Return ID
                Else
                    Return MyBase.ItemValue(name)
                End If
            End Get
            Set(value As String)
                If name = NameOf(ID) Then
                    ID = value
                Else
                    MyBase.ItemValue(name) = value
                End If
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(id$)
            Me.ID = id
            Me.Properties = New Dictionary(Of String, String)
        End Sub

        Sub New(id$, props As Dictionary(Of String, String))
            Me.ID = id
            Me.Properties = props
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(obj As EntityObject)
            Call Me.New(obj.ID, New Dictionary(Of String, String)(obj.Properties))
        End Sub

        ''' <summary>
        ''' Copy prop[erty value
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Copy() As EntityObject
            Return New EntityObject With {
                .ID = ID,
                .Properties = New Dictionary(Of String, String)(Properties)
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"{ID}: ({Properties.Count}) {Properties.Keys.JoinBy(", ")}"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path$">
        ''' MetaScripting:
        ''' 
        ''' path: file path or directory scripting
        ''' 
        ''' filepath: file.csv or file.tsv
        ''' scripting: dir/* means all *.csv or *.tsv
        '''            dir/M* means all file match pattern M*.csv or M*.tsv
        ''' </param>
        ''' <param name="uidMap$"></param>
        ''' <param name="tsv"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDataSet(path$,
                                           Optional ByRef uidMap$ = Nothing,
                                           Optional fieldNameMaps As Dictionary(Of String, String) = Nothing,
                                           Optional tsv As Boolean = False,
                                           Optional encoding As Encoding = Nothing,
                                           Optional silent As Boolean = False) As IEnumerable(Of EntityObject)

            ' 2018-09-04 在原来的代码这里，空的path字符串是返回空集合的
            ' 为了保持和原来的代码的兼容性，在这里使用LastOrDefault来防止抛出错误
            If path.LastOrDefault = "*"c Then
                Dim data As New List(Of EntityObject)
                Dim dir$ = path.Trim("*"c)

                For Each file As String In ls - l - r - ("*.csv" Or "*.tsv".When(tsv)) <= dir
                    data += LoadDataSet(Of EntityObject)(file, uidMap, fieldNameMaps, tsv, encoding:=encoding, silent:=silent)
                Next

                Return data
            Else
                Return LoadDataSet(Of EntityObject)(path, uidMap, fieldNameMaps, tsv, encoding:=encoding, silent:=silent)
            End If
        End Function

        ''' <summary>
        ''' 如果文件头之中存在ID列,则返回除了ID列以外的名称集合
        ''' 如果文件头之中不存在ID列的话,则返回跳过第一列的名称的集合
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="tsv"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Shared Function GetPropertyNames(path$, Optional tsv As Boolean = False, Optional encoding As Encoding = Nothing) As String()
            Dim headers$() = readHeaders(path, tsv, encoding, Nothing)

            If headers.Any(Function(s) s = NameOf(EntityObject.ID)) Then
                Return headers _
                    .Where(Function(s) Not s = NameOf(EntityObject.ID)) _
                    .ToArray
            Else
                Return headers.Skip(1).ToArray
            End If
        End Function

        ''' <summary>
        ''' 会自动查找ID列
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path$"></param>
        ''' <param name="uidMap$"></param>
        ''' <param name="tsv"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Shared Function LoadDataSet(Of T As EntityObject)(path$,
                                                                 Optional ByRef uidMap$ = Nothing,
                                                                 Optional fieldNameMaps As Dictionary(Of String, String) = Nothing,
                                                                 Optional tsv As Boolean = False,
                                                                 Optional encoding As Encoding = Nothing,
                                                                 Optional silent As Boolean = False) As IEnumerable(Of T)
            If Not path.FileExists Then
#If DEBUG Then
                Call $"{path} is missing on your file system!".Warning
#End If
                Return {}
            Else
                uidMap = FileFormat.SolveDataSetIDMapping(path, uidMap, tsv, encoding)
            End If

            With New NameMapping(fieldNameMaps)
                Call .Add(uidMap, NameOf(EntityObject.ID))

                If tsv Then
                    Return path.LoadTsv(Of T)(
                        nameMaps:= .ByRef,
                        encoding:=encoding
                    )
                Else
                    Return path.LoadCsv(Of T)(
                        explicit:=False,
                        maps:= .ByRef,
                        encoding:=encoding,
                        mute:=silent
                    )
                End If
            End With
        End Function

        ''' <summary>
        ''' 选出列的值等于目标字符串值的所有数据
        ''' </summary>
        ''' <param name="filter"><see cref="NamedValue(Of String).IsEmpty"/> means select all data.</param>
        ''' <returns></returns>
        Public Shared Function CreateFilter(filter As NamedValue(Of String)) As Func(Of EntityObject, Boolean)
            If filter.IsEmpty Then
                Return Function(obj) True
            Else
                Return Function(obj) obj(filter.Name) = filter.Value
            End If
        End Function

        Public Shared Function LoadDataSet(Of T As EntityObject)(stream As File, Optional ByRef uidMap$ = Nothing) As IEnumerable(Of T)
            Dim map As New Dictionary(Of String, String) From {
                {uidMap Or stream(0, 0).AsDefault, NameOf(EntityObject.ID)}
            }
            Return stream.AsDataSource(Of T)(strict:=False, maps:=map)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDataSet(stream As File, Optional ByRef uidMap$ = Nothing) As IEnumerable(Of EntityObject)
            Return LoadDataSet(Of EntityObject)(stream, uidMap)
        End Function
    End Class
End Namespace
