#Region "Microsoft.VisualBasic::65b8ab3fb4f7136464bfe9d294c3f62a, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IXml.vb"

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

Imports System.Text

Public MustInherit Class IXml

    Protected MustOverride Function filePath() As String
    Protected MustOverride Function toXml() As String

    Public Overrides Function ToString() As String
        Return filePath()
    End Function

    Public Function WriteXml(dir$) As Boolean
        Dim path$ = dir & "/" & filePath()
        Dim xml$ = toXml()
        Return xml.SaveTo(path, Encoding.UTF8)
    End Function
End Class
