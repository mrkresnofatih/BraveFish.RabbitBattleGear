using System;

namespace BraveFish.RabbitBattleGear
{
    public class UnregisteredQueueException : Exception
    {
        public UnregisteredQueueException() : base("Target Queue Unregistered! Make sure you " +
                                                   "register it in the Rabbit Battle Gear Context!")
        {
        }
    }
}