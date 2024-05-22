#Region "Microsoft.VisualBasic::2e7f55177d63fdf2b40f29d1db8eadd4, Data_science\DataMining\DataMining\Evaluation\LabelEvaluate\ChangePoint.vb"

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
    '    Code Lines: 14 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (22.22%)
    '     File Size: 474 B


    '     Class ChangePoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Evaluation

    Friend Class ChangePoint

        Public Sub New(tp As Integer, fp As Integer, tn As Integer, fn As Integer)
            Me.TP = tp
            Me.FP = fp
            Me.TN = tn
            Me.FN = fn
        End Sub

        Public TP, FP, TN, FN As Integer

        Public Overrides Function ToString() As String
            Return String.Format("{0}:{1}:{2}:{3}", TP, FP, TN, FN)
        End Function
    End Class
End Namespace
