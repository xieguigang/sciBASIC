Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Math.Scripting

Public Class Config

    <DataFrameColumn> Public Property learnRate As Double = 0.1
    <DataFrameColumn> Public Property momentum As Double = 0.9
    <DataFrameColumn> Public Property iterations As Integer = 10000

    ''' <summary>
    ''' ``func(args)``, using parser <see cref="FuncParser.TryParse(String)"/>
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property default_active As String = "Sigmoid()"

    <DataFrameColumn> Public Property input_active As String
    <DataFrameColumn> Public Property hiddens_active As String
    <DataFrameColumn> Public Property output_active As String

    ''' <summary>
    ''' ``a,b,c``使用逗号分隔的隐藏层每一层网络的节点数量的列表
    ''' </summary>
    ''' <returns></returns>
    <DataFrameColumn> Public Property hidden_size As String

    Public Shared ReadOnly Property [Default] As DefaultValue(Of Config) = New Config

End Class
