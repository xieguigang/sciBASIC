#Region "Microsoft.VisualBasic::ba3d66170f3cba105d18b9a45d3ae4e3, sciBASIC#\Microsoft.VisualBasic.Core\src\Net\MIME\ContentType.vb"

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

    '   Total Lines: 74
    '    Code Lines: 38
    ' Comment Lines: 25
    '   Blank Lines: 11
    '     File Size: 2.23 KB


    '     Structure ContentType
    ' 
    '         Properties: Details, FileExt, IsEmpty, MIMEType, Name
    ' 
    '         Function: __createObject, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Text

Namespace Net.Protocols.ContentTypes

    ''' <summary>
    ''' MIME types / Internet Media Types
    ''' </summary>
    Public Structure ContentType : Implements IsEmpty

        ''' <summary>
        ''' Type name or brief info
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String

        ''' <summary>
        ''' MIME Type / Internet Media Type
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' example as:
        ''' 
        ''' plain/text
        ''' text/html
        ''' </remarks>
        Public Property MIMEType As String

        ''' <summary>
        ''' File Extension
        ''' </summary>
        ''' <returns></returns>
        Public Property FileExt As String

        ''' <summary>
        ''' More Details
        ''' </summary>
        ''' <returns></returns>
        Public Property Details As String

        Public ReadOnly Property IsEmpty() As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Name Is Nothing AndAlso
                    MIMEType Is Nothing AndAlso
                    FileExt Is Nothing AndAlso
                    Details Is Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{MIMEType} (*{FileExt})"
        End Function

        Friend Shared Function __createObject(line As String) As ContentType
            Dim tokens As String() = line.Split(ASCII.TAB)

            If tokens.IsNullOrEmpty OrElse tokens.Length < 3 Then
                Call line.Warning
                Return Nothing
            Else
                Dim mime As New ContentType With {
                    .Name = tokens(Scan0),
                    .MIMEType = tokens(1),
                    .FileExt = tokens(2),
                    .Details = tokens(3)
                }
                Return mime
            End If
        End Function
    End Structure

End Namespace
