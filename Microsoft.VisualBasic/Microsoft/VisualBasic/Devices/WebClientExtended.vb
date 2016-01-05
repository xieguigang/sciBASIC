Imports System
Imports System.Net

Namespace Microsoft.VisualBasic.Devices
    Friend Class WebClientExtended
        Inherits WebClient
        ' Methods
        Friend Sub New()
        End Sub

        Protected Overrides Function GetWebRequest(address As Uri) As WebRequest
            Dim webRequest As WebRequest = MyBase.GetWebRequest(address)
            If (Not webRequest Is Nothing) Then
                webRequest.Timeout = Me.m_Timeout
                If Me.m_UseNonPassiveFtp Then
                    Dim request3 As FtpWebRequest = TryCast(webRequest, FtpWebRequest)
                    If (Not request3 Is Nothing) Then
                        request3.UsePassive = False
                    End If
                End If
                Dim request2 As HttpWebRequest = TryCast(webRequest, HttpWebRequest)
                If (Not request2 Is Nothing) Then
                    request2.AllowAutoRedirect = False
                End If
            End If
            Return webRequest
        End Function


        ' Properties
        Public WriteOnly Property Timeout As Integer
            Set(value As Integer)
                Me.m_Timeout = value
            End Set
        End Property

        Public WriteOnly Property UseNonPassiveFtp As Boolean
            Set(value As Boolean)
                Me.m_UseNonPassiveFtp = value
            End Set
        End Property


        ' Fields
        Private m_Timeout As Integer = &H186A0
        Private m_UseNonPassiveFtp As Boolean
    End Class
End Namespace

