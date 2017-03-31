#Region "Microsoft.VisualBasic::af18493fe141e76e55cdd4ad74edb5e8, ..\sciBASIC#\gr\Landscape\Graphics.vb"

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

Namespace Data

    ''' <summary>
    ''' The data model of the landscape 3D model.
    ''' </summary>
    Public Class Graphics

        Public Property Surfaces As Surface()
        ''' <summary>
        ''' The scene paint background, its value definition is the same as <see cref="Surface.paint"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property bg As String

    End Class
End Namespace
