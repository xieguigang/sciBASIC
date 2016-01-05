Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Media
Imports System.Security
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.Devices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class Audio
        ' Methods
        <SecuritySafeCritical>
        Private Shared Sub InternalStop(sound As SoundPlayer)
            Call New SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert
            Try
                sound.Stop()
            Finally
                CodeAccessPermission.RevertAssert()
            End Try
        End Sub

        Public Sub Play(location As String)
            Me.Play(location, AudioPlayMode.Background)
        End Sub

        Public Sub Play(location As String, playMode As AudioPlayMode)
            Me.ValidateAudioPlayModeEnum(playMode, "playMode")
            Dim sound As New SoundPlayer(Me.ValidateFilename(location))
            Me.Play(sound, playMode)
        End Sub

        Public Sub Play(data As Byte(), playMode As AudioPlayMode)
            If (data Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("data")
            End If
            Me.ValidateAudioPlayModeEnum(playMode, "playMode")
            Dim stream As New MemoryStream(data)
            Me.Play(stream, playMode)
            stream.Close()
        End Sub

        Public Sub Play(stream As Stream, playMode As AudioPlayMode)
            Me.ValidateAudioPlayModeEnum(playMode, "playMode")
            If (stream Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("stream")
            End If
            Me.Play(New SoundPlayer(stream), playMode)
        End Sub

        Private Sub Play(sound As SoundPlayer, mode As AudioPlayMode)
            If (Not Me.m_Sound Is Nothing) Then
                Audio.InternalStop(Me.m_Sound)
            End If
            Me.m_Sound = sound
            Select Case mode
                Case AudioPlayMode.WaitToComplete
                    Me.m_Sound.PlaySync()
                    Return
                Case AudioPlayMode.Background
                    Me.m_Sound.Play()
                    Return
                Case AudioPlayMode.BackgroundLoop
                    Me.m_Sound.PlayLooping()
                    Return
            End Select
        End Sub

        Public Sub PlaySystemSound(systemSound As SystemSound)
            If (systemSound Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("systemSound")
            End If
            systemSound.Play()
        End Sub

        Public Sub [Stop]()
            Audio.InternalStop(New SoundPlayer)
        End Sub

        Private Sub ValidateAudioPlayModeEnum(value As AudioPlayMode, paramName As String)
            If ((value < AudioPlayMode.WaitToComplete) OrElse (value > AudioPlayMode.BackgroundLoop)) Then
                Throw New InvalidEnumArgumentException(paramName, CInt(value), GetType(AudioPlayMode))
            End If
        End Sub

        Private Function ValidateFilename(location As String) As String
            If (location = "") Then
                Throw ExceptionUtils.GetArgumentNullException("location")
            End If
            Return location
        End Function


        ' Fields
        Private m_Sound As SoundPlayer
    End Class
End Namespace

