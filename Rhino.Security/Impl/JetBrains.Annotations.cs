using System;

namespace JetBrains.Annotations
{
    internal class AssertionMethodAttribute : Attribute
    {

    }

    internal class AssertionConditionAttribute : Attribute
    {
        public AssertionConditionAttribute(AssertionConditionType conditionType)
        {
            
        }
    }

    internal enum AssertionConditionType
    {
        IS_TRUE,
        IS_FALSE
    }
}