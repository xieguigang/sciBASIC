#Region "Microsoft.VisualBasic::d5170ee876e433f9c0a8511334ee641f, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\HashValue.vb"

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
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure HashValue : Implements sIdEnumerable

    Public Property Identifier As String Implements sIdEnumerable.Identifier
    Public Property value As String

    Sub New(name As String, value As String)
        Me.Identifier = name
        Me.value = value
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Operator +(hash As Dictionary(Of String, String), tag As HashValue) As Dictionary(Of String, String)
        Call hash.Add(tag.Identifier, tag.value)
        Return hash
    End Operator
End Structure
