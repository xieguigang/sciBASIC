#Region "Microsoft.VisualBasic::af5d3871d19674398339c3dc10c7167e, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\I3DModel.vb"

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

Imports System.Drawing

Namespace Drawing3D.Models

    Public Interface I3DModel : Inherits IEnumerable(Of Point3D)

        Function Copy(data As IEnumerable(Of Point3D)) As I3DModel
        Sub Draw(ByRef canvas As Graphics, camera As Camera)
    End Interface
End Namespace
