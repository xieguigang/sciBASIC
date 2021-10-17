﻿#Region "Microsoft.VisualBasic::2185e62b6d1095ada711c065f0d01a44, Data_science\MachineLearning\xgboost\TGBoost\Loss.vb"

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

    '     Class Loss
    ' 
    ' 
    ' 
    '     Class SquareLoss
    ' 
    '         Function: grad, hess, transform
    ' 
    '     Class LogisticLoss
    ' 
    '         Function: clip, grad, hess, transform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java
Imports stdNum = System.Math

Namespace train

    Public MustInherit Class Loss

        Public MustOverride Function grad(pred As Double(), label As Double()) As Double()
        Public MustOverride Function hess(pred As Double(), label As Double()) As Double()
        Public MustOverride Function transform(pred As Double()) As Double()

    End Class

    Friend Class SquareLoss : Inherits Loss

        Public Overrides Function transform(pred As Double()) As Double()
            Return pred
        End Function

        Public Overrides Function grad(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = pred(i) - label(i)
            Next

            Return ret
        End Function

        Public Overrides Function hess(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}
            Arrays.fill(ret, 1.0)
            Return ret
        End Function
    End Class

    Friend Class LogisticLoss : Inherits Loss

        Private Function clip(val As Double) As Double
            If val < 0.00001 Then
                Return 0.00001
            ElseIf val > 0.99999 Then
                Return 0.99999
            Else
                Return val
            End If
        End Function

        Public Overrides Function transform(pred As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = clip(1.0 / (1.0 + stdNum.Exp(-pred(i))))
            Next

            Return ret
        End Function

        Public Overrides Function grad(pred As Double(), label As Double()) As Double()
            Dim pred1 = transform(pred)
            Dim ret = New Double(pred1.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = pred1(i) - label(i)
            Next

            Return ret
        End Function

        Public Overrides Function hess(pred As Double(), label As Double()) As Double()
            Dim pred1 = transform(pred)
            Dim ret = New Double(pred.Length - 1) {}

            For i = 0 To ret.Length - 1
                ret(i) = pred1(i) * (1.0 - pred1(i))
            Next

            Return ret
        End Function
    End Class
End Namespace
