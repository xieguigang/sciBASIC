#Region "Microsoft.VisualBasic::df8d10f74a715345ad368fbfeb96e567, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Text\Text.vb"

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

    '   Total Lines: 23
    '    Code Lines: 6 (26.09%)
    ' Comment Lines: 16 (69.57%)
    '    - Xml Docs: 56.25%
    ' 
    '   Blank Lines: 1 (4.35%)
    '     File Size: 992 B


    '     Class Text
    ' 
    '         Properties: Body, Description
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
    ''' Definitions for Texts in a ProgressBar
    ''' </summary>
    Partial Public Class Text
        ''' <summary>
        ''' Definition of the text in the same line as ProgressBar (Body)
        ''' </summary>
        Public ReadOnly Property Body As TextBody = New TextBody()

        ''' <summary>
        ''' Definition of the texts in the lines below a ProgressBar (Description)
        ''' </summary>
        Public ReadOnly Property Description As TextDescription = New TextDescription()
    End Class
End Namespace

