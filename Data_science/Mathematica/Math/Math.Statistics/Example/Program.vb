#Region "Microsoft.VisualBasic::35601e6b3539dce40d6ff34415d38fb7, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Example\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Statistics

Public Module ExampleMonteCarlo

    Sub Main()
        Call MonteCarlo()
        Call Pause()
    End Sub

    Public Sub MonteCarlo()
        ' this Is a very trivial example of creating a monte carlo using a
        ' standard normal distribution
        Dim sn As New Distributions.MethodOfMoments.Normal()

        ' output now contains 10000 random normally distributed values.
        Dim output As Vector = sn.GetInvCDF(rand(10000))

        ' to evaluate the mean And standard deviation of the output
        ' you can use Basic Product Moment Stats
        Dim BPM As New MomentFunctions.BasicProductMoments(output)

        Call println("Mean: %s", BPM.Mean())
        Call println("StDev: %s", BPM.StDev())
        Call println("Sample Size: %s", BPM.SampleSize())
        Call println("Minimum: %s", BPM.Min())
        Call println("Maximum: %s", BPM.Max())
    End Sub
End Module
