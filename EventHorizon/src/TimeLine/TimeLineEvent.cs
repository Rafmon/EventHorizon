using System;
using Iot.Device.Mcp23xxx;
using System.IO.Ports;
using UnitsNet;

namespace EventHorizon.src.TimeLine
{
    public class TimeLineEvent : IComparable<TimeLineEvent>

    {
        public Boolean IsActive { get; }
        public int ExecutionTime { get; }
        public MemoryAddress Address { get; }
        public String Name { get; }


        public TimeLineEvent(bool isActive, float executionTime, MemoryAddress address, String name)
        {
            IsActive = isActive;
            ExecutionTime = (int)executionTime;
            Address = address;
            Name = name;
        }

        public int CompareTo(TimeLineEvent? other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return ExecutionTime.CompareTo(other.ExecutionTime);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + IsActive.GetHashCode();
                hash = hash * 23 + ExecutionTime.GetHashCode();
                hash = hash * 23 + Address.GetHashCode();
                return hash;
            }
        }
        public bool Equals(TimeLineEvent other)
        {
            if (other == null) return false;
            return (this.GetHashCode().Equals(other.GetHashCode()));
        }
    }
}
