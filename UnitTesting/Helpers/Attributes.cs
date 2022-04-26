using System;

namespace UnitTesting
{
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }

        public TestPriorityAttribute(int priority) => Priority = priority;
    }

    public class CollectionPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }

        public CollectionPriorityAttribute(int priority) => Priority = priority;
    }
}