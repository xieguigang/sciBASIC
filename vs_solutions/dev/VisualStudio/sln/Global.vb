#Region "Microsoft.VisualBasic::1095d194f47d7e7d60cda0c853481f6a, vs_solutions\dev\VisualStudio\sln\Global.vb"

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

    '   Total Lines: 6
    '    Code Lines: 4 (66.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (33.33%)
    '     File Size: 74 B


    '     Class [Global]
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace sln

    ''' <summary>
    ''' Carries the content of the solution ``GlobalSection`` pairs
    ''' (e.g. ``SolutionGuid`` and other global properties).
    ''' </summary>
    Public Class [Global]

        ''' <summary>
        ''' The solution GUID, if declared in ``GlobalSection(SolutionProperties)``.
        ''' </summary>
        Public Property SolutionGuid As String

        ''' <summary>
        ''' All raw key/value pairs found in the global sections.
        ''' </summary>
        Public Property Properties As New Dictionary(Of String, String)

        ''' <summary>
        ''' Get a global property value by key (case-insensitive).
        ''' </summary>
        Public Function GetValue(key As String) As String
            If Properties Is Nothing Then
                Return String.Empty
            End If

            For Each kv In Properties
                If String.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase) Then
                    Return kv.Value
                End If
            Next

            Return String.Empty
        End Function
    End Class
End Namespace
