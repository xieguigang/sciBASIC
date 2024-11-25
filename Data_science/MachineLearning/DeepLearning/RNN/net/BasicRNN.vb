#Region "Microsoft.VisualBasic::3f69e31964a3352beab8ee66583c0d76, Data_science\MachineLearning\DeepLearning\RNN\net\BasicRNN.vb"

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

    '   Total Lines: 13
    '    Code Lines: 6 (46.15%)
    ' Comment Lines: 5 (38.46%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 2 (15.38%)
    '     File Size: 387 B


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

        ' Initializes the net for this vocabulary size.
        ' Requires vocabularySize > 0.
        Public MustOverride Sub initialize(vocabularySize As Integer)
	End Class
End Namespace
