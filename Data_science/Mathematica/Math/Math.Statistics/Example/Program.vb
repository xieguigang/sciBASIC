#Region "Microsoft.VisualBasic::f1fa4d1df7f156b0fe243748914e3ac8, Data_science\Mathematica\Math\Math.Statistics\Example\Program.vb"

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

    '   Total Lines: 29
    '    Code Lines: 18
    ' Comment Lines: 5
    '   Blank Lines: 6
    '     File Size: 1017 B


    ' Module ExampleMonteCarlo
    ' 
    '     Sub: Main, MonteCarlo
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics

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
