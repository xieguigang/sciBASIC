
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Colors

    Public Module Designer

        Public Function Sequence(n%, ParamArray colors As Color()) As Color()
            Dim out As New List(Of Color)
            Dim steps! = n / colors.Length
            Dim previous As Value(Of Color) = colors.First

            For Each c As Color In colors
                out += ColorCube.GetColorSequence(
                    source:=previous,
                    target:=previous = c,
                    increment:=steps!)
            Next

            Return out
        End Function
    End Module
End Namespace