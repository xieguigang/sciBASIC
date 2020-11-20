#Region "Microsoft.VisualBasic::2b6440fa8dbd88f61e62f5a63663f0d0, Microsoft.VisualBasic.Core\ApplicationServices\Debugger\Exception\VisualBasicAppException.vb"

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

    '     Class VisualBasicAppException
    ' 
    '         Properties: args, Environment, System
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Creates
    ' 
    '     Module ExceptionExtensions
    ' 
    ' 
    '         Delegate Sub
    ' 
    '             Function: Fail
    ' 
    ' 
    ' 
    ' /********************************************************************************/

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

    <HideModuleName>
    Public Module ExceptionExtensions

        ''' <summary>
        ''' 处理错误的工作逻辑的抽象接口
        ''' </summary>
        ''' <param name="ex">Socket的内部错误信息</param>
        ''' <remarks></remarks>
        Public Delegate Sub ExceptionHandler(ex As Exception)

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
