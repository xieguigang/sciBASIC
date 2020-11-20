#Region "Microsoft.VisualBasic::2a3bc2feefb61f9a020768d46e1657e5, mime\text%yaml\1.2\Syntax\BlockScalarModifier.vb"

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
