#Region "Microsoft.VisualBasic::973f255d8316c1efca2fad1e584e86ac, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\distance\DistanceFunction.vb"

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

    '   Total Lines: 12
    '    Code Lines: 5
    ' Comment Lines: 5
    '   Blank Lines: 2
    '     File Size: 325 B


    '     Interface DistanceFunction
    ' 
    '         Function: distance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions.distance

    ''' <summary>
    ''' kenny
    ''' 
    ''' Distance is analogous to the 1 - normalized(similarityScore)
    ''' </summary>
    Public Interface DistanceFunction
        Function distance(item1 As DenseMatrix, item2 As DenseMatrix) As Double
    End Interface

End Namespace
