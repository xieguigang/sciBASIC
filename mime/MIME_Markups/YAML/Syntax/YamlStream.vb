#Region "Microsoft.VisualBasic::e546fcfa9f653b8ba0b6048f4c3f672e, ..\sciBASIC#\mime\markups\YAML\Syntax\YamlStream.vb"

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
Imports Microsoft.VisualBasic.Scripting

Namespace YAML.Syntax

    Public Class YamlStream

        Public Property Documents As New List(Of YamlDocument)()

        Public Iterator Function Enumerative() As IEnumerable(Of Dictionary(Of MappingEntry))
            For Each doc As YamlDocument In Documents
                Yield __maps(doc)
            Next
        End Function

        Private Function __maps(doc As YamlDocument) As Dictionary(Of MappingEntry)
            Dim root As Mapping = doc.Root.As(Of Mapping)
            Return root.GetMaps
        End Function
    End Class
End Namespace
