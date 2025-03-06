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

        Public Shared Function fullmds(d As Double()(), [dim] As Integer) As Double()()
            Dim result = RectangularArray.Matrix(Of Double)([dim], d.Length)
            Data.randomize(result)
            Dim evals = New Double(result.Length - 1) {}
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
