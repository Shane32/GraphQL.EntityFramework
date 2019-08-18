using System;
using System.Diagnostics;

[DebuggerDisplay("PropertyName = {PropertyName}, PropertyType = {PropertyType}")]
class Navigation
{
    public Navigation(string propertyName, Type propertyType)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));
        if (propertyType == null) throw new ArgumentNullException(nameof(propertyType));
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public string PropertyName { get; }
    public Type PropertyType { get; }
}