Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.Runtime.Remoting.Messaging
Imports System.Security

Namespace Microsoft.VisualBasic.MyServices.Internal
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public Class ContextValue(Of T)
        ' Methods
        Public Sub New()
            Me.m_ContextKey = Guid.NewGuid.ToString
        End Sub


        ' Properties
        Public Property Value As T
            <SecuritySafeCritical> _
            Get
                Dim current As Object = SkuSafeHttpContext.Current
                If (Not current Is Nothing) Then
                    Dim arguments As Object() = New Object() { Me.m_ContextKey }
                    Return DirectCast(NewLateBinding.LateGet(current, Nothing, "Items", arguments, Nothing, Nothing, Nothing), T)
                End If
                Return DirectCast(CallContext.GetData(Me.m_ContextKey), T)
            End Get
            <SecuritySafeCritical>
            Set(value As T)
                Dim current As Object = SkuSafeHttpContext.Current
                If (Not current Is Nothing) Then
                    Dim arguments As Object() = New Object() {Me.m_ContextKey, value}
                    NewLateBinding.LateSet(current, Nothing, "Items", arguments, Nothing, Nothing)
                Else
                    CallContext.SetData(Me.m_ContextKey, value)
                End If
            End Set
        End Property


        ' Fields
        Private ReadOnly m_ContextKey As String
    End Class
End Namespace

