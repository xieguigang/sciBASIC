Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags

Namespace ApplicationServices.Terminal.TablePrinter
    Public Class CharMapDefinition
        Public Shared FramePipDefinition As Dictionary(Of CharMapPositions, Char) = New Dictionary(Of CharMapPositions, Char) From {
            {CharMapPositions.TopLeft, "┌"c},
            {CharMapPositions.TopCenter, "┬"c},
            {CharMapPositions.TopRight, "┐"c},
            {CharMapPositions.MiddleLeft, "├"c},
            {CharMapPositions.MiddleCenter, "┼"c},
            {CharMapPositions.MiddleRight, "┤"c},
            {CharMapPositions.BottomLeft, "└"c},
            {CharMapPositions.BottomCenter, "┴"c},
            {CharMapPositions.BottomRight, "┘"c},
            {CharMapPositions.BorderLeft, "│"c},
            {CharMapPositions.BorderRight, "│"c},
            {CharMapPositions.BorderTop, "─"c},
            {CharMapPositions.BorderBottom, "─"c},
            {CharMapPositions.DividerY, "│"c},
            {CharMapPositions.DividerX, "─"c}
        }
        Public Shared FrameDoublePipDefinition As Dictionary(Of CharMapPositions, Char) = New Dictionary(Of CharMapPositions, Char) From {
            {CharMapPositions.TopLeft, "╔"c},
            {CharMapPositions.TopCenter, "╤"c},
            {CharMapPositions.TopRight, "╗"c},
            {CharMapPositions.MiddleLeft, "╟"c},
            {CharMapPositions.MiddleCenter, "┼"c},
            {CharMapPositions.MiddleRight, "╢"c},
            {CharMapPositions.BottomLeft, "╚"c},
            {CharMapPositions.BottomCenter, "╧"c},
            {CharMapPositions.BottomRight, "╝"c},
            {CharMapPositions.BorderLeft, "║"c},
            {CharMapPositions.BorderRight, "║"c},
            {CharMapPositions.BorderTop, "═"c},
            {CharMapPositions.BorderBottom, "═"c},
            {CharMapPositions.DividerY, "│"c},
            {CharMapPositions.DividerX, "─"c}
        }
    End Class
End Namespace
