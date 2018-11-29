Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

Namespace Drawing2D.Text.ASCIIArt

    Public Class WeightedChar

        Public Property Character As String
        Public Property CharacterImage As Bitmap
        Public Property Weight As Double

        Public Overrides Function ToString() As String
            Return $"{Character} ({Weight})"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function getDefaultCharSet() As DefaultValue(Of WeightedChar())
            Return CharSet.GenerateFontWeights()
        End Function
    End Class
End Namespace