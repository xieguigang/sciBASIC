#Region "Microsoft.VisualBasic::19de6bd9898c3ec432f52570db8e30c0, Microsoft.VisualBasic.Core\src\Extensions\Math\NumeralSystem.vb"

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


    ' Code Statistics:

    '   Total Lines: 26
    '    Code Lines: 15
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 750 B


    '     Module NumeralSystem
    ' 
    '         Function: TranslateDecimal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Math

    Public Module NumeralSystem

        ''' <summary>
        ''' 将十进制数转换到另外的一个数进制
        ''' </summary>
        ''' <param name="d%"></param>
        ''' <param name="alphas"></param>
        ''' <returns></returns>
        <Extension> Public Function TranslateDecimal(d%, alphas As Char()) As String
            Dim r = d Mod alphas.Length
            Dim result$

            If (d - r = 0) Then
                result = alphas(r)
            Else
                result = ((d - r) \ alphas.Length).TranslateDecimal(alphas) & alphas(r)
            End If

            Return result
        End Function
    End Module
End Namespace
