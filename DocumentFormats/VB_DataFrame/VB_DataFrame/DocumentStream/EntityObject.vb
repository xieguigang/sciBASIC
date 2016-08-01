#Region "Microsoft.VisualBasic::095d013c489c058076ead01c98adb5e2, ..\VisualBasic_AppFramework\DocumentFormats\VB_DataFrame\VB_DataFrame\DocumentStream\EntityObject.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace DocumentStream

    Public Class EntityObject : Inherits DynamicPropertyBase(Of String)
        Implements sIdEnumerable

        ''' <summary>
        ''' This object identifier
        ''' </summary>
        ''' <returns></returns>
        Public Property Identifier As String Implements sIdEnumerable.Identifier

        ''' <summary>
        ''' Copy prop[erty value
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Copy() As EntityObject
            Return New EntityObject With {
                .Identifier = Identifier,
                .Properties = New Dictionary(Of String, String)(Properties)
            }
        End Function

        Public Shared Iterator Function LoadDataSet(path As String, uidMap As String) As IEnumerable(Of EntityObject)
            Dim map As New Dictionary(Of String, String) From {{uidMap, NameOf(EntityObject.Identifier)}}
            For Each x As EntityObject In path.LoadCsv(Of EntityObject)(explicit:=False, maps:=map)
                Yield x
            Next
        End Function
    End Class

    Public Class DataSet : Inherits DynamicPropertyBase(Of Double)
        Implements sIdEnumerable

        Public Property Identifier As String Implements sIdEnumerable.Identifier

        ''' <summary>
        ''' Copy prop[erty value
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Copy() As DataSet
            Return New DataSet With {
                .Identifier = Identifier,
                .Properties = New Dictionary(Of String, Double)(Properties)
            }
        End Function

        Public Shared Iterator Function LoadDataSet(path As String, uidMap As String) As IEnumerable(Of DataSet)
            Dim map As New Dictionary(Of String, String) From {{uidMap, NameOf(DataSet.Identifier)}}
            For Each x As DataSet In path.LoadCsv(Of DataSet)(explicit:=False, maps:=map)
                Yield x
            Next
        End Function
    End Class
End Namespace
