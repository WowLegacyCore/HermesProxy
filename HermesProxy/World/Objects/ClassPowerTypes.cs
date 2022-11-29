using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects
{
    public static class ClassPowerTypes
    {
        // ChrClassesXPowerTypes.db2
        public static sbyte GetPowerSlotForClass(Class classId, PowerType power)
        {
            switch (classId)
            {
                case Class.Warrior:
                {
                    switch (power)
                    {
                        case PowerType.Rage:
                            return 0;
                        case PowerType.ComboPoints:
                            return 1;
                    }
                    break;
                }
                case Class.Paladin:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                    }
                    break;
                }
                case Class.Hunter:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                    }
                    break;
                }
                case Class.Rogue:
                {
                    switch (power)
                    {
                        case PowerType.Energy:
                            return 0;
                        case PowerType.ComboPoints:
                            return 1;
                    }
                    break;
                }
                case Class.Priest:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                    }
                    break;
                }
                case Class.Shaman:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                    }
                    break;
                }
                case Class.Mage:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                    }
                    break;
                }
                case Class.Warlock:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                    }
                    break;
                }
                case Class.Druid:
                {
                    switch (power)
                    {
                        case PowerType.Mana:
                            return 0;
                        case PowerType.Rage:
                            return 1;
                        case PowerType.Energy:
                            return 2;
                        case PowerType.ComboPoints:
                            return 3;
                    }
                    break;
                }
            }

            return -1;
        }
        public static sbyte GetPowerSlotForPet(PowerType power)
        {
            switch (power)
            {
                case PowerType.Focus:
                    return 0;
                case PowerType.Happiness:
                    return 3;
            }

            return -1;
        }
    }
}
