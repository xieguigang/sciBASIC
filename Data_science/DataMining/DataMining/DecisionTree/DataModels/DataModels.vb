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