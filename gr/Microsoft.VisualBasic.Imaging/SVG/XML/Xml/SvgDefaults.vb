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
