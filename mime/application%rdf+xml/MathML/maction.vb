''' <summary>
''' The MathML ``&lt;maction>`` element provides a possibility 
''' to bind actions to (sub-) expressions. The action itself 
''' is specified by the actiontype attribute, which accepts 
''' several values. To specify which child elements are addressed 
''' by the action, you can make use of the selection attribute.
''' </summary>
Public Class maction

    Public Property actiontype As actiontypes
    Public Property [class] As String
    Public Property id As String
    Public Property style As String

End Class

Public Enum actiontypes
    statusline
    toggle
    ''' <summary>
    ''' When the pointer moves over the expression, a tooltip box with a message is displayed near the expression.
    ''' The syntax Is: ``&lt;maction actiontype = "tooltip"> expression message &lt;/maction>``.
    ''' </summary>
    tooltip
End Enum