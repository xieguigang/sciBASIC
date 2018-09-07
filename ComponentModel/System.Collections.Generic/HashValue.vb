#Region "Microsoft.VisualBasic::dc28bafa34d0dc0d8bcc76f7e836698a, Microsoft.VisualBasic.Core\ComponentModel\System.Collections.Generic\HashValue.vb"

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

    '     Structure HashValue
    ' 
    '         Properties: key, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    Public Structure HashValue : Implements INamedValue

        Public Property key As String Implements INamedValue.Key
        Public Property value As String

        Sub New(name As String, value As String)
            Me.key = name
            Me.value = value
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Operator +(hash As Dictionary(Of String, String), tag As HashValue) As Dictionary(Of String, String)
            Call hash.Add(tag.key, tag.value)
            Return hash
        End Operator
    End Structure
End Namespace
