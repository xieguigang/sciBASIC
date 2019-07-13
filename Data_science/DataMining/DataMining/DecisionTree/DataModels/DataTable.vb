#Region "Microsoft.VisualBasic::3cdd342711bb5fc508f154f35733db69, Data_science\DataMining\DataMining\DecisionTree\DataModels\DataTable.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class DataTable
    ' 
    '         Properties: columns, decisions, headers, rows
    ' 
    '         Function: ToString
    ' 
    '         Sub: RemoveColumn
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree.Data

    ''' <summary>
    ''' 训练集样本数据表
    ''' </summary>
    Public Class DataTable

        ''' <summary>
        ''' Factor titles of <see cref="Entity.entityVector"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property headers As String()

        ''' <summary>
        ''' 分类结果的显示标题
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property decisions As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return headers.Last
            End Get
        End Property

        ''' <summary>
        ''' Training set data
        ''' </summary>
        ''' <returns></returns>
        Public Property rows As Entity()

        ''' <summary>
        ''' Get the property fields count
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property columns As Integer
            Get
                Return headers.Length
            End Get
        End Property

        Default Public ReadOnly Property GetRow(index As Integer) As Entity
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return rows(index)
            End Get
        End Property

        Public Sub RemoveColumn(index As Integer)
            headers = headers.Delete(index)
            rows.DoEach(Sub(r) r.entityVector = r.entityVector.Delete(index))
        End Sub

        Public Overrides Function ToString() As String
            Return headers.GetJson
        End Function
    End Class
End Namespace
