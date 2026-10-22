''' <summary>
''' A single parsed heading from the markdown source. The <see cref="Index"/> is
''' aligned 1:1 with the order of ``&lt;h1&gt;``..``&lt;h6&gt;`` elements in the
''' rendered HTML (after id injection), so it can be used as a navigation anchor.
''' </summary>
Public Class HeadingInfo
    Public Index As Integer
    Public Level As Integer
    Public Text As String
    Public LineStart As Integer
    Public LineLength As Integer
End Class
