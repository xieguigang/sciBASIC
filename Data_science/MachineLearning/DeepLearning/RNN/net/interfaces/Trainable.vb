#Region "Microsoft.VisualBasic::5b68e0afbe63fac08c109499e401e6b1, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\Trainable.vb"

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

    '   Total Lines: 17
    '    Code Lines: 5 (29.41%)
    ' Comment Lines: 9 (52.94%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (17.65%)
    '     File Size: 487 B


    '     Interface Trainable
    ' 
    '         Function: forwardBackward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN
    ' Trainable neural network.
    Public Interface Trainable

        ' 
        ' 			Performs a forward-backward pass for the given indices.
        ' 	
        ' 			ix.length and iy.length lengths must match.
        ' 			All indices must be less than the vocabulary size.
        ' 	
        ' 			Returns the cross-entropy loss.
        ' 		
        Function forwardBackward(ix As Integer(), iy As Integer()) As Double
    End Interface


End Namespace
