Namespace ComponentModel.Ranges.Unit

    Public Class Convertor : Inherits Attribute

        Public ReadOnly Property UnitType As Type

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="TUnit">枚举类型？</typeparam>
        ''' <param name="value"></param>
        ''' <param name="toUnit"></param>
        ''' <returns></returns>
        Public Delegate Function Convertor(Of TUnit As Structure)(value As UnitValue(Of TUnit), toUnit As TUnit) As UnitValue(Of TUnit)

        Sub New(type As Type)
            UnitType = type
        End Sub
    End Class

End Namespace