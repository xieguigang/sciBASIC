Namespace Imaging

    Public Interface IStyleSelector(Of T)

        Function GetElementById(id As String) As T
        Function GetElementsByClassName(classname As String) As T()
        Function GetElementsByName(name As String) As T()

    End Interface
End Namespace