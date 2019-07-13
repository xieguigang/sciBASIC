#Region "Microsoft.VisualBasic::5eebe6cecbfa88f354fe3df3a5f553cc, www\Microsoft.VisualBasic.NETProtocol\Mailto\MailContents.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class MailContents
    ' 
    '         Properties: Attatchments, Body, Logo, Subject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net.Mail
Imports System.Runtime.CompilerServices

Namespace Mailto

    ''' <summary>
    ''' E-Mail content data model
    ''' </summary>
    Public Class MailContents

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Property Subject As String
        ''' <summary>
        ''' Body html
        ''' </summary>
        ''' <returns></returns>
        Public Property Body As String

        ''' <summary>
        ''' The path list of the attachments file.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Attatchments As New List(Of String)
        ''' <summary>
        ''' The file path of the logo image.
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Logo As String

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0} -- {1}", Subject, Body)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(msg As MailContents) As MailMessage
            Return msg.CreateMailMessage
        End Operator
    End Class
End Namespace
