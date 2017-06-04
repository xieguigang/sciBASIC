#Region "Microsoft.VisualBasic::22237e734219f77fd67a1771f6d81f5c, ..\sciBASIC#\mime\text%yaml\yaml\Syntax\BlockScalarModifier.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.MIME.YAML.Grammar

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
