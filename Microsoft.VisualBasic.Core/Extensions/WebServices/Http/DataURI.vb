#Region "Microsoft.VisualBasic::37bdd4e6b6a4686138d108592f43fae6, Microsoft.VisualBasic.Core\Extensions\WebServices\Http\DataURI.vb"

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

    '     Class DataURI
    ' 
    '         Function: FromFile, ToString
    ' 
    '         Sub: New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices

Namespace Net.Http

    ''' <summary>
    ''' Data URI scheme
    ''' </summary>
    Public Class DataURI

        ReadOnly mime$
        ReadOnly base64$
        ReadOnly chartSet$

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="codepage$">The chartset codepage name, by default is ``ASCII``.</param>
        Sub New(file As String, Optional codepage$ = Nothing)
            mime = Strings.LCase(file.FileMimeType.MIMEType)
            base64 = file.ReadBinary.ToBase64String
            codepage = codepage
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromFile(file As String) As DataURI
            Return New DataURI(file)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If chartSet.StringEmpty Then
                Return $"data:{mime};base64,{base64}"
            Else
                Return $"data:{mime};charset={chartSet};base64,{base64}"
            End If
        End Function
    End Class
End Namespace
