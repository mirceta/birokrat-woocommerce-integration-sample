Use factories for retrieving diagnostic classes. For example, if you need a location check to check birokrat location, use:

common_ops.diagnostics.Checks.Location.LocationChecksFactory

```CSHARP
var biroExeLocationCheck = new LocationChecksFactory().Build_BiroExeLocationCheck();
```

This will return ICheck an interface with a default implementation of BirokratExe_Location_Check

The concrete class in this example is `common_ops.diagnostics.Checks.Location.Checks.BirokratExe_Location_Check` which should **ONLY** be called with the default implementation with the factory.  Calling the class without a factory is only used in unit tests. **This rule is true for all checks**.


## Default parameters in checks

In many checks parameters are optional. This is because in many checks we are asserting that we are respecting default naming/location/key... Those default values are stated in classes: `common_ops.diagnostics.Constants.BiroNextConstants` and `common_ops.diagnostics.Constants.BiroLocationConstants` 

In location check example stated above default parameter for birokrat exe location is `C:\Birokrat`
