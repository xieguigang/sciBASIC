#Region "Microsoft.VisualBasic::bd8d8f8267e3104286ab6b9402a16cf8, Data_science\MachineLearning\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: Delta
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public Module Extensions

    ''' <summary>
    ''' Small delta for GA mutations
    ''' </summary>
    ''' <param name="x#"></param>
    ''' <param name="d#"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 1 = 10 ^ 0  ~  0.1 = 10 ^ 1 * 0.1
    ''' 10 = 10 ^ 1  ~ 1 = 10 ^ 2 * 0.1
    ''' </remarks>
    <Extension>
    Public Function Delta(x#, Optional d# = 1 / 10) As Double
        Dim p10 = Fix(Math.Log10(x))
        Dim small = (10 ^ (p10 + 1)) * d
        Return small
    End Function
End Module
