#Region "Microsoft.VisualBasic::96f329062aadf57cb020880ee5682796, Data_science\MachineLearning\MachineLearning\SVM\SVMModel.vb"

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

    '   Total Lines: 110
    '    Code Lines: 38 (34.55%)
    ' Comment Lines: 57 (51.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (13.64%)
    '     File Size: 4.38 KB


    '     Class SVMModel
    ' 
    '         Properties: dimensionNames, factors, model, SVR, transform
    ' 
    '         Function: ToString
    ' 
    '     Class SVMMultipleSet
    ' 
    '         Properties: dimensionNames, topics
    ' 
    '         Function: trainingSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    ''' <summary>
    ''' A trained svm data model that can be apply for classify analysis.
    ''' </summary>
    <KnownType(GetType(RangeTransform))>
    <KnownType(GetType(GaussianTransform))>
    Public Class SVMModel : Inherits MachineLearning.Model

        ''' <summary>
        ''' The trained LibSVM model object which contains the support vectors 
        ''' and the decision coefficients.
        ''' </summary>
        ''' <returns>A <see cref="SVM.Model"/> object.</returns>
        Public Property model As Model
        ''' <summary>
        ''' The range transform which is used for normalizing the input data 
        ''' before the prediction.
        ''' </summary>
        ''' <returns>An <see cref="IRangeTransform"/> object.</returns>
        Public Property transform As IRangeTransform

        ''' <summary>
        ''' use for get <see cref="ColorClass"/> based on 
        ''' the prediction result value
        ''' </summary>
        ''' <returns>A <see cref="ClassEncoder"/> object which maps the prediction result to the class label.</returns>
        Public Property factors As ClassEncoder

        ''' <summary>
        ''' Does this model is a support vector regression (SVR) model?
        ''' The SVR model is selected when the <see cref="Parameter.svmType"/> 
        ''' is either <see cref="SvmType.EPSILON_SVR"/> or <see cref="SvmType.NU_SVR"/>.
        ''' </summary>
        ''' <returns>
        ''' ``True`` when this model is a regression model, otherwise ``False`` 
        ''' for a classification model.
        ''' </returns>
        Public ReadOnly Property SVR As Boolean
            Get
                Dim type = model.parameter.svmType
                Dim is_svr = type = SvmType.EPSILON_SVR OrElse type = SvmType.NU_SVR

                Return is_svr
            End Get
        End Property

        ''' <summary>
        ''' The names of the feature dimensions of this model.
        ''' </summary>
        ''' <returns>An array of the dimension names.</returns>
        Public ReadOnly Property dimensionNames As String()
            Get
                Return model.dimensionNames
            End Get
        End Property

        ''' <summary>
        ''' Display the dimension names of this model as a json string.
        ''' </summary>
        ''' <returns>A json text which contains the dimension names.</returns>
        Public Overrides Function ToString() As String
            Return dimensionNames.GetJson
        End Function
    End Class

    ''' <summary>
    ''' A collection of trained svm data model that can be apply for 
    ''' multiple dimension class classify analysis.
    ''' </summary>
    Public Class SVMMultipleSet

        ''' <summary>
        ''' The names of the feature dimensions of this multiple model set.
        ''' </summary>
        ''' <returns>An array of the dimension names.</returns>
        Public Property dimensionNames As String()

        ''' <summary>
        ''' The trained support vector machine models, the dictionary key is the 
        ''' name of the target class.
        ''' </summary>
        ''' <returns>A dictionary which maps the class name to its <see cref="SVMModel"/>.</returns>
        Public Property topics As Dictionary(Of String, SVMModel)

        ''' <summary>
        ''' Gets the number of the training samples which was used by the models 
        ''' in this multiple model set.
        ''' </summary>
        ''' <returns>The number of the training samples.</returns>
        ''' <exception cref="InvalidDataContractException">
        ''' Thrown when the models in this set were not trained with the same 
        ''' number of the training samples.
        ''' </exception>
        Public Function trainingSize() As Integer
            Dim sizeList = topics.Values.Select(Function(a) a.model.trainingSize).ToArray

            If Not sizeList.All(Function(a) a = sizeList(Scan0)) Then
                Throw New InvalidDataContractException("model is invalid!")
            End If

            Return sizeList(Scan0)
        End Function

    End Class
End Namespace
