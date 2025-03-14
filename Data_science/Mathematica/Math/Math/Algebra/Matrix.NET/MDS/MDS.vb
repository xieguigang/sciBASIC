#Region "Microsoft.VisualBasic::422a34a6f4b9ee38b941e41d8357fe75, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\MDS\MDS.vb"

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

    '   Total Lines: 81
    '    Code Lines: 38 (46.91%)
    ' Comment Lines: 33 (40.74%)
    '    - Xml Docs: 48.48%
    ' 
    '   Blank Lines: 10 (12.35%)
    '     File Size: 3.01 KB


    '     Class MDS
    ' 
    '         Function: classicalScaling, (+2 Overloads) distanceScaling, fullmds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdf = System.Math

' 
'  Copyright (C) 2014. Daniel Asarnow
' 
'  This program is free software: you can redistribute it and/or modify
'  it under the terms of the GNU General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' 
'  This program is distributed in the hope that it will be useful,
'  but WITHOUT ANY WARRANTY; without even the implied warranty of
'  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'  GNU General Public License for more details.
' 
'  You should have received a copy of the GNU General Public License
'  along with this program.  If not, see <http://www.gnu.org/licenses/>.
' 

Namespace LinearAlgebra.Matrix.MDSScale

    ''' <summary>
    ''' Created by IntelliJ IDEA.
    ''' User: da
    ''' Date: 10/8/11
    ''' Time: 2:25 AM
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/asarnow/mdscale
    ''' </remarks>
    Public Class MDS

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="d">should be a NxN distance matrix.</param>
        ''' <param name="dim"></param>
        ''' <returns>
        ''' this function returns an column array with <paramref name="dim"/> elements, each elements is a dimension column result
        ''' </returns>
        Public Shared Function fullmds(d As Double()(), [dim] As Integer) As Double()()
            Dim result = RectangularArray.Matrix(Of Double)([dim], d.Length)
            Dim evals = New Double(result.Length - 1) {}

            Data.randomize(result)
            Data.squareEntries(d)
            Data.doubleCenter(d)
            Data.multiply(d, -0.5R)
            Data.eigen(d, result, evals)

            For i = 0 To result.Length - 1
                evals(i) = stdf.Sqrt(evals(i))
                For j = 0 To result(0).Length - 1
                    result(i)(j) *= evals(i)
                Next
            Next
            Return result
        End Function

        Public Shared Function classicalScaling(distances As Double()(), [dim] As Integer) As Double()()
            Dim d = Data.copyMatrix(distances)
            Return fullmds(d, [dim])
        End Function

        Public Shared Function distanceScaling(d As Double()(), [dim] As Integer) As Double()()
            Dim x = classicalScaling(d, [dim])
            Dim smacof As SMACOF = New SMACOF(d, x)
            smacof.iterate(100, 3)
            Return smacof.Positions
        End Function

        Public Shared Function distanceScaling(d As Double()(), [dim] As Integer, iter As Integer, threshold As Integer) As Double()()
            Dim x = classicalScaling(d, [dim])
            Dim smacof As SMACOF = New SMACOF(d, x)
            smacof.iterate(iter, threshold)
            Return smacof.Positions
        End Function
    End Class

End Namespace
