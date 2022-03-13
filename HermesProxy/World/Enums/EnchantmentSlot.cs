using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums
{
    namespace Vanilla
    {
        public static class EnchantmentSlot
        {
            public static int Perm = 0;
            public static int Temp = 1;
            public static int MaxInspected = 2;

            public static int Prop0 = 3;                   // used with RandomSuffix
            public static int Prop1 = 4;                   // used with RandomSuffix
            public static int Prop2 = 5;                   // used with RandomSuffix
            public static int Prop3 = 6;
            public static int Max = 7;
        };
    }
    namespace TBC
    {
        public static class EnchantmentSlot
        {
            public static int Perm = 0;
            public static int Temp = 1;
            public static int Sock1 = 2;
            public static int Sock2 = 3;
            public static int Sock3 = 4;
            public static int Bonus = 5;
            public static int MaxInspected = 6;

            public static int Prop0 = 6;                   // used with RandomSuffix
            public static int Prop1 = 7;                   // used with RandomSuffix
            public static int Prop2 = 8;                   // used with RandomSuffix and RandomProperty
            public static int Prop3 = 9;                   // used with RandomProperty
            public static int Prop4 = 10;                  // used with RandomProperty
            public static int Max = 11;
        };
    }
    namespace WotLK
    {
        public static class EnchantmentSlot
        {
            public static int Perm = 0;
            public static int Temp = 1;
            public static int Sock1 = 2;
            public static int Sock2 = 3;
            public static int Sock3 = 4;
            public static int Bonus = 5;
            public static int Prismatic = 6;               // added at apply special permanent enchantment
            public static int MaxInspected = 7;

            public static int Prop0 = 7;                   // used with RandomSuffix
            public static int Prop1 = 8;                   // used with RandomSuffix
            public static int Prop2 = 9;                   // used with RandomSuffix and RandomProperty
            public static int Prop3 = 10;                  // used with RandomProperty
            public static int Prop4 = 11;                  // used with RandomProperty
            public static int Max = 12;
        };
    }
    namespace Classic
    {
        public static class EnchantmentSlot
        {
            public static int Perm = 0;
            public static int Temp = 1;
            public static int Sock1 = 2;
            public static int Sock2 = 3;
            public static int Sock3 = 4;
            public static int Bonus = 5;
            public static int Prismatic = 6;               // added at apply special permanent enchantment
            public static int Use = 7;

            public static int MaxInspected = 8;

            public static int Prop0 = 8;                   // used with RandomSuffix
            public static int Prop1 = 9;                   // used with RandomSuffix
            public static int Prop2 = 10;                  // used with RandomSuffix and RandomProperty
            public static int Prop3 = 11;                  // used with RandomProperty
            public static int Prop4 = 12;                  // used with RandomProperty
            public static int Max = 13;
        }
    }
    

    public enum EnchantmentOffset
    {
        Id = 0,
        Duration = 1,
        Charges = 2,                         // now here not only charges, but something new in wotlk
        Max = 3
    }
}
