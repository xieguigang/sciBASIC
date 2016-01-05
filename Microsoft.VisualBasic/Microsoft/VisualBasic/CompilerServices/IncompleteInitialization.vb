Imports System
Imports System.ComponentModel
Imports System.Runtime.Serialization

Namespace Microsoft.VisualBasic.CompilerServices
    <Serializable, EditorBrowsable(EditorBrowsableState.Never), DynamicallyInvokableAttribute> _
    Public NotInheritable Class IncompleteInitialization
        Inherits Exception
        ' Methods
        <DynamicallyInvokableAttribute> _
        Public Sub New()
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Private Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info, context)
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Sub New(message As String, innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

    End Class
End Namespace

