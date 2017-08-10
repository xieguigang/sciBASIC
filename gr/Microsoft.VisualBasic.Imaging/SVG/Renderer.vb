#Region "Microsoft.VisualBasic::cfe7cde58febab2ca5a0997194867fa2, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\Renderer.vb"

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

Imports Microsoft.VisualBasic.Imaging.Driver

Namespace SVG

    ''' <summary>
    ''' 将SVG图像渲染为gdi+图像<see cref="Drawing.Image"/>
    ''' </summary>
    Public Class Renderer

        Public Function DrawImage(svg As SVGData) As Drawing.Image
            Throw New NotImplementedException
        End Function
    End Class
End Namespace
