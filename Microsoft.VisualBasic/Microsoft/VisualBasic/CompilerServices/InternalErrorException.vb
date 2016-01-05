Imports System
Imports System.ComponentModel
Imports System.Runtime.Serialization

Namespace Microsoft.VisualBasic.CompilerServices
    <Serializable, EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class InternalErrorException
        Inherits Exception
        ' Methods
        Public Sub New()
            MyBase.New(Utils.GetResourceString("InternalError"))
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

