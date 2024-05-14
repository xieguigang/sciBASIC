#Region "Microsoft.VisualBasic::35d4a890c675ca142ea2cfd875d4b753, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\Adjustment.vb"

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

    '   Total Lines: 169
    '    Code Lines: 131
    ' Comment Lines: 16
    '   Blank Lines: 22
    '     File Size: 6.50 KB


    '     Module Adjustment
    ' 
    '         Function: adjust_text, make_text_rectangle, measureSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports np = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.Numpy

Namespace Drawing2D.Text.Nudge

    Public Module Adjustment

        Private Function measureSize(xy As Double(), text As String, marge As Double(), ax As GraphicsTextHandle) As SizeF
            Dim x_scope = ax.get_xlim()(1) - ax.get_xlim()(0)
            Dim y_scope = ax.get_ylim()(1) - ax.get_ylim()(0)
            Dim figwidth = ax.get_figwidth()
            Dim figheight = ax.get_figheight()
            Dim sizing_dict As New Dictionary(Of String, Double) From {
                {"default", 1.1},
                {"a", 1.1},
                {"b", 1.1},
                {"c", 1},
                {"d", 1.1},
                {"e", 1.1},
                {"f", 0.65},
                {"g", 1.1},
                {"h", 1.1},
                {"i", 0.55},
                {"j", 0.55},
                {"k", 1.1},
                {"l", 0.55},
                {"m", 1.54},
                {"n", 1.1},
                {"o", 1.1},
                {"p", 1.1},
                {"q", 1.1},
                {"r", 0.825},
                {"s", 1},
                {"t", 0.65},
                {"u", 1.1},
                {"v", 1.1},
                {"w", 1.3},
                {"x", 1.1},
                {"y", 1.1},
                {"z", 1},
                {"_", 0.9},
                {"'", 0.55},
                {"1", 1.15},
                {"2", 1.15},
                {"3", 1.15},
                {"4", 1.15},
                {"5", 1.15},
                {"6", 1.15},
                {"7", 1.15},
                {"8", 1.15},
                {"9", 1.15},
                {"0", 1.15}
            }

            ' as we compute character size in a x_scope of length 16 and figwidth of 10
            ', we have to rescale the character's size to the targeted figure scale.
            For Each key In sizing_dict.Keys
                sizing_dict(key) = sizing_dict(key) * x_scope / (figwidth * 10)
            Next

            Dim h = 1.5 * x_scope / (figwidth * 10)
            Dim l As Double = 0
            For Each c As Char In text.ToLower
                If Not sizing_dict.ContainsKey(c) Then
                    'raise ValueError("character {} in text not recognize by sizing\
                    'dict. Only lower character without accent are accepted, and only _ and ' are\
                    'accepted for punctuation".format(c))
                    l += sizing_dict("default")
                Else
                    l += sizing_dict(c)
                End If
            Next

            Return New SizeF(l, h)
        End Function

        ''' <summary>
        ''' function to create text rectangle from xy coordonnee and text to print.
        ''' Added a coef to balance output for some reason
        ''' </summary>
        ''' <param name="marge"></param>
        ''' <param name="ax"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function make_text_rectangle(label As Label, marge As Double(), ax As GraphicsTextHandle) As TextRectangle
            Dim xy As Double() = New Double() {label.X, label.Y}
            Dim l As Double = label.width
            Dim h As Double = label.height

            Return New TextRectangle(label.text, New PlateRectangle(xy + np.array(marge), l, h), 1, marge)
        End Function

        <Extension>
        Public Function adjust_text(ax As GraphicsTextHandle,
                                    Optional add_marge As Boolean = True,
                                    Optional arrows As Boolean = False,
                                    Optional maxLoop As Integer = 30,
                                    Optional debug As Boolean = True) As Boolean

            If ax.texts.IsNullOrEmpty Then
                Return False
            End If
            'If the Then axis aspect Is Set To equal To keep proportion (For cercle) For example, the x_scope Is going To be multiply by two When we will enlarge 
            'If ax Then.get_aspect() == "equal":
            '	x_scope *= 2
            Dim x_scope = ax.get_xlim()(1) - ax.get_xlim()(0)
            Dim y_scope = ax.get_ylim()(1) - ax.get_ylim()(0)
            Dim figwidth = ax.get_figwidth()
            Dim figheight = ax.get_figheight()
            Dim marge As Vector

            If add_marge Then
                marge = np.array({x_scope / figwidth, y_scope / figheight}) / 15
            Else
                marge = np.array({0, 0})
            End If

            Dim list_tr As New List(Of TextRectangle)

            For Each text As Label In ax.texts
                Call text _
                    .make_text_rectangle(marge, ax) _
                    .DoCall(AddressOf list_tr.Add)
            Next

            Dim cloud As New CloudOfTextRectangle(list_tr)
            Dim [loop] As Integer = 1

            Do While cloud.conflicts.Count > 0
                Call cloud.arrange_text(arrows, moveAll:=True)

                If debug Then
                    Call Console.WriteLine($"[{[loop]}/{maxLoop}] {cloud.ToString}")
                End If

                If maxLoop = [loop] Then
                    Exit Do
                Else
                    [loop] += 1
                End If
            Loop

            For i As Integer = 0 To ax.texts.Length - 1
                Dim tr = cloud.list_tr(i)
                Dim label = ax.texts(i)

                If Not (label.X = tr.r.x1(0) AndAlso label.Y = tr.r.x1(1)) Then
                    Dim move As String = $"[{label.X.ToString("F3")},{ label.Y.ToString("F3")}] -> [{tr.r.x1(0).ToString("F3")},{tr.r.x1(1).ToString("F3")}]"
                    Dim offset As String = {label.X - tr.r.x1(0), label.Y - tr.r.x1(1)}.GetJson

                    label.X = tr.r.x1(0)
                    label.Y = tr.r.x1(1)

                    If debug Then
                        Call Console.WriteLine($"move '{label.text}' {move}, offset {offset}")
                    End If
                End If
            Next

            Return True
        End Function
    End Module
End Namespace
