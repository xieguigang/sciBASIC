#Region "Microsoft.VisualBasic::807d51f45f3e8f915cb53e437948d149, vs_solutions\JavaScript\Runtime\JsRegExp.vb"

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

    '   Total Lines: 102
    '    Code Lines: 69 (67.65%)
    ' Comment Lines: 15 (14.71%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 18 (17.65%)
    '     File Size: 3.73 KB


    '     Class JsRegExp
    ' 
    '         Properties: IgnoreCase, IsGlobal, LastIndex, Multiline, Sticky
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: MatchFromState, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

Namespace Runtime

    ''' <summary>
    ''' JavaScript RegExp: wraps a .NET <see cref="Regex"/> with JS flag semantics
    ''' (<c>g</c> <c>i</c> <c>m</c> <c>s</c> <c>u</c> <c>y</c>) and the mutable
    ''' <see cref="LastIndex"/> state used by <c>test</c>/<c>exec</c>.
    ''' </summary>
    Public NotInheritable Class JsRegExp

        Public ReadOnly Source As String
        Public ReadOnly Flags As String
        Public ReadOnly Engine As Regex

        ''' <summary>Scan position of the stateful methods (global / sticky).</summary>
        Public Property LastIndex As Integer

        Public Sub New(source As String, Optional flags As String = "")
            Me.Source = source
            Me.Flags = If(flags, "")

            Dim opts = RegexOptions.None
            For Each f In Me.Flags
                Select Case f
                    Case "i"c : opts = opts Or RegexOptions.IgnoreCase
                    Case "m"c : opts = opts Or RegexOptions.Multiline
                    Case "s"c : opts = opts Or RegexOptions.Singleline
                    Case "g"c, "u"c
                        ' 'g' is stateful and handled by the callers; .NET is already unicode aware
                    Case "y"c
                        ' sticky: approximated by requiring the match to start at LastIndex
                    Case Else
                        Throw JsRuntimeException.SyntaxError("Invalid regular expression flag '" & f & "'")
                End Select
            Next

            Try
                Me.Engine = New Regex(source, opts)
            Catch ex As Exception
                Throw JsRuntimeException.SyntaxError("Invalid regular expression: " & ex.Message)
            End Try
        End Sub

        ''' <summary>'g' flag: stateful scanning through <see cref="LastIndex"/>.</summary>
        Public ReadOnly Property IsGlobal As Boolean
            Get
                Return Flags.Contains("g")
            End Get
        End Property

        Public ReadOnly Property Sticky As Boolean
            Get
                Return Flags.Contains("y")
            End Get
        End Property

        ''' <summary>'i' flag.</summary>
        Public ReadOnly Property IgnoreCase As Boolean
            Get
                Return Flags.Contains("i")
            End Get
        End Property

        ''' <summary>'m' flag.</summary>
        Public ReadOnly Property Multiline As Boolean
            Get
                Return Flags.Contains("m")
            End Get
        End Property

        ''' <summary>
        ''' Match honouring the global/sticky state: global and sticky searches start
        ''' at <see cref="LastIndex"/>; sticky also requires the match to begin there.
        ''' </summary>
        Public Function MatchFromState(input As String) As Match
            If Not IsGlobal AndAlso Not Sticky Then Return Engine.Match(input)

            Dim start = System.Math.Min(System.Math.Max(0, LastIndex), input.Length)
            Dim m = Engine.Match(input, start)

            If Sticky AndAlso m.Success AndAlso m.Index <> start Then
                LastIndex = 0
                Return Nothing
            End If

            If m.Success Then
                LastIndex = m.Index + m.Length
                If m.Length = 0 Then LastIndex += 1      ' avoid an infinite scan loop
            Else
                LastIndex = 0
            End If
            Return m
        End Function

        Public Overrides Function ToString() As String
            Return "/" & Source & "/" & Flags
        End Function

    End Class

End Namespace
