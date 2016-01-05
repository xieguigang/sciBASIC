Imports System.CodeDom.Compiler
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.MyServices.Internal

Namespace My
    <StandardModule, HideModuleName, GeneratedCode("MyTemplate", "11.0.0.0")> _
    Friend NotInheritable Class MyProject
        ' Properties
        <HelpKeyword("My.Application")> _
        Friend Shared ReadOnly Property Application As MyApplication
            <DebuggerHidden> _
            Get
                Return MyProject.m_AppObjectProvider.GetInstance
            End Get
        End Property

        <HelpKeyword("My.Computer")> _
        Friend Shared ReadOnly Property Computer As MyComputer
            <DebuggerHidden> _
            Get
                Return MyProject.m_ComputerObjectProvider.GetInstance
            End Get
        End Property

        <HelpKeyword("My.User")> _
        Friend Shared ReadOnly Property User As User
            <DebuggerHidden> _
            Get
                Return MyProject.m_UserObjectProvider.GetInstance
            End Get
        End Property

        <HelpKeyword("My.WebServices")> _
        Friend Shared ReadOnly Property WebServices As MyWebServices
            <DebuggerHidden> _
            Get
                Return MyProject.m_MyWebServicesObjectProvider.GetInstance
            End Get
        End Property


        ' Fields
        Private Shared ReadOnly m_AppObjectProvider As ThreadSafeObjectProvider(Of MyApplication) = New ThreadSafeObjectProvider(Of MyApplication)
        Private Shared ReadOnly m_ComputerObjectProvider As ThreadSafeObjectProvider(Of MyComputer) = New ThreadSafeObjectProvider(Of MyComputer)
        Private Shared ReadOnly m_MyWebServicesObjectProvider As ThreadSafeObjectProvider(Of MyWebServices) = New ThreadSafeObjectProvider(Of MyWebServices)
        Private Shared ReadOnly m_UserObjectProvider As ThreadSafeObjectProvider(Of User) = New ThreadSafeObjectProvider(Of User)

        ' Nested Types
        <EditorBrowsable(EditorBrowsableState.Never), MyGroupCollection("System.Web.Services.Protocols.SoapHttpClientProtocol", "Create__Instance__", "Dispose__Instance__", "")> _
        Friend NotInheritable Class MyWebServices
            ' Methods
            <DebuggerHidden>
            Private Shared Function Create__Instance__(Of T As New)(instance As T) As T
                If (instance Is Nothing) Then
                    Return Activator.CreateInstance(Of T)
                End If
                Return instance
            End Function

            <DebuggerHidden>
            Private Sub Dispose__Instance__(Of T)(ByRef instance As T)
                instance = CType(Nothing, T)
            End Sub

            <EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden>
            Public Overrides Function Equals(o As Object) As Boolean
                Return MyBase.Equals(o)
            End Function

            <EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden> _
            Public Overrides Function GetHashCode() As Integer
                Return MyBase.GetHashCode
            End Function

            <EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden>
            Friend Overloads Function [GetType]() As Type
                Return GetType(MyWebServices)
            End Function

            <EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden> _
            Public Overrides Function ToString() As String
                Return MyBase.ToString
            End Function

        End Class

        <EditorBrowsable(EditorBrowsableState.Never), ComVisible(False)> _
        Friend NotInheritable Class ThreadSafeObjectProvider(Of T As New)
            ' Methods
            <DebuggerHidden, EditorBrowsable(EditorBrowsableState.Never)> _
            Public Sub New()
                Me.m_Context = New ContextValue(Of T)
            End Sub


            ' Properties
            Friend ReadOnly Property GetInstance As T
                <DebuggerHidden> _
                Get
                    Dim local As T = Me.m_Context.Value
                    If (local Is Nothing) Then
                        local = Activator.CreateInstance(Of T)
                        Me.m_Context.Value = local
                    End If
                    Return local
                End Get
            End Property


            ' Fields
            Private ReadOnly m_Context As ContextValue(Of T)
        End Class
    End Class
End Namespace

