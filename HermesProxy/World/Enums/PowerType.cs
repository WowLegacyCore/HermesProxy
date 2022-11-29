namespace HermesProxy.World.Enums
{
    public enum PowerType : sbyte
    {
        Invalid                       = -1,
        Mana                          = 0,            // UNIT_FIELD_POWER1
        Rage                          = 1,            // UNIT_FIELD_POWER2
        Focus                         = 2,            // UNIT_FIELD_POWER3
        Energy                        = 3,            // UNIT_FIELD_POWER4
        Happiness                     = 4,            // UNIT_FIELD_POWER5
        Rune                          = 5,            // UNIT_FIELD_POWER6
        RunicPower                    = 6,            // UNIT_FIELD_POWER7
        ComboPoints                   = 14,           // not real, so we know to set PLAYER_FIELD_BYTES,1
    };
}
