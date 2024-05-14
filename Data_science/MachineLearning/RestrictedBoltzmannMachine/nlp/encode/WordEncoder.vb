#Region "Microsoft.VisualBasic::282ec0b2ade9bff6b4d3517fdfdef2ca, Data_science\MachineLearning\RestrictedBoltzmannMachine\nlp\encode\WordEncoder.vb"

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
    '    Code Lines: 6
    ' Comment Lines: 4
    '   Blank Lines: 3
    '     File Size: 353 B


    '     Interface WordEncoder
    ' 
    '         Function: encode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math

Namespace nlp.encode

    ''' <summary>
    ''' Created by kenny on 6/3/14.
    ''' TODO create word encoder that captures
    ''' </summary>
    Public Interface WordEncoder
        Function encode(word As String) As DenseMatrix
    End Interface

End Namespace
