#Region "Microsoft.VisualBasic::97f53272e58f1a2881a84bd30d751d92, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\IntegerSampleable.vb"

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
    '    Code Lines: 6 (35.29%)
    ' Comment Lines: 9 (52.94%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (11.76%)
    '     File Size: 706 B


    ' 	Interface IntegerSampleable
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN
	' Network that can be sampled for a sequence of integers.
	Public Interface IntegerSampleable

		' Samples n indices, advances the state.
		' Seed must be at least one index.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		Function sampleIndices(n As Integer, seed As Integer(), temp As Double) As Integer()

		' Samples n indices, choose whether to advance the state.
		' Seed must be at least one index.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		Function sampleIndices(n As Integer, seed As Integer(), temp As Double, advance As Boolean) As Integer()
	End Interface
End Namespace
