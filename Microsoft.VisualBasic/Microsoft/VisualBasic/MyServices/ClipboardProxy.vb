Imports System
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Security.Permissions
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.MyServices
    <EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class ClipboardProxy
        ' Methods
        Friend Sub New()
        End Sub

        Public Sub Clear()
            Clipboard.Clear
        End Sub

        Public Function ContainsAudio() As Boolean
            Return Clipboard.ContainsAudio
        End Function

        Public Function ContainsData(format As String) As Boolean
            Return Clipboard.ContainsData(format)
        End Function

        Public Function ContainsFileDropList() As Boolean
            Return Clipboard.ContainsFileDropList
        End Function

        Public Function ContainsImage() As Boolean
            Return Clipboard.ContainsImage
        End Function

        Public Function ContainsText() As Boolean
            Return Clipboard.ContainsText
        End Function

        Public Function ContainsText(format As TextDataFormat) As Boolean
            Return Clipboard.ContainsText(format)
        End Function

        Public Function GetAudioStream() As Stream
            Return Clipboard.GetAudioStream
        End Function

        Public Function GetData(format As String) As Object
            Return Clipboard.GetData(format)
        End Function

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Function GetDataObject() As IDataObject
            Return Clipboard.GetDataObject
        End Function

        Public Function GetFileDropList() As StringCollection
            Return Clipboard.GetFileDropList
        End Function

        Public Function GetImage() As Image
            Return Clipboard.GetImage
        End Function

        Public Function GetText() As String
            Return Clipboard.GetText
        End Function

        Public Function GetText(format As TextDataFormat) As String
            Return Clipboard.GetText(format)
        End Function

        Public Sub SetAudio(audioBytes As Byte())
            Clipboard.SetAudio(audioBytes)
        End Sub

        Public Sub SetAudio(audioStream As Stream)
            Clipboard.SetAudio(audioStream)
        End Sub

        Public Sub SetData(format As String, data As Object)
            Clipboard.SetData(format, data)
        End Sub

        <EditorBrowsable(EditorBrowsableState.Advanced)>
        Public Sub SetDataObject(data As DataObject)
            Clipboard.SetDataObject(data)
        End Sub

        Public Sub SetFileDropList(filePaths As StringCollection)
            Clipboard.SetFileDropList(filePaths)
        End Sub

        Public Sub SetImage(image As Image)
            Clipboard.SetImage(image)
        End Sub

        Public Sub SetText([text] As String)
            Clipboard.SetText([text])
        End Sub

        Public Sub SetText([text] As String, format As TextDataFormat)
            Clipboard.SetText([text], format)
        End Sub

    End Class
End Namespace

