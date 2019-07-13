#Region "Microsoft.VisualBasic::e270c7bfe2e57aa9c8ebffeaaef2d8e9, mime\application%rtf\Models\Font.vb"

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

    '     Class Font
    ' 
    '         Properties: FontBold, FontColor, FontFamilyName, FontItalic, FontSize
    '                     FontUnderline
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Models

    Public MustInherit Class Font

        Public Property FontSize As Integer
        Public Property FontBold As Boolean
        Public Property FontFamilyName As String
        Public Property FontItalic As Boolean
        Public Property FontColor As Color
        Public Property FontUnderline As Boolean

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
