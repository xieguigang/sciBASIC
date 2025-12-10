#Region "Microsoft.VisualBasic::3ff8ce6e5807d0c7d2b7ec2bfe5670e5, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\ElementList.vb"

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

    '   Total Lines: 40
    '    Code Lines: 14 (35.00%)
    ' Comment Lines: 22 (55.00%)
    '    - Xml Docs: 68.18%
    ' 
    '   Blank Lines: 4 (10.00%)
    '     File Size: 1.35 KB


    '     Class ElementList
    ' 
    '         Properties: List
    ' 
    '         Function: AddNew, Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar
    ''' <summary>
    ''' A list of Elements of a ProgressBar
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class ElementList(Of T)
        ''' <summary>
        ''' The List of Elements of a Progressbar
        ''' </summary>
        Public ReadOnly Property List As New List(Of Element(Of T))()

        ''' <summary>
        ''' Clears the List of elements of a ProgressBar
        ''' </summary>
        ''' <returns></returns>
        Public Function Clear() As ElementList(Of T)
            List.Clear()
            Return Me
        End Function

        ''' <summary>
        ''' Adds new Element to the List
        ''' </summary>
        ''' <returns></returns>
        Public Function AddNew() As Element(Of T)
            Dim line = New Element(Of T)()
            List.Add(line)
            Return line
        End Function
    End Class

End Namespace
