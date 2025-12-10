#Region "Microsoft.VisualBasic::e7af76e93f1b302ff39bb9c11ca2bef1, Microsoft.VisualBasic.Core\test\test\progrsssBarTest.vb"

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

    '   Total Lines: 21
    '    Code Lines: 15 (71.43%)
    ' Comment Lines: 2 (9.52%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (19.05%)
    '     File Size: 594 B


    ' Module progrsssBarTest
    ' 
    '     Sub: testLoop
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar

Module progrsssBarTest

    Sub testLoop()
        Const max = 500

        'Create the ProgressBar
        ' Maximum: The Max value in ProgressBar (Default is 100)
        Using pb = New ProgressBar() With {
            .Maximum = Nothing
        }
            For i = 0 To max - 1
                Call Task.Delay(10).Wait() 'Do something
                pb.PerformStep() 'Step in ProgressBar (Default is 1)
            Next
        End Using

        Pause()
    End Sub
End Module
