#Region "Microsoft.VisualBasic::92a2dc54f9402d7c4fa1b455ea533055, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\Python\NamedTuple.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Python

    ''' <summary>
    ''' ``namedtuple()`` Factory Function for Tuples with Named Fields
    ''' </summary>
    Public Class NamedTuple : Inherits [Property](Of Object)
        Implements sIdEnumerable

        Public Property Type As String Implements sIdEnumerable.Identifier

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
