#Region "Microsoft.VisualBasic::8d11faab51df609e335a1482c06810a9, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\DownloadPogressExample.vb"

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
    '    Code Lines: 37 (92.50%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (7.50%)
    '     File Size: 1.79 KB


    '     Class DownloadProgressExample
    ' 
    '         Function: StartAsync
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Linq
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class DownloadProgressExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim files = New String() {"https://github.com/Mpdreamz/shellprogressbar/archive/4.3.0.zip", "https://github.com/Mpdreamz/shellprogressbar/archive/4.2.0.zip", "https://github.com/Mpdreamz/shellprogressbar/archive/4.1.0.zip", "https://github.com/Mpdreamz/shellprogressbar/archive/4.0.0.zip"}
            Dim childOptions = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ProgressCharacter = "▓"c
}
            Dim pbar = New ProgressBar(files.Length, "downloading")
            For Each fileI In files.[Select](Function(f, i) (f, i))
                Dim file = fileI.Item1
                Dim i = fileI.Item2
                Dim data As Byte() = Nothing
                Dim child = pbar.Spawn(100, "page: " & i, childOptions)
                Try
                    Dim client = New WebClient()
                    AddHandler client.DownloadProgressChanged, Sub(o, args) child.Tick(args.ProgressPercentage)
                    AddHandler client.DownloadDataCompleted, Sub(o, args) data = args.Result
                    client.DownloadDataAsync(New Uri(file))
                    While client.IsBusy
                        Thread.Sleep(1)
                    End While

                    pbar.Tick()
                Catch [error] As WebException
                    pbar.WriteLine([error].Message)
                End Try
            Next

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
