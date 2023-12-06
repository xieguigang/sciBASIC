
Imports ClassLibrary1.math

Namespace nlp.encode

    ''' <summary>
    ''' Created by kenny on 6/3/14.
    ''' TODO create word encoder that captures
    ''' </summary>
    Public Interface WordEncoder
        Function encode(word As String) As Matrix
    End Interface

End Namespace
