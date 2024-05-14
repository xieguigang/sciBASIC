#Region "Microsoft.VisualBasic::7da1d0311966dcd31d1574e2b64e9f98, Data_science\MachineLearning\MachineLearning\SVM\SVMModel.vb"

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

    '   Total Lines: 64
    '    Code Lines: 38
    ' Comment Lines: 12
    '   Blank Lines: 14
    '     File Size: 2.07 KB


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

        Public Property model As Model
        Public Property transform As IRangeTransform

        ''' <summary>
        ''' use for get <see cref="ColorClass"/> based on 
        ''' the prediction result value
        ''' </summary>
        ''' <returns></returns>
        Public Property factors As ClassEncoder

        Public ReadOnly Property SVR As Boolean
            Get
                Dim type = model.parameter.svmType
                Dim is_svr = type = SvmType.EPSILON_SVR OrElse type = SvmType.NU_SVR

                Return is_svr
            End Get
        End Property

        Public ReadOnly Property dimensionNames As String()
            Get
                Return model.dimensionNames
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return dimensionNames.GetJson
        End Function
    End Class

    ''' <summary>
    ''' A collection of trained svm data model that can be apply for 
    ''' multiple dimension class classify analysis.
    ''' </summary>
    Public Class SVMMultipleSet

        Public Property dimensionNames As String()
        Public Property topics As Dictionary(Of String, SVMModel)

        Public Function trainingSize() As Integer
            Dim sizeList = topics.Values.Select(Function(a) a.model.trainingSize).ToArray

            If Not sizeList.All(Function(a) a = sizeList(Scan0)) Then
                Throw New InvalidDataContractException("model is invalid!")
            End If

            Return sizeList(Scan0)
        End Function

    End Class
End Namespace
