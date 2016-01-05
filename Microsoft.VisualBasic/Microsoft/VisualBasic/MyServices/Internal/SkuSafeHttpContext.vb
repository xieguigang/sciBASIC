Imports System
Imports System.ComponentModel
Imports System.Reflection

Namespace Microsoft.VisualBasic.MyServices.Internal
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend Class SkuSafeHttpContext
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function InitContext() As PropertyInfo
            Dim type As Type = Type.GetType("System.Web.HttpContext,System.Web,Version=4.0.0.0,Culture=neutral,PublicKeyToken=B03F5F7F11D50A3A")
            If (Not type Is Nothing) Then
                Return type.GetProperty("Current")
            End If
            Return Nothing
        End Function


        ' Properties
        Public Shared ReadOnly Property Current As Object
            Get
                If (Not SkuSafeHttpContext.m_HttpContextCurrent Is Nothing) Then
                    Return SkuSafeHttpContext.m_HttpContextCurrent.GetValue(Nothing, Nothing)
                End If
                Return Nothing
            End Get
        End Property


        ' Fields
        Private Shared m_HttpContextCurrent As PropertyInfo = SkuSafeHttpContext.InitContext
    End Class
End Namespace

