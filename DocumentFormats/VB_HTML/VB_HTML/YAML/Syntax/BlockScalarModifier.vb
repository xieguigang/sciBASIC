Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.MarkupLanguage.YAML.Grammar

Namespace YAML.Syntax

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
