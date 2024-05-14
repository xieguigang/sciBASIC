#Region "Microsoft.VisualBasic::166a1fd791f6aa3ed5c75ae59b35b7bf, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\IntegrationWithIProgressExample.vb"

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
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 1.19 KB


    '     Class IntegrationWithIProgressExample
    ' 
    '         Function: Start
    ' 
    '         Sub: DoWork, ProcessFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IntegrationWithIProgressExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim files = Enumerable.Range(1, 10).[Select](Function(e) New FileInfo($"Data{e:D2}.csv")).ToList()
            Using pbar = New ProgressBar(files.Count, "A console progress that integrates with IProgress<T>")
                ProcessFiles(files, pbar.AsProgress(Of FileInfo)(Function(e) $"Processed {e.Name}"))
            End Using
            Return Task.FromResult(1)
        End Function

        Public Shared Sub ProcessFiles(files As IEnumerable(Of FileInfo), progress As IProgress(Of FileInfo))
            For Each file In files
                DoWork(file)
                progress?.Report(file)
            Next
        End Sub

        Private Shared Sub DoWork(file As FileInfo)
            Thread.Sleep(200)
        End Sub
    End Class
End Namespace
