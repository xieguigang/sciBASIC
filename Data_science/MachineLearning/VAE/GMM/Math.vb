#Region "Microsoft.VisualBasic::94b33723b2286383a4223138d7ddd2f9, Data_science\MachineLearning\VAE\GMM\Math.vb"

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

    '   Total Lines: 57
    '    Code Lines: 42
    ' Comment Lines: 5
    '   Blank Lines: 10
    '     File Size: 2.09 KB


    '     Module Math
    ' 
    '         Function: columnSum, (+2 Overloads) distToCenterL1, divisionScalar, l1Norm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    Public Module Math

        Public Function divisionScalar(X As IList(Of Double), j As Double) As Double()
            If j = 0.0 Then
                Throw New ArithmeticException("divisionScalar failed, scalar cannot be 0.0")
            Else
                Return New Vector(X) * (1 / j)
            End If
        End Function

        ''' <summary>
        ''' idea is that we have a list of lists (inner list is like a row), and we sum over all columns.
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Function columnSum(input As IList(Of IList(Of Double))) As IList(Of Double)
            Dim results As Vector = Vector.Zero([Dim]:=input(0).Count)

            For Each i As IList(Of Double) In input
                results = results + New Vector(i)
            Next

            Return results.AsList
        End Function

        Public Function l1Norm(inVector As IList(Of Double)) As IList(Of Double)
            Dim sum = 0.0
            For Each vi In inVector
                sum += std.Abs(vi)
            Next
            Return divisionScalar(inVector, sum)
        End Function

        Public Function distToCenterL1(x As Double, centers As IList(Of Double)) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)()
            For Each ci In centers
                results.Add(std.Abs(x - ci))
            Next
            Return l1Norm(results)
        End Function

        Public Function distToCenterL1(x As Double(), centers As IList(Of Double())) As IList(Of Double)
            Dim results As IList(Of Double) = New List(Of Double)()
            For Each center As Double() In centers
                results.Add(x.EuclideanDistance(center))
            Next
            Return l1Norm(results)
        End Function
    End Module

End Namespace
