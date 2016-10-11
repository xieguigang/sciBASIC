
Namespace ComponentModel.DataSourceModel

    Public Interface IProperty

        ''' <summary>
        ''' Gets property value from <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        Function GetValue(target As Object) As Object

        ''' <summary>
        ''' Set <paramref name="value"/> to the property of <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="value"></param>
        Sub SetValue(target As Object, value As Object)
    End Interface
End Namespace