Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Public Class Parser

    Public Property func As String
    Public Property parameters As String()
    Public Property pipeNext As Parser

    Public Function Parse(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim value As InnerPlantText

        Select Case func
            Case "css"
                value = CssQuery(document, isArray)
            Case "attr"
                value = QueryAttribute(document, isArray)
            Case "xpath"
                value = XPathQuery(document, isArray)
            Case Else
                value = env.Execute(document, func, parameters, isArray)
        End Select

        If Not pipeNext Is Nothing Then
            value = pipeNext.Parse(value, isArray, env)
        End If

        Return value
    End Function

    Private Function XPathQuery(document As HtmlElement, isArray As Boolean) As InnerPlantText
        Dim xpath As XPath = XPathParser.Parse(parameters(Scan0))
        Dim engine As New XPathQuery(xpath)
        Dim query As InnerPlantText

        If isArray Then
            query = New HtmlElement With {
                .TagName = parameters(Scan0),
                .HtmlElements = engine _
                    .QueryAll(document) _
                    .Select(Function(n)
                                Return DirectCast(DirectCast(n, HtmlElement), InnerPlantText)
                            End Function) _
                    .ToArray
            }
        Else
            query = engine.QuerySingle(document)
        End If

        Return query
    End Function

    Private Function CssQuery(document As HtmlElement, isArray As Boolean) As InnerPlantText
        Dim query As String = parameters(Scan0)

        If query.First = "#"c Then
            ' get element by id
            Return DirectCast(document, HtmlElement).getElementById(query.Substring(1))
        ElseIf query.First = "."c Then
            Dim n As String = parameters.ElementAtOrDefault(1)
            Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByClassName(query.Substring(1))

            If isArray Then
                ' get elements by class name
                Return New HtmlElement With {
                    .TagName = query,
                    .HtmlElements = list
                }
            Else
                Return list(CInt(Val(n)))
            End If
        Else
            Dim n As String = parameters.ElementAtOrDefault(1)
            Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByTagName(query)

            If isArray Then
                ' get elements by tag name
                Return New HtmlElement With {
                    .TagName = query,
                    .HtmlElements = list
                }
            Else
                Return list(CInt(Val(n)))
            End If
        End If
    End Function

    Private Function QueryAttribute(document As HtmlElement, isArray As Boolean) As InnerPlantText
        If isArray Then
            Return New HtmlElement With {
                .HtmlElements = DirectCast(document, HtmlElement)(parameters(Scan0)).Values _
                    .Select(Function(a)
                                Return New InnerPlantText With {
                                    .InnerText = a
                                }
                            End Function) _
                    .ToArray
            }
        Else
            Return New InnerPlantText With {
                .InnerText = DirectCast(document, HtmlElement)(parameters(Scan0)).Value
            }
        End If
    End Function

    Public Overrides Function ToString() As String
        Dim thisText As String = $"{func}({parameters.JoinBy(", ")})"

        If Not pipeNext Is Nothing Then
            Return $"{thisText} -> {pipeNext}"
        Else
            Return thisText
        End If
    End Function

    Public Shared Operator &(left As Parser, [next] As Parser) As Parser
        If left.pipeNext Is Nothing Then
            left.pipeNext = [next]
        Else
#Disable Warning BC42004 ' Expression recursively calls the containing Operator
            left.pipeNext = left.pipeNext & [next]
#Enable Warning BC42004 ' Expression recursively calls the containing Operator
        End If

        Return left
    End Operator

End Class
