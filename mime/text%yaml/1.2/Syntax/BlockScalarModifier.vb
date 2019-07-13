#Region "Microsoft.VisualBasic::2a3bc2feefb61f9a020768d46e1657e5, mime\text%yaml\1.2\Syntax\BlockScalarModifier.vb"

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

    '     Class BlockScalarModifier
    ' 
    '         Function: GetChompingMethod, GetIndent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar

Namespace Syntax

    Public Class BlockScalarModifier

        Public Indent As Char
        Public Chomp As Char

        Public Function GetIndent() As Integer
            If Indent > "0"c AndAlso Indent <= "9"c Then
                Return Asc(Indent) - Asc("0"c)
            Else
                Return 1
            End If
        End Function

        Public Function GetChompingMethod() As ChompingMethod
            Select Case Chomp
                Case "-"c
                    Return ChompingMethod.Strip
                Case "+"c
                    Return ChompingMethod.Keep
                Case Else
                    Return ChompingMethod.Clip
            End Select
        End Function
    End Class
End Namespace
