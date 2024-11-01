#Region "Microsoft.VisualBasic::dcd7eaf3088e1c4069cfc5de1785a5b2, Data_science\MachineLearning\DeepLearning\RNN\net\interfaces\CharacterSampleable.vb"

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
    '    Code Lines: 6 (33.33%)
    ' Comment Lines: 10 (55.56%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (11.11%)
    '     File Size: 802 B


    ' 	Interface CharacterSampleable
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN
	Public Interface CharacterSampleable

		' Samples length characters, advances the state.
		' Seed must be at least one character.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		' Throws, if any character in seed is not part of the alphabet.
		Function sampleString(length As Integer, seed As String, temp As Double) As String

		' Samples length characters, choose whether to advance the state.
		' Seed must be at least one character.
		' temp is the must be in (0.0,1.0]. Lower temp means more conservative
		' predictions.
		' Throws, if any character in seed is not part of the alphabet.
		Function sampleString(length As Integer, seed As String, temp As Double, advance As Boolean) As String
	End Interface
End Namespace
