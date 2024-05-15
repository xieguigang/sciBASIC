#Region "Microsoft.VisualBasic::34458e86aa01e3980d681c1e02533e8a, Data_science\Mathematica\Math\Gibbs\test\Program.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 594 B


    ' Module Program
    ' 
    '     Sub: gibbsTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.GibbsSampling
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main(args As String())
        Call gibbsTest()

        Console.WriteLine("Hello World!")
    End Sub

    Public Sub gibbsTest()
        Dim data = New String() {"ABCDAAAABDB", "AAAADCBBCA", "DDBCABAAAACBBD", "AABAAAACCDD", "ABCBDBDDDDDBCBBCBC", "ABAAAACBBDAABAAAACC", "CDAAAABDBAA", "AADCBBCADDB"}
        Dim length = 4
        Dim gibbs As Gibbs = New Gibbs(data, length)

        Call Console.WriteLine(gibbs.sample.GetJson)
    End Sub
End Module
