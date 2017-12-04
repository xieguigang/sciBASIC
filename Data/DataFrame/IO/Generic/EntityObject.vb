#Region "Microsoft.VisualBasic::21edcb99ea2a809956c4bca5669b5fe9, ..\sciBASIC#\Data\DataFrame\IO\Generic\EntityObject.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace IO

    ''' <summary>
    ''' The object entity, <see cref="DynamicPropertyBase(Of String)"/>, <see cref="String"/>.
    ''' (有名称属性的表抽象对象)
    ''' </summary>
    Public Class EntityObject : Inherits Table
        Implements INamedValue

        ''' <summary>
        ''' This object identifier
        ''' </summary>
        ''' <returns></returns>
        <Column("ID")>
        Public Overridable Property ID As String Implements INamedValue.Key

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

        Sub New(x As EntityObject)
            Call Me.New(x.ID, New Dictionary(Of String, String)(x.Properties))
        End Sub

        ''' <summary>
        ''' Copy prop[erty value
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Copy() As EntityObject
            Return New EntityObject With {
                .ID = ID,
                .Properties = New Dictionary(Of String, String)(Properties)
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"{ID} => ({Properties.Count}) {Properties.Keys.ToArray.GetJson}"
        End Function

        Public Shared Function LoadDataSet(path As String, Optional uidMap As String = Nothing, Optional tsv As Boolean = False) As IEnumerable(Of EntityObject)
            Return LoadDataSet(Of EntityObject)(path, uidMap, tsv)
        End Function

        Public Shared Function GetIDList(path$, Optional uidMap As String = Nothing, Optional tsv As Boolean = False, Optional ignoreMapErrors As Boolean = False) As String()
            Dim table As File = If(tsv, File.LoadTsv(path), File.Load(path))
            Dim getIDsDefault =
                Function()
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
                    If .ref = -1 AndAlso ignoreMapErrors Then
                        Return getIDsDefault()
                    Else
                        ' 当不忽略错误的时候，不存在的uidMap其index位置会出现越界的错误直接在这里报错
                        Return table.Columns(.ref) _
                            .Skip(1) _
                            .ToArray
                    End If
                End With
            End If
        End Function

        Public Shared Function LoadDataSet(Of T As EntityObject)(path$, Optional uidMap$ = Nothing, Optional tsv As Boolean = False) As IEnumerable(Of T)
            If Not path.FileExists Then
                Return {}
            End If
            If uidMap.StringEmpty Then
                If Not tsv Then
                    Dim first As New RowObject(path.ReadFirstLine)
                    uidMap = first.First
                Else
                    uidMap = path.ReadFirstLine.Split(ASCII.TAB).First
                End If
            End If
            Dim map As New Dictionary(Of String, String) From {
                {uidMap, NameOf(EntityObject.ID)}
            }
            If tsv Then
                Return path.LoadTsv(Of T)(nameMaps:=map)
            Else
                Return path.LoadCsv(Of T)(explicit:=False, maps:=map)
            End If
        End Function
    End Class
End Namespace
