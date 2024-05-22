#Region "Microsoft.VisualBasic::7c63fc693df57c0efe131ba9a05d5f8a, Data_science\MachineLearning\xgboost\learner\ObjFunction.vb"

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

    '   Total Lines: 75
    '    Code Lines: 38 (50.67%)
    ' Comment Lines: 26 (34.67%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 11 (14.67%)
    '     File Size: 3.02 KB


    '     Class ObjFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: fromName, (+2 Overloads) predTransform
    ' 
    '         Sub: register, useFastMathExp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.config

Namespace learner

    ''' <summary>
    ''' Objective function implementations.
    ''' </summary>
    <Serializable>
    Public Class ObjFunction

        Private Shared ReadOnly FUNCTIONS As New Dictionary(Of String, ObjFunction)()

        Shared Sub New()
            Call register("rank:pairwise", New ObjFunction())
            Call register("binary:logistic", New RegLossObjLogistic())
            Call register("binary:logitraw", New ObjFunction())
            Call register("multi:softmax", New SoftmaxMultiClassObjClassify())
            Call register("multi:softprob", New SoftmaxMultiClassObjProb())
            Call register("reg:linear", New ObjFunction())
            Call register("reg:squarederror", New ObjFunction())
            Call register("reg:logistic", New RegLossObjLogistic())
        End Sub

        ''' <summary>
        ''' Gets <seealso cref="ObjFunction"/> from given name.
        ''' </summary>
        ''' <param name="name"> name of objective function </param>
        ''' <returns> objective function </returns>
        Public Shared Function fromName(name As String) As ObjFunction
            Dim result = FUNCTIONS.GetValueOrNull(name)

            If result Is Nothing Then
                Throw New ArgumentException(name & " is not supported objective function.")
            End If

            Return result
        End Function

        ''' <summary>
        ''' Register an <seealso cref="ObjFunction"/> for a given name.
        ''' </summary>
        ''' <param name="name"> name of objective function </param>
        ''' <param name="objFunction"> objective function </param>
        ''' @deprecated This method will be made private. Please use <seealso cref="PredictorConfiguration.BuilderType"/> instead. 
        Public Shared Sub register(name As String, objFunction As ObjFunction)
            FUNCTIONS(name) = objFunction
        End Sub

        Public Shared Sub useFastMathExp()
            Call register("binary:logistic", New RegLossObjLogistic())
            Call register("reg:logistic", New RegLossObjLogistic())
            Call register("multi:softprob", New SoftmaxMultiClassObjProb())
        End Sub

        ''' <summary>
        ''' Transforms prediction values.
        ''' </summary>
        ''' <param name="preds"> prediction </param>
        ''' <returns> transformed values </returns>
        Public Overridable Function predTransform(preds As Double()) As Double()
            ' do nothing
            Return preds
        End Function

        ''' <summary>
        ''' Transforms a prediction value.
        ''' </summary>
        ''' <param name="pred"> prediction </param>
        ''' <returns> transformed value </returns>
        Public Overridable Function predTransform(pred As Double) As Double
            ' do nothing
            Return pred
        End Function
    End Class
End Namespace
