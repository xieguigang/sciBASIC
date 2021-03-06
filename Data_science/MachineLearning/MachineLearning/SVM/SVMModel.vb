﻿#Region "Microsoft.VisualBasic::fc50a5f635477e0da86ab9216e7ae374, Data_science\MachineLearning\MachineLearning\SVM\SVMModel.vb"

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

    '     Class SVMModel
    ' 
    '         Properties: dimensionNames, factors, model, transform
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
    Public Class SVMModel

        Public Property model As Model
        Public Property transform As IRangeTransform

        ''' <summary>
        ''' use for get <see cref="ColorClass"/> based on 
        ''' the prediction result value
        ''' </summary>
        ''' <returns></returns>
        Public Property factors As ClassEncoder

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
