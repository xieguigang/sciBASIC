#Region "Microsoft.VisualBasic::349d392d8758400a9965393d010f4694, Data_science\MachineLearning\MachineLearning\SVM\Solver\ONE_CLASS_Q.vb"

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

    '   Total Lines: 51
    '    Code Lines: 38 (74.51%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (25.49%)
    '     File Size: 1.42 KB


    '     Class ONE_CLASS_Q
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetQ, GetQD
    ' 
    '         Sub: SwapIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace SVM

    ''' <summary>
    ''' The one-class SVM formulation of the kernel matrix.
    ''' </summary>
    Friend Class ONE_CLASS_Q : Inherits Kernel

        ReadOnly cache As Cache
        ReadOnly QD As Double()

        ''' <summary>
        ''' Create the one-class SVM formulation of the kernel matrix.
        ''' </summary>
        ''' <param name="prob">The training data.</param>
        ''' <param name="param">The training parameters.</param>
        Public Sub New(prob As Problem, param As Parameter)
            MyBase.New(prob.count, prob.X, param)

            cache = New Cache(prob.count, CLng(param.cacheSize) * (1 << 20))
            QD = New Double(prob.count - 1) {}

            For i = 0 To prob.count - 1
                QD(i) = KernelFunction(i, i)
            Next
        End Sub

        ''' <summary>
        ''' Request a column of the kernel matrix.
        ''' </summary>
        ''' <param name="i">The zero based index of the target row.</param>
        ''' <param name="len">The number of the elements that will be requested.</param>
        ''' <returns>An array which contains the requested elements of the target column.</returns>
        Public Overrides Function GetQ(i As Integer, len As Integer) As Single()
            Dim data As Single() = Nothing
            Dim start As i32 = 0
            Dim j As Integer

            If (start = cache.GetData(i, data, len)) < len Then
                For j = start To len - 1
                    data(j) = CSng(KernelFunction(i, j))
                Next
            End If

            Return data
        End Function

        ''' <summary>
        ''' Gets the diagonal elements of the kernel matrix.
        ''' </summary>
        ''' <returns>An array which contains the diagonal elements <i>Q(i, i)</i>.</returns>
        Public Overrides Function GetQD() As Double()
            Return QD
        End Function

        ''' <summary>
        ''' Swap the position of the two samples in the kernel matrix and the 
        ''' cached diagonal elements.
        ''' </summary>
        ''' <param name="i">The zero based index of the first sample.</param>
        ''' <param name="j">The zero based index of the second sample.</param>
        Public Overrides Sub SwapIndex(i As Integer, j As Integer)
            cache.SwapIndex(i, j)
            MyBase.SwapIndex(i, j)

            Do
                Dim __ = QD(i)
                QD(i) = QD(j)
                QD(j) = __
            Loop While False
        End Sub
    End Class

End Namespace
