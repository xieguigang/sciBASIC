#Region "Microsoft.VisualBasic::88f602ab45af69e54cca0f6b165f5eea, Microsoft.VisualBasic.Core\src\Serialization\JSON\JsonFormatter.vb"

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

    '   Total Lines: 34
    '    Code Lines: 14 (41.18%)
    ' Comment Lines: 15 (44.12%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (14.71%)
    '     File Size: 1.43 KB


    '     Module JsonFormatter
    ' 
    '         Function: Format, Minify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON.Formatter.Internals
Imports r = System.Text.RegularExpressions.Regex

Namespace Serialization.JSON.Formatter

    ''' <summary>
    ''' Provides JSON formatting functionality.
    ''' </summary>
    Public Module JsonFormatter

        ''' <summary>
        ''' Returns a 'pretty printed' version of the specified JSON string, formatted for human
        ''' consumption.
        ''' </summary>
        ''' <param name="json">A valid JSON string.</param>
        ''' <returns>A 'pretty printed' version of the specified JSON string.</returns>
        Public Function Format(json As String) As String
            Dim context As New JsonFormatterStrategyContext()
            Dim formatter As New JsonFormatterInternal(context)

            Return formatter.Format(json Or die("json should not be null."))
        End Function

        ''' <summary>
        ''' Returns a 'minified' version of the specified JSON string, stripped of all 
        ''' non-essential characters.
        ''' </summary>
        ''' <param name="json">A valid JSON string.</param>
        ''' <returns>A 'minified' version of the specified JSON string.</returns>
        Public Function Minify(json As String) As String
            Return r.Replace(json Or die("json should not be null."), "(""(?:[^""\\]|\\.)*"")|\s+", "$1")
        End Function
    End Module
End Namespace
