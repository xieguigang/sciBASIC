#Region "Microsoft.VisualBasic::aac9d1524e25fd793439ec21f138c997, gr\Microsoft.VisualBasic.Imaging\SVG\Geometry\Transform.vb"

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

    '   Total Lines: 49
    '    Code Lines: 39 (79.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.68 KB


    '     Class Transform
    ' 
    '         Properties: translate
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Parse, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVG

    Public Class Transform

        ReadOnly transform As New Dictionary(Of String, String())

        Public ReadOnly Property translate As PointF
            Get
                Dim translate_pars As String() = transform.TryGetValue("translate")

                If translate_pars.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return New PointF(Val(translate_pars(0)), Val(translate_pars(1)))
                End If
            End Get
        End Property

        Friend Sub New(str As String)
            transform = Parse(str) _
                .ToDictionary(Function(a) a.op,
                              Function(a)
                                  Return a.pars
                              End Function)
        End Sub

        Public Function GetOffsetTransform() As PointF
            Dim x As Single = 0
            Dim y As Single = 0

            For Each apply As KeyValuePair(Of String, String()) In transform
                Select Case apply.Key
                    Case "translate"
                        Dim translate_pars = apply.Value

                        x += Val(translate_pars(0))
                        y += Val(translate_pars(1))
                    Case "matrix"
                        Dim matrix As New MatrixTransform(apply.Value.AsDouble)

                        x += matrix.x
                        y += matrix.y
                End Select
            Next

            Return New PointF(x, y)
        End Function

        Public Overrides Function ToString() As String
            Return transform.GetJson
        End Function

        Private Shared Iterator Function Parse(str As String) As IEnumerable(Of (op$, pars As String()))
            Dim matches = str.Matches($"[a-z]+\s*\({SimpleNumberPattern}(\s+{SimpleNumberPattern})*\)")

            For Each op As String In matches
                Dim op_name = op.Match("[a-z]+")
                Dim pars = op.GetStackValue("(", ")") _
                    .Split _
                    .Select(AddressOf Strings.Trim) _
                    .Where(Function(si) si.Length > 0) _
                    .ToArray

                Yield (Strings.LCase(op_name), pars)
            Next
        End Function
    End Class

    Public Class MatrixTransform

        Public ReadOnly a, b, c, d, x, y As Single

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">水平缩放因子</param>
        ''' <param name="b">水平倾斜因子</param>
        ''' <param name="c">垂直倾斜因子</param>
        ''' <param name="d">垂直缩放因子</param>
        ''' <param name="e">水平移动距离</param>
        ''' <param name="f">垂直移动距离</param>
        Sub New(a!, b!, c!, d!, e!, f!)
            Me.a = a
            Me.b = b
            Me.c = c
            Me.d = d
            Me.x = e
            Me.y = f
        End Sub

        Sub New(d As Double())
            Call Me.New(d(0), d(1), d(2), d(3), d(4), d(5))
        End Sub

        Public Function GetOffset() As PointF
            Return New PointF(x, y)
        End Function

    End Class
End Namespace
