#Region "Microsoft.VisualBasic::2017947f4565118930c2ab5af95fb68d, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\SwayBar.vb"

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

    '   Total Lines: 107
    '    Code Lines: 62 (57.94%)
    ' Comment Lines: 24 (22.43%)
    '    - Xml Docs: 95.83%
    ' 
    '   Blank Lines: 21 (19.63%)
    '     File Size: 3.24 KB


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
    '     Function: buildBlankPointer
    ' 
    '     Sub: (+2 Overloads) [Step], ClearBar, PlacePointer, PrintMessage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace ApplicationServices.Terminal.ProgressBar

    ''' <summary>
    ''' 像乒乓球一样左右碰撞的进度条
    ''' </summary>
    Public Class SwayBar : Inherits AbstractBar

        Dim bar As String
        Dim pointer As String
        Dim blankPointer As String
        Dim counter As Integer
        Dim currdir As direction

        Private Enum direction
            right
            left
        End Enum

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="length%">
        ''' 进度条的碰撞区域的字符数量宽度
        ''' </param>
        Public Sub New(Optional length% = 50)
            MyBase.New()

            bar = $"|{New String(" "c, length)}|"
            pointer = "***"
            blankPointer = buildBlankPointer()
            currdir = direction.right
            counter = 1
        End Sub

        ''' <summary>
        ''' sets the atribute blankPointer with a empty string the same length that the pointer
        ''' </summary>
        ''' <returns>A string filled with space characters</returns>
        Private Function buildBlankPointer() As String
            Dim blank As New StringBuilder()

            For cont As Integer = 0 To pointer.Length - 1
                blank.Append(" ")
            Next

            Return blank.ToString()
        End Function

        ''' <summary>
        ''' reset the bar to its original state
        ''' </summary>
        Private Sub ClearBar()
            bar = bar.Replace(pointer, blankPointer)
        End Sub

        ''' <summary>
        ''' remove the previous pointer and place it in a new possition
        ''' </summary>
        ''' <param name="start">start index</param>
        ''' <param name="end">end index</param>
        Private Sub PlacePointer(start As Integer, [end] As Integer)
            Call ClearBar()

            bar = bar.Remove(start, [end])
            bar = bar.Insert(start, pointer)
        End Sub

        Public Overrides Sub PrintMessage(msg As String)

        End Sub

        ''' <summary>
        ''' prints the progress bar acorrding to pointers and current direction
        ''' </summary>
        Public Overrides Sub [Step]()
            Call [Step](message:="")
        End Sub

        Public Overloads Sub [Step](message As String)
            If currdir = direction.right Then
                PlacePointer(counter, pointer.Length)
                counter += 1

                If counter + pointer.Length = bar.Length Then
                    currdir = direction.left
                End If
            Else
                PlacePointer(counter - pointer.Length, pointer.Length)
                counter -= 1

                If counter = pointer.Length Then
                    currdir = direction.right
                End If
            End If

            message = bar & message

            If message.Length > Console.WindowWidth Then
                message = Mid(message, 1, Console.WindowWidth - 15) & "..."
            End If

            Call Console.Write(message & vbCr)
        End Sub
    End Class
End Namespace
