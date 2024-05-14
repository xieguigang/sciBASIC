#Region "Microsoft.VisualBasic::4d38f7308785f2a3ebe3d2cfb5aacc1e, Data_science\Mathematica\data\LP\LP_tool\Module2.vb"

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

    '   Total Lines: 16
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 354 B


    ' Module Module2
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming

Module Module2

    Sub Main()

        Dim model = "E:\repo\xDoc\Yilia\runtime\sciBASIC#\Data_science\Mathematica\data\LP\map00220_lpp.XML".LoadXml(Of LPPModel)
        Dim lpp As New LPP(model)
        Dim solution = lpp.solve


        Pause()

    End Sub

End Module
