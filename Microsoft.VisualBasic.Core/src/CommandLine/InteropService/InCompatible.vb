#Region "Microsoft.VisualBasic::d3381a220021d85959f81ad7dab17afc, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\InCompatible.vb"

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

    '   Total Lines: 30
    '    Code Lines: 17
    ' Comment Lines: 8
    '   Blank Lines: 5
    '     File Size: 1017 B


    '     Class InCompatibleAttribute
    ' 
    '         Function: CLRProcessCompatible, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection

Namespace CommandLine.InteropService

    ''' <summary>
    ''' 这个CLI方法是和.NET的<see cref="System.Diagnostics.Process"/>调用不兼容的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class InCompatibleAttribute : Inherits Attribute

        ''' <summary>
        ''' 判断目标方法是否是和CLR调用兼容？
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        Public Shared Function CLRProcessCompatible(CLI As MethodInfo) As Boolean
            Dim attrs = CLI.GetCustomAttribute(Of InCompatibleAttribute)

            If attrs Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Function ToString() As String
            Return "Incompatible with CLR Process Calls"
        End Function
    End Class
End Namespace
