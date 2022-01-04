Imports System.Runtime.CompilerServices

Namespace Layouts

    Public Interface IPlanner

        ''' <summary>
        ''' Calculates the physics updates.
        ''' run a step of the current layout algorithm 
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub Collide(Optional timeStep As Double = Double.NaN)

    End Interface
End Namespace