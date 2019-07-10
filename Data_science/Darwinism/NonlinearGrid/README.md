# Nonlinear Grid Dynamics Inference By GA

Define a system with variables like:

```vbnet
X = |x1, x2, x3, x4, x5, ...| 
```

Then we can create a ``Nonlinear Grid Dynamics System`` with an equation:

```vbnet
f(X)  = a1 * p1(X) * x1 ^ c1(X) + a2 * p2(X) * x2 ^ c2(X) + a3 * p3(X) * x3 ^ c3(X) + ...

A     = |a1, a2, a3, a4, ...|
ci(X) = b1 * x1 + b2 * x2 + b3 * x3 + ... 
pi(X) = x1^w1 * x2 ^w2 * x3^w3 * ...
```

Where: 

+ ``X`` is the variables in target system
+ ``A`` is the variable sign vector, **only have value of -1, 0 and 1**.
+ and ``c1``, ``c2``, ``c3`` is a linear system of ``X``, which in form of ``c(X) = b1 * x1 + b2 * x2 + b3 * x3 + ...``, each function ``c(X)`` have individual factor vector ``B``, where ``B`` is ``|b1, b2, b3, ...|``.
+ ``p1``, ``p2``, ``p3`` is a non-linear system of ``X``, which in form of ``p(X) = x1^w1 * x2^w2 * x3^w3 * ...``, each function ``p(X)`` have individual factor vector ``W``, where ``W``
is ``|w1, w2, w3, ...|``.

Which is a really crazy complex dynamics system to solve. But solve such nonlinear system in GA is too much simple.


