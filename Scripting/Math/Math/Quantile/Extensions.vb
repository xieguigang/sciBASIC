#Region "Microsoft.VisualBasic::8d31086910a76ca89b741e3d5c0b936a, ..\visualbasic_App\Scripting\Math\Math\Quantile\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Runtime.CompilerServices

Namespace Quantile

    Public Module Extensions

        Const epsilon As Double = 0.001

        <Extension>
        Public Function GKQuantile(source As IEnumerable(Of Long),
                                   Optional epsilon As Double = Extensions.epsilon,
                                   Optional compact_size As Integer = 1000) As QuantileEstimationGK

            Dim estimator As New QuantileEstimationGK(epsilon, compact_size)

            For Each x As Long In source
                Call estimator.insert(x)
            Next

            Return estimator
        End Function

        Const window_size As Integer = 10000

        Private Sub Test()
            Dim shuffle As Long() = New Long(window_size - 1) {}

            For i As Integer = 0 To shuffle.Length - 1
                shuffle(i) = i
            Next

            shuffle = shuffle.Shuffles

            Dim estimator As QuantileEstimationGK = shuffle.GKQuantile
            Dim quantiles As Double() = {0.5, 0.9, 0.95, 0.99, 1.0}

            For Each q As Double In quantiles
                Dim estimate As Long = estimator.query(q)
                Dim actual As Long = shuffle.actually(q)
                Console.WriteLine(String.Format("Estimated {0:F2} quantile as {1:D} (actually {2:D})", q, estimate, actual))
            Next
        End Sub

        <Extension>
        Public Function actually(source As Long(), q As Double) As Long
            Dim actual As Long = CLng(Fix((q) * (source.Length - 1)))
            Return actual
        End Function
    End Module
End Namespace
