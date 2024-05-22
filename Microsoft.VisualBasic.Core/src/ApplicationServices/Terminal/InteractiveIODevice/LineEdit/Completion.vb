#Region "Microsoft.VisualBasic::bd2ed9dfb67fd60e6f1d01fe9e98f64a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\InteractiveIODevice\LineEdit\Completion.vb"

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
    '    Code Lines: 10 (29.41%)
    ' Comment Lines: 21 (61.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (8.82%)
    '     File Size: 1.35 KB


    '     Class Completion
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.LineEdit

    ''' <summary>
    ''' Completion results returned by the completion handler.
    ''' </summary>
    ''' <remarks>
    ''' You create an instance of this class to return the completion
    ''' results for the text at the specific position.   The prefix parameter
    ''' indicates the common prefix in the results, and the results contain the
    ''' results without the prefix.   For example, when completing "ToString" and "ToDate"
    ''' prefix would be "To" and the completions would be "String" and "Date".
    ''' </remarks>
    Public Class Completion
        ''' <summary>
        ''' Array of results, with the stem removed.
        ''' </summary>
        Public Result As String()

        ''' <summary>
        ''' Shared prefix for the completion results.
        ''' </summary>
        Public Prefix As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Completion"/> class.
        ''' </summary>
        ''' <param name="prefix">Common prefix for all results, an be null.</param>
        ''' <param name="result">Array of possible completions.</param>
        Public Sub New(prefix As String, result As String())
            Me.Prefix = prefix
            Me.Result = result
        End Sub
    End Class
End Namespace
