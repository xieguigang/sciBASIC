#Region "Microsoft.VisualBasic::6e1ef9f1ff40cb8a2fc2b5a719c3c250, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\Program.vb"

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

    '   Total Lines: 44
    '    Code Lines: 21
    ' Comment Lines: 17
    '   Blank Lines: 6
    '     File Size: 1.45 KB


    '     Module Program
    ' 
    '         Function: ProgressText
    ' 
    '         Sub: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace ApplicationServices.Terminal.ProgressBar

    Public Module Program

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bar"></param>
        ''' <param name="wait">Sleep time of the thread</param>
        ''' <param name="[end]">Ends at this iteration</param>
        <Extension>
        Public Sub Run(bar As AbstractBar, wait%, end%)
            For cont As Integer = 0 To [end] - 1
                Call bar.[Step]()
                Call Thread.Sleep(wait)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p">[0,1] represents the progress percentage</param>
        ''' <returns>
        ''' a progress bar liked string value, example as:
        ''' 
        ''' ```
        ''' ####.......
        ''' ```
        ''' </returns>
        Public Function ProgressText(p As Double,
                                     Optional width As Integer = 16,
                                     Optional fill As Char = "#"c,
                                     Optional empty As Char = ".") As String

            Dim fills As String = New String(fill, CInt(p * width))
            Dim emptys As String = New String(empty, CInt((1 - p) * width))

            Return fills & emptys
        End Function
    End Module
End Namespace
