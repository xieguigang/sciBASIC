#Region "Microsoft.VisualBasic::cb50a5279570864449b5e8bd7b1531d8, Microsoft.VisualBasic.Core\ApplicationServices\Terminal\ProgressBar\SwayBar.vb"

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

    '     Class SwayBar
    ' 
    ' 
    '         Enum direction
    ' 
    '             left, right
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: BlankPointer
    ' 
    '     Sub: [Step], ClearBar, PlacePointer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Terminal.ProgressBar

    Public Class SwayBar : Inherits AbstractBar

        Private bar As String
        Private pointer As String
        Private _blankPointer As String
        Private counter As Integer
        Private currdir As direction
        Private Enum direction
            right
            left
        End Enum
        Public Sub New()
            MyBase.New()
            Me.bar = "|                         |"
            Me.pointer = "***"
            Me._blankPointer = Me.BlankPointer()
            Me.currdir = direction.right
            Me.counter = 1
        End Sub

        ''' <summary>
        ''' sets the atribute blankPointer with a empty string the same length that the pointer
        ''' </summary>
        ''' <returns>A string filled with space characters</returns>
        Private Function BlankPointer() As String
            Dim blank As New StringBuilder()
            For cont As Integer = 0 To Me.pointer.Length - 1
                blank.Append(" ")
            Next
            Return blank.ToString()
        End Function

        ''' <summary>
        ''' reset the bar to its original state
        ''' </summary>
        Private Sub ClearBar()
            Me.bar = Me.bar.Replace(Me.pointer, Me._blankPointer)
        End Sub

        ''' <summary>
        ''' remove the previous pointer and place it in a new possition
        ''' </summary>
        ''' <param name="start">start index</param>
        ''' <param name="end">end index</param>
        Private Sub PlacePointer(start As Integer, [end] As Integer)
            Me.ClearBar()
            Me.bar = Me.bar.Remove(start, [end])
            Me.bar = Me.bar.Insert(start, Me.pointer)
        End Sub

        ''' <summary>
        ''' prints the progress bar acorrding to pointers and current direction
        ''' </summary>
        Public Overrides Sub [Step]()
            If Me.currdir = direction.right Then
                Me.PlacePointer(counter, Me.pointer.Length)
                Me.counter += 1
                If Me.counter + Me.pointer.Length = Me.bar.Length Then
                    Me.currdir = direction.left
                End If
            Else
                Me.PlacePointer(counter - Me.pointer.Length, Me.pointer.Length)
                Me.counter -= 1
                If Me.counter = Me.pointer.Length Then
                    Me.currdir = direction.right
                End If
            End If
            Console.Write(Me.bar & vbCr)
        End Sub
    End Class
End Namespace
