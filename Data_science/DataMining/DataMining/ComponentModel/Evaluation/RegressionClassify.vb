#Region "Microsoft.VisualBasic::b322d411c1fb3ff3889d3460c9da2449, sciBASIC#\Data_science\DataMining\DataMining\ComponentModel\Evaluation\RegressionClassify.vb"

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

    '   Total Lines: 21
    '    Code Lines: 13
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 519 B


    '     Class RegressionClassify
    ' 
    '         Properties: actual, errors, predicts, sampleID
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace ComponentModel.Evaluation

    ''' <summary>
    ''' The regression classifier result.
    ''' </summary>
    Public Class RegressionClassify

        Public Property sampleID As String
        Public Property actual As Double
        Public Property predicts As Double

        Public ReadOnly Property errors As Double
            Get
                Return stdNum.Abs(predicts - actual)
            End Get
        End Property

    End Class
End Namespace
