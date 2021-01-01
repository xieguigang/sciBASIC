Namespace Net.Protocols.ContentTypes

    ''' <summary>
    ''' MIME types / Internet Media Types attribute data that tagged on the content generator class or module
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct, AllowMultiple:=True, Inherited:=True)>
    Public Class ContentTypeAttribute : Inherits Attribute

        ''' <summary>
        ''' Internet media type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As String
        ''' <summary>
        ''' Type of format
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Description As String

        ''' <summary>
        ''' A media type (formerly known as MIME type) is a two-part identifier for file formats and format contents transmitted on the Internet. 
        ''' The Internet Assigned Numbers Authority (IANA) is the official authority for the standardization and publication of these 
        ''' classifications. Media types were originally defined in Request for Comments 2045 in November 1996 as a part of MIME (Multipurpose 
        ''' Internet Mail Extensions) specification, for denoting type of email message content and attachments; hence the name MIME type. 
        ''' Media types are also used by other internet protocols such as HTTP and document file formats such as HTML, for similar purpose.
        ''' </summary>
        ''' <param name="type">
        ''' A media type consists of a type and a subtype, which is further structured into a tree. A media type can optionally define a 
        ''' suffix and parameters:
        ''' 
        ''' ```
        ''' type "/" [tree "."] subtype ["+" suffix] *[";" parameter]
        ''' ```
        ''' 
        ''' The currently registered types are: ``application``, ``audio``, ``example``, ``font``, ``image``, ``message``, ``model``, 
        ''' ``multipart``, ``text`` And ``video``.
        ''' </param>
        ''' <param name="description"></param>
        Sub New(type$, Optional description$ = Nothing)
            Me.Type = type
            Me.Description = description
        End Sub

        Public Overrides Function ToString() As String
            Return Type
        End Function
    End Class

End Namespace