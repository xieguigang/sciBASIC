#Region "Microsoft.VisualBasic::6e8ca794754a10d8b7ee0cabf1f7c6cb, Data_science\MachineLearning\xgboost\TGBoost\Data.vb"

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

    '   Total Lines: 56
    '    Code Lines: 32 (57.14%)
    ' Comment Lines: 8 (14.29%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 16 (28.57%)
    '     File Size: 1.86 KB


    '     Class Data
    ' 
    ' 
    ' 
    '     Class TrainData
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ValidationData
    ' 
    ' 
    ' 
    '     Class TestData
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'parse the csv file, get features and label. the format: feature1,feature2,...,label
'first scan, get the feature dimension, dataset size, count of missing value for each feature
'second scan, get each feature's (value,index) and missing value indexes
'if we use ArrayList,only one scanning is needed, but it is memory consumption

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

<Assembly: InternalsVisibleTo("Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet")>
<Assembly: InternalsVisibleTo("MLkit")>
<Assembly: InternalsVisibleTo("enigma")>

Namespace train

    Public MustInherit Class Data

        'we use -Double.MAX_VALUE to represent missing value
        Public Const NA As Single = -Single.MaxValue

        ''' <summary>
        ''' <see cref="GBM.predict(Single()())"/>
        ''' </summary>
        Friend origin_feature As Single()()
        Friend feature_dim As Integer
        Friend dataset_size As Integer

    End Class

    Public Class TrainData : Inherits ValidationData

        ''' <summary>
        ''' 【已废弃】解析阶段为「按值排序」而构造的 (value, row) 对。
        '''
        ''' 该结构每 (样本,特征) 分配一个两元素 <c>Single()</c> 对象（约 32~40 字节），
        ''' 10 万 × 100 的数据集上约占用 400MB，是训练期最大的内存热点。
        ''' 自本次重构起由 <see cref="kernels.ColumnBlock"/>（SoA）取代：
        ''' <c>AttributeList</c> 构建列块后会调用 <see cref="ReleaseRawColumns"/> 释放本字段。
        ''' </summary>
        Friend feature_value_index As Single()()()
        Friend missing_index As Integer()()
        Friend missing_count As New List(Of Integer)()
        Friend cat_features_names As List(Of String)
        Friend cat_features_cols As New List(Of Integer)()

        Public Sub New(categorical_features As IEnumerable(Of String))
            cat_features_names = categorical_features.AsList
        End Sub

        ''' <summary>
        ''' 释放解析阶段构造的 <see cref="feature_value_index"/>。
        '''
        ''' 调用后仍可继续使用：<see cref="kernels.ColumnBlock"/> 会在该字段为空时
        ''' 回落到从 <see cref="Data.origin_feature"/> 直接构建。
        ''' </summary>
        Friend Sub ReleaseRawColumns()
            feature_value_index = Nothing
        End Sub

        Public Overrides Function ToString() As String
            Return cat_features_names.ToArray.GetJson
        End Function
    End Class

    Public Class ValidationData : Inherits Data

        Friend label As Double()

    End Class

    Public Class TestData : Inherits Data

    End Class
End Namespace
