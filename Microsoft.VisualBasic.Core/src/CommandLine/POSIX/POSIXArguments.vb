#Region "Microsoft.VisualBasic::f2fdd8611e3bc881462155feaba57b58, Microsoft.VisualBasic.Core\src\CommandLine\POSIX\POSIXArguments.vb"

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

    '   Total Lines: 19
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 593 B


    '     Class POSIXArguments
    ' 
    '         Sub: PrintHelp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace CommandLine.POSIX

    Public Class POSIXArguments

        Public Shared Sub PrintHelp(mail$, home$, help$)
            Call New StringBuilder() _
                .AppendLine($"Report bugs to: {mail}") _
                .AppendLine($"pkg home page: <{home}>") _
                .AppendLine($"General help using GNU software: <{help}>") _
                .DoCall(Sub(str)
                            Console.WriteLine(str.ToString)
                        End Sub)
        End Sub

    End Class
End Namespace
