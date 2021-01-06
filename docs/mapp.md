# Simplee |> Aabel |> Mapp
The implementation of the **Mapp** monad and some of its variations: **MappR** a queue where its operations return a **Result** value and the bind function takes case of the error cases.

### Mapp
- Namespaces: *Simplee.Mapp*, *Simplee.Mapp.ComputationExpression* 
- Source: [Mapp.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/Mapp.fs)
- Test: [TMapp.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TMapp.fs)
- Computation expression: **_mapp**


### MappR
- Namespaces: *Simplee.MappR*, *Simplee.MappR.ComputationExpression* 
- Source: [MappR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/MappR.fs)
- Test: [TMappR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TMappR.fs)
- Computation expression: **_mappR**


### MappAR
- Namespaces: *Simplee.MappAR*, *Simplee.MappAR.ComputationExpression* 
- Source: [MappAR.fs](https://github.com/veminovici/aabel/blob/main/src/Aabel/MappAR.fs)
- Test: [TMappAR.fs](https://github.com/veminovici/aabel/blob/main/tests/XUno/TMappAR.fs)
- Computation expression: **_queueAR**

More examples: [QtMappR.fsx](https://github.com/veminovici/aabel/blob/main/tests/Scripts/QtMappR.fsx)
