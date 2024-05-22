#Region "Microsoft.VisualBasic::a50c8dafaaa55942ae280072f56ddd73, Data_science\DataMining\DataMining\DecisionTree\DataModels\DataModels.vb"

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

    '   Total Lines: 54
    '    Code Lines: 35 (64.81%)
    ' Comment Lines: 10 (18.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (16.67%)
    '     File Size: 1.80 KB


    '     Class Entity
    ' 
    '         Properties: decisions
    ' 
    '         Function: Clone, ToString
    ' 
    '     Class ClassifyResult
    ' 
    '         Properties: explains, result
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree.Data

    ''' <summary>
    ''' A row in data table.(分类用的对象实例)
    ''' </summary>
    ''' <remarks>
    ''' 属性向量<see cref="Entity.entityVector"/>的最后一个值总是用来表示<see cref="Entity.decisions"/>结果值
    ''' </remarks>
    Public Class Entity : Inherits EntityBase(Of String)
        Implements ICloneable

        ''' <summary>
        ''' 分类结果
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property decisions As String
            Get
                Return entityVector(Length - 1)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{decisions} ~ {entityVector.Take(Length - 1).GetJson}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Entity With {
                .entityVector = entityVector.ToArray
            }
        End Function
    End Class

    Public Class ClassifyResult

        Public Property result As String
        Public Property explains As New List(Of String)

        Public Overrides Function ToString() As String
            Dim reason As String = explains _
                .Take(explains.Count - 1) _
                .Split(2) _
                .Select(Function(exp) $"({exp(Scan0)} Is '{exp(1)}')") _
                .JoinBy(" And ")

            Return $"{result} As [ {reason} ]"
        End Function
    End Class
End Namespace
