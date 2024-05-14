#Region "Microsoft.VisualBasic::9ee5b34274a4f448ba9c4299a9425880, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\SvgDefaults.vb"

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

    '   Total Lines: 57
    '    Code Lines: 50
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 2.63 KB


    '     Module SvgDefaults
    ' 
    ' 
    '         Class Attributes
    ' 
    ' 
    '             Class FillAndStroke
    ' 
    ' 
    ' 
    '             Class Position
    ' 
    ' 
    ' 
    '             Class Size
    ' 
    ' 
    ' 
    '             Class Radius
    ' 
    ' 
    ' 
    '             Class Gradient
    ' 
    ' 
    ' 
    '             Class Text
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    '     Enum LineJoin
    ' 
    '         Miter
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums

Namespace SVG.XML
    Public Module SvgDefaults
        Public NotInheritable Class Attributes
            Public NotInheritable Class FillAndStroke
                Public Shared ReadOnly FillOpacity As Double = 1
                Public Shared ReadOnly StrokeOpacity As Double = 1
                Public Shared ReadOnly StrokeWidth As Double = 0
                Public Shared ReadOnly StrokeLineJoin As LineJoin = LineJoin.Miter
                Public Shared ReadOnly StrokeLineCap As SvgStrokeLineCap = SvgStrokeLineCap.Butt
                Public Shared ReadOnly StrokeDashArray As Double() = Nothing
                Public Shared ReadOnly Fill As String = "#000000"
                Public Shared ReadOnly Stroke As String = "#000000"
                Public Shared ReadOnly Opacity As Double = 1
                Public Shared ReadOnly FillRule As SvgFillRule = SvgFillRule.NonZero
            End Class

            Public NotInheritable Class Position
                Public Shared ReadOnly X As Double = 0
                Public Shared ReadOnly Y As Double = 0
                Public Shared ReadOnly CX As Double = 0
                Public Shared ReadOnly CY As Double = 0
                Public Shared ReadOnly DX As Double = 0
                Public Shared ReadOnly DY As Double = 0
            End Class

            Public NotInheritable Class Size
                Public Shared ReadOnly Width As Double = 0
                Public Shared ReadOnly Height As Double = 0
            End Class

            Public NotInheritable Class Radius
                Public Shared ReadOnly R As Double = 0
                Public Shared ReadOnly RX As Double = 0
                Public Shared ReadOnly RY As Double = 0
            End Class

            Public NotInheritable Class Gradient
                Public Shared ReadOnly Offset As Double = 0
                Public Shared ReadOnly StopOpacity As Double = 1
                Public Shared ReadOnly StopColor As String = "#000000"
            End Class

            Public NotInheritable Class Text
                Public Shared ReadOnly FontSize As Double = 16
                Public Shared ReadOnly FontFamily As String = "Helvetica, Arial, sans-serif"
                Public Shared ReadOnly TextAnchor As SvgTextAnchor = SvgTextAnchor.Start
                Public Shared ReadOnly DominantBaseline As SvgDominantBaseline = SvgDominantBaseline.Auto
            End Class
        End Class
    End Module

    Public Enum LineJoin
        Miter
    End Enum
End Namespace
