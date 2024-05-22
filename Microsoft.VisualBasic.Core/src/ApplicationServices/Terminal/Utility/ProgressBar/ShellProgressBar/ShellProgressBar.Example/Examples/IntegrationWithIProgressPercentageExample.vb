#Region "Microsoft.VisualBasic::2437900f8bfa1b7e8a09bd25c464f171, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\IntegrationWithIProgressPercentageExample.vb"

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

    '   Total Lines: 30
    '    Code Lines: 27 (90.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (10.00%)
    '     File Size: 1.14 KB


    '     Class IntegrationWithIProgressPercentageExample
    ' 
    '         Function: Start
    ' 
    '         Sub: DoWork, ProcessFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IntegrationWithIProgressPercentageExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Using pbar = New ProgressBar(100, "A console progress that integrates with IProgress<float>")
                Call ProcessFiles(pbar.AsProgress(Of Single)())
            End Using
            Return Task.FromResult(1)
        End Function

        Public Shared Sub ProcessFiles(progress As IProgress(Of Single))
            Dim files = Enumerable.Range(1, 10).[Select](Function(e) New FileInfo($"Data{e:D2}.csv")).ToList()
            Dim i = 0
            For Each file In files
                DoWork(file)
                progress?.Report(Interlocked.Increment(i) / CSng(files.Count))
            Next
        End Sub

        Private Shared Sub DoWork(file As FileInfo)
            Thread.Sleep(200)
        End Sub
    End Class
End Namespace
