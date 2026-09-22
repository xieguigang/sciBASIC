#Region "Microsoft.VisualBasic::5af00e64b7d36716ea420bcb743351ac, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\Trainable.vb"

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

    '   Total Lines: 18
    '    Code Lines: 5 (27.78%)
    ' Comment Lines: 9 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (22.22%)
    '     File Size: 625 B


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

        ''' <summary>
        ''' Performs a forward-backward pass for the given indices.
        ''' </summary>
        ''' <param name="ix">The input indices; its length must match <paramref name="iy"/>.</param>
        ''' <param name="iy">The target indices; every index must be smaller than the vocabulary size.</param>
        ''' <returns>The cross-entropy loss.</returns>
        Function forwardBackward(ix As Integer(), iy As Integer()) As Double

    End Interface

End Namespace
