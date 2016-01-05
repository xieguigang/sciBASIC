Imports System
Imports System.ComponentModel
Imports System.Runtime.Serialization

Namespace Microsoft.VisualBasic.ApplicationServices
    <Serializable, EditorBrowsable(EditorBrowsableState.Never)> _
    Public Class NoStartupFormException
        Inherits Exception
        ' Methods
        Public Sub New()
            MyBase.New(Utils.GetResourceString("AppModel_NoStartupForm"))
        End Sub

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Protected Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info, context)
        End Sub

        Public Sub New(message As String, inner As Exception)
            MyBase.New(message, inner)
        End Sub

    End Class
End Namespace

