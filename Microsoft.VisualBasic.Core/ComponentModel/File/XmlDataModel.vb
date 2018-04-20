Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Web.Script.Serialization
Imports System.Xml
Imports System.Xml.Serialization

Namespace ComponentModel

    Public MustInherit Class XmlDataModel

        ''' <summary>
        ''' ReadOnly
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <XmlAnyElement>
        <ScriptIgnore>
        <IgnoreDataMember()>
        <SoapIgnore>
        Public Property TypeComment As XmlComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetTypeReferenceComment()
            End Get
            Set(value As XmlComment)
                ' Do Nothing
            End Set
        End Property

        Private Function GetTypeReferenceComment() As XmlComment
            Dim modelType As Type = Me.GetType
            Dim fullName$ = modelType.FullName
            Dim assembly$ = modelType.Assembly.FullName
            Dim trace$ = vbCrLf &
                "     model:    " & fullName & vbCrLf &
                "     assembly: " & assembly & vbCrLf &
                "  "

            Return New XmlDocument().CreateComment(trace)
        End Function
    End Class
End Namespace