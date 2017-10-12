#Region "Microsoft.VisualBasic::fd93a3876779e1315d8dd53fbc24523f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Debugger\VisualBasicAppException.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' VisualBasic application exception wrapper
    ''' </summary>
    Public Class VisualBasicAppException : Inherits Exception

        ''' <summary>
        ''' The CLI arguments string
        ''' </summary>
        ''' <returns></returns>
        Public Property args As String
        ''' <summary>
        ''' The internal App environment variables
        ''' </summary>
        ''' <returns></returns>
        Public Property Environment As Dictionary(Of String, String)
        ''' <summary>
        ''' The system version information
        ''' </summary>
        ''' <returns></returns>
        Public Property System As Dictionary(Of String, String)

        ''' <summary>
        ''' <see cref="Exception"/> inner wrapper
        ''' </summary>
        ''' <param name="ex">The exception details</param>
        ''' <param name="calls">Method name where occurs this exception.</param>
        Sub New(ex As Exception, calls As String)
            MyBase.New("@" & calls, ex)
        End Sub

        Public Shared Function Creates(msg As String, calls As String) As VisualBasicAppException
            Return New VisualBasicAppException(New Exception(msg), calls)
        End Function
    End Class

    Public Module ExceptionExtensions

        ''' <summary>
        ''' Just throw exception, but the exception contains more details information for the debugging
        ''' </summary>
        ''' <param name="msg$"></param>
        ''' <returns></returns>
        Public Function Fail(msg$, <CallerMemberName> Optional caller$ = "") As VisualBasicAppException
            Return VisualBasicAppException.Creates(msg, caller)
        End Function
    End Module
End Namespace
