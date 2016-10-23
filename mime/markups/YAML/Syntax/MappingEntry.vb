#Region "Microsoft.VisualBasic::1ede4d3d207a3af08c42655a18c6718f, ..\visualbasic_App\mime\Markups\YAML\Syntax\MappingEntry.vb"

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

Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace YAML.Syntax

    Public Class MappingEntry : Implements sIdEnumerable

        Public Key As DataItem

        Public Value As DataItem

        Private Property Identifier As String Implements sIdEnumerable.Identifier
            Get
                Return Scripting.ToString(Key)
            End Get
            Set(value As String)
                Throw New ReadOnlyException
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return [String].Format("{{Key:{0}, Value:{1}}}", Key, Value)
        End Function
    End Class
End Namespace
