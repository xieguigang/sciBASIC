#Region "Microsoft.VisualBasic::a0f641fb6ef0df2e32b739fa1c8cdad8, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\Exception\VisualBasicAppException.vb"

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

    '   Total Lines: 44
    '    Code Lines: 18
    ' Comment Lines: 20
    '   Blank Lines: 6
    '     File Size: 1.62 KB


    '     Class VisualBasicAppException
    ' 
    '         Properties: args, Environment, System
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Creates
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
        Public Property args As String = App.Command
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
            MyBase.New($"{ex.GetType.Name}@<{calls}>", ex)
        End Sub

        Sub New(message As String, <CallerMemberName> Optional caller As String = Nothing)
            Call MyBase.New($"{caller}: " & message)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Creates(msg As String, calls As String) As VisualBasicAppException
            Return New VisualBasicAppException(New Exception(msg), calls)
        End Function
    End Class
End Namespace
