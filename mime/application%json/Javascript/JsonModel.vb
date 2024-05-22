#Region "Microsoft.VisualBasic::9181e8afa491fce795988254b89aae9d, mime\application%json\Javascript\JsonModel.vb"

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

    '   Total Lines: 38
    '    Code Lines: 30 (78.95%)
    ' Comment Lines: 3 (7.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (13.16%)
    '     File Size: 1.25 KB


    '     Class JsonModel
    ' 
    '         Sub: DisposeObjects
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Javascript

    ''' <summary>
    ''' The abstract model of the array/object
    ''' </summary>
    Public MustInherit Class JsonModel : Inherits JsonElement

#Region "Json property and value"
        Default Public Property Item(str As String) As JsonElement
            Get
                Return CType(Me, JsonObject)(str)
            End Get
            Set(value As JsonElement)
                CType(Me, JsonObject)(str) = value
            End Set
        End Property

        Default Public Property Item(index As Integer) As JsonElement
            Get
                Return CType(Me, JsonArray)(index)
            End Get
            Set(value As JsonElement)
                CType(Me, JsonArray)(index) = value
            End Set
        End Property
#End Region

        Protected Shared Sub DisposeObjects(obj As JsonElement)
            If TypeOf obj Is JsonObject Then
                Call DirectCast(obj, JsonObject).Dispose()
            ElseIf TypeOf obj Is JsonArray Then
                Call DirectCast(obj, JsonArray).list.DoEach(AddressOf DisposeObjects)
                Call DirectCast(obj, JsonArray).list.Clear()
            End If
        End Sub

    End Class
End Namespace
