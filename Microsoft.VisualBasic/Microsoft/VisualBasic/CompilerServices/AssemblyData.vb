Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.IO

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend NotInheritable Class AssemblyData
        ' Methods
        Friend Sub New()
            Dim list As New ArrayList(&H100)
            Dim obj2 As Object = Nothing
            Dim num As Integer = 0
            Do
                list.Add(obj2)
                num += 1
            Loop While (num <= &HFF)
            Me.m_Files = list
        End Sub

        Friend Function GetChannelObj(lChannel As Integer) As VB6File
            Dim obj2 As Object
            If (lChannel < Me.m_Files.Count) Then
                obj2 = Me.m_Files.Item(lChannel)
            Else
                obj2 = Nothing
            End If
            Return DirectCast(obj2, VB6File)
        End Function

        Friend Sub SetChannelObj(lChannel As Integer, oFile As VB6File)
            If (Me.m_Files Is Nothing) Then
                Me.m_Files = New ArrayList(&H100)
            End If
            If (oFile Is Nothing) Then
                Dim file As VB6File = DirectCast(Me.m_Files.Item(lChannel), VB6File)
                If (Not file Is Nothing) Then
                    file.CloseFile
                End If
                Me.m_Files.Item(lChannel) = Nothing
            Else
                Dim obj2 As Object = oFile
                Me.m_Files.Item(lChannel) = obj2
            End If
        End Sub


        ' Fields
        Friend m_DirAttributes As FileAttributes
        Friend m_DirFiles As FileSystemInfo()
        Friend m_DirNextFileIndex As Integer
        Public m_Files As ArrayList
    End Class
End Namespace

