#Region "Microsoft.VisualBasic::df2a5da5bf340853f086230dc9943ad9, Data_science\MachineLearning\xgboost\gbm\GradBooster.vb"

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

    '   Total Lines: 83
    '    Code Lines: 37
    ' Comment Lines: 35
    '   Blank Lines: 11
    '     File Size: 3.67 KB


    '     Interface GradBooster
    ' 
    '         Properties: numClass
    ' 
    '         Function: predict, predictLeaf, predictSingle
    ' 
    '         Sub: loadModel
    ' 
    '     Class GradBooster_Factory
    ' 
    '         Function: createGradBooster
    ' 
    '     Class GBBase
    ' 
    '         Properties: numClass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace gbm

    ''' <summary>
    ''' Interface of gradient boosting model.
    ''' </summary>
    Public Interface GradBooster
        WriteOnly Property numClass As Integer

        ''' <summary>
        ''' Loads model from stream.
        ''' </summary>
        ''' <param name="reader">       input stream </param>
        ''' <param name="with_pbuffer"> whether the incoming data contains pbuffer </param>
        ''' <exception cref="IOException"> If an I/O error occurs </exception>
        Sub loadModel(reader As ModelReader, with_pbuffer As Boolean)

        ''' <summary>
        ''' Generates predictions for given feature vector.
        ''' </summary>
        ''' <param name="feat">        feature vector </param>
        ''' <param name="ntree_limit"> limit the number of trees used in prediction </param>
        ''' <returns> prediction result </returns>
        Function predict(feat As FVec, ntree_limit As Integer) As Double()

        ''' <summary>
        ''' Generates a prediction for given feature vector.
        ''' <para>
        ''' This method only works when the model outputs single value.
        ''' </para>
        ''' </summary>
        ''' <param name="feat">        feature vector </param>
        ''' <param name="ntree_limit"> limit the number of trees used in prediction </param>
        ''' <returns> prediction result </returns>
        Function predictSingle(feat As FVec, ntree_limit As Integer) As Double

        ''' <summary>
        ''' Predicts the leaf index of each tree. This is only valid in gbtree predictor.
        ''' </summary>
        ''' <param name="feat">        feature vector </param>
        ''' <param name="ntree_limit"> limit the number of trees used in prediction </param>
        ''' <returns> predicted leaf indexes </returns>
        Function predictLeaf(feat As FVec, ntree_limit As Integer) As Integer()
    End Interface

    Public Class GradBooster_Factory
        ''' <summary>
        ''' Creates a gradient booster from given name.
        ''' </summary>
        ''' <param name="name"> name of gradient booster </param>
        ''' <returns> created gradient booster </returns>
        Public Shared Function createGradBooster(name As String) As GradBooster
            If "gbtree".Equals(name) Then
                Return New GBTree()
            ElseIf "gblinear".Equals(name) Then
                Return New GBLinear()
            ElseIf "dart".Equals(name) Then
                Return New Dart()
            End If

            Throw New ArgumentException(name & " is not supported model.")
        End Function
    End Class

    <Serializable>
    Public MustInherit Class GBBase
        Implements GradBooster

        Public MustOverride Function predictLeaf(feat As FVec, ntree_limit As Integer) As Integer() Implements GradBooster.predictLeaf
        Public MustOverride Function predictSingle(feat As FVec, ntree_limit As Integer) As Double Implements GradBooster.predictSingle
        Public MustOverride Function predict(feat As FVec, ntree_limit As Integer) As Double() Implements GradBooster.predict
        Public MustOverride Sub loadModel(reader As ModelReader, with_pbuffer As Boolean) Implements GradBooster.loadModel
        Protected Friend num_class As Integer

        Public Overridable WriteOnly Property numClass As Integer Implements GradBooster.numClass
            Set(value As Integer)
                num_class = value
            End Set
        End Property
    End Class
End Namespace
