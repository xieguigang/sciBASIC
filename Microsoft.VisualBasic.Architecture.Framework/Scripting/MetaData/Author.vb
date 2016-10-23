#Region "Microsoft.VisualBasic::cd2a7ecc69bf94e0958ea235c7e9c2cf, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Scripting\MetaData\Author.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting.MetaData

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=True, Inherited:=True)>
    Public Class Author : Inherits Attribute

        Public ReadOnly Property value As NamedValue(Of String)

        Sub New(name As String, email As String)
            value = New NamedValue(Of String)(name, email)
        End Sub

        Public Sub EMail()
            Call Diagnostics.Process.Start($"mailto://{value.x}")
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
