#Region "Microsoft.VisualBasic::616513fac774ab64fb8535159bb821e0, Data_science\Mathematica\SignalProcessing\SignalProcessing\NDtw\Preprocessing\CentralizationPreprocessor.vb"

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

    '   Total Lines: 19
    '    Code Lines: 12
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 595 B


    '     Class CentralizationPreprocessor
    ' 
    '         Function: Preprocess, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' f(x) = x - mean
    ''' </summary>
    Public Class CentralizationPreprocessor : Inherits IPreprocessor

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Preprocess(data As Double()) As Double()
            Return SIMD.Subtract.f64_op_subtract_f64_scalar(data, data.Average)
        End Function

        Public Overrides Function ToString() As String
            Return "Centralization"
        End Function
    End Class
End Namespace
