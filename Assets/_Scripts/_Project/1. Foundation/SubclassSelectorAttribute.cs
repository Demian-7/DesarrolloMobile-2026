using System;
using UnityEngine;

namespace Game.Foundation
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute { }
}