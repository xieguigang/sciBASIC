#Region "Microsoft.VisualBasic::36c5635da05afdcaa42489d18300079e, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\Trainable.vb"

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
    '    Code Lines: 5 (25.00%)
    ' Comment Lines: 11 (55.00%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 4 (20.00%)
    '     File Size: 530 B


    '     Interface Trainable
    ' 
    '         Function: forwardBackward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

    ''' <summary>
    ''' Trainable neural network.
    ''' </summary>
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
