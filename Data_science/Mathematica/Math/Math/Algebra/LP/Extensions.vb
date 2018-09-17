#Region "Microsoft.VisualBasic::a4ef484859b530385e7aa739f0651d29, Data_science\Mathematica\Math\Math\Algebra\LP\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: copyOf, formatDecimals, ParseType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Algebra.LinearProgramming

    Public Module Extensions

        Friend Function copyOf(Of T)(original As T(), newLength%) As T()
            Dim copy As T() = New T(newLength - 1) {}
            Array.Copy(original, 0, copy, 0, VBMath.Min(original.Length, newLength))
            Return copy
        End Function

        ''' <summary>
        ''' String formatting helper function.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function formatDecimals(d As Double) As String
            Return d.ToString("#,###.###")
        End Function

        <Extension>
        Friend Function ParseType(type As String) As OptimizationType
            Select Case LCase(type)
                Case "max", "maximize"
                    Return OptimizationType.MAX
                Case "min", "minimize"
                    Return OptimizationType.MIN
                Case Else
                    Throw New NotImplementedException(type)
            End Select
        End Function
    End Module
End Namespace
