#Region "Microsoft.VisualBasic::988275e92e47fc46635f172eabef3f06, Data_science\Mathematica\Math\Math.Statistics\Example\Fisher.vb"

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

    '   Total Lines: 22
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 646 B


    ' Module Fisher
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.FishersExact

Module Fisher

    Sub Main()


        Dim p222 = FishersExactTest.FishersExact(65, 70, 235, 19930)

        Console.WriteLine(FisherTest.FisherPvalue(65, 70, 235, 19930))
        Console.WriteLine(p222.ToString)

        Pause()

        Dim p = FisherTest.FisherPvalue(1, 9, 11, 3) '0.0013460761879122358
        Dim p2 = FisherTest.FisherPvalue(0, 10, 12, 2) '3.3651904697805894E-05
        Dim p3 = FisherTest.FisherPvalue(3, 40, 297, 19960) '0.021858115774312393

        Pause()
    End Sub
End Module
