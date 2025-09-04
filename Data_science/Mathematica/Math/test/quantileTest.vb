#Region "Microsoft.VisualBasic::43043136898fc0034c6f09b4be51bfe6, sciBASIC#\Data_science\Mathematica\Math\test\quantileTest.vb"

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

    '   Total Lines: 34
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 885.00 B


    ' Module quantileTest
    ' 
    '     Sub: Main, quartile
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile

Module quantileTest

    Sub Main()

        Call quantileTest.quartile()


        Dim q = New Double() {5, 100, 200, 2000, 300, 20, 20, 20, 20, 3000, 9999999, 1, 1, 1, 1, 1, 99}.GKQuantile

        For Each l In {0, 0.25, 0.5, 0.75, 1}
            Call q.Query(l).debug
        Next
    End Sub

    Sub quartile()
        Dim test = Sub(x As Vector)
                       Dim q = x.Quartile
                       Dim div = x.AsVector.Outlier(q)
                   End Sub

        Call test({3465, 345, 73, 495})
        Call test({3, 46, 5, 345, 73, 49, 5})
        Call test({3465, 3, 45, 73, 49, 5})
        Call test({3465, 345, 73495})
        Call test({3465, 34573495})
        Call test({3465345})


        Pause()
    End Sub
End Module
