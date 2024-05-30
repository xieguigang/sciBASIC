#Region "Microsoft.VisualBasic::efa6f09c0496565601a66da72f3ba44d, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\UnixMan\ManFile\ManIndex.vb"

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

    '   Total Lines: 20
    '    Code Lines: 12 (60.00%)
    ' Comment Lines: 3 (15.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (25.00%)
    '     File Size: 591 B


    '     Class ManIndex
    ' 
    '         Properties: [date], category, index, keyword, title
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' the document index
    ''' </summary>
    Public Class ManIndex

        Public Property index As String
        Public Property category As Integer
        Public Property [date] As Date = Now
        Public Property keyword As String
        Public Property title As String

        Public Overrides Function ToString() As String
            Return $".TH {Strings.UCase(index)} {category} {[date].ToString("yyyy-MMM")} ""{keyword}"" ""{title}"""
        End Function

    End Class
End Namespace
