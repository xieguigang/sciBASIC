Namespace My.JavaScript

    Public Interface IJavaScriptObjectAccessor
        Default Property Accessor(name As String) As Object
    End Interface

    Public Enum MemberAccessorResult
        ''' <summary>
        ''' Member is not exists in current javascript object
        ''' </summary>
        Undefined
        ''' <summary>
        ''' IS a member property in this javascript object
        ''' </summary>
        ClassMemberProperty
        ''' <summary>
        ''' Is an extension property object this javascript object
        ''' </summary>
        ExtensionProperty
    End Enum

End Namespace