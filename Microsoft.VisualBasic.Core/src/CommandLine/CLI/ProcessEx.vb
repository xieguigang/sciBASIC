#Region "Microsoft.VisualBasic::93974714e2265da5681b38c933f18e82, Microsoft.VisualBasic.Core\src\CommandLine\CLI\ProcessEx.vb"

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

    '   Total Lines: 53
    '    Code Lines: 42 (79.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (20.75%)
    '     File Size: 1.84 KB


    '     Structure ProcessEx
    ' 
    '         Properties: Bin, CLIArguments, StandardOutput
    ' 
    '         Function: Run, Start, ToString
    ' 
    '         Sub: wait
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CommandLine

    Public Structure ProcessEx : Implements IIORedirectAbstract

        Public Property Bin As String Implements IIORedirectAbstract.Bin
        Public Property CLIArguments As String Implements IIORedirectAbstract.CLIArguments

        Public ReadOnly Property StandardOutput As String Implements IIORedirectAbstract.StandardOutput
            Get
                Throw New NotSupportedException
            End Get
        End Property

        Public Event ProcessExit(exitCode As Integer, exitTime As String) Implements IIORedirectAbstract.ProcessExit

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function Run() As Integer Implements IIORedirectAbstract.Run
            Return Start(True)
        End Function

        Public Function Start(Optional waitForExit As Boolean = False) As Integer Implements IIORedirectAbstract.Start
            Dim proc As New Process

            Try
                proc.StartInfo = New ProcessStartInfo(Bin, CLIArguments)
                proc.Start()
            Catch ex As Exception
                ex = New Exception(Me.GetJson, ex)
                Throw ex
            End Try

            If waitForExit Then
                Call wait(proc)
                Return proc.ExitCode
            Else
                Dim h As Action(Of Process) = AddressOf wait
                Call New Thread(Sub() Call h(proc)).Start()
                Return 0
            End If
        End Function

        Private Sub wait(proc As Process)
            Call proc.WaitForExit()
            RaiseEvent ProcessExit(proc.ExitCode, Now.ToString)
        End Sub
    End Structure
End Namespace
