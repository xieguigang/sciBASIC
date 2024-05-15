#Region "Microsoft.VisualBasic::6151b35ff43a48a945b448be85569e47, Data_science\MachineLearning\MachineLearning\test\Module1.vb"

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

    '   Total Lines: 20
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 458 B


    ' Module Module1
    ' 
    '     Sub: functionParserTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations

Module Module1

    Sub Main()
        Call functionParserTest()
    End Sub

    Sub functionParserTest()

        Dim func As New BipolarSigmoid(alpha:=3)
        Dim str = func.ToString
        Dim model = ActiveFunction.Parse(str)
        Dim func2 As BipolarSigmoid = model

        Pause()
    End Sub

End Module
