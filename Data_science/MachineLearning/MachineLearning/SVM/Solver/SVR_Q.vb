#Region "Microsoft.VisualBasic::45ced4b9f22596b9124dbc9823c935d8, Data_science\MachineLearning\MachineLearning\SVM\Solver\SVR_Q.vb"

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

    '   Total Lines: 109
    '    Code Lines: 65 (59.63%)
    ' Comment Lines: 29 (26.61%)
    '    - Xml Docs: 96.55%
    ' 
    '   Blank Lines: 15 (13.76%)
    '     File Size: 4.05 KB


    '     Class SVR_Q
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

Namespace SVM

    ''' <summary>
    ''' The epsilon-SVR / nu-SVR formulation of the kernel matrix: the kernel 
    ''' matrix is expanded to twice of the sample size so that the two slack 
    ''' variables of each sample can be handled at the same time.
    ''' </summary>
    Friend Class SVR_Q : Inherits Kernel

        Private ReadOnly l As Integer
        Private ReadOnly cache As Cache
        Private ReadOnly sign As SByte()
        Private ReadOnly index As Integer()
        Private next_buffer As Integer
        Private buffer As Single()()
        Private ReadOnly QD As Double()

        ''' <summary>
        ''' Create the epsilon-SVR / nu-SVR formulation of the kernel matrix.
        ''' </summary>
        ''' <param name="prob">The training data.</param>
        ''' <param name="param">The training parameters.</param>
        Public Sub New(prob As Problem, param As Parameter)
            Call MyBase.New(prob.count, prob.X, param)

            l = prob.count
            cache = New Cache(l, CLng(param.cacheSize) * (1 << 20))
            QD = New Double(2 * l - 1) {}
            sign = New SByte(2 * l - 1) {}
            index = New Integer(2 * l - 1) {}

            For k = 0 To l - 1
                sign(k) = 1
                sign(k + l) = -1
                index(k) = k
                index(k + l) = k
                QD(k) = KernelFunction(k, k)
                QD(k + l) = QD(k)
            Next

            buffer = New Single()() {New Single(2 * l - 1) {}, New Single(2 * l - 1) {}}
            next_buffer = 0
        End Sub

        ''' <summary>
        ''' Swap the position of the two samples in the expanded kernel matrix, 
        ''' the sign values, the sample indices and the diagonal elements are 
        ''' swapped at the same time.
        ''' </summary>
        ''' <param name="i">The zero based index of the first sample variable.</param>
        ''' <param name="j">The zero based index of the second sample variable.</param>
        Public Overrides Sub SwapIndex(i As Integer, j As Integer)
            Do
                Dim __ = sign(i)
                sign(i) = sign(j)
                sign(j) = __
            Loop While False

            Do
                Dim __ = index(i)
                index(i) = index(j)
                index(j) = __
            Loop While False

            Do
                Dim __ = QD(i)
                QD(i) = QD(j)
                QD(j) = __
            Loop While False
        End Sub

        ''' <summary>
        ''' Request a column of the expanded kernel matrix, the result is 
        ''' reordered by the sign value of the slack variables.
        ''' </summary>
        ''' <param name="i">The zero based index of the target row.</param>
        ''' <param name="len">The number of the elements that will be requested.</param>
        ''' <returns>An array which contains the requested elements of the target column.</returns>
        Public Overrides Function GetQ(i As Integer, len As Integer) As Single()
            Dim data As Single() = Nothing
            Dim j As Integer, real_i = index(i)

            If cache.GetData(real_i, data, l) < l Then
                For j = 0 To l - 1
                    data(j) = CSng(KernelFunction(real_i, j))
                Next
            End If

            ' reorder and copy
            Dim buf = buffer(next_buffer)
            next_buffer = 1 - next_buffer
            Dim si = sign(i)

            For j = 0 To len - 1
                buf(j) = CSng(si) * sign(j) * data(index(j))
            Next

            Return buf
        End Function

        ''' <summary>
        ''' Gets the diagonal elements of the expanded kernel matrix.
        ''' </summary>
        ''' <returns>An array which contains the diagonal elements <i>Q(i, i)</i>.</returns>
        Public Overrides Function GetQD() As Double()
            Return QD
        End Function
    End Class
End Namespace
