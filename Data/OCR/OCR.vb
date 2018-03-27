Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module OCR

    <Extension>
    Public Iterator Function GetCharacters(view As Image, library As Library) As IEnumerable(Of (position As Rectangle, obj As Char, score#))
        For Each block In view.ToVector(size:=library.Window, fillDeli:=True)
            Dim pixels As Vector = block.Maps
            Dim subject As New OpticalCharacter With {
                .PixelsVector = pixels
            }
            Dim find = library.Match(subject)

            If find.score > 0 Then
                Yield (New Rectangle(block.Key, library.Window), find.recognized, find.score)
            End If
        Next
    End Function
End Module
