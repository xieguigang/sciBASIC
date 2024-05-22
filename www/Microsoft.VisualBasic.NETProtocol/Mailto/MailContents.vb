#Region "Microsoft.VisualBasic::5eebe6cecbfa88f354fe3df3a5f553cc, www\Microsoft.VisualBasic.NETProtocol\Mailto\MailContents.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 45
    '    Code Lines: 19 (42.22%)
    ' Comment Lines: 19 (42.22%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 7 (15.56%)
    '     File Size: 1.25 KB


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
