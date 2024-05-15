#Region "Microsoft.VisualBasic::5ccaf427df832fba69d3b7a1edb74305, Data_science\Mathematica\Math\DataFittings\LMA.vb"

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

    '   Total Lines: 36
    '    Code Lines: 15
    ' Comment Lines: 16
    '   Blank Lines: 5
    '     File Size: 1.13 KB


    ' Module LMA
    ' 
    '     Function: NonLinearFit
    '     Structure FitInput
    ' 
    '         Function: ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ### Levenberg–Marquardt algorithm
''' 
''' In mathematics and computing, the Levenberg–Marquardt algorithm 
''' (LMA or just LM), also known as the damped least-squares (DLS) 
''' method, is used to solve non-linear least squares problems. 
''' 
''' These minimization problems arise especially in least squares 
''' curve fitting.
''' </summary>
Public Module LMA

    Public Structure FitInput
        ''' <summary>
        ''' The matrix row
        ''' </summary>
        Dim factors As Dictionary(Of String, Double)
        ''' <summary>
        ''' The non-linear function result output 
        ''' </summary>
        Dim y As Double

        Public Overrides Function ToString() As String
            Return $"f(X) = {y} = f({factors.GetJson})"
        End Function
    End Structure

    <Extension>
    Public Function NonLinearFit(matrix As IEnumerable(Of FitInput), Optional iterations% = 2000) As Object

        Throw New NotImplementedException
    End Function
End Module
