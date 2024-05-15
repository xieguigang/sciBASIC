#Region "Microsoft.VisualBasic::d63db75a34be823b7f8d21daa552cb38, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\factory\RBMFactory.vb"

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

    '   Total Lines: 10
    '    Code Lines: 5
    ' Comment Lines: 3
    '   Blank Lines: 2
    '     File Size: 258 B


    '     Interface RBMFactory
    ' 
    '         Function: build
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace nn.rbm.factory

    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' </summary>
    Public Interface RBMFactory
        Function build(numVisibleNodes As Integer, numHiddenNodes As Integer) As RBM
    End Interface

End Namespace
