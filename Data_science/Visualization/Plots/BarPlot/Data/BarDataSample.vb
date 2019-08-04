Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BarPlot.Data

    ''' <summary>
    ''' Named value of double vector.
    ''' </summary>
    Public Class BarDataSample : Implements INamedValue

        ''' <summary>
        ''' 分组名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Tag As String Implements INamedValue.Key
        ''' <summary>
        ''' 当前分组下的每一个序列的数据值
        ''' </summary>
        ''' <returns></returns>
        Public Property data As Double()

        ''' <summary>
        ''' The sum of <see cref="data"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StackedSum As Double
            Get
                Return data.Sum
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace