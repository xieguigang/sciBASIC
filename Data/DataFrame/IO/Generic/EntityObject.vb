#Region "Microsoft.VisualBasic::b14e8a3148a3b68af9768a7da00701eb, ..\sciBASIC#\Data\DataFrame\IO\EntityObject.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

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
        Public Property ID As String Implements INamedValue.Key

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

        Public Shared Function LoadDataSet(path As String, Optional uidMap As String = Nothing) As IEnumerable(Of EntityObject)
            Return LoadDataSet(Of EntityObject)(path, uidMap)
        End Function

        Public Shared Function LoadDataSet(Of T As EntityObject)(path As String, Optional uidMap As String = Nothing) As IEnumerable(Of T)
            If uidMap.StringEmpty Then
                Dim first As New RowObject(path.ReadFirstLine)
                uidMap = first.First
            End If
            Dim map As New Dictionary(Of String, String) From {
                {uidMap, NameOf(EntityObject.ID)}
            }
            Return path.LoadCsv(Of T)(explicit:=False, maps:=map)
        End Function
    End Class
End Namespace
