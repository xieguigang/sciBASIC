#Region "Microsoft.VisualBasic::0809e617a5a56173150bbb740c000336, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\TextRectangle.vb"

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

    '   Total Lines: 164
    '    Code Lines: 117
    ' Comment Lines: 25
    '   Blank Lines: 22
    '     File Size: 5.51 KB


    '     Class TextRectangle
    ' 
    '         Properties: rect, text
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: change_state, deepCopy, ToString
    ' 
    '         Sub: down_translation, left_translation, right_translation, upper_translation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports np = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.Numpy

Namespace Drawing2D.Text.Nudge

    ''' <summary>
    ''' class of particular rectangle in the context of pyplot.
    ''' For the need of our problematic which Is To avoid texts annotation
    ''' overlayering.
    '''
    ''' a TextRectngle Is just a rectangle With a state. A state Is a given 
    ''' position compared To the point at wich the rectangle (i.e the text 
    ''' in pyplot) Is attached.
    '''
    ''' We consider 4 possible state. The first state Is the Default state
    ''' which Is the text positionned at the top-right of the attached point.
    ''' Second state the text Is at the top left, In the third state it's at the
    ''' right bottom And at the fourth state it's at the left bottom.
    '''
    ''' Given this, estimate the length Is important To avoid an important shift
    ''' For the second And fourth states.
    ''' </summary>
    Public Class TextRectangle

        Friend r As PlateRectangle
        Friend state As States
        Friend marge As Double()

        Public ReadOnly Property text As String
        Public ReadOnly Property rect As RectangleF
            Get
                Return r.Rectangle
            End Get
        End Property

        Sub New(text As String, r As PlateRectangle, state As States, Optional marge As Double() = Nothing)
            If marge.IsNullOrEmpty Then
                marge = New Double() {0, 0}
            End If

            Me.r = r
            Me.state = state
            Me.marge = marge
            Me.text = text
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(text As String, r As RectangleF,
                Optional state As States = States.top_right,
                Optional marge As Double() = Nothing)

            Call Me.New(text, New PlateRectangle(r), state, marge)
        End Sub

        Private Sub New()
        End Sub

        Friend Function deepCopy() As TextRectangle
            Return New TextRectangle With {
                .marge = marge.ToArray,
                .r = New PlateRectangle(r.x1, r.l, r.h),
                .state = state
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{state.Description}] {text} ({marge.Select(Function(d) d.ToString("F4")).JoinBy(", ")}) {r.ToString}"
        End Function

        Public Sub left_translation()
            Dim x1 = r.x1 + np.array({-r.l - 2 * marge(0), 0})
            r = New PlateRectangle(x1, r.l, r.h)
            state += 1
        End Sub

        Public Sub right_translation()
            Dim x1 = r.x1 + np.array({r.l + 2 * marge(0), 0})
            r = New PlateRectangle(x1, r.l, r.h)
            state -= 1
        End Sub

        Public Sub upper_translation()
            Dim x1 = r.x1 + np.array({0, r.h + 2 * marge(1)})
            r = New PlateRectangle(x1, r.l, r.h)
            state -= 2
        End Sub

        Public Sub down_translation()
            Dim x1 = r.x1 + np.array({0, -r.h - 2 * marge(1)})
            r = New PlateRectangle(x1, r.l, r.h)
            state += 2
        End Sub

        ''' <summary>
        ''' 通过这个函数尝试进行位置更新
        ''' </summary>
        ''' <param name="state"></param>
        ''' <returns></returns>
        Public Function change_state(state As States) As Integer
            If Me.state = state Then
                Return 0
            End If

            ' 因为在调用了translation方法之后，自身的state会发生改变
            ' 在修改完状态后还会继续做判断
            ' 所以在这里不可以使用switch语法

            If Me.state = 1 Then
                If state = 2 Then
                    left_translation()
                End If
                If state = 3 Then
                    down_translation()
                End If
                If state = 4 Then
                    down_translation()
                    left_translation()
                End If
            End If

            If Me.state = 2 Then
                If state = 1 Then
                    right_translation()
                End If
                If state = 3 Then
                    down_translation()
                    right_translation()
                End If
                If state = 4 Then
                    down_translation()
                End If
            End If

            If Me.state = 3 Then
                If state = 1 Then
                    upper_translation()
                End If
                If state = 2 Then
                    left_translation()
                    upper_translation()
                End If
                If state = 4 Then
                    left_translation()
                End If
            End If

            If Me.state = 4 Then
                If state = 1 Then
                    upper_translation()
                    right_translation()
                End If
                If state = 2 Then
                    upper_translation()
                End If
                If state = 3 Then
                    right_translation()
                End If
            End If

            Return 1
        End Function
    End Class
End Namespace
