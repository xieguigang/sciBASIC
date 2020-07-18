Public Class HtmlWriter

    Public Shared Function img(src As String, Optional id As String = Nothing, Optional [class] As String = Nothing, Optional style As String = Nothing) As XElement
        Return <img src=<%= src %> id=<%= id %> class=<%= [class] %> style=<%= style %>/>
    End Function

    Public Shared Function p(content As String, Optional id As String = Nothing, Optional [class] As String = Nothing, Optional style As String = Nothing) As XElement
        Return <p id=<%= id %> class=<%= [class] %> style=<%= style %>>
                   <%= content %>
               </p>
    End Function

    Public Shared Function div(content As String, Optional id As String = Nothing, Optional [class] As String = Nothing, Optional style As String = Nothing) As XElement
        Return <div id=<%= id %> class=<%= [class] %> style=<%= style %>>
                   <%= content %>
               </div>
    End Function
End Class
