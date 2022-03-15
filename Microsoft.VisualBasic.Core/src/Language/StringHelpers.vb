#Region "Microsoft.VisualBasic::1586eae08de218790350d3185ca1749c, sciBASIC#\Microsoft.VisualBasic.Core\src\Language\StringHelpers.vb"

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

    '   Total Lines: 69
    '    Code Lines: 40
    ' Comment Lines: 17
    '   Blank Lines: 12
    '     File Size: 2.20 KB


    '     Module FormatHelpers
    ' 
    '         Function: StringFormat, Trim, xFormat
    ' 
    '     Structure FormatHelper
    ' 
    '         Function: ToString
    '         Operators: (+2 Overloads) <=, (+2 Overloads) >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.C

Namespace Language

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` syntax helpers
    ''' </summary>
    Public Module FormatHelpers

        <Extension>
        Public Function Trim(str As Value(Of String), c As Char()) As String
            If str Is Nothing OrElse str.Value Is Nothing Then
                Return ""
            Else
                Return str.Value.Trim(c)
            End If
        End Function

        ''' <summary>
        ''' ``<see cref="sprintf"/>`` extensions
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function xFormat(s As String) As FormatHelper
            Return New FormatHelper With {.source = s}
        End Function

        ''' <summary>
        ''' Synax like ``"formats" &lt;= {args}.xFormat`` 
        ''' Format by <see cref="sprintf"/>
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StringFormat(args As String()) As FormatHelper
            Return New FormatHelper With {.args = args}
        End Function
    End Module

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` reference
    ''' </summary>
    Public Structure FormatHelper

        Dim source$, args$()

        Public Overrides Function ToString() As String
            Return source
        End Function

        Public Shared Operator <=(pattern As String, format As FormatHelper) As String
            Return sprintf(pattern, format.args)
        End Operator

        Public Shared Operator >=(pattern As String, format As FormatHelper) As String
            Throw New NotSupportedException
        End Operator

        Public Shared Operator <=(format As FormatHelper, args As String()) As String
            Return sprintf(format.source, args)
        End Operator

        Public Shared Operator >=(format As FormatHelper, args As String()) As String
            Throw New NotSupportedException
        End Operator
    End Structure
End Namespace
