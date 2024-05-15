#Region "Microsoft.VisualBasic::f8c67a02e8a413d7afc1b96f69ac38e9, Data_science\MachineLearning\MachineLearning\SVM\Logging.vb"

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

    '   Total Lines: 28
    '    Code Lines: 19
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 764 B


    '     Class Logging
    ' 
    '         Properties: IsVerbose
    ' 
    '         Sub: flush, info
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace SVM

    Public Class Logging

        ''' <summary>
        ''' Whether the system will output information to the console during the training process.
        ''' </summary>
        Public Shared Property IsVerbose As Boolean

        Shared svm_print_stdout As TextWriter = Console.Out

        Public Shared Sub flush()
            SyncLock svm_print_stdout
                Call svm_print_stdout.Flush()
            End SyncLock
        End Sub

        Public Shared Sub info(s As String)
            If _IsVerbose Then
                SyncLock svm_print_stdout
                    Call svm_print_stdout.Write(s)
                End SyncLock
            End If
        End Sub
    End Class
End Namespace
