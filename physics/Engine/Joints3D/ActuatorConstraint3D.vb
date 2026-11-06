Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' a position servo about one axis, solved as a constraint of the world
    ''' </summary>
    ''' <remarks>
    ''' The reference solver turns a ``position`` actuator into a force
    ''' (``engine_forward.c``: ``actuator_force = gain·ctrl + bias`` with
    ''' ``bias = kp·(qref − q)`` for the affine servo), clamps it to the force range
    ''' and adds it to the same acceleration equation that carries the gravity and
    ''' the constraints. The joint then settles wherever the force balance is, and the
    ''' speed it reports is a physical speed.
    '''
    ''' The port used to write the joint speed outright from a sub step hook, which
    ''' works but is not the same system: the observation carries a speed no
    ''' reference run ever produces, the constraint solver can not redistribute a
    ''' value that is simply overwritten, and the loop has no force limit.
    '''
    ''' This class is the same law written where the solver can see it: the torque is
    ''' recomputed every iteration from the current state, the *increment* of its
    ''' impulse is applied and the accumulated part is kept, so the iterations
    ''' converge instead of fighting (writing the speed per iteration measured
    ''' 23/33/60 on the gait bench).
    ''' </remarks>
    Public Class ActuatorConstraint3D
        Implements IConstraint3D

        ''' <summary>the body that carries the hinge</summary>
        Public Property A As RigidBody3D
        ''' <summary>the body that the hinge turns</summary>
        Public Property B As RigidBody3D
        ''' <summary>the hinge axis in world coordinates, refreshed by the owner</summary>
        Public Property Axis As Vector3 = New Vector3(0, 0, 1)
        ''' <summary>the angle of the hinge in radians, refreshed by the owner</summary>
        Public Property Angle As Double = 0
        ''' <summary>the target angle of the servo, radians</summary>
        Public Property Target As Double = 0

        ''' <summary>the proportional gain, N·m/rad (the kp of the mjcf actuator)</summary>
        Public Property Kp As Double = 0.55
        ''' <summary>the derivative gain, N·m·s/rad</summary>
        Public Property Kv As Double = 0.0
        ''' <summary>the smallest torque the motor can deliver</summary>
        Public Property ForceMin As Double = -0.96
        ''' <summary>the largest torque the motor can deliver</summary>
        Public Property ForceMax As Double = 0.96

        ''' <summary>the angular impulse that has been applied in this sub step</summary>
        Public Property Accumulated As Double = 0

        ''' <summary>the torque of the last iteration, for the diagnostics</summary>
        Public ReadOnly Property LastTorque As Double
            Get
                Return m_lastTorque
            End Get
        End Property

        Private m_lastTorque As Double = 0

        ''' <summary>forget the impulse of the previous sub step</summary>
        Public Sub Reset()
            Accumulated = 0
        End Sub

        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            If A Is Nothing OrElse B Is Nothing OrElse dt <= 0 OrElse Kp <= 0 Then
                Return
            End If

            Dim invIA As Matrix3x3 = A.InvInertiaWorld()
            Dim invIB As Matrix3x3 = B.InvInertiaWorld()
            Dim compliance As Double = Vector3Math.Dot(Axis, invIA.MultiplyLeft(Axis)) +
                                       Vector3Math.Dot(Axis, invIB.MultiplyLeft(Axis))

            If compliance < 1.0E-12 Then
                Return
            End If

            ' the servo is a *soft* constraint, not a torque that is integrated
            ' explicitly: the reference solver builds efc_R and efc_D from the
            ' impedance of the actuator (mj_solref / mj_solimp) and solves
            ' (J M^-1 J' + R) λ = -(J qacc - aref), which is stable for any gain at
            ' any step size. Writing the same law as a torque needs a step size that
            ' the armature of these motors does not allow (measured: 22 of 200 steps
            ' upright, against 250 of 250 for the velocity servo of the hook).
            '
            ' The softening term of Box2D is the same algebra:
            '   gamma = 1 / (dt·(d + dt·k)),  mass = 1 / (1/m_eff + gamma)
            '   bias  = C·dt·k·gamma,         Δλ = -mass·(v + bias + gamma·λ)
            Dim gamma As Double = 1.0 / (dt * (Kv + dt * Kp))
            Dim effectiveMass As Double = 1.0 / (compliance + gamma)
            ' the sign convention of the soft constraint is C = (current - target):
            ' a positive position error has to produce a negative impulse, otherwise
            ' the servo pushes the joint away from its set point (measured: 21 of 200
            ' steps upright, the legs simply fold)
            Dim bias As Double = (Angle - Target) * dt * Kp * gamma
            Dim speed As Double = Vector3Math.Dot(B.AngularVelocity - A.AngularVelocity, Axis)
            Dim delta As Double = -effectiveMass * (speed + bias + gamma * Accumulated)
            Dim previous As Double = Accumulated

            ' the force range of the motor becomes a range of the impulse
            Accumulated = Clamp(previous + delta, ForceMin * dt, ForceMax * dt)
            delta = Accumulated - previous

            m_lastTorque = Accumulated / dt

            If delta = 0 Then
                Return
            End If

            A.AngularVelocity = A.AngularVelocity - invIA.MultiplyLeft(Axis * delta)
            B.AngularVelocity = B.AngularVelocity + invIB.MultiplyLeft(Axis * delta)
        End Sub

        ' the servo has no position level step: the velocity pass drives the error
        ' to where the force balance allows, exactly like the reference actuator
        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
        End Sub

        Private Shared Function Clamp(value As Double, low As Double, high As Double) As Double
            If value < low Then
                Return low
            End If

            If value > high Then
                Return high
            End If

            Return value
        End Function
    End Class
End Namespace
