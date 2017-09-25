Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports r = System.Text.RegularExpressions.Regex

Namespace Drawing2D.Colors

    ''' <summary>
    ''' ```vbnet
    ''' lighter(term, percentage)
    ''' darker(term, percentage)
    ''' alpha(term, percentage)
    ''' reverse(term)
    ''' skip(term, n)
    ''' take(term, n)
    ''' ```
    ''' </summary>
    Public Structure DesignerExpression

        Dim Term$
        Dim API As NamedValue(Of String)

        Public Const FunctionPattern$ = "[a-z0-9_]+\(.+\)"

        Sub New(exp$)
            If exp.IsPattern(FunctionPattern) Then
                With exp.GetTagValue("(", trim:=True)
                    Dim api$ = .Name
                    Dim arg$

                    With .Value _
                        .Trim(")"c) _
                        .GetTagValue(",", trim:=True)

                        Term = .Name.Trim
                        arg = .Value
                    End With

                    Me.API = New NamedValue(Of String) With {
                        .Name = api,
                        .Value = arg
                    }
                End With
            Else
                Term = exp
            End If
        End Sub

        Public Overrides Function ToString() As String
            If API.IsEmpty Then
                Return Term
            Else
                With API
                    If .Value.StringEmpty Then
                        Return $"{ .Name} ( {Term} )"
                    Else
                        Return $"{ .Name} ( {Term}, { .Value} )"
                    End If
                End With
            End If
        End Function

        Delegate Function Apply(colors As Color(), value$) As Color()

        Friend Shared ReadOnly actions As New Dictionary(Of String, Apply) From {
            {"lighter", New Apply(AddressOf lighter)},
            {"darker", New Apply(AddressOf darker)},
            {"alpha", New Apply(AddressOf alpha)},
            {"reverse", New Apply(AddressOf reverse)},
            {"skip", New Apply(AddressOf skip)},
            {"take", New Apply(AddressOf take)}
        }

        Public Function Modify(colors As Color()) As Color()
            If API.IsEmpty Then
                Return colors
            Else
                With API
                    Return actions(.Name.ToLower)(colors, .Value)
                End With
            End If
        End Function

        Private Shared Function lighter(colors As Color(), value$) As Color()
            Dim percentage# = value.ParseDouble

            Return colors _
                .Select(Function(c)
                            Return HSLColor _
                                .GetHSL(c) _
                                .Lighten(percentage, Color.White)
                        End Function) _
                .ToArray
        End Function

        Private Shared Function darker(colors As Color(), value$) As Color()
            Dim percentage# = value.ParseDouble

            Return colors _
                .Select(Function(c) c.Dark(percentage)) _
                .ToArray
        End Function

        Private Shared Function alpha(colors As Color(), value$) As Color()
            Dim percentage# = value.ParseDouble
            Return colors.Alpha(255 * percentage)
        End Function

        Private Shared Function reverse(colors As Color(), value$) As Color()
            Return colors.Reverse.ToArray
        End Function

        Private Shared Function skip(colors As Color(), value$) As Color()
            Return colors.Skip(CInt(Val(value))).ToArray
        End Function

        Private Shared Function take(colors As Color(), value$) As Color()
            Return colors.Take(CInt(Val(value))).ToArray
        End Function
    End Structure
End Namespace