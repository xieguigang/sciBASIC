#Region "Microsoft.VisualBasic::65b6ad84a20bf49c36fd0a3bb8ce3f8a, Data_science\MachineLearning\DeepLearning\CNN\data\TrainResult.vb"

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

    '   Total Lines: 47
    '    Code Lines: 34 (72.34%)
    ' Comment Lines: 3 (6.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (21.28%)
    '     File Size: 1.34 KB


    '     Class TrainResult
    ' 
    '         Properties: Loss
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace CNN.data

    ''' <summary>
    ''' Created by danielp on 1/27/17.
    ''' </summary>
    Public Class TrainResult

        Friend fwd_time, bwd_time As Long
        Friend l2_decay_loss, l1_decay_loss, cost_loss, softmax_loss

        Dim m_loss As Double

        Public Overridable ReadOnly Property Loss As Double
            Get
                Return m_loss
            End Get
        End Property

        Public Sub New(fwd_time As Long,
                       bwd_time As Long,
                       l1_decay_loss As Double,
                       l2_decay_loss As Double,
                       cost_loss As Double,
                       softmax_loss As Double,
                       loss As Double)

            Me.fwd_time = fwd_time
            Me.bwd_time = bwd_time
            Me.l1_decay_loss = l1_decay_loss
            Me.l2_decay_loss = l2_decay_loss
            Me.cost_loss = cost_loss
            Me.softmax_loss = softmax_loss

            m_loss = loss
        End Sub

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("loss: ")
            sb.Append(cost_loss)
            Return sb.ToString()
        End Function
    End Class

End Namespace
