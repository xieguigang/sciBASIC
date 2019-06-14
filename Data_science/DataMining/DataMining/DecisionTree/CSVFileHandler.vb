Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree

    ''' <summary>
    ''' A row in data table
    ''' </summary>
    Public Class Entity : Inherits EntityBase(Of String)

        Public Property decisions As String

        Public Overrides Function ToString() As String
            Return $"{decisions} ~ {MyBase.ToString}"
        End Function

    End Class

    Public Class DataTable

        ''' <summary>
        ''' Factor titles of <see cref="Entity.entityVector"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property headers As String()

        ''' <summary>
        ''' Training set data
        ''' </summary>
        ''' <returns></returns>
        Public Property rows As Entity()

        Default Public ReadOnly Property GetRow(index As Integer) As Entity
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return rows(index)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return headers.GetJson
        End Function

    End Class
End Namespace