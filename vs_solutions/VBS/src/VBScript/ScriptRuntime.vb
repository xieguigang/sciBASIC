#Region "Microsoft.VisualBasic::f3c8dacdcdfd54af284606dd1c9ba41e, vs_solutions\VBS\src\VBScript\ScriptRuntime.vb"

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

    '   Total Lines: 56
    '    Code Lines: 35 (62.50%)
    ' Comment Lines: 10 (17.86%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (19.64%)
    '     File Size: 2.11 KB


    '     Class ScriptRuntime
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CommandArgs, Run
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Script

    Public Class ScriptRuntime : Implements IDisposable

        Dim disposedValue As Boolean
        Dim ctx As ScriptLoadContext
        Dim asm As Assembly

        Sub New(ctx As ScriptLoadContext, asm As Assembly)
            Me.ctx = ctx
            Me.asm = asm
        End Sub

        Public Function Run(args As String()) As Integer
            Return DynamicDll.Run(asm, CommandArgs(args))
        End Function

        Private Function CommandArgs(args As String()) As Object
            Dim type As Type = ctx.GetType(asm, GetType(CommandLine).FullName)
            Dim ctor As MethodInfo = type.GetMethod(NameOf(CommandLine.BuildFromArguments), {GetType(String()), GetType(Boolean)})
            Dim cmdl As Object = ctor.Invoke(Nothing, {args, False})

            Return cmdl
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call ctx.Unload()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
