#Region "Microsoft.VisualBasic::6b6befe93c30706ceda3c73fc642e4be, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ConsoleDevices\Utility\EventProc.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Terminal.Utility

    Public Class EventProc

        Dim current As Integer
        Public Property Capacity As Integer
            Get
                Return _Capacity
            End Get
            Set(value As Integer)
                _Capacity = value
                delta = Capacity / 100
                current = CInt(_Capacity * p)
            End Set
        End Property

        Dim _Capacity As Integer
        Dim delta As Integer
        Dim TAG As String

        Sub New(n As Integer, <CallerMemberName> Optional TAG As String = "")
            Me.Capacity = n
            Me.TAG = TAG

            If String.IsNullOrEmpty(Me.TAG) Then
                Me.TAG = vbTab
            End If
        End Sub

        Public Function Tick() As Integer
            If delta = 0 Then
                Return 0
            End If

            current += 1
            If current Mod delta = 0 Then
                Call ToString.__DEBUG_ECHO
            Else
                Call Console.Write(".")
            End If

            Return current
        End Function

        Dim p As Double
        Dim sw As Stopwatch = Stopwatch.StartNew
        Dim pre As Long

        Public Overrides Function ToString() As String
            If Capacity = 0 Then
                Return ""
            End If

            Dim dt As Long = sw.ElapsedMilliseconds - pre
            pre = sw.ElapsedMilliseconds
            p = current / Capacity

            Return $" [{TAG}, {dt}ms] * ...... {Math.Round(100 * p, 2)}%"
        End Function
    End Class
End Namespace
