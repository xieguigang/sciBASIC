#Region "Microsoft.VisualBasic::6006d9eea536d9af39cb653367fda4bb, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Language\StringHelpers.vb"

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
Imports Microsoft.VisualBasic.Terminal.STDIO__

Namespace Language

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` syntax helpers
    ''' </summary>
    Public Module FormatHelpers

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
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        <Extension>
        Public Function xFormat(args As String()) As FormatHelper
            Return New FormatHelper With {.args = args}
        End Function
    End Module

    ''' <summary>
    ''' ``<see cref="sprintf"/>`` reference
    ''' </summary>
    Public Structure FormatHelper

        Dim source As String
        Dim args As String()

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
