#Region "Microsoft.VisualBasic::2da1f98b1791d7f3e146faf7a8aa2275, Data_science\MachineLearning\DeepLearning\RNN\net\BasicRNN.vb"

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

    '   Total Lines: 15
    '    Code Lines: 6 (40.00%)
    ' Comment Lines: 7 (46.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (13.33%)
    '     File Size: 506 B


    '     Class BasicRNN
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace RNN

    ''' <summary>
    ''' RNN that uses integer indices as inputs and outputs.
    ''' </summary>
    <Serializable>
    Public MustInherit Class BasicRNN : Inherits RNN

        ''' <summary>
        ''' Initializes the network for the given vocabulary size.
        ''' </summary>
        ''' <param name="vocabularySize">The vocabulary size; it must be greater than zero.</param>
        Public MustOverride Sub initialize(vocabularySize As Integer)
	End Class
End Namespace
