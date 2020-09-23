#Region "Microsoft.VisualBasic::4bb8b3315b46ca29365da78baf34c58e, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\Models\SvmModelJSON.vb"

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

    '     Class SvmModelJSON
    ' 
    '         Properties: factors, gaussianTransform, model, rangeTransform
    ' 
    '         Function: CreateJSONModel, CreateSVMModel, ToString
    ' 
    '     Class SVMMultipleSetJSON
    ' 
    '         Properties: dimensionNames, topics
    ' 
    '         Function: CreateJSONModel, CreateSVMModel, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM.StorageProcedure

    Public Class SvmModelJSON

        Public Property model As Model
        Public Property rangeTransform As RangeTransformModel
        Public Property gaussianTransform As GaussianTransformModel
        Public Property factors As ColorClass()

        Public Shared Function CreateJSONModel(svm As SVMModel) As SvmModelJSON
            Return New SvmModelJSON With {
                .model = svm.model,
                .factors = svm.factors.Colors,
                .gaussianTransform = If(TypeOf svm.transform Is GaussianTransform, New GaussianTransformModel(svm.transform), Nothing),
                .rangeTransform = If(TypeOf svm.transform Is RangeTransform, New RangeTransformModel(svm.transform), Nothing)
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function CreateSVMModel() As SVMModel
            Return New SVMModel With {
                .factors = New ClassEncoder(factors),
                .model = model,
                .transform = If(rangeTransform Is Nothing, gaussianTransform.GetTransform, rangeTransform.GetTransform)
            }
        End Function

    End Class

    Public Class SVMMultipleSetJSON

        Public Property dimensionNames As String()
        Public Property topics As Dictionary(Of String, SvmModelJSON)

        Public Shared Function CreateJSONModel(svm As SVMMultipleSet) As SVMMultipleSetJSON
            Return New SVMMultipleSetJSON With {
                .dimensionNames = svm.dimensionNames,
                .topics = svm.topics _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return SvmModelJSON.CreateJSONModel(a.Value)
                                  End Function)
            }
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function CreateSVMModel() As SVMMultipleSet
            Return New SVMMultipleSet With {
                .dimensionNames = dimensionNames,
                .topics = topics _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return a.Value.CreateSVMModel()
                                  End Function)
            }
        End Function

    End Class
End Namespace
