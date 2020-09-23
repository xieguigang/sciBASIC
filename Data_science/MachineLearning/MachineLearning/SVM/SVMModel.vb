#Region "Microsoft.VisualBasic::9833d1f53aa632aa1036bb3cb2096257, Data_science\MachineLearning\MachineLearning\SVM\SVMModel.vb"

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
    '         Properties: DimensionNames, factors, model, transform
    ' 
    '         Function: ToString
    ' 
    '     Class SVMMultipleSet
    ' 
    '         Properties: dimensionNames, topics
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    Public Class SVMModel

        Public Property model As Model
        Public Property transform As IRangeTransform

        ''' <summary>
        ''' use for get <see cref="ColorClass"/> based on 
        ''' the prediction result value
        ''' </summary>
        ''' <returns></returns>
        Public Property factors As ClassEncoder

        Public ReadOnly Property DimensionNames As String()
            Get
                Return model.DimensionNames
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DimensionNames.GetJson
        End Function
    End Class

    Public Class SVMMultipleSet

        Public Property dimensionNames As String()
        Public Property topics As Dictionary(Of String, SVMModel)

    End Class
End Namespace
