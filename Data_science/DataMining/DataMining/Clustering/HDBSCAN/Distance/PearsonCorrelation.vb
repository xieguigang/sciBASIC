#Region "Microsoft.VisualBasic::daf74a53cdaafaedd888669914a670ca, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Distance\PearsonCorrelation.vb"

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
    '    Code Lines: 29
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 1.67 KB


    '     Class PearsonCorrelation
    ' 
    '         Function: ComputeDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes the euclidean distance between two points, d = 1 - (cov(X,Y) / (std_dev(X) * std_dev(Y)))
    ''' </summary>
    Public Class PearsonCorrelation
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim meanOne As Double = 0
            Dim meanTwo As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                meanOne += attributesOne(i)
                meanTwo += attributesTwo(i)
                i += 1
            End While
            meanOne = meanOne / attributesOne.Length
            meanTwo = meanTwo / attributesTwo.Length
            Dim covariance As Double = 0
            Dim standardDeviationOne As Double = 0
            Dim standardDeviationTwo As Double = 0

            i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                covariance += (attributesOne(i) - meanOne) * (attributesTwo(i) - meanTwo)
                standardDeviationOne += (attributesOne(i) - meanOne) * (attributesOne(i) - meanOne)
                standardDeviationTwo += (attributesTwo(i) - meanTwo) * (attributesTwo(i) - meanTwo)
                i += 1
            End While
            Return stdNum.Max(0, 1 - covariance / stdNum.Sqrt(standardDeviationOne * standardDeviationTwo))
        End Function
    End Class
End Namespace
