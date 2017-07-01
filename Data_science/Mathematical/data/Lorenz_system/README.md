## Solving Lorenz_system

In VisualBasic, that you can using the ODEs script for solving the dynamics system simulation problem, example for the ``Lorenz_system``:

```vbnet
Dim x, y, z As var
Dim sigma# = 10
Dim rho# = 28
Dim beta# = 8 / 3
Dim t = (a:=0, b:=120, dt:=0.005)

Call Let$(list:=Function() {x = 1, y = 1, z = 1})
Call {
    x = Function() sigma * (y - x),
    y = Function() x * (rho - z) - y,
    z = Function() x * y - beta * z
}.Solve(dt:=t) _
 .DataFrame _
 .Save($"{App.HOME}/Lorenz_system.csv")
```

And then plot this result in 3D, that we can get such image output:

![](./Lorenz_system.png)