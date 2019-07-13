#Region "Microsoft.VisualBasic::f0b3709a47a69b50c0b2581b3f1df83d, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\ASCIIArt\WeightedChar.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class WeightedChar
    ' 
    '         Properties: Character, CharacterImage, Weight
    ' 
    '         Function: getDefaultCharSet, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        Friend Shared Function getDefaultCharSet() As [Default](Of  WeightedChar())
            Return CharSet.GenerateFontWeights()
        End Function
    End Class
End Namespace
