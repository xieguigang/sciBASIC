#### Example

Consider the linear program

> ###### Minimize
> ![Z=-2x-3y-4z](./images/31713ea9f0a8d46d2aa23a8371ca6c0d7f5925fd.png)
> ###### Subject to
> ![](./images/5703e5dc6ad161cbff1181bb7bb40ed8ca7ea6c9.png)

With the addition of slack variables ``s`` and ``t``, this is represented by the canonical tableau

![](./images/7dfdf84892688931940107bdf3c033cd31a217d6.png)

where columns 5 and 6 represent the basic variables ``s`` and ``t`` and the corresponding basic feasible solution is

![](./images/8aacf2a642318dc42e1f9fee8f8c354d1b84c968.png)

Columns 2, 3, and 4 can be selected as pivot columns, for this example column 4 is selected. The values of ``z`` resulting from the choice of rows 2 and 3 as pivot rows are ``10/1 = 10`` and ``15/3 = 5`` respectively. Of these the minimum is 5, so row 3 must be the pivot row. Performing the pivot produces

![](./images/a4fb20f720b50eefdf29ed28c61ddcfe2f97de92.png)

Now columns 4 and 5 represent the basic variables ``z`` and ``s`` and the corresponding basic feasible solution is

![](./images/76cc17ff3d2007824ac6a58fae19ae8e9bc8bd80.png)

For the next step, there are no positive entries in the objective row and in fact

![](./images/af784d934c642e9556373e3b44094012b49fc4c5.png)

so the minimum value of ``Z`` is −20.
