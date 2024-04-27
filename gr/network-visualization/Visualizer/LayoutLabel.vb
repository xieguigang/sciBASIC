﻿#Region "Microsoft.VisualBasic::6d64f3864e053198bf2e705956364aa1, G:/GCModeller/src/runtime/sciBASIC#/gr/network-visualization/Visualizer//LayoutLabel.vb"

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

    '   Total Lines: 38
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.12 KB


    ' Class LayoutLabel
    ' 
    '     Properties: hasGDIData, offsetDistance
    ' 
    '     Function: GetTextAnchor, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Math2D

Friend Class LayoutLabel

    Public label As Label
    Public anchor As Anchor
    Public style As Font
    Public color As Brush
    Public node As Node
    Public shapeRectangle As RectangleF

    Public ReadOnly Property offsetDistance As Double
        Get
            Dim text As Point = GetTextAnchor()
            Dim anchor As New Point(anchor.X, anchor.Y)

            Return text.Distance(anchor)
        End Get
    End Property

    Public ReadOnly Property hasGDIData As Boolean
        Get
            Return (Not label.text.StringEmpty) AndAlso Not style Is Nothing
        End Get
    End Property

    Public Function GetTextAnchor() As Point
        Return label.GetTextAnchor(anchor)
    End Function

    Public Overrides Function ToString() As String
        Return $"{label.text} @ {shapeRectangle.ToString}"
    End Function
End Class
