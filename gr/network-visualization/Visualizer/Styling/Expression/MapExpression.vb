Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Styling

    ''' <summary>
    ''' 从graph的属性值到相应的图形属性(节点大小，颜色，字体，形状)的映射操作类型
    ''' </summary>
    Public Enum MapperTypes
        ''' <summary>
        ''' 连续的数值型的映射
        ''' </summary>
        Continuous
        ''' <summary>
        ''' 离散的分类映射
        ''' </summary>
        Discrete
        ''' <summary>
        ''' 直接映射
        ''' </summary>
        Passthrough
    End Enum

    Public Structure MapExpression

        Dim propertyName As String
        Dim type As MapperTypes
        Dim values As String()

        Public ReadOnly Property AsDictionary As Dictionary(Of String, String)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return values _
                    .Select(Function(s) s.GetTagValue("=", trim:=True)) _
                    .ToDictionary(Function(t) t.Name,
                                  Function(t) t.Value)
            End Get
        End Property

        Public Overrides Function ToString() As String
            If type = MapperTypes.Continuous Then
                Return $"Dim '{propertyName}' = [{values.JoinBy(", ")}]"
            Else
                Return $"Dim '{propertyName}' = {Me.AsDictionary.GetJson}"
            End If
        End Function
    End Structure
End Namespace