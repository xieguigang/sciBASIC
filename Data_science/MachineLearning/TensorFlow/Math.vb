#Region "Microsoft.VisualBasic::014f5bcd17a5ff37909ce3d57b91c55f, Data_science\MachineLearning\TensorFlow\Math.vb"

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

    '   Total Lines: 30
    '    Code Lines: 23 (76.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (23.33%)
    '     File Size: 762 B


    ' Module Math
    ' 
    '     Function: clip_by_value, exp, log, reduce_mean, reduce_sum
    '               square
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Math

    Public Function reduce_sum(v As Vector) As Double
        Return v.Sum
    End Function

    Public Function reduce_mean(v As Vector) As Double
        Return v.Average
    End Function

    Public Function clip_by_value(v As Vector, min As Double, max As Double) As Vector
        v(v < min) = Vector.Scalar(min)
        v(v > max) = Vector.Scalar(max)
        Return v
    End Function

    Public Function square(v As Vector) As Vector
        Return v ^ 2
    End Function

    Public Function exp(v As Vector) As Vector
        Return v.Exp
    End Function

    Public Function log(v As Vector) As Vector
        Return v.Log
    End Function
End Module
