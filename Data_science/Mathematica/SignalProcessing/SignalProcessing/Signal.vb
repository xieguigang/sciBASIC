Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' A vector of the <see cref="TimeSignal"/>
''' </summary>
Public Class Signal : Inherits Vector(Of TimeSignal)

    Public ReadOnly Property times As Vector
        Get
            Return New Vector(From xi As TimeSignal In Buffer Select xi.time)
        End Get
    End Property

    ''' <summary>
    ''' Get the signal intensity vector of current signal data
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property intensities As Vector
        Get
            Return New Vector(From xi As TimeSignal In Buffer Select xi.intensity)
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(data As IEnumerable(Of TimeSignal))
        Call MyBase.New(data)
    End Sub

    Sub New(data As IEnumerable(Of ITimeSignal))
        Call MyBase.New(From ti As ITimeSignal In data.SafeQuery Select New TimeSignal(ti))
    End Sub

    Public Shared Operator +(a As Signal, b As Signal) As Signal
        Throw New NotImplementedException
    End Operator
End Class
