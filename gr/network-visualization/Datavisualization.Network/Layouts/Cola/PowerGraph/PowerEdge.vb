#Region "Microsoft.VisualBasic::4b4a45c46a6d158ec2c0c0395d08c86e, gr\network-visualization\Datavisualization.Network\Layouts\Cola\PowerGraph\PowerEdge.vb"

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

    '     Class PowerEdge
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Layouts.Cola

    Public Class PowerEdge(Of T)

        Public source As T
        Public target As T
        Public type As Integer

        Default Public Property Item(name As String) As T
            Get
                If name = NameOf(source) Then
                    Return source
                ElseIf name = NameOf(target) Then
                    Return target
                Else
                    Throw New NotImplementedException(name)
                End If
            End Get
            Set
                If name = NameOf(source) Then
                    source = Value
                ElseIf name = NameOf(target) Then
                    target = Value
                Else
                    Throw New NotImplementedException(name)
                End If
            End Set
        End Property

        Sub New()
        End Sub

        Sub New(source As T, target As T, type As Integer)
            Me.source = source
            Me.target = target
            Me.type = type
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{source}, {target}]"
        End Function
    End Class
End Namespace
