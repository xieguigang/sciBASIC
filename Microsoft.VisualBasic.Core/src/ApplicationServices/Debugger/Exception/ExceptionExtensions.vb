#Region "Microsoft.VisualBasic::575a21c47e9c5c1fe0f6fa46933415e6, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\ExceptionExtensions.vb"

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

    '   Total Lines: 24
    '    Code Lines: 10
    ' Comment Lines: 10
    '   Blank Lines: 4
    '     File Size: 844 B


    '     Delegate Sub
    ' 
    ' 
    '     Module ExceptionExtensions
    ' 
    '         Function: Fail
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 处理错误的工作逻辑的抽象接口
    ''' </summary>
    ''' <param name="ex">Socket的内部错误信息</param>
    ''' <remarks></remarks>
    Public Delegate Sub ExceptionHandler(ex As Exception)

    <HideModuleName>
    Public Module ExceptionExtensions

        ''' <summary>
        ''' Just throw exception, but the exception contains more details information for the debugging
        ''' </summary>
        ''' <param name="msg$"></param>
        ''' <returns></returns>
        Public Function Fail(msg$, <CallerMemberName> Optional caller$ = Nothing) As VisualBasicAppException
            Return VisualBasicAppException.Creates(msg, caller)
        End Function
    End Module
End Namespace
