﻿#Region "Microsoft.VisualBasic::4b6d3c707d1c144be31f823785fa4b4b, Microsoft.VisualBasic.Core\src\Drawing\GDI+\Driver\DeviceInterop.vb"

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


    ' Code Statistics:

    '   Total Lines: 32
    '    Code Lines: 9 (28.12%)
    ' Comment Lines: 17 (53.12%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 6 (18.75%)
    '     File Size: 1.30 KB


    '     Class DeviceInterop
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Imaging.Driver

    Public MustInherit Class DeviceInterop

        Public MustOverride Function CreateGraphic(size As Size, fill As Color, dpi As Integer) As IGraphics

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="background"></param>
        ''' <param name="direct_access">
        ''' create the graphics canvas directly based on the input background image resource if set this parameter to true, 
        ''' or make a copy of the image and then create the graphics canvas if set this parameter false.
        ''' </param>
        ''' <returns></returns>
        Public MustOverride Function CreateCanvas2D(background As Bitmap, direct_access As Boolean) As IGraphics
        Public MustOverride Function CreateCanvas2D(background As Image, direct_access As Boolean) As IGraphics

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="padding">
        ''' the graphics drawing layout context information, should be an integer vector of paddings
        ''' </param>
        ''' <returns></returns>
        Public MustOverride Function GetData(g As IGraphics, padding As Integer()) As IGraphicsData

    End Class
End Namespace
