#Region "Microsoft.VisualBasic::7c33444871f1ff2e5530c090660d19b6, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\factory\RandomRBMFactory.vb"

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

    '   Total Lines: 32
    '    Code Lines: 21 (65.62%)
    ' Comment Lines: 3 (9.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (25.00%)
    '     File Size: 911 B


    '     Class RandomRBMFactory
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: build, randomWeight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nn.rbm.factory

    ''' <summary>
    ''' Created by kenny on 5/12/14.
    ''' </summary>
    Public Class RandomRBMFactory
        Implements RBMFactory

        Public Sub New()
        End Sub

        Public Function build(numVisibleNodes As Integer, numHiddenNodes As Integer) As RBM Implements RBMFactory.build
            Dim rbm As RBM = New RBM(numVisibleNodes, numHiddenNodes)

            Dim weights = rbm.Weights
            For i = 0 To numVisibleNodes - 1
                For j = 0 To numHiddenNodes - 1
                    weights.set(i, j, randomWeight())
                Next
            Next
            Return rbm
        End Function

        Private Shared Function randomWeight() As Double
            Return randf.NextGaussian() * 0.1
        End Function

    End Class

End Namespace
