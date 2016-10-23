#Region "Microsoft.VisualBasic::8a99e39f4df3bbf47a92d14666408958, ..\visualbasic_App\Data\SearchEngine\SearchEngine\Adapter\IObject.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure IObject

    Public ReadOnly Property Type As Type
    Public ReadOnly Property Schema As Dictionary(Of String, IProperty)

    Sub New(type As Type)
        Me.Type = type
        Me.Schema = New Dictionary(Of String, IProperty)

        For Each p In DataFrameColumnAttribute _
            .LoadMapping(type, , mapsAll:=True)
            Call Schema.Add(p.Key, p.Value)
        Next
    End Sub

    ''' <summary>
    ''' 这个接口是针对字典类型的对象而准备的
    ''' </summary>
    ''' <param name="keys"></param>
    Sub New(keys As IEnumerable(Of String))
        Type = GetType(IDictionary)
        Schema = New Dictionary(Of String, IProperty)

        For Each key$ In keys
            Schema.Add(key$, New DictionaryKey(key$))
        Next
    End Sub

    ''' <summary>
    ''' 返回: ``FiledName: value_string``
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function EnumerateFields(x As Object) As IEnumerable(Of NamedValue(Of String))
        For Each field In Schema.Values
            Yield New NamedValue(Of String) With {
                .Name = field.Identity,
                .x = Scripting.ToString(field.GetValue(x))
            }
        Next
    End Function

    Public Overrides Function ToString() As String
        Return New NamedValue(Of String()) With {
            .Name = Type.FullName,
            .x = Schema.Keys.ToArray
        }.GetJson
    End Function
End Structure
