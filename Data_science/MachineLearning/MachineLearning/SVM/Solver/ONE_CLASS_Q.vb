#Region "Microsoft.VisualBasic::790a5fa7b3d9a6815b236c2972e7be35, Data_science\MachineLearning\MachineLearning\SVM\Solver\ONE_CLASS_Q.vb"

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
Imports Microsoft.VisualBasic.Text
Imports stdNum = System.Math

Namespace SVM

    Friend Class ONE_CLASS_Q
        Inherits Kernel

        Private ReadOnly cache As Cache
        Private ReadOnly QD As Double()

        Public Sub New(prob As Problem, param As Parameter)
            MyBase.New(prob.Count, prob.X, param)
            cache = New Cache(prob.Count, CLng(param.CacheSize) * (1 << 20))
            QD = New Double(prob.Count - 1) {}

            For i = 0 To prob.Count - 1
                QD(i) = KernelFunction(i, i)
            Next
        End Sub

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

        Public Overrides Function GetQD() As Double()
            Return QD
        End Function

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
