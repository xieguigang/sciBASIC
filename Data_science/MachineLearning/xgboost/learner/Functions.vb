#Region "Microsoft.VisualBasic::f5a40d9581c9db31626876902944be0b, Data_science\MachineLearning\xgboost\learner\Functions.vb"

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

    '   Total Lines: 92
    '    Code Lines: 62 (67.39%)
    ' Comment Lines: 9 (9.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 21 (22.83%)
    '     File Size: 2.59 KB


    '     Class RegLossObjLogistic
    ' 
    '         Function: (+2 Overloads) predTransform, sigmoid
    ' 
    '     Class SoftmaxMultiClassObjClassify
    ' 
    '         Function: (+2 Overloads) predTransform
    ' 
    '     Class SoftmaxMultiClassObjProb
    ' 
    '         Function: exp, (+2 Overloads) predTransform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace learner

    ''' <summary>
    ''' Logistic regression.
    ''' </summary>
    <Serializable>
    Friend Class RegLossObjLogistic
        Inherits ObjFunction

        Public Overloads Overrides Function predTransform(preds As Double()) As Double()
            For i = 0 To preds.Length - 1
                preds(i) = sigmoid(preds(i))
            Next

            Return preds
        End Function

        Public Overloads Overrides Function predTransform(pred As Double) As Double
            Return sigmoid(pred)
        End Function

        Friend Overridable Function sigmoid(x As Double) As Double
            Return 1 / (1 + stdNum.Exp(-x))
        End Function
    End Class

    ''' <summary>
    ''' Multiclass classification.
    ''' </summary>
    <Serializable>
    Friend Class SoftmaxMultiClassObjClassify
        Inherits ObjFunction

        Public Overloads Overrides Function predTransform(preds As Double()) As Double()
            Dim maxIndex = 0
            Dim max = preds(0)

            For i = 1 To preds.Length - 1

                If max < preds(i) Then
                    maxIndex = i
                    max = preds(i)
                End If
            Next

            Return New Double() {maxIndex}
        End Function

        Public Overloads Overrides Function predTransform(pred As Double) As Double
            Throw New NotSupportedException()
        End Function
    End Class

    ''' <summary>
    ''' Multiclass classification (predicted probability).
    ''' </summary>
    <Serializable>
    Friend Class SoftmaxMultiClassObjProb
        Inherits ObjFunction

        Public Overloads Overrides Function predTransform(preds As Double()) As Double()
            Dim max = preds(0)

            For i = 1 To preds.Length - 1
                max = stdNum.Max(preds(i), max)
            Next

            Dim sum As Double = 0

            For i = 0 To preds.Length - 1
                preds(i) = exp(preds(i) - max)
                sum += preds(i)
            Next

            For i = 0 To preds.Length - 1
                preds(i) /= CSng(sum)
            Next

            Return preds
        End Function

        Public Overloads Overrides Function predTransform(pred As Double) As Double
            Throw New NotSupportedException()
        End Function

        Friend Overridable Function exp(x As Double) As Double
            Return stdNum.Exp(x)
        End Function
    End Class
End Namespace
