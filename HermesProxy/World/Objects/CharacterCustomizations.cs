using HermesProxy.World.Server.Packets;
using HermesProxy.World.Enums;
using System.Collections.Generic;

namespace HermesProxy.World.Objects
{
    public enum LegacyCustomizationOption
    {
        None,
        Skin,
        Face,
        HairStyle,
        HairColor,
        FacialHair,
    }

    public static class CharacterCustomizations
    {
        public static LegacyCustomizationOption GetLegacyCustomizationOption(uint option)
        {
            switch (option)
            {
                /*
                case 1: // UNK - Skin Color
                    return 0;
                case 2: // UNK - Face
                    return 1;
                case 3: // UNK - Hair Style
                    return 2;
                case 4: // UNK - Hair Color
                    return 3;
                case 5: // UNK - Facial Hair
                    return 4;
                case 6: // UNK - Tattoo Style
                    return 5;
                case 7: // UNK - Horn Style
                    return 6;
                case 8: // UNK - Blindfolds
                    return 7;
                */
                case 9: // Human Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 10: // Human Male - Face
                    return LegacyCustomizationOption.Face;
                case 11: // Human Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 12: // Human Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 13: // Human Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 14: // Human Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 15: // Human Female - Face
                    return LegacyCustomizationOption.Face;
                case 16: // Human Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 17: // Human Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 18: // Human Female - Piercings
                    return LegacyCustomizationOption.FacialHair;
                case 19: // Orc Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 20: // Orc Male - Face
                    return LegacyCustomizationOption.Face;
                case 21: // Orc Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 22: // Orc Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 23: // Orc Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 25: // Orc Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 26: // Orc Female - Face
                    return LegacyCustomizationOption.Face;
                case 27: // Orc Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 28: // Orc Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 29: // Orc Female - Piercings
                    return LegacyCustomizationOption.FacialHair;
                case 30: // Dwarf Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 31: // Dwarf Male - Face
                    return LegacyCustomizationOption.Face;
                case 32: // Dwarf Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 33: // Dwarf Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 34: // Dwarf Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 35: // Dwarf Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 36: // Dwarf Female - Face
                    return LegacyCustomizationOption.Face;
                case 37: // Dwarf Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 38: // Dwarf Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 39: // Dwarf Female - Piercings
                    return LegacyCustomizationOption.FacialHair;
                case 40: // Night Elf Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 41: // Night Elf Male - Face
                    return LegacyCustomizationOption.Face;
                case 42: // Night Elf Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 43: // Night Elf Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 44: // Night Elf Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 49: // Night Elf Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 50: // Night Elf Female - Face
                    return LegacyCustomizationOption.Face;
                case 51: // Night Elf Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 52: // Night Elf Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 53: // Night Elf Female - Markings
                    return LegacyCustomizationOption.FacialHair;
                case 58: // Undead Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 59: // Undead Male - Face
                    return LegacyCustomizationOption.Face;
                case 60: // Undead Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 61: // Undead Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 62: // Undead Male - Features
                    return LegacyCustomizationOption.FacialHair;
                case 63: // Undead Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 64: // Undead Female - Face
                    return LegacyCustomizationOption.Face;
                case 65: // Undead Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 66: // Undead Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 67: // Undead Female - Features
                    return LegacyCustomizationOption.FacialHair;
                case 68: // Tauren Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 71: // Tauren Male - Horn Style
                    return LegacyCustomizationOption.HairStyle;
                case 72: // Tauren Male - Horn Color
                    return LegacyCustomizationOption.HairColor;
                case 73: // Tauren Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 74: // Tauren Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 77: // Tauren Female - Horn Style
                    return LegacyCustomizationOption.HairStyle;
                case 78: // Tauren Female - Horn Color
                    return LegacyCustomizationOption.HairColor;
                case 79: // Tauren Female - Hair
                    return LegacyCustomizationOption.FacialHair;
                case 80: // Gnome Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 81: // Gnome Male - Face
                    return LegacyCustomizationOption.Face;
                case 82: // Gnome Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 83: // Gnome Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 84: // Gnome Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 85: // Gnome Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 86: // Gnome Female - Face
                    return LegacyCustomizationOption.Face;
                case 87: // Gnome Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 88: // Gnome Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 89: // Gnome Female - Earrings
                    return LegacyCustomizationOption.FacialHair;
                case 90: // Troll Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 91: // Troll Male - Face
                    return LegacyCustomizationOption.Face;
                case 92: // Troll Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 93: // Troll Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 94: // Troll Male - Tusks
                    return LegacyCustomizationOption.FacialHair;
                case 95: // Troll Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 96: // Troll Female - Face
                    return LegacyCustomizationOption.Face;
                case 97: // Troll Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 98: // Troll Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 99: // Troll Female - Tusks
                    return LegacyCustomizationOption.FacialHair;
                case 100: // Goblin Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 102: // Goblin Male - Hair Style
                    return LegacyCustomizationOption.Face;
                case 105: // Goblin Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 110: // Blood Elf Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 111: // Blood Elf Male - Face
                    return LegacyCustomizationOption.Face;
                case 112: // Blood Elf Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 113: // Blood Elf Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 114: // Blood Elf Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 119: // Blood Elf Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 120: // Blood Elf Female - Face
                    return LegacyCustomizationOption.Face;
                case 121: // Blood Elf Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 122: // Blood Elf Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 123: // Blood Elf Female - Earrings
                    return LegacyCustomizationOption.FacialHair;
                case 128: // Draenei Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 129: // Draenei Male - Face
                    return LegacyCustomizationOption.Face;
                case 130: // Draenei Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 131: // Draenei Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 132: // Draenei Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 133: // Draenei Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 134: // Draenei Female - Face
                    return LegacyCustomizationOption.Face;
                case 135: // Draenei Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 136: // Draenei Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 137: // Draenei Female - Horn Style
                    return LegacyCustomizationOption.FacialHair;
                case 138: // Fel Orc Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 139: // Fel Orc Male - Face
                    return LegacyCustomizationOption.Face;
                case 140: // Fel Orc Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 141: // Fel Orc Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 142: // Fel Orc Female - Hair Style
                    return LegacyCustomizationOption.Skin;
                case 143: // Fel Orc Female - Hair Color
                    return LegacyCustomizationOption.Face;
                case 144: // Naga Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 145: // Naga Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 146: // Naga Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 147: // Naga Female - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 148: // Naga Female - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 149: // Naga Female - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 150: // Broken Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 151: // Broken Male - Face
                    return LegacyCustomizationOption.Face;
                case 152: // Broken Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 153: // Broken Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 154: // Broken Female - Hair Style
                    return LegacyCustomizationOption.Skin;
                case 155: // Broken Female - Hair Color
                    return LegacyCustomizationOption.Face;
                case 156: // Skeleton Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 157: // Skeleton Male - Face
                    return LegacyCustomizationOption.Face;
                case 158: // Skeleton Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 159: // Skeleton Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 160: // Skeleton Female - Hair Style
                    return LegacyCustomizationOption.Skin;
                case 161: // Skeleton Female - Hair Color
                    return LegacyCustomizationOption.Face;
                case 176: // Forest Troll Male - Skin Color
                    return LegacyCustomizationOption.Skin;
                case 177: // Forest Troll Male - Face
                    return LegacyCustomizationOption.Face;
                case 178: // Forest Troll Male - Hair Style
                    return LegacyCustomizationOption.HairStyle;
                case 179: // Forest Troll Male - Hair Color
                    return LegacyCustomizationOption.HairColor;
                case 180: // Forest Troll Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 181: // Forest Troll Female - Hair Style
                    return LegacyCustomizationOption.Skin;
                case 182: // Forest Troll Female - Hair Color
                    return LegacyCustomizationOption.Face;
                case 378: // Tauren Male - Face
                    return LegacyCustomizationOption.Face;
                case 379: // Tauren Female - Face
                    return LegacyCustomizationOption.Face;
                case 1000: // Fel Orc Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 1001: // Fel Orc Female - Facial Hair
                    return LegacyCustomizationOption.HairStyle;
                case 1002: // Naga Male - Face
                    return LegacyCustomizationOption.Face;
                case 1003: // Naga Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 1004: // Naga Female - Face
                    return LegacyCustomizationOption.Face;
                case 1005: // Naga Female - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 1006: // Broken Female - Facial Hair
                    return LegacyCustomizationOption.HairStyle;
                case 1007: // Skeleton Male - Facial Hair
                    return LegacyCustomizationOption.FacialHair;
                case 1008: // Skeleton Female - Facial Hair
                    return LegacyCustomizationOption.HairStyle;
                case 1009: // Forest Troll Female - Facial Hair
                    return LegacyCustomizationOption.HairStyle;
            }
            return LegacyCustomizationOption.None;
        }

        public static byte GetLegacyCustomizationChoice(uint value)
        {
            switch (value)
            {
                case 17160: // Human Male - Skin Color
                    return 0;
                case 17161: // Human Male - Skin Color
                    return 1;
                case 17162: // Human Male - Skin Color
                    return 2;
                case 17163: // Human Male - Skin Color
                    return 3;
                case 17164: // Human Male - Skin Color
                    return 4;
                case 17165: // Human Male - Skin Color
                    return 5;
                case 17166: // Human Male - Skin Color
                    return 6;
                case 17167: // Human Male - Skin Color
                    return 7;
                case 17168: // Human Male - Skin Color
                    return 8;
                case 17169: // Human Male - Skin Color
                    return 9;
                case 17170: // Human Male - Skin Color
                    return 10;
                case 17171: // Human Male - Skin Color
                    return 11;
                case 17172: // Human Male - Face
                    return 0;
                case 17173: // Human Male - Face
                    return 1;
                case 17174: // Human Male - Face
                    return 2;
                case 17175: // Human Male - Face
                    return 3;
                case 17176: // Human Male - Face
                    return 4;
                case 17177: // Human Male - Face
                    return 5;
                case 17178: // Human Male - Face
                    return 6;
                case 17179: // Human Male - Face
                    return 7;
                case 17180: // Human Male - Face
                    return 8;
                case 17181: // Human Male - Face
                    return 9;
                case 17182: // Human Male - Face
                    return 10;
                case 17183: // Human Male - Face
                    return 11;
                case 17184: // Human Male - Hair Style
                    return 0;
                case 17185: // Human Male - Hair Style
                    return 1;
                case 17186: // Human Male - Hair Style
                    return 2;
                case 17187: // Human Male - Hair Style
                    return 3;
                case 17188: // Human Male - Hair Style
                    return 4;
                case 17189: // Human Male - Hair Style
                    return 5;
                case 17190: // Human Male - Hair Style
                    return 6;
                case 17191: // Human Male - Hair Style
                    return 7;
                case 17192: // Human Male - Hair Style
                    return 8;
                case 17193: // Human Male - Hair Style
                    return 9;
                case 17194: // Human Male - Hair Style
                    return 10;
                case 17195: // Human Male - Hair Style
                    return 11;
                case 17196: // Human Male - Hair Color
                    return 0;
                case 17197: // Human Male - Hair Color
                    return 1;
                case 17198: // Human Male - Hair Color
                    return 2;
                case 17199: // Human Male - Hair Color
                    return 3;
                case 17200: // Human Male - Hair Color
                    return 4;
                case 17201: // Human Male - Hair Color
                    return 5;
                case 17202: // Human Male - Hair Color
                    return 6;
                case 17203: // Human Male - Hair Color
                    return 7;
                case 17204: // Human Male - Hair Color
                    return 8;
                case 17205: // Human Male - Hair Color
                    return 9;
                case 17206: // Human Male - Facial Hair
                    return 0;
                case 17207: // Human Male - Facial Hair
                    return 1;
                case 17208: // Human Male - Facial Hair
                    return 2;
                case 17209: // Human Male - Facial Hair
                    return 3;
                case 17210: // Human Male - Facial Hair
                    return 4;
                case 17211: // Human Male - Facial Hair
                    return 5;
                case 17212: // Human Male - Facial Hair
                    return 6;
                case 17213: // Human Male - Facial Hair
                    return 7;
                case 17214: // Human Male - Facial Hair
                    return 8;
                case 17215: // Human Female - Skin Color
                    return 0;
                case 17216: // Human Female - Skin Color
                    return 1;
                case 17217: // Human Female - Skin Color
                    return 2;
                case 17218: // Human Female - Skin Color
                    return 3;
                case 17219: // Human Female - Skin Color
                    return 4;
                case 17220: // Human Female - Skin Color
                    return 5;
                case 17221: // Human Female - Skin Color
                    return 6;
                case 17222: // Human Female - Skin Color
                    return 7;
                case 17223: // Human Female - Skin Color
                    return 8;
                case 17224: // Human Female - Skin Color
                    return 9;
                case 17225: // Human Female - Skin Color
                    return 10;
                case 17226: // Human Female - Skin Color
                    return 11;
                case 17227: // Human Female - Face
                    return 0;
                case 17228: // Human Female - Face
                    return 1;
                case 17229: // Human Female - Face
                    return 2;
                case 17230: // Human Female - Face
                    return 3;
                case 17231: // Human Female - Face
                    return 4;
                case 17232: // Human Female - Face
                    return 5;
                case 17233: // Human Female - Face
                    return 6;
                case 17234: // Human Female - Face
                    return 7;
                case 17235: // Human Female - Face
                    return 8;
                case 17236: // Human Female - Face
                    return 9;
                case 17237: // Human Female - Face
                    return 10;
                case 17238: // Human Female - Face
                    return 11;
                case 17239: // Human Female - Face
                    return 12;
                case 17240: // Human Female - Face
                    return 13;
                case 17241: // Human Female - Face
                    return 14;
                case 17242: // Human Female - Hair Style
                    return 0;
                case 17243: // Human Female - Hair Style
                    return 1;
                case 17244: // Human Female - Hair Style
                    return 2;
                case 17245: // Human Female - Hair Style
                    return 3;
                case 17246: // Human Female - Hair Style
                    return 4;
                case 17247: // Human Female - Hair Style
                    return 5;
                case 17248: // Human Female - Hair Style
                    return 6;
                case 17249: // Human Female - Hair Style
                    return 7;
                case 17250: // Human Female - Hair Style
                    return 8;
                case 17251: // Human Female - Hair Style
                    return 9;
                case 17252: // Human Female - Hair Style
                    return 10;
                case 17253: // Human Female - Hair Style
                    return 11;
                case 17254: // Human Female - Hair Style
                    return 12;
                case 17255: // Human Female - Hair Style
                    return 13;
                case 17256: // Human Female - Hair Style
                    return 14;
                case 17257: // Human Female - Hair Style
                    return 15;
                case 17258: // Human Female - Hair Style
                    return 16;
                case 17259: // Human Female - Hair Style
                    return 17;
                case 17260: // Human Female - Hair Style
                    return 18;
                case 17261: // Human Female - Hair Color
                    return 0;
                case 17262: // Human Female - Hair Color
                    return 1;
                case 17263: // Human Female - Hair Color
                    return 2;
                case 17264: // Human Female - Hair Color
                    return 3;
                case 17265: // Human Female - Hair Color
                    return 4;
                case 17266: // Human Female - Hair Color
                    return 5;
                case 17267: // Human Female - Hair Color
                    return 6;
                case 17268: // Human Female - Hair Color
                    return 7;
                case 17269: // Human Female - Hair Color
                    return 8;
                case 17270: // Human Female - Hair Color
                    return 9;
                case 17271: // Human Female - Piercings
                    return 0;
                case 17272: // Human Female - Piercings
                    return 1;
                case 17273: // Human Female - Piercings
                    return 2;
                case 17274: // Human Female - Piercings
                    return 3;
                case 17275: // Human Female - Piercings
                    return 4;
                case 17276: // Human Female - Piercings
                    return 5;
                case 17277: // Human Female - Piercings
                    return 6;
                case 17278: // Orc Male - Skin Color
                    return 0;
                case 17279: // Orc Male - Skin Color
                    return 1;
                case 17280: // Orc Male - Skin Color
                    return 2;
                case 17281: // Orc Male - Skin Color
                    return 3;
                case 17282: // Orc Male - Skin Color
                    return 4;
                case 17283: // Orc Male - Skin Color
                    return 5;
                case 17284: // Orc Male - Skin Color
                    return 6;
                case 17285: // Orc Male - Skin Color
                    return 7;
                case 17286: // Orc Male - Skin Color
                    return 8;
                case 17287: // Orc Male - Skin Color
                    return 9;
                case 17288: // Orc Male - Skin Color
                    return 10;
                case 17289: // Orc Male - Skin Color
                    return 11;
                case 17290: // Orc Male - Skin Color
                    return 12;
                case 17291: // Orc Male - Skin Color
                    return 13;
                case 17292: // Orc Male - Skin Color
                    return 14;
                case 17293: // Orc Male - Face
                    return 0;
                case 17294: // Orc Male - Face
                    return 1;
                case 17295: // Orc Male - Face
                    return 2;
                case 17296: // Orc Male - Face
                    return 3;
                case 17297: // Orc Male - Face
                    return 4;
                case 17298: // Orc Male - Face
                    return 5;
                case 17299: // Orc Male - Face
                    return 6;
                case 17300: // Orc Male - Face
                    return 7;
                case 17301: // Orc Male - Face
                    return 8;
                case 17302: // Orc Male - Hair Style
                    return 0;
                case 17303: // Orc Male - Hair Style
                    return 1;
                case 17304: // Orc Male - Hair Style
                    return 2;
                case 17305: // Orc Male - Hair Style
                    return 3;
                case 17306: // Orc Male - Hair Style
                    return 4;
                case 17307: // Orc Male - Hair Style
                    return 5;
                case 17308: // Orc Male - Hair Style
                    return 6;
                case 17309: // Orc Male - Hair Color
                    return 0;
                case 17310: // Orc Male - Hair Color
                    return 1;
                case 17311: // Orc Male - Hair Color
                    return 2;
                case 17312: // Orc Male - Hair Color
                    return 3;
                case 17313: // Orc Male - Hair Color
                    return 4;
                case 17314: // Orc Male - Hair Color
                    return 5;
                case 17315: // Orc Male - Hair Color
                    return 6;
                case 17316: // Orc Male - Hair Color
                    return 7;
                case 17317: // Orc Male - Facial Hair
                    return 0;
                case 17318: // Orc Male - Facial Hair
                    return 1;
                case 17319: // Orc Male - Facial Hair
                    return 2;
                case 17320: // Orc Male - Facial Hair
                    return 3;
                case 17321: // Orc Male - Facial Hair
                    return 4;
                case 17322: // Orc Male - Facial Hair
                    return 5;
                case 17323: // Orc Male - Facial Hair
                    return 6;
                case 17324: // Orc Male - Facial Hair
                    return 7;
                case 17325: // Orc Male - Facial Hair
                    return 8;
                case 17326: // Orc Male - Facial Hair
                    return 9;
                case 17327: // Orc Male - Facial Hair
                    return 10;
                case 17328: // Orc Female - Skin Color
                    return 0;
                case 17329: // Orc Female - Skin Color
                    return 1;
                case 17330: // Orc Female - Skin Color
                    return 2;
                case 17331: // Orc Female - Skin Color
                    return 3;
                case 17332: // Orc Female - Skin Color
                    return 4;
                case 17333: // Orc Female - Skin Color
                    return 5;
                case 17334: // Orc Female - Skin Color
                    return 6;
                case 17335: // Orc Female - Skin Color
                    return 7;
                case 17336: // Orc Female - Skin Color
                    return 8;
                case 17337: // Orc Female - Skin Color
                    return 9;
                case 17338: // Orc Female - Skin Color
                    return 10;
                case 17339: // Orc Female - Face
                    return 0;
                case 17340: // Orc Female - Face
                    return 1;
                case 17341: // Orc Female - Face
                    return 2;
                case 17342: // Orc Female - Face
                    return 3;
                case 17343: // Orc Female - Face
                    return 4;
                case 17344: // Orc Female - Face
                    return 5;
                case 17345: // Orc Female - Face
                    return 6;
                case 17346: // Orc Female - Face
                    return 7;
                case 17347: // Orc Female - Face
                    return 8;
                case 17348: // Orc Female - Hair Style
                    return 0;
                case 17349: // Orc Female - Hair Style
                    return 1;
                case 17350: // Orc Female - Hair Style
                    return 2;
                case 17351: // Orc Female - Hair Style
                    return 3;
                case 17352: // Orc Female - Hair Style
                    return 4;
                case 17353: // Orc Female - Hair Style
                    return 5;
                case 17354: // Orc Female - Hair Style
                    return 6;
                case 17355: // Orc Female - Hair Style
                    return 7;
                case 17356: // Orc Female - Hair Color
                    return 0;
                case 17357: // Orc Female - Hair Color
                    return 1;
                case 17358: // Orc Female - Hair Color
                    return 2;
                case 17359: // Orc Female - Hair Color
                    return 3;
                case 17360: // Orc Female - Hair Color
                    return 4;
                case 17361: // Orc Female - Hair Color
                    return 5;
                case 17362: // Orc Female - Hair Color
                    return 6;
                case 17363: // Orc Female - Hair Color
                    return 7;
                case 17364: // Orc Female - Piercings
                    return 0;
                case 17365: // Orc Female - Piercings
                    return 1;
                case 17366: // Orc Female - Piercings
                    return 2;
                case 17367: // Orc Female - Piercings
                    return 3;
                case 17368: // Orc Female - Piercings
                    return 4;
                case 17369: // Orc Female - Piercings
                    return 5;
                case 17370: // Orc Female - Piercings
                    return 6;
                case 17371: // Dwarf Male - Skin Color
                    return 0;
                case 17372: // Dwarf Male - Skin Color
                    return 1;
                case 17373: // Dwarf Male - Skin Color
                    return 2;
                case 17374: // Dwarf Male - Skin Color
                    return 3;
                case 17375: // Dwarf Male - Skin Color
                    return 4;
                case 17376: // Dwarf Male - Skin Color
                    return 5;
                case 17377: // Dwarf Male - Skin Color
                    return 6;
                case 17378: // Dwarf Male - Skin Color
                    return 7;
                case 17379: // Dwarf Male - Skin Color
                    return 8;
                case 17380: // Dwarf Male - Skin Color
                    return 9;
                case 17381: // Dwarf Male - Skin Color
                    return 10;
                case 17382: // Dwarf Male - Skin Color
                    return 11;
                case 17383: // Dwarf Male - Skin Color
                    return 12;
                case 17384: // Dwarf Male - Skin Color
                    return 13;
                case 17385: // Dwarf Male - Skin Color
                    return 14;
                case 17386: // Dwarf Male - Skin Color
                    return 15;
                case 17387: // Dwarf Male - Skin Color
                    return 16;
                case 17388: // Dwarf Male - Skin Color
                    return 17;
                case 17389: // Dwarf Male - Skin Color
                    return 18;
                case 17390: // Dwarf Male - Face
                    return 0;
                case 17391: // Dwarf Male - Face
                    return 1;
                case 17392: // Dwarf Male - Face
                    return 2;
                case 17393: // Dwarf Male - Face
                    return 3;
                case 17394: // Dwarf Male - Face
                    return 4;
                case 17395: // Dwarf Male - Face
                    return 5;
                case 17396: // Dwarf Male - Face
                    return 6;
                case 17397: // Dwarf Male - Face
                    return 7;
                case 17398: // Dwarf Male - Face
                    return 8;
                case 17399: // Dwarf Male - Face
                    return 9;
                case 17400: // Dwarf Male - Hair Style
                    return 0;
                case 17401: // Dwarf Male - Hair Style
                    return 1;
                case 17402: // Dwarf Male - Hair Style
                    return 2;
                case 17403: // Dwarf Male - Hair Style
                    return 3;
                case 17404: // Dwarf Male - Hair Style
                    return 4;
                case 17405: // Dwarf Male - Hair Style
                    return 5;
                case 17406: // Dwarf Male - Hair Style
                    return 6;
                case 17407: // Dwarf Male - Hair Style
                    return 7;
                case 17408: // Dwarf Male - Hair Style
                    return 8;
                case 17409: // Dwarf Male - Hair Style
                    return 9;
                case 17410: // Dwarf Male - Hair Style
                    return 10;
                case 17411: // Dwarf Male - Hair Color
                    return 0;
                case 17412: // Dwarf Male - Hair Color
                    return 1;
                case 17413: // Dwarf Male - Hair Color
                    return 2;
                case 17414: // Dwarf Male - Hair Color
                    return 3;
                case 17415: // Dwarf Male - Hair Color
                    return 4;
                case 17416: // Dwarf Male - Hair Color
                    return 5;
                case 17417: // Dwarf Male - Hair Color
                    return 6;
                case 17418: // Dwarf Male - Hair Color
                    return 7;
                case 17419: // Dwarf Male - Hair Color
                    return 8;
                case 17420: // Dwarf Male - Hair Color
                    return 9;
                case 17421: // Dwarf Male - Facial Hair
                    return 0;
                case 17422: // Dwarf Male - Facial Hair
                    return 1;
                case 17423: // Dwarf Male - Facial Hair
                    return 2;
                case 17424: // Dwarf Male - Facial Hair
                    return 3;
                case 17425: // Dwarf Male - Facial Hair
                    return 4;
                case 17426: // Dwarf Male - Facial Hair
                    return 5;
                case 17427: // Dwarf Male - Facial Hair
                    return 6;
                case 17428: // Dwarf Male - Facial Hair
                    return 7;
                case 17429: // Dwarf Male - Facial Hair
                    return 8;
                case 17430: // Dwarf Male - Facial Hair
                    return 9;
                case 17431: // Dwarf Male - Facial Hair
                    return 10;
                case 17432: // Dwarf Female - Skin Color
                    return 0;
                case 17433: // Dwarf Female - Skin Color
                    return 1;
                case 17434: // Dwarf Female - Skin Color
                    return 2;
                case 17435: // Dwarf Female - Skin Color
                    return 3;
                case 17436: // Dwarf Female - Skin Color
                    return 4;
                case 17437: // Dwarf Female - Skin Color
                    return 5;
                case 17438: // Dwarf Female - Skin Color
                    return 6;
                case 17439: // Dwarf Female - Skin Color
                    return 7;
                case 17440: // Dwarf Female - Skin Color
                    return 8;
                case 17441: // Dwarf Female - Skin Color
                    return 9;
                case 17442: // Dwarf Female - Skin Color
                    return 10;
                case 17443: // Dwarf Female - Face
                    return 0;
                case 17444: // Dwarf Female - Face
                    return 1;
                case 17445: // Dwarf Female - Face
                    return 2;
                case 17446: // Dwarf Female - Face
                    return 3;
                case 17447: // Dwarf Female - Face
                    return 4;
                case 17448: // Dwarf Female - Face
                    return 5;
                case 17449: // Dwarf Female - Face
                    return 6;
                case 17450: // Dwarf Female - Face
                    return 7;
                case 17451: // Dwarf Female - Face
                    return 8;
                case 17452: // Dwarf Female - Face
                    return 9;
                case 17453: // Dwarf Female - Hair Style
                    return 0;
                case 17454: // Dwarf Female - Hair Style
                    return 1;
                case 17455: // Dwarf Female - Hair Style
                    return 2;
                case 17456: // Dwarf Female - Hair Style
                    return 3;
                case 17457: // Dwarf Female - Hair Style
                    return 4;
                case 17458: // Dwarf Female - Hair Style
                    return 5;
                case 17459: // Dwarf Female - Hair Style
                    return 6;
                case 17460: // Dwarf Female - Hair Style
                    return 7;
                case 17461: // Dwarf Female - Hair Style
                    return 8;
                case 17462: // Dwarf Female - Hair Style
                    return 9;
                case 17463: // Dwarf Female - Hair Style
                    return 10;
                case 17464: // Dwarf Female - Hair Style
                    return 11;
                case 17465: // Dwarf Female - Hair Style
                    return 12;
                case 17466: // Dwarf Female - Hair Style
                    return 13;
                case 17467: // Dwarf Female - Hair Color
                    return 0;
                case 17468: // Dwarf Female - Hair Color
                    return 1;
                case 17469: // Dwarf Female - Hair Color
                    return 2;
                case 17470: // Dwarf Female - Hair Color
                    return 3;
                case 17471: // Dwarf Female - Hair Color
                    return 4;
                case 17472: // Dwarf Female - Hair Color
                    return 5;
                case 17473: // Dwarf Female - Hair Color
                    return 6;
                case 17474: // Dwarf Female - Hair Color
                    return 7;
                case 17475: // Dwarf Female - Hair Color
                    return 8;
                case 17476: // Dwarf Female - Hair Color
                    return 9;
                case 17477: // Dwarf Female - Piercings
                    return 0;
                case 17478: // Dwarf Female - Piercings
                    return 1;
                case 17479: // Dwarf Female - Piercings
                    return 2;
                case 17480: // Dwarf Female - Piercings
                    return 3;
                case 17481: // Dwarf Female - Piercings
                    return 4;
                case 17482: // Dwarf Female - Piercings
                    return 5;
                case 17483: // Night Elf Male - Skin Color
                    return 0;
                case 17484: // Night Elf Male - Skin Color
                    return 1;
                case 17485: // Night Elf Male - Skin Color
                    return 2;
                case 17486: // Night Elf Male - Skin Color
                    return 3;
                case 17487: // Night Elf Male - Skin Color
                    return 4;
                case 17488: // Night Elf Male - Skin Color
                    return 5;
                case 17489: // Night Elf Male - Skin Color
                    return 6;
                case 17490: // Night Elf Male - Skin Color
                    return 7;
                case 17491: // Night Elf Male - Skin Color
                    return 8;
                case 17492: // Night Elf Male - Face
                    return 0;
                case 17493: // Night Elf Male - Face
                    return 1;
                case 17494: // Night Elf Male - Face
                    return 2;
                case 17495: // Night Elf Male - Face
                    return 3;
                case 17496: // Night Elf Male - Face
                    return 4;
                case 17497: // Night Elf Male - Face
                    return 5;
                case 17498: // Night Elf Male - Face
                    return 6;
                case 17499: // Night Elf Male - Face
                    return 7;
                case 17500: // Night Elf Male - Face
                    return 8;
                case 17501: // Night Elf Male - Hair Style
                    return 0;
                case 17502: // Night Elf Male - Hair Style
                    return 1;
                case 17503: // Night Elf Male - Hair Style
                    return 2;
                case 17504: // Night Elf Male - Hair Style
                    return 3;
                case 17505: // Night Elf Male - Hair Style
                    return 4;
                case 17506: // Night Elf Male - Hair Style
                    return 5;
                case 17507: // Night Elf Male - Hair Style
                    return 6;
                case 17508: // Night Elf Male - Hair Color
                    return 0;
                case 17509: // Night Elf Male - Hair Color
                    return 1;
                case 17510: // Night Elf Male - Hair Color
                    return 2;
                case 17511: // Night Elf Male - Hair Color
                    return 3;
                case 17512: // Night Elf Male - Hair Color
                    return 4;
                case 17513: // Night Elf Male - Hair Color
                    return 5;
                case 17514: // Night Elf Male - Hair Color
                    return 6;
                case 17515: // Night Elf Male - Hair Color
                    return 7;
                case 17516: // Night Elf Male - Facial Hair
                    return 0;
                case 17517: // Night Elf Male - Facial Hair
                    return 1;
                case 17518: // Night Elf Male - Facial Hair
                    return 2;
                case 17519: // Night Elf Male - Facial Hair
                    return 3;
                case 17520: // Night Elf Male - Facial Hair
                    return 4;
                case 17521: // Night Elf Male - Facial Hair
                    return 5;
                case 17522: // Night Elf Female - Skin Color
                    return 0;
                case 17523: // Night Elf Female - Skin Color
                    return 1;
                case 17524: // Night Elf Female - Skin Color
                    return 2;
                case 17525: // Night Elf Female - Skin Color
                    return 3;
                case 17526: // Night Elf Female - Skin Color
                    return 4;
                case 17527: // Night Elf Female - Skin Color
                    return 5;
                case 17528: // Night Elf Female - Skin Color
                    return 6;
                case 17529: // Night Elf Female - Skin Color
                    return 7;
                case 17530: // Night Elf Female - Skin Color
                    return 8;
                case 17531: // Night Elf Female - Face
                    return 0;
                case 17532: // Night Elf Female - Face
                    return 1;
                case 17533: // Night Elf Female - Face
                    return 2;
                case 17534: // Night Elf Female - Face
                    return 3;
                case 17535: // Night Elf Female - Face
                    return 4;
                case 17536: // Night Elf Female - Face
                    return 5;
                case 17537: // Night Elf Female - Face
                    return 6;
                case 17538: // Night Elf Female - Face
                    return 7;
                case 17539: // Night Elf Female - Face
                    return 8;
                case 17540: // Night Elf Female - Hair Style
                    return 0;
                case 17541: // Night Elf Female - Hair Style
                    return 1;
                case 17542: // Night Elf Female - Hair Style
                    return 2;
                case 17543: // Night Elf Female - Hair Style
                    return 3;
                case 17544: // Night Elf Female - Hair Style
                    return 4;
                case 17545: // Night Elf Female - Hair Style
                    return 5;
                case 17546: // Night Elf Female - Hair Style
                    return 6;
                case 17547: // Night Elf Female - Hair Color
                    return 0;
                case 17548: // Night Elf Female - Hair Color
                    return 1;
                case 17549: // Night Elf Female - Hair Color
                    return 2;
                case 17550: // Night Elf Female - Hair Color
                    return 3;
                case 17551: // Night Elf Female - Hair Color
                    return 4;
                case 17552: // Night Elf Female - Hair Color
                    return 5;
                case 17553: // Night Elf Female - Hair Color
                    return 6;
                case 17554: // Night Elf Female - Hair Color
                    return 7;
                case 17555: // Night Elf Female - Markings
                    return 0;
                case 17556: // Night Elf Female - Markings
                    return 1;
                case 17557: // Night Elf Female - Markings
                    return 2;
                case 17558: // Night Elf Female - Markings
                    return 3;
                case 17559: // Night Elf Female - Markings
                    return 4;
                case 17560: // Night Elf Female - Markings
                    return 5;
                case 17561: // Night Elf Female - Markings
                    return 6;
                case 17562: // Night Elf Female - Markings
                    return 7;
                case 17563: // Night Elf Female - Markings
                    return 8;
                case 17564: // Night Elf Female - Markings
                    return 9;
                case 17565: // Undead Male - Skin Color
                    return 0;
                case 17566: // Undead Male - Skin Color
                    return 1;
                case 17567: // Undead Male - Skin Color
                    return 2;
                case 17568: // Undead Male - Skin Color
                    return 3;
                case 17569: // Undead Male - Skin Color
                    return 4;
                case 17570: // Undead Male - Skin Color
                    return 5;
                case 17571: // Undead Male - Face
                    return 0;
                case 17572: // Undead Male - Face
                    return 1;
                case 17573: // Undead Male - Face
                    return 2;
                case 17574: // Undead Male - Face
                    return 3;
                case 17575: // Undead Male - Face
                    return 4;
                case 17576: // Undead Male - Face
                    return 5;
                case 17577: // Undead Male - Face
                    return 6;
                case 17578: // Undead Male - Face
                    return 7;
                case 17579: // Undead Male - Face
                    return 8;
                case 17580: // Undead Male - Face
                    return 9;
                case 17581: // Undead Male - Hair Style
                    return 0;
                case 17582: // Undead Male - Hair Style
                    return 1;
                case 17583: // Undead Male - Hair Style
                    return 2;
                case 17584: // Undead Male - Hair Style
                    return 3;
                case 17585: // Undead Male - Hair Style
                    return 4;
                case 17586: // Undead Male - Hair Style
                    return 5;
                case 17587: // Undead Male - Hair Style
                    return 6;
                case 17588: // Undead Male - Hair Style
                    return 7;
                case 17589: // Undead Male - Hair Style
                    return 8;
                case 17590: // Undead Male - Hair Style
                    return 9;
                case 17591: // Undead Male - Hair Color
                    return 0;
                case 17592: // Undead Male - Hair Color
                    return 1;
                case 17593: // Undead Male - Hair Color
                    return 2;
                case 17594: // Undead Male - Hair Color
                    return 3;
                case 17595: // Undead Male - Hair Color
                    return 4;
                case 17596: // Undead Male - Hair Color
                    return 5;
                case 17597: // Undead Male - Hair Color
                    return 6;
                case 17598: // Undead Male - Hair Color
                    return 7;
                case 17599: // Undead Male - Hair Color
                    return 8;
                case 17600: // Undead Male - Hair Color
                    return 9;
                case 17601: // Undead Male - Features
                    return 0;
                case 17602: // Undead Male - Features
                    return 1;
                case 17603: // Undead Male - Features
                    return 2;
                case 17604: // Undead Male - Features
                    return 3;
                case 17605: // Undead Male - Features
                    return 4;
                case 17606: // Undead Male - Features
                    return 5;
                case 17607: // Undead Male - Features
                    return 6;
                case 17608: // Undead Male - Features
                    return 7;
                case 17609: // Undead Male - Features
                    return 8;
                case 17610: // Undead Male - Features
                    return 9;
                case 17611: // Undead Male - Features
                    return 10;
                case 17612: // Undead Male - Features
                    return 11;
                case 17613: // Undead Male - Features
                    return 12;
                case 17614: // Undead Male - Features
                    return 13;
                case 17615: // Undead Male - Features
                    return 14;
                case 17616: // Undead Male - Features
                    return 15;
                case 17617: // Undead Male - Features
                    return 16;
                case 17618: // Undead Female - Skin Color
                    return 0;
                case 17619: // Undead Female - Skin Color
                    return 1;
                case 17620: // Undead Female - Skin Color
                    return 2;
                case 17621: // Undead Female - Skin Color
                    return 3;
                case 17622: // Undead Female - Skin Color
                    return 4;
                case 17623: // Undead Female - Skin Color
                    return 5;
                case 17624: // Undead Female - Face
                    return 0;
                case 17625: // Undead Female - Face
                    return 1;
                case 17626: // Undead Female - Face
                    return 2;
                case 17627: // Undead Female - Face
                    return 3;
                case 17628: // Undead Female - Face
                    return 4;
                case 17629: // Undead Female - Face
                    return 5;
                case 17630: // Undead Female - Face
                    return 6;
                case 17631: // Undead Female - Face
                    return 7;
                case 17632: // Undead Female - Face
                    return 8;
                case 17633: // Undead Female - Face
                    return 9;
                case 17634: // Undead Female - Hair Style
                    return 0;
                case 17635: // Undead Female - Hair Style
                    return 1;
                case 17636: // Undead Female - Hair Style
                    return 2;
                case 17637: // Undead Female - Hair Style
                    return 3;
                case 17638: // Undead Female - Hair Style
                    return 4;
                case 17639: // Undead Female - Hair Style
                    return 5;
                case 17640: // Undead Female - Hair Style
                    return 6;
                case 17641: // Undead Female - Hair Style
                    return 7;
                case 17642: // Undead Female - Hair Style
                    return 8;
                case 17643: // Undead Female - Hair Style
                    return 9;
                case 17644: // Undead Female - Hair Color
                    return 0;
                case 17645: // Undead Female - Hair Color
                    return 1;
                case 17646: // Undead Female - Hair Color
                    return 2;
                case 17647: // Undead Female - Hair Color
                    return 3;
                case 17648: // Undead Female - Hair Color
                    return 4;
                case 17649: // Undead Female - Hair Color
                    return 5;
                case 17650: // Undead Female - Hair Color
                    return 6;
                case 17651: // Undead Female - Hair Color
                    return 7;
                case 17652: // Undead Female - Hair Color
                    return 8;
                case 17653: // Undead Female - Hair Color
                    return 9;
                case 17654: // Undead Female - Features
                    return 0;
                case 17655: // Undead Female - Features
                    return 1;
                case 17656: // Undead Female - Features
                    return 2;
                case 17657: // Undead Female - Features
                    return 3;
                case 17658: // Undead Female - Features
                    return 4;
                case 17659: // Undead Female - Features
                    return 5;
                case 17660: // Undead Female - Features
                    return 6;
                case 17661: // Undead Female - Features
                    return 7;
                case 17662: // Tauren Male - Skin Color
                    return 0;
                case 17663: // Tauren Male - Skin Color
                    return 1;
                case 17664: // Tauren Male - Skin Color
                    return 2;
                case 17665: // Tauren Male - Skin Color
                    return 3;
                case 17666: // Tauren Male - Skin Color
                    return 4;
                case 17667: // Tauren Male - Skin Color
                    return 5;
                case 17668: // Tauren Male - Skin Color
                    return 6;
                case 17669: // Tauren Male - Skin Color
                    return 7;
                case 17670: // Tauren Male - Skin Color
                    return 8;
                case 17671: // Tauren Male - Skin Color
                    return 9;
                case 17672: // Tauren Male - Skin Color
                    return 10;
                case 17673: // Tauren Male - Skin Color
                    return 11;
                case 17674: // Tauren Male - Skin Color
                    return 12;
                case 17675: // Tauren Male - Skin Color
                    return 13;
                case 17676: // Tauren Male - Skin Color
                    return 14;
                case 17677: // Tauren Male - Skin Color
                    return 15;
                case 17678: // Tauren Male - Skin Color
                    return 16;
                case 17679: // Tauren Male - Skin Color
                    return 17;
                case 17680: // Tauren Male - Skin Color
                    return 18;
                case 17681: // 378 - Face
                    return 0;
                case 17682: // 378 - Face
                    return 1;
                case 17683: // 378 - Face
                    return 2;
                case 17684: // 378 - Face
                    return 3;
                case 17685: // 378 - Face
                    return 4;
                case 17686: // Tauren Male - Horn Style
                    return 0;
                case 17687: // Tauren Male - Horn Style
                    return 1;
                case 17688: // Tauren Male - Horn Style
                    return 2;
                case 17689: // Tauren Male - Horn Style
                    return 3;
                case 17690: // Tauren Male - Horn Style
                    return 4;
                case 17691: // Tauren Male - Horn Style
                    return 5;
                case 17692: // Tauren Male - Horn Style
                    return 6;
                case 17693: // Tauren Male - Horn Style
                    return 7;
                case 17694: // Tauren Male - Horn Color
                    return 0;
                case 17695: // Tauren Male - Horn Color
                    return 1;
                case 17696: // Tauren Male - Horn Color
                    return 2;
                case 17697: // Tauren Male - Facial Hair
                    return 0;
                case 17698: // Tauren Male - Facial Hair
                    return 1;
                case 17699: // Tauren Male - Facial Hair
                    return 2;
                case 17700: // Tauren Male - Facial Hair
                    return 3;
                case 17701: // Tauren Male - Facial Hair
                    return 4;
                case 17702: // Tauren Male - Facial Hair
                    return 5;
                case 17703: // Tauren Male - Facial Hair
                    return 6;
                case 17704: // Tauren Female - Skin Color
                    return 0;
                case 17705: // Tauren Female - Skin Color
                    return 1;
                case 17706: // Tauren Female - Skin Color
                    return 2;
                case 17707: // Tauren Female - Skin Color
                    return 3;
                case 17708: // Tauren Female - Skin Color
                    return 4;
                case 17709: // Tauren Female - Skin Color
                    return 5;
                case 17710: // Tauren Female - Skin Color
                    return 6;
                case 17711: // Tauren Female - Skin Color
                    return 7;
                case 17712: // Tauren Female - Skin Color
                    return 8;
                case 17713: // Tauren Female - Skin Color
                    return 9;
                case 17714: // Tauren Female - Skin Color
                    return 10;
                case 17715: // 379 - Face
                    return 0;
                case 17716: // 379 - Face
                    return 1;
                case 17717: // 379 - Face
                    return 2;
                case 17718: // 379 - Face
                    return 3;
                case 17719: // Tauren Female - Horn Style
                    return 0;
                case 17720: // Tauren Female - Horn Style
                    return 1;
                case 17721: // Tauren Female - Horn Style
                    return 2;
                case 17722: // Tauren Female - Horn Style
                    return 3;
                case 17723: // Tauren Female - Horn Style
                    return 4;
                case 17724: // Tauren Female - Horn Style
                    return 5;
                case 17725: // Tauren Female - Horn Style
                    return 6;
                case 17726: // Tauren Female - Horn Color
                    return 0;
                case 17727: // Tauren Female - Horn Color
                    return 1;
                case 17728: // Tauren Female - Horn Color
                    return 2;
                case 17729: // Tauren Female - Hair
                    return 0;
                case 17730: // Tauren Female - Hair
                    return 1;
                case 17731: // Tauren Female - Hair
                    return 2;
                case 17732: // Tauren Female - Hair
                    return 3;
                case 17733: // Tauren Female - Hair
                    return 4;
                case 17734: // Gnome Male - Skin Color
                    return 0;
                case 17735: // Gnome Male - Skin Color
                    return 1;
                case 17736: // Gnome Male - Skin Color
                    return 2;
                case 17737: // Gnome Male - Skin Color
                    return 3;
                case 17738: // Gnome Male - Skin Color
                    return 4;
                case 17739: // Gnome Male - Skin Color
                    return 5;
                case 17740: // Gnome Male - Skin Color
                    return 6;
                case 17741: // Gnome Male - Face
                    return 0;
                case 17742: // Gnome Male - Face
                    return 1;
                case 17743: // Gnome Male - Face
                    return 2;
                case 17744: // Gnome Male - Face
                    return 3;
                case 17745: // Gnome Male - Face
                    return 4;
                case 17746: // Gnome Male - Face
                    return 5;
                case 17747: // Gnome Male - Face
                    return 6;
                case 17748: // Gnome Male - Hair Style
                    return 0;
                case 17749: // Gnome Male - Hair Style
                    return 1;
                case 17750: // Gnome Male - Hair Style
                    return 2;
                case 17751: // Gnome Male - Hair Style
                    return 3;
                case 17752: // Gnome Male - Hair Style
                    return 4;
                case 17753: // Gnome Male - Hair Style
                    return 5;
                case 17754: // Gnome Male - Hair Style
                    return 6;
                case 17755: // Gnome Male - Hair Color
                    return 0;
                case 17756: // Gnome Male - Hair Color
                    return 1;
                case 17757: // Gnome Male - Hair Color
                    return 2;
                case 17758: // Gnome Male - Hair Color
                    return 3;
                case 17759: // Gnome Male - Hair Color
                    return 4;
                case 17760: // Gnome Male - Hair Color
                    return 5;
                case 17761: // Gnome Male - Hair Color
                    return 6;
                case 17762: // Gnome Male - Hair Color
                    return 7;
                case 17763: // Gnome Male - Hair Color
                    return 8;
                case 17764: // Gnome Male - Facial Hair
                    return 0;
                case 17765: // Gnome Male - Facial Hair
                    return 1;
                case 17766: // Gnome Male - Facial Hair
                    return 2;
                case 17767: // Gnome Male - Facial Hair
                    return 3;
                case 17768: // Gnome Male - Facial Hair
                    return 4;
                case 17769: // Gnome Male - Facial Hair
                    return 5;
                case 17770: // Gnome Male - Facial Hair
                    return 6;
                case 17771: // Gnome Male - Facial Hair
                    return 7;
                case 17772: // Gnome Female - Skin Color
                    return 0;
                case 17773: // Gnome Female - Skin Color
                    return 1;
                case 17774: // Gnome Female - Skin Color
                    return 2;
                case 17775: // Gnome Female - Skin Color
                    return 3;
                case 17776: // Gnome Female - Skin Color
                    return 4;
                case 17777: // Gnome Female - Skin Color
                    return 5;
                case 17778: // Gnome Female - Skin Color
                    return 6;
                case 17779: // Gnome Female - Face
                    return 0;
                case 17780: // Gnome Female - Face
                    return 1;
                case 17781: // Gnome Female - Face
                    return 2;
                case 17782: // Gnome Female - Face
                    return 3;
                case 17783: // Gnome Female - Face
                    return 4;
                case 17784: // Gnome Female - Face
                    return 5;
                case 17785: // Gnome Female - Face
                    return 6;
                case 17786: // Gnome Female - Hair Style
                    return 0;
                case 17787: // Gnome Female - Hair Style
                    return 1;
                case 17788: // Gnome Female - Hair Style
                    return 2;
                case 17789: // Gnome Female - Hair Style
                    return 3;
                case 17790: // Gnome Female - Hair Style
                    return 4;
                case 17791: // Gnome Female - Hair Style
                    return 5;
                case 17792: // Gnome Female - Hair Style
                    return 6;
                case 17793: // Gnome Female - Hair Color
                    return 0;
                case 17794: // Gnome Female - Hair Color
                    return 1;
                case 17795: // Gnome Female - Hair Color
                    return 2;
                case 17796: // Gnome Female - Hair Color
                    return 3;
                case 17797: // Gnome Female - Hair Color
                    return 4;
                case 17798: // Gnome Female - Hair Color
                    return 5;
                case 17799: // Gnome Female - Hair Color
                    return 6;
                case 17800: // Gnome Female - Hair Color
                    return 7;
                case 17801: // Gnome Female - Hair Color
                    return 8;
                case 17802: // Gnome Female - Earrings
                    return 0;
                case 17803: // Gnome Female - Earrings
                    return 1;
                case 17804: // Gnome Female - Earrings
                    return 2;
                case 17805: // Gnome Female - Earrings
                    return 3;
                case 17806: // Gnome Female - Earrings
                    return 4;
                case 17807: // Gnome Female - Earrings
                    return 5;
                case 17808: // Gnome Female - Earrings
                    return 6;
                case 17809: // Troll Male - Skin Color
                    return 0;
                case 17810: // Troll Male - Skin Color
                    return 1;
                case 17811: // Troll Male - Skin Color
                    return 2;
                case 17812: // Troll Male - Skin Color
                    return 3;
                case 17813: // Troll Male - Skin Color
                    return 4;
                case 17814: // Troll Male - Skin Color
                    return 5;
                case 17815: // Troll Male - Skin Color
                    return 6;
                case 17816: // Troll Male - Skin Color
                    return 7;
                case 17817: // Troll Male - Skin Color
                    return 8;
                case 17818: // Troll Male - Skin Color
                    return 9;
                case 17819: // Troll Male - Skin Color
                    return 10;
                case 17820: // Troll Male - Skin Color
                    return 11;
                case 17821: // Troll Male - Skin Color
                    return 12;
                case 17822: // Troll Male - Skin Color
                    return 13;
                case 17823: // Troll Male - Skin Color
                    return 14;
                case 17824: // Troll Male - Face
                    return 0;
                case 17825: // Troll Male - Face
                    return 1;
                case 17826: // Troll Male - Face
                    return 2;
                case 17827: // Troll Male - Face
                    return 3;
                case 17828: // Troll Male - Face
                    return 4;
                case 17829: // Troll Male - Hair Style
                    return 0;
                case 17830: // Troll Male - Hair Style
                    return 1;
                case 17831: // Troll Male - Hair Style
                    return 2;
                case 17832: // Troll Male - Hair Style
                    return 3;
                case 17833: // Troll Male - Hair Style
                    return 4;
                case 17834: // Troll Male - Hair Style
                    return 5;
                case 17835: // Troll Male - Hair Color
                    return 0;
                case 17836: // Troll Male - Hair Color
                    return 1;
                case 17837: // Troll Male - Hair Color
                    return 2;
                case 17838: // Troll Male - Hair Color
                    return 3;
                case 17839: // Troll Male - Hair Color
                    return 4;
                case 17840: // Troll Male - Hair Color
                    return 5;
                case 17841: // Troll Male - Hair Color
                    return 6;
                case 17842: // Troll Male - Hair Color
                    return 7;
                case 17843: // Troll Male - Hair Color
                    return 8;
                case 17844: // Troll Male - Hair Color
                    return 9;
                case 17845: // Troll Male - Tusks
                    return 0;
                case 17846: // Troll Male - Tusks
                    return 1;
                case 17847: // Troll Male - Tusks
                    return 2;
                case 17848: // Troll Male - Tusks
                    return 3;
                case 17849: // Troll Male - Tusks
                    return 4;
                case 17850: // Troll Male - Tusks
                    return 5;
                case 17851: // Troll Male - Tusks
                    return 6;
                case 17852: // Troll Male - Tusks
                    return 7;
                case 17853: // Troll Male - Tusks
                    return 8;
                case 17854: // Troll Male - Tusks
                    return 9;
                case 17855: // Troll Male - Tusks
                    return 10;
                case 17856: // Troll Female - Skin Color
                    return 0;
                case 17857: // Troll Female - Skin Color
                    return 1;
                case 17858: // Troll Female - Skin Color
                    return 2;
                case 17859: // Troll Female - Skin Color
                    return 3;
                case 17860: // Troll Female - Skin Color
                    return 4;
                case 17861: // Troll Female - Skin Color
                    return 5;
                case 17862: // Troll Female - Skin Color
                    return 6;
                case 17863: // Troll Female - Skin Color
                    return 7;
                case 17864: // Troll Female - Skin Color
                    return 8;
                case 17865: // Troll Female - Skin Color
                    return 9;
                case 17866: // Troll Female - Skin Color
                    return 10;
                case 17867: // Troll Female - Skin Color
                    return 11;
                case 17868: // Troll Female - Skin Color
                    return 12;
                case 17869: // Troll Female - Skin Color
                    return 13;
                case 17870: // Troll Female - Skin Color
                    return 14;
                case 17871: // Troll Female - Face
                    return 0;
                case 17872: // Troll Female - Face
                    return 1;
                case 17873: // Troll Female - Face
                    return 2;
                case 17874: // Troll Female - Face
                    return 3;
                case 17875: // Troll Female - Face
                    return 4;
                case 17876: // Troll Female - Face
                    return 5;
                case 17877: // Troll Female - Hair Style
                    return 0;
                case 17878: // Troll Female - Hair Style
                    return 1;
                case 17879: // Troll Female - Hair Style
                    return 2;
                case 17880: // Troll Female - Hair Style
                    return 3;
                case 17881: // Troll Female - Hair Style
                    return 4;
                case 17882: // Troll Female - Hair Color
                    return 0;
                case 17883: // Troll Female - Hair Color
                    return 1;
                case 17884: // Troll Female - Hair Color
                    return 2;
                case 17885: // Troll Female - Hair Color
                    return 3;
                case 17886: // Troll Female - Hair Color
                    return 4;
                case 17887: // Troll Female - Hair Color
                    return 5;
                case 17888: // Troll Female - Hair Color
                    return 6;
                case 17889: // Troll Female - Hair Color
                    return 7;
                case 17890: // Troll Female - Hair Color
                    return 8;
                case 17891: // Troll Female - Hair Color
                    return 9;
                case 17892: // Troll Female - Tusks
                    return 0;
                case 17893: // Troll Female - Tusks
                    return 1;
                case 17894: // Troll Female - Tusks
                    return 2;
                case 17895: // Troll Female - Tusks
                    return 3;
                case 17896: // Troll Female - Tusks
                    return 4;
                case 17897: // Troll Female - Tusks
                    return 5;
                case 17898: // Goblin Male - Skin Color
                    return 0;
                case 17899: // Goblin Male - Skin Color
                    return 1;
                case 17900: // Goblin Male - Skin Color
                    return 2;
                case 17901: // Goblin Male - Hair Style
                    return 0;
                case 17902: // Goblin Male - Hair Style
                    return 1;
                case 17903: // Goblin Female - Skin Color
                    return 0;
                case 17904: // Goblin Female - Skin Color
                    return 1;
                case 17905: // Goblin Female - Skin Color
                    return 2;
                case 17906: // Blood Elf Male - Skin Color
                    return 0;
                case 17907: // Blood Elf Male - Skin Color
                    return 1;
                case 17908: // Blood Elf Male - Skin Color
                    return 2;
                case 17909: // Blood Elf Male - Skin Color
                    return 3;
                case 17910: // Blood Elf Male - Skin Color
                    return 4;
                case 17911: // Blood Elf Male - Skin Color
                    return 5;
                case 17912: // Blood Elf Male - Skin Color
                    return 6;
                case 17913: // Blood Elf Male - Skin Color
                    return 7;
                case 17914: // Blood Elf Male - Skin Color
                    return 8;
                case 17915: // Blood Elf Male - Skin Color
                    return 9;
                case 17916: // Blood Elf Male - Skin Color
                    return 10;
                case 17917: // Blood Elf Male - Skin Color
                    return 11;
                case 17918: // Blood Elf Male - Skin Color
                    return 12;
                case 17919: // Blood Elf Male - Skin Color
                    return 13;
                case 17920: // Blood Elf Male - Skin Color
                    return 14;
                case 17921: // Blood Elf Male - Skin Color
                    return 15;
                case 17922: // Blood Elf Male - Face
                    return 0;
                case 17923: // Blood Elf Male - Face
                    return 1;
                case 17924: // Blood Elf Male - Face
                    return 2;
                case 17925: // Blood Elf Male - Face
                    return 3;
                case 17926: // Blood Elf Male - Face
                    return 4;
                case 17927: // Blood Elf Male - Face
                    return 5;
                case 17928: // Blood Elf Male - Face
                    return 6;
                case 17929: // Blood Elf Male - Face
                    return 7;
                case 17930: // Blood Elf Male - Face
                    return 8;
                case 17931: // Blood Elf Male - Face
                    return 9;
                case 17932: // Blood Elf Male - Hair Style
                    return 0;
                case 17933: // Blood Elf Male - Hair Style
                    return 1;
                case 17934: // Blood Elf Male - Hair Style
                    return 2;
                case 17935: // Blood Elf Male - Hair Style
                    return 3;
                case 17936: // Blood Elf Male - Hair Style
                    return 4;
                case 17937: // Blood Elf Male - Hair Style
                    return 5;
                case 17938: // Blood Elf Male - Hair Style
                    return 6;
                case 17939: // Blood Elf Male - Hair Style
                    return 7;
                case 17940: // Blood Elf Male - Hair Style
                    return 8;
                case 17941: // Blood Elf Male - Hair Style
                    return 9;
                case 17942: // Blood Elf Male - Hair Style
                    return 10;
                case 17943: // Blood Elf Male - Hair Color
                    return 0;
                case 17944: // Blood Elf Male - Hair Color
                    return 1;
                case 17945: // Blood Elf Male - Hair Color
                    return 2;
                case 17946: // Blood Elf Male - Hair Color
                    return 3;
                case 17947: // Blood Elf Male - Hair Color
                    return 4;
                case 17948: // Blood Elf Male - Hair Color
                    return 5;
                case 17949: // Blood Elf Male - Hair Color
                    return 6;
                case 17950: // Blood Elf Male - Hair Color
                    return 7;
                case 17951: // Blood Elf Male - Hair Color
                    return 8;
                case 17952: // Blood Elf Male - Hair Color
                    return 9;
                case 17953: // Blood Elf Male - Facial Hair
                    return 0;
                case 17954: // Blood Elf Male - Facial Hair
                    return 1;
                case 17955: // Blood Elf Male - Facial Hair
                    return 2;
                case 17956: // Blood Elf Male - Facial Hair
                    return 3;
                case 17957: // Blood Elf Male - Facial Hair
                    return 4;
                case 17958: // Blood Elf Male - Facial Hair
                    return 5;
                case 17959: // Blood Elf Male - Facial Hair
                    return 6;
                case 17960: // Blood Elf Male - Facial Hair
                    return 7;
                case 17961: // Blood Elf Male - Facial Hair
                    return 8;
                case 17962: // Blood Elf Male - Facial Hair
                    return 9;
                case 17963: // Blood Elf Female - Skin Color
                    return 0;
                case 17964: // Blood Elf Female - Skin Color
                    return 1;
                case 17965: // Blood Elf Female - Skin Color
                    return 2;
                case 17966: // Blood Elf Female - Skin Color
                    return 3;
                case 17967: // Blood Elf Female - Skin Color
                    return 4;
                case 17968: // Blood Elf Female - Skin Color
                    return 5;
                case 17969: // Blood Elf Female - Skin Color
                    return 6;
                case 17970: // Blood Elf Female - Skin Color
                    return 7;
                case 17971: // Blood Elf Female - Skin Color
                    return 8;
                case 17972: // Blood Elf Female - Skin Color
                    return 9;
                case 17973: // Blood Elf Female - Skin Color
                    return 10;
                case 17974: // Blood Elf Female - Skin Color
                    return 11;
                case 17975: // Blood Elf Female - Skin Color
                    return 12;
                case 17976: // Blood Elf Female - Skin Color
                    return 13;
                case 17977: // Blood Elf Female - Skin Color
                    return 14;
                case 17978: // Blood Elf Female - Skin Color
                    return 15;
                case 17979: // Blood Elf Female - Face
                    return 0;
                case 17980: // Blood Elf Female - Face
                    return 1;
                case 17981: // Blood Elf Female - Face
                    return 2;
                case 17982: // Blood Elf Female - Face
                    return 3;
                case 17983: // Blood Elf Female - Face
                    return 4;
                case 17984: // Blood Elf Female - Face
                    return 5;
                case 17985: // Blood Elf Female - Face
                    return 6;
                case 17986: // Blood Elf Female - Face
                    return 7;
                case 17987: // Blood Elf Female - Face
                    return 8;
                case 17988: // Blood Elf Female - Face
                    return 9;
                case 17989: // Blood Elf Female - Hair Style
                    return 0;
                case 17990: // Blood Elf Female - Hair Style
                    return 1;
                case 17991: // Blood Elf Female - Hair Style
                    return 2;
                case 17992: // Blood Elf Female - Hair Style
                    return 3;
                case 17993: // Blood Elf Female - Hair Style
                    return 4;
                case 17994: // Blood Elf Female - Hair Style
                    return 5;
                case 17995: // Blood Elf Female - Hair Style
                    return 6;
                case 17996: // Blood Elf Female - Hair Style
                    return 7;
                case 17997: // Blood Elf Female - Hair Style
                    return 8;
                case 17998: // Blood Elf Female - Hair Style
                    return 9;
                case 17999: // Blood Elf Female - Hair Style
                    return 10;
                case 18000: // Blood Elf Female - Hair Style
                    return 11;
                case 18001: // Blood Elf Female - Hair Style
                    return 12;
                case 18002: // Blood Elf Female - Hair Style
                    return 13;
                case 18004: // Blood Elf Female - Hair Color
                    return 0;
                case 18005: // Blood Elf Female - Hair Color
                    return 1;
                case 18006: // Blood Elf Female - Hair Color
                    return 2;
                case 18007: // Blood Elf Female - Hair Color
                    return 3;
                case 18008: // Blood Elf Female - Hair Color
                    return 4;
                case 18009: // Blood Elf Female - Hair Color
                    return 5;
                case 18010: // Blood Elf Female - Hair Color
                    return 6;
                case 18011: // Blood Elf Female - Hair Color
                    return 7;
                case 18012: // Blood Elf Female - Hair Color
                    return 8;
                case 18013: // Blood Elf Female - Hair Color
                    return 9;
                case 18014: // Blood Elf Female - Earrings
                    return 0;
                case 18015: // Blood Elf Female - Earrings
                    return 1;
                case 18016: // Blood Elf Female - Earrings
                    return 2;
                case 18017: // Blood Elf Female - Earrings
                    return 3;
                case 18018: // Blood Elf Female - Earrings
                    return 4;
                case 18019: // Blood Elf Female - Earrings
                    return 5;
                case 18020: // Blood Elf Female - Earrings
                    return 6;
                case 18021: // Blood Elf Female - Earrings
                    return 7;
                case 18022: // Blood Elf Female - Earrings
                    return 8;
                case 18023: // Blood Elf Female - Earrings
                    return 9;
                case 18024: // Blood Elf Female - Earrings
                    return 10;
                case 18025: // Draenei Male - Skin Color
                    return 0;
                case 18026: // Draenei Male - Skin Color
                    return 1;
                case 18027: // Draenei Male - Skin Color
                    return 2;
                case 18028: // Draenei Male - Skin Color
                    return 3;
                case 18029: // Draenei Male - Skin Color
                    return 4;
                case 18030: // Draenei Male - Skin Color
                    return 5;
                case 18031: // Draenei Male - Skin Color
                    return 6;
                case 18032: // Draenei Male - Skin Color
                    return 7;
                case 18033: // Draenei Male - Skin Color
                    return 8;
                case 18034: // Draenei Male - Skin Color
                    return 9;
                case 18035: // Draenei Male - Skin Color
                    return 10;
                case 18036: // Draenei Male - Skin Color
                    return 11;
                case 18037: // Draenei Male - Skin Color
                    return 12;
                case 18038: // Draenei Male - Skin Color
                    return 13;
                case 18039: // Draenei Male - Face
                    return 0;
                case 18040: // Draenei Male - Face
                    return 1;
                case 18041: // Draenei Male - Face
                    return 2;
                case 18042: // Draenei Male - Face
                    return 3;
                case 18043: // Draenei Male - Face
                    return 4;
                case 18044: // Draenei Male - Face
                    return 5;
                case 18045: // Draenei Male - Face
                    return 6;
                case 18046: // Draenei Male - Face
                    return 7;
                case 18047: // Draenei Male - Face
                    return 8;
                case 18048: // Draenei Male - Face
                    return 9;
                case 18049: // Draenei Male - Hair Style
                    return 0;
                case 18050: // Draenei Male - Hair Style
                    return 1;
                case 18051: // Draenei Male - Hair Style
                    return 2;
                case 18052: // Draenei Male - Hair Style
                    return 3;
                case 18053: // Draenei Male - Hair Style
                    return 4;
                case 18054: // Draenei Male - Hair Style
                    return 5;
                case 18055: // Draenei Male - Hair Style
                    return 6;
                case 18056: // Draenei Male - Hair Style
                    return 7;
                case 18057: // Draenei Male - Hair Style
                    return 8;
                case 18058: // Draenei Male - Hair Color
                    return 0;
                case 18059: // Draenei Male - Hair Color
                    return 1;
                case 18060: // Draenei Male - Hair Color
                    return 2;
                case 18061: // Draenei Male - Hair Color
                    return 3;
                case 18062: // Draenei Male - Hair Color
                    return 4;
                case 18063: // Draenei Male - Hair Color
                    return 5;
                case 18064: // Draenei Male - Hair Color
                    return 6;
                case 18065: // Draenei Male - Facial Hair
                    return 0;
                case 18066: // Draenei Male - Facial Hair
                    return 1;
                case 18067: // Draenei Male - Facial Hair
                    return 2;
                case 18068: // Draenei Male - Facial Hair
                    return 3;
                case 18069: // Draenei Male - Facial Hair
                    return 4;
                case 18070: // Draenei Male - Facial Hair
                    return 5;
                case 18071: // Draenei Male - Facial Hair
                    return 6;
                case 18072: // Draenei Male - Facial Hair
                    return 7;
                case 18073: // Draenei Female - Skin Color
                    return 0;
                case 18074: // Draenei Female - Skin Color
                    return 1;
                case 18075: // Draenei Female - Skin Color
                    return 2;
                case 18076: // Draenei Female - Skin Color
                    return 3;
                case 18077: // Draenei Female - Skin Color
                    return 4;
                case 18078: // Draenei Female - Skin Color
                    return 5;
                case 18079: // Draenei Female - Skin Color
                    return 6;
                case 18080: // Draenei Female - Skin Color
                    return 7;
                case 18081: // Draenei Female - Skin Color
                    return 8;
                case 18082: // Draenei Female - Skin Color
                    return 9;
                case 18083: // Draenei Female - Skin Color
                    return 10;
                case 18084: // Draenei Female - Skin Color
                    return 11;
                case 18085: // Draenei Female - Face
                    return 0;
                case 18086: // Draenei Female - Face
                    return 1;
                case 18087: // Draenei Female - Face
                    return 2;
                case 18088: // Draenei Female - Face
                    return 3;
                case 18089: // Draenei Female - Face
                    return 4;
                case 18090: // Draenei Female - Face
                    return 5;
                case 18091: // Draenei Female - Face
                    return 6;
                case 18092: // Draenei Female - Face
                    return 7;
                case 18093: // Draenei Female - Face
                    return 8;
                case 18094: // Draenei Female - Face
                    return 9;
                case 18095: // Draenei Female - Hair Style
                    return 0;
                case 18096: // Draenei Female - Hair Style
                    return 1;
                case 18097: // Draenei Female - Hair Style
                    return 2;
                case 18098: // Draenei Female - Hair Style
                    return 3;
                case 18099: // Draenei Female - Hair Style
                    return 4;
                case 18100: // Draenei Female - Hair Style
                    return 5;
                case 18101: // Draenei Female - Hair Style
                    return 6;
                case 18102: // Draenei Female - Hair Style
                    return 7;
                case 18103: // Draenei Female - Hair Style
                    return 8;
                case 18104: // Draenei Female - Hair Style
                    return 9;
                case 18105: // Draenei Female - Hair Style
                    return 10;
                case 18106: // Draenei Female - Hair Color
                    return 0;
                case 18107: // Draenei Female - Hair Color
                    return 1;
                case 18108: // Draenei Female - Hair Color
                    return 2;
                case 18109: // Draenei Female - Hair Color
                    return 3;
                case 18110: // Draenei Female - Hair Color
                    return 4;
                case 18111: // Draenei Female - Hair Color
                    return 5;
                case 18112: // Draenei Female - Hair Color
                    return 6;
                case 18113: // Draenei Female - Horn Style
                    return 0;
                case 18114: // Draenei Female - Horn Style
                    return 1;
                case 18115: // Draenei Female - Horn Style
                    return 2;
                case 18116: // Draenei Female - Horn Style
                    return 3;
                case 18117: // Draenei Female - Horn Style
                    return 4;
                case 18118: // Draenei Female - Horn Style
                    return 5;
                case 18119: // Draenei Female - Horn Style
                    return 6;
                case 18120: // Fel Orc Male - Skin Color
                    return 0;
                case 18121: // Fel Orc Male - Skin Color
                    return 1;
                case 18122: // Fel Orc Male - Skin Color
                    return 2;
                case 18123: // Fel Orc Male - Face
                    return 0;
                case 18124: // Fel Orc Male - Hair Style
                    return 0;
                case 18125: // Fel Orc Male - Hair Color
                    return 0;
                case 18126: // Fel Orc Male - Facial Hair
                    return 0;
                case 18127: // Fel Orc Female - Hair Style
                    return 0;
                case 18128: // Fel Orc Female - Hair Color
                    return 0;
                case 18129: // Fel Orc Female - Facial Hair
                    return 0;
                case 18130: // Naga Male - Skin Color
                    return 0;
                case 18131: // Naga Male - Skin Color
                    return 1;
                case 18132: // Naga Male - Skin Color
                    return 2;
                case 18133: // Naga Male - Skin Color
                    return 3;
                case 18134: // Naga Male - Skin Color
                    return 4;
                case 18135: // Naga Male - Face
                    return 0;
                case 18136: // Naga Male - Hair Style
                    return 0;
                case 18137: // Naga Male - Hair Color
                    return 0;
                case 18138: // Naga Male - Facial Hair
                    return 0;
                case 18139: // Naga Female - Skin Color
                    return 0;
                case 18140: // Naga Female - Skin Color
                    return 1;
                case 18141: // Naga Female - Skin Color
                    return 2;
                case 18142: // Naga Female - Skin Color
                    return 3;
                case 18143: // Naga Female - Skin Color
                    return 4;
                case 18144: // Naga Female - Face
                    return 0;
                case 18145: // Naga Female - Hair Style
                    return 0;
                case 18146: // Naga Female - Hair Color
                    return 0;
                case 18147: // Naga Female - Facial Hair
                    return 0;
                case 18148: // Broken Male - Skin Color
                    return 0;
                case 18149: // Broken Male - Skin Color
                    return 1;
                case 18150: // Broken Male - Skin Color
                    return 2;
                case 18151: // Broken Male - Skin Color
                    return 3;
                case 18152: // Broken Male - Skin Color
                    return 4;
                case 18153: // Broken Male - Skin Color
                    return 5;
                case 18154: // Broken Male - Face
                    return 0;
                case 18155: // Broken Male - Hair Style
                    return 0;
                case 18156: // Broken Male - Hair Style
                    return 1;
                case 18157: // Broken Male - Hair Style
                    return 2;
                case 18158: // Broken Male - Hair Color
                    return 0;
                case 18159: // Broken Male - Hair Color
                    return 1;
                case 18160: // Broken Male - Hair Color
                    return 2;
                case 18161: // Broken Male - Hair Color
                    return 3;
                case 18162: // Broken Male - Hair Color
                    return 4;
                case 18163: // Broken Male - Hair Color
                    return 5;
                case 18164: // Broken Male - Hair Color
                    return 6;
                case 18165: // Broken Male - Hair Color
                    return 7;
                case 18166: // Broken Male - Hair Color
                    return 8;
                case 18167: // Broken Male - Hair Color
                    return 9;
                case 18168: // Broken Female - Hair Style
                    return 0;
                case 18169: // Broken Female - Hair Color
                    return 0;
                case 18170: // Broken Female - Facial Hair
                    return 0;
                case 18171: // Skeleton Male - Skin Color
                    return 0;
                case 18172: // Skeleton Male - Skin Color
                    return 1;
                case 18173: // Skeleton Male - Skin Color
                    return 2;
                case 18174: // Skeleton Male - Skin Color
                    return 3;
                case 18175: // Skeleton Male - Skin Color
                    return 4;
                case 18176: // Skeleton Male - Skin Color
                    return 5;
                case 18177: // Skeleton Male - Face
                    return 0;
                case 18178: // Skeleton Male - Hair Style
                    return 0;
                case 18179: // Skeleton Male - Hair Color
                    return 0;
                case 18180: // Skeleton Male - Facial Hair
                    return 0;
                case 18181: // Skeleton Female - Hair Style
                    return 0;
                case 18182: // Skeleton Female - Hair Color
                    return 0;
                case 18183: // Skeleton Female - Facial Hair
                    return 0;
                case 18184: // Forest Troll Male - Skin Color
                    return 0;
                case 18185: // Forest Troll Male - Skin Color
                    return 1;
                case 18186: // Forest Troll Male - Skin Color
                    return 2;
                case 18187: // Forest Troll Male - Skin Color
                    return 3;
                case 18188: // Forest Troll Male - Skin Color
                    return 4;
                case 18189: // Forest Troll Male - Skin Color
                    return 5;
                case 18190: // Forest Troll Male - Face
                    return 0;
                case 18191: // Forest Troll Male - Face
                    return 1;
                case 18192: // Forest Troll Male - Face
                    return 2;
                case 18193: // Forest Troll Male - Face
                    return 3;
                case 18194: // Forest Troll Male - Face
                    return 4;
                case 18195: // Forest Troll Male - Hair Style
                    return 0;
                case 18196: // Forest Troll Male - Hair Style
                    return 1;
                case 18197: // Forest Troll Male - Hair Style
                    return 2;
                case 18198: // Forest Troll Male - Hair Style
                    return 3;
                case 18199: // Forest Troll Male - Hair Style
                    return 4;
                case 18200: // Forest Troll Male - Hair Style
                    return 5;
                case 18201: // Forest Troll Male - Hair Color
                    return 0;
                case 18202: // Forest Troll Male - Hair Color
                    return 1;
                case 18203: // Forest Troll Male - Hair Color
                    return 2;
                case 18204: // Forest Troll Male - Hair Color
                    return 3;
                case 18205: // Forest Troll Male - Hair Color
                    return 4;
                case 18206: // Forest Troll Male - Hair Color
                    return 5;
                case 18207: // Forest Troll Male - Hair Color
                    return 6;
                case 18208: // Forest Troll Male - Hair Color
                    return 7;
                case 18209: // Forest Troll Male - Hair Color
                    return 8;
                case 18210: // Forest Troll Male - Hair Color
                    return 9;
                case 18211: // Forest Troll Male - Facial Hair
                    return 0;
                case 18212: // Forest Troll Male - Facial Hair
                    return 1;
                case 18213: // Forest Troll Male - Facial Hair
                    return 2;
                case 18214: // Forest Troll Male - Facial Hair
                    return 3;
                case 18215: // Forest Troll Male - Facial Hair
                    return 4;
                case 18216: // Forest Troll Male - Facial Hair
                    return 5;
                case 18217: // Forest Troll Male - Facial Hair
                    return 6;
                case 18218: // Forest Troll Male - Facial Hair
                    return 7;
                case 18219: // Forest Troll Male - Facial Hair
                    return 8;
                case 18220: // Forest Troll Male - Facial Hair
                    return 9;
                case 18221: // Forest Troll Male - Facial Hair
                    return 10;
                case 18222: // Forest Troll Female - Hair Style
                    return 0;
                case 18223: // Forest Troll Female - Hair Color
                    return 0;
                case 18224: // Forest Troll Female - Facial Hair
                    return 0;
            }
            return 0;
        }

        public static void ConvertModernCustomizationsToLegacy(List<ChrCustomizationChoice> customizations, out byte skin, out byte face, out byte hairStyle, out byte hairColor, out byte facialHair)
        {
            skin = 0;
            face = 0;
            hairStyle = 0;
            hairColor = 0;
            facialHair = 0;
            foreach (var custom in customizations)
            {
                LegacyCustomizationOption option = GetLegacyCustomizationOption(custom.ChrCustomizationOptionID);
                byte choice = GetLegacyCustomizationChoice(custom.ChrCustomizationChoiceID);

                switch (option)
                {
                    case LegacyCustomizationOption.Skin:
                        {
                            skin = choice;
                            break;
                        }
                    case LegacyCustomizationOption.Face:
                        {
                            face = choice;
                            break;
                        }
                    case LegacyCustomizationOption.HairStyle:
                        {
                            hairStyle = choice;
                            break;
                        }
                    case LegacyCustomizationOption.HairColor:
                        {
                            hairColor = choice;
                            break;
                        }
                    case LegacyCustomizationOption.FacialHair:
                        {
                            facialHair = choice;
                            break;
                        }
                }
            }
        }

        public static uint GetModernCustomizationOption(Race raceId, Gender gender, LegacyCustomizationOption option)
        {
            switch (raceId)
            {
                case Race.Human:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 9; // Human Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 10; // Human Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 11; // Human Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 12; // Human Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 13; // Human Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 14; // Human Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 15; // Human Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 16; // Human Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 17; // Human Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 18; // Human Female - Piercings
                        }
                    }
                    return 0;
                }
                case Race.Orc:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 19; // Orc Male - Skin Color
                            case LegacyCustomizationOption.Face: 
                                return 20; // Orc Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 21; // Orc Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 22; // Orc Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 23; // Orc Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 25; // Orc Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 26; // Orc Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 27; // Orc Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 28; // Orc Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 29; // Orc Female - Piercings
                        }
                    }
                    return 0;
                }
                case Race.Dwarf:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 30; // Dwarf Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 31; // Dwarf Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 32; // Dwarf Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 33; // Dwarf Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 34; // Dwarf Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 35; // Dwarf Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 36; // Dwarf Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 37; // Dwarf Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 38; // Dwarf Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 39; // Dwarf Female - Piercings
                        }
                    }
                    return 0;
                } 
                case Race.NightElf:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 40; // Night Elf Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 41; // Night Elf Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 42; // Night Elf Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 43; // Night Elf Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 44; // Night Elf Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 49; // Night Elf Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 50; // Night Elf Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 51; // Night Elf Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 52; // Night Elf Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 53; // Night Elf Female - Markings
                        }
                    }
                    return 0;
                } 
                case Race.Undead:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 58; // Undead Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 59; // Undead Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 60; // Undead Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 61; // Undead Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 62; // Undead Male - Features
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 63; // Undead Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 64; // Undead Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 65; // Undead Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 66; // Undead Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 67; // Undead Female - Features
                        }
                    }
                    return 0;
                }
                case Race.Tauren:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 68; // Tauren Male - Skin Color
                            case LegacyCustomizationOption.HairStyle:
                                return 71; // Tauren Male - Horn Style
                            case LegacyCustomizationOption.HairColor:
                                return 72; // Tauren Male - Horn Color
                            case LegacyCustomizationOption.FacialHair:
                                return 73; // Tauren Male - Facial Hair
                            case LegacyCustomizationOption.Face:
                                return 378; // Tauren Male - Face
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 74; // Tauren Female - Skin Color
                            case LegacyCustomizationOption.HairStyle:
                                return 77; // Tauren Female - Horn Style
                            case LegacyCustomizationOption.HairColor:
                                return 78; // Tauren Female - Horn Color
                            case LegacyCustomizationOption.FacialHair:
                                return 79; // Tauren Female - Hair
                            case LegacyCustomizationOption.Face:
                                return 379; // Tauren Female - Face
                        }
                    }
                    return 0;
                } 
                case Race.Gnome:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 80; // Gnome Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 81; // Gnome Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 82; // Gnome Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 83; // Gnome Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 84; // Gnome Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 85; // Gnome Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 86; // Gnome Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 87; // Gnome Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 88; // Gnome Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 89; // Gnome Female - Earrings
                        }
                    }
                    return 0;
                }
                case Race.Troll:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 90; // Troll Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 91; // Troll Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 92; // Troll Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 93; // Troll Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 94; // Troll Male - Tusks
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 95; // Troll Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 96; // Troll Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 97; // Troll Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 98; // Troll Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 99; // Troll Female - Tusks
                        }
                    }
                    return 0;
                } 
                case Race.Goblin:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 100; // Goblin Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 102; // Goblin Male - Hair Style
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 105; // Goblin Female - Skin Color
                        }
                    }
                    return 0;
                }
                case Race.BloodElf:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 110; // Blood Elf Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 111; // Blood Elf Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 112; // Blood Elf Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 113; // Blood Elf Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 114; // Blood Elf Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 119; // Blood Elf Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 120; // Blood Elf Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 121; // Blood Elf Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 122; // Blood Elf Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 123; // Blood Elf Female - Earrings
                        }
                    }
                    return 0;
                } 
                case Race.Draenei:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 128; // Draenei Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 129; // Draenei Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 130; // Draenei Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 131; // Draenei Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 132; // Draenei Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 133; // Draenei Female - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 134; // Draenei Female - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 135; // Draenei Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 136; // Draenei Female - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 137; // Draenei Female - Horn Style
                        }
                    }
                    return 0;
                } 
                case Race.FelOrc:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 138; // Fel Orc Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 139; // Fel Orc Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 140; // Fel Orc Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 141; // Fel Orc Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 1000; // Fel Orc Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 142; // Fel Orc Female - Hair Style
                            case LegacyCustomizationOption.Face:
                                return 143; // Fel Orc Female - Hair Color
                            case LegacyCustomizationOption.HairStyle:
                                return 1001; // Fel Orc Female - Facial Hair
                        }
                    }
                    return 0;
                }
                case Race.Naga:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 144; // Naga Male - Skin Color
                            case LegacyCustomizationOption.HairStyle:
                                return 145; // Naga Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 146; // Naga Male - Hair Color
                            case LegacyCustomizationOption.Face:
                                return 1002; // Naga Male - Face
                            case LegacyCustomizationOption.FacialHair:
                                return 1003; // Naga Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 147; // Naga Female - Skin Color
                            case LegacyCustomizationOption.HairStyle:
                                return 148; // Naga Female - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 149; // Naga Female - Hair Color
                            case LegacyCustomizationOption.Face:
                                return 1004; // Naga Female - Face
                            case LegacyCustomizationOption.FacialHair:
                                return 1005; // Naga Female - Facial Hair
                        }
                    }
                    return 0;
                }
                case Race.Broken:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 150; // Broken Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 151; // Broken Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 152; // Broken Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 153; // Broken Male - Hair Color
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 154; // Broken Female - Hair Style
                            case LegacyCustomizationOption.Face:
                                return 155; // Broken Female - Hair Color
                            case LegacyCustomizationOption.HairStyle:
                                return 1006; // Broken Female - Facial Hair
                        }
                    }
                    return 0;
                }
                case Race.Skeleton:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 156; // Skeleton Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 157; // Skeleton Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 158; // Skeleton Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 159; // Skeleton Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 1007; // Skeleton Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 160; // Skeleton Female - Hair Style
                            case LegacyCustomizationOption.Face:
                                return 161; // Skeleton Female - Hair Color
                            case LegacyCustomizationOption.HairStyle:
                                return 1008; // Skeleton Female - Facial Hair
                        }
                    }
                    return 0;
                }
                case Race.ForestTroll:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 176; // Forest Troll Male - Skin Color
                            case LegacyCustomizationOption.Face:
                                return 177; // Forest Troll Male - Face
                            case LegacyCustomizationOption.HairStyle:
                                return 178; // Forest Troll Male - Hair Style
                            case LegacyCustomizationOption.HairColor:
                                return 179; // Forest Troll Male - Hair Color
                            case LegacyCustomizationOption.FacialHair:
                                return 180; // Forest Troll Male - Facial Hair
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                                return 181; // Forest Troll Female - Hair Style
                            case LegacyCustomizationOption.Face:
                                return 182; // Forest Troll Female - Hair Color
                            case LegacyCustomizationOption.HairStyle:
                                return 1009; // Forest Troll Female - Facial Hair
                        }
                    }
                    return 0;
                }
            }
            return 0;
        }

        public static uint GetModernCustomizationChoice(Race raceId, Gender gender, LegacyCustomizationOption option, byte value)
        {
            switch (raceId)
            {
                case Race.Human:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Human Male - Skin Color
                                        return 17160;
                                    case 1: // Human Male - Skin Color
                                        return 17161;
                                    case 2: // Human Male - Skin Color
                                        return 17162;
                                    case 3: // Human Male - Skin Color
                                        return 17163;
                                    case 4: // Human Male - Skin Color
                                        return 17164;
                                    case 5: // Human Male - Skin Color
                                        return 17165;
                                    case 6: // Human Male - Skin Color
                                        return 17166;
                                    case 7: // Human Male - Skin Color
                                        return 17167;
                                    case 8: // Human Male - Skin Color
                                        return 17168;
                                    case 9: // Human Male - Skin Color
                                        return 17169;
                                    case 10: // Human Male - Skin Color
                                        return 17170;
                                    case 11: // Human Male - Skin Color
                                        return 17171;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Human Male - Face
                                        return 17172;
                                    case 1: // Human Male - Face
                                        return 17173;
                                    case 2: // Human Male - Face
                                        return 17174;
                                    case 3: // Human Male - Face
                                        return 17175;
                                    case 4: // Human Male - Face
                                        return 17176;
                                    case 5: // Human Male - Face
                                        return 17177;
                                    case 6: // Human Male - Face
                                        return 17178;
                                    case 7: // Human Male - Face
                                        return 17179;
                                    case 8: // Human Male - Face
                                        return 17180;
                                    case 9: // Human Male - Face
                                        return 17181;
                                    case 10: // Human Male - Face
                                        return 17182;
                                    case 11: // Human Male - Face
                                        return 17183;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Human Male - Hair Style
                                        return 17184;
                                    case 1: // Human Male - Hair Style
                                        return 17185;
                                    case 2: // Human Male - Hair Style
                                        return 17186;
                                    case 3: // Human Male - Hair Style
                                        return 17187;
                                    case 4: // Human Male - Hair Style
                                        return 17188;
                                    case 5: // Human Male - Hair Style
                                        return 17189;
                                    case 6: // Human Male - Hair Style
                                        return 17190;
                                    case 7: // Human Male - Hair Style
                                        return 17191;
                                    case 8: // Human Male - Hair Style
                                        return 17192;
                                    case 9: // Human Male - Hair Style
                                        return 17193;
                                    case 10: // Human Male - Hair Style
                                        return 17194;
                                    case 11: // Human Male - Hair Style
                                        return 17195;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Human Male - Hair Color
                                        return 17196;
                                    case 1: // Human Male - Hair Color
                                        return 17197;
                                    case 2: // Human Male - Hair Color
                                        return 17198;
                                    case 3: // Human Male - Hair Color
                                        return 17199;
                                    case 4: // Human Male - Hair Color
                                        return 17200;
                                    case 5: // Human Male - Hair Color
                                        return 17201;
                                    case 6: // Human Male - Hair Color
                                        return 17202;
                                    case 7: // Human Male - Hair Color
                                        return 17203;
                                    case 8: // Human Male - Hair Color
                                        return 17204;
                                    case 9: // Human Male - Hair Color
                                        return 17205;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Human Male - Facial Hair
                                        return 17206;
                                    case 1: // Human Male - Facial Hair
                                        return 17207;
                                    case 2: // Human Male - Facial Hair
                                        return 17208;
                                    case 3: // Human Male - Facial Hair
                                        return 17209;
                                    case 4: // Human Male - Facial Hair
                                        return 17210;
                                    case 5: // Human Male - Facial Hair
                                        return 17211;
                                    case 6: // Human Male - Facial Hair
                                        return 17212;
                                    case 7: // Human Male - Facial Hair
                                        return 17213;
                                    case 8: // Human Male - Facial Hair
                                        return 17214;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Human Female - Skin Color
                                        return 17215;
                                    case 1: // Human Female - Skin Color
                                        return 17216;
                                    case 2: // Human Female - Skin Color
                                        return 17217;
                                    case 3: // Human Female - Skin Color
                                        return 17218;
                                    case 4: // Human Female - Skin Color
                                        return 17219;
                                    case 5: // Human Female - Skin Color
                                        return 17220;
                                    case 6: // Human Female - Skin Color
                                        return 17221;
                                    case 7: // Human Female - Skin Color
                                        return 17222;
                                    case 8: // Human Female - Skin Color
                                        return 17223;
                                    case 9: // Human Female - Skin Color
                                        return 17224;
                                    case 10: // Human Female - Skin Color
                                        return 17225;
                                    case 11: // Human Female - Skin Color
                                        return 17226;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Human Female - Face
                                        return 17227;
                                    case 1: // Human Female - Face
                                        return 17228;
                                    case 2: // Human Female - Face
                                        return 17229;
                                    case 3: // Human Female - Face
                                        return 17230;
                                    case 4: // Human Female - Face
                                        return 17231;
                                    case 5: // Human Female - Face
                                        return 17232;
                                    case 6: // Human Female - Face
                                        return 17233;
                                    case 7: // Human Female - Face
                                        return 17234;
                                    case 8: // Human Female - Face
                                        return 17235;
                                    case 9: // Human Female - Face
                                        return 17236;
                                    case 10: // Human Female - Face
                                        return 17237;
                                    case 11: // Human Female - Face
                                        return 17238;
                                    case 12: // Human Female - Face
                                        return 17239;
                                    case 13: // Human Female - Face
                                        return 17240;
                                    case 14: // Human Female - Face
                                        return 17241;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Human Female - Hair Style
                                        return 17242;
                                    case 1: // Human Female - Hair Style
                                        return 17243;
                                    case 2: // Human Female - Hair Style
                                        return 17244;
                                    case 3: // Human Female - Hair Style
                                        return 17245;
                                    case 4: // Human Female - Hair Style
                                        return 17246;
                                    case 5: // Human Female - Hair Style
                                        return 17247;
                                    case 6: // Human Female - Hair Style
                                        return 17248;
                                    case 7: // Human Female - Hair Style
                                        return 17249;
                                    case 8: // Human Female - Hair Style
                                        return 17250;
                                    case 9: // Human Female - Hair Style
                                        return 17251;
                                    case 10: // Human Female - Hair Style
                                        return 17252;
                                    case 11: // Human Female - Hair Style
                                        return 17253;
                                    case 12: // Human Female - Hair Style
                                        return 17254;
                                    case 13: // Human Female - Hair Style
                                        return 17255;
                                    case 14: // Human Female - Hair Style
                                        return 17256;
                                    case 15: // Human Female - Hair Style
                                        return 17257;
                                    case 16: // Human Female - Hair Style
                                        return 17258;
                                    case 17: // Human Female - Hair Style
                                        return 17259;
                                    case 18: // Human Female - Hair Style
                                        return 17260;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Human Female - Hair Color
                                        return 17261;
                                    case 1: // Human Female - Hair Color
                                        return 17262;
                                    case 2: // Human Female - Hair Color
                                        return 17263;
                                    case 3: // Human Female - Hair Color
                                        return 17264;
                                    case 4: // Human Female - Hair Color
                                        return 17265;
                                    case 5: // Human Female - Hair Color
                                        return 17266;
                                    case 6: // Human Female - Hair Color
                                        return 17267;
                                    case 7: // Human Female - Hair Color
                                        return 17268;
                                    case 8: // Human Female - Hair Color
                                        return 17269;
                                    case 9: // Human Female - Hair Color
                                        return 17270;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Human Female - Piercings
                                        return 17271;
                                    case 1: // Human Female - Piercings
                                        return 17272;
                                    case 2: // Human Female - Piercings
                                        return 17273;
                                    case 3: // Human Female - Piercings
                                        return 17274;
                                    case 4: // Human Female - Piercings
                                        return 17275;
                                    case 5: // Human Female - Piercings
                                        return 17276;
                                    case 6: // Human Female - Piercings
                                        return 17277;
                                }
                                return 0;
                            }
                        }
                    }

                    return 0;
                }
                case Race.Orc:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Male - Skin Color
                                        return 17278;
                                    case 1: // Orc Male - Skin Color
                                        return 17279;
                                    case 2: // Orc Male - Skin Color
                                        return 17280;
                                    case 3: // Orc Male - Skin Color
                                        return 17281;
                                    case 4: // Orc Male - Skin Color
                                        return 17282;
                                    case 5: // Orc Male - Skin Color
                                        return 17283;
                                    case 6: // Orc Male - Skin Color
                                        return 17284;
                                    case 7: // Orc Male - Skin Color
                                        return 17285;
                                    case 8: // Orc Male - Skin Color
                                        return 17286;
                                    case 9: // Orc Male - Skin Color
                                        return 17287;
                                    case 10: // Orc Male - Skin Color
                                        return 17288;
                                    case 11: // Orc Male - Skin Color
                                        return 17289;
                                    case 12: // Orc Male - Skin Color
                                        return 17290;
                                    case 13: // Orc Male - Skin Color
                                        return 17291;
                                    case 14: // Orc Male - Skin Color
                                        return 17292;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Male - Face
                                        return 17293;
                                    case 1: // Orc Male - Face
                                        return 17294;
                                    case 2: // Orc Male - Face
                                        return 17295;
                                    case 3: // Orc Male - Face
                                        return 17296;
                                    case 4: // Orc Male - Face
                                        return 17297;
                                    case 5: // Orc Male - Face
                                        return 17298;
                                    case 6: // Orc Male - Face
                                        return 17299;
                                    case 7: // Orc Male - Face
                                        return 17300;
                                    case 8: // Orc Male - Face
                                        return 17301;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Male - Hair Style
                                        return 17302;
                                    case 1: // Orc Male - Hair Style
                                        return 17303;
                                    case 2: // Orc Male - Hair Style
                                        return 17304;
                                    case 3: // Orc Male - Hair Style
                                        return 17305;
                                    case 4: // Orc Male - Hair Style
                                        return 17306;
                                    case 5: // Orc Male - Hair Style
                                        return 17307;
                                    case 6: // Orc Male - Hair Style
                                        return 17308;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Male - Hair Color
                                        return 17309;
                                    case 1: // Orc Male - Hair Color
                                        return 17310;
                                    case 2: // Orc Male - Hair Color
                                        return 17311;
                                    case 3: // Orc Male - Hair Color
                                        return 17312;
                                    case 4: // Orc Male - Hair Color
                                        return 17313;
                                    case 5: // Orc Male - Hair Color
                                        return 17314;
                                    case 6: // Orc Male - Hair Color
                                        return 17315;
                                    case 7: // Orc Male - Hair Color
                                        return 17316;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Male - Facial Hair
                                        return 17317;
                                    case 1: // Orc Male - Facial Hair
                                        return 17318;
                                    case 2: // Orc Male - Facial Hair
                                        return 17319;
                                    case 3: // Orc Male - Facial Hair
                                        return 17320;
                                    case 4: // Orc Male - Facial Hair
                                        return 17321;
                                    case 5: // Orc Male - Facial Hair
                                        return 17322;
                                    case 6: // Orc Male - Facial Hair
                                        return 17323;
                                    case 7: // Orc Male - Facial Hair
                                        return 17324;
                                    case 8: // Orc Male - Facial Hair
                                        return 17325;
                                    case 9: // Orc Male - Facial Hair
                                        return 17326;
                                    case 10: // Orc Male - Facial Hair
                                        return 17327;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Female - Skin Color
                                        return 17328;
                                    case 1: // Orc Female - Skin Color
                                        return 17329;
                                    case 2: // Orc Female - Skin Color
                                        return 17330;
                                    case 3: // Orc Female - Skin Color
                                        return 17331;
                                    case 4: // Orc Female - Skin Color
                                        return 17332;
                                    case 5: // Orc Female - Skin Color
                                        return 17333;
                                    case 6: // Orc Female - Skin Color
                                        return 17334;
                                    case 7: // Orc Female - Skin Color
                                        return 17335;
                                    case 8: // Orc Female - Skin Color
                                        return 17336;
                                    case 9: // Orc Female - Skin Color
                                        return 17337;
                                    case 10: // Orc Female - Skin Color
                                        return 17338;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Female - Face
                                        return 17339;
                                    case 1: // Orc Female - Face
                                        return 17340;
                                    case 2: // Orc Female - Face
                                        return 17341;
                                    case 3: // Orc Female - Face
                                        return 17342;
                                    case 4: // Orc Female - Face
                                        return 17343;
                                    case 5: // Orc Female - Face
                                        return 17344;
                                    case 6: // Orc Female - Face
                                        return 17345;
                                    case 7: // Orc Female - Face
                                        return 17346;
                                    case 8: // Orc Female - Face
                                        return 17347;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Female - Hair Style
                                        return 17348;
                                    case 1: // Orc Female - Hair Style
                                        return 17349;
                                    case 2: // Orc Female - Hair Style
                                        return 17350;
                                    case 3: // Orc Female - Hair Style
                                        return 17351;
                                    case 4: // Orc Female - Hair Style
                                        return 17352;
                                    case 5: // Orc Female - Hair Style
                                        return 17353;
                                    case 6: // Orc Female - Hair Style
                                        return 17354;
                                    case 7: // Orc Female - Hair Style
                                        return 17355;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Female - Hair Color
                                        return 17356;
                                    case 1: // Orc Female - Hair Color
                                        return 17357;
                                    case 2: // Orc Female - Hair Color
                                        return 17358;
                                    case 3: // Orc Female - Hair Color
                                        return 17359;
                                    case 4: // Orc Female - Hair Color
                                        return 17360;
                                    case 5: // Orc Female - Hair Color
                                        return 17361;
                                    case 6: // Orc Female - Hair Color
                                        return 17362;
                                    case 7: // Orc Female - Hair Color
                                        return 17363;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Orc Female - Piercings
                                        return 17364;
                                    case 1: // Orc Female - Piercings
                                        return 17365;
                                    case 2: // Orc Female - Piercings
                                        return 17366;
                                    case 3: // Orc Female - Piercings
                                        return 17367;
                                    case 4: // Orc Female - Piercings
                                        return 17368;
                                    case 5: // Orc Female - Piercings
                                        return 17369;
                                    case 6: // Orc Female - Piercings
                                        return 17370;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.Dwarf:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Male - Skin Color
                                        return 17371;
                                    case 1: // Dwarf Male - Skin Color
                                        return 17372;
                                    case 2: // Dwarf Male - Skin Color
                                        return 17373;
                                    case 3: // Dwarf Male - Skin Color
                                        return 17374;
                                    case 4: // Dwarf Male - Skin Color
                                        return 17375;
                                    case 5: // Dwarf Male - Skin Color
                                        return 17376;
                                    case 6: // Dwarf Male - Skin Color
                                        return 17377;
                                    case 7: // Dwarf Male - Skin Color
                                        return 17378;
                                    case 8: // Dwarf Male - Skin Color
                                        return 17379;
                                    case 9: // Dwarf Male - Skin Color
                                        return 17380;
                                    case 10: // Dwarf Male - Skin Color
                                        return 17381;
                                    case 11: // Dwarf Male - Skin Color
                                        return 17382;
                                    case 12: // Dwarf Male - Skin Color
                                        return 17383;
                                    case 13: // Dwarf Male - Skin Color
                                        return 17384;
                                    case 14: // Dwarf Male - Skin Color
                                        return 17385;
                                    case 15: // Dwarf Male - Skin Color
                                        return 17386;
                                    case 16: // Dwarf Male - Skin Color
                                        return 17387;
                                    case 17: // Dwarf Male - Skin Color
                                        return 17388;
                                    case 18: // Dwarf Male - Skin Color
                                        return 17389;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Male - Face
                                        return 17390;
                                    case 1: // Dwarf Male - Face
                                        return 17391;
                                    case 2: // Dwarf Male - Face
                                        return 17392;
                                    case 3: // Dwarf Male - Face
                                        return 17393;
                                    case 4: // Dwarf Male - Face
                                        return 17394;
                                    case 5: // Dwarf Male - Face
                                        return 17395;
                                    case 6: // Dwarf Male - Face
                                        return 17396;
                                    case 7: // Dwarf Male - Face
                                        return 17397;
                                    case 8: // Dwarf Male - Face
                                        return 17398;
                                    case 9: // Dwarf Male - Face
                                        return 17399;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Male - Hair Style
                                        return 17400;
                                    case 1: // Dwarf Male - Hair Style
                                        return 17401;
                                    case 2: // Dwarf Male - Hair Style
                                        return 17402;
                                    case 3: // Dwarf Male - Hair Style
                                        return 17403;
                                    case 4: // Dwarf Male - Hair Style
                                        return 17404;
                                    case 5: // Dwarf Male - Hair Style
                                        return 17405;
                                    case 6: // Dwarf Male - Hair Style
                                        return 17406;
                                    case 7: // Dwarf Male - Hair Style
                                        return 17407;
                                    case 8: // Dwarf Male - Hair Style
                                        return 17408;
                                    case 9: // Dwarf Male - Hair Style
                                        return 17409;
                                    case 10: // Dwarf Male - Hair Style
                                        return 17410;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Male - Hair Color
                                        return 17411;
                                    case 1: // Dwarf Male - Hair Color
                                        return 17412;
                                    case 2: // Dwarf Male - Hair Color
                                        return 17413;
                                    case 3: // Dwarf Male - Hair Color
                                        return 17414;
                                    case 4: // Dwarf Male - Hair Color
                                        return 17415;
                                    case 5: // Dwarf Male - Hair Color
                                        return 17416;
                                    case 6: // Dwarf Male - Hair Color
                                        return 17417;
                                    case 7: // Dwarf Male - Hair Color
                                        return 17418;
                                    case 8: // Dwarf Male - Hair Color
                                        return 17419;
                                    case 9: // Dwarf Male - Hair Color
                                        return 17420;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Male - Facial Hair
                                        return 17421;
                                    case 1: // Dwarf Male - Facial Hair
                                        return 17422;
                                    case 2: // Dwarf Male - Facial Hair
                                        return 17423;
                                    case 3: // Dwarf Male - Facial Hair
                                        return 17424;
                                    case 4: // Dwarf Male - Facial Hair
                                        return 17425;
                                    case 5: // Dwarf Male - Facial Hair
                                        return 17426;
                                    case 6: // Dwarf Male - Facial Hair
                                        return 17427;
                                    case 7: // Dwarf Male - Facial Hair
                                        return 17428;
                                    case 8: // Dwarf Male - Facial Hair
                                        return 17429;
                                    case 9: // Dwarf Male - Facial Hair
                                        return 17430;
                                    case 10: // Dwarf Male - Facial Hair
                                        return 17431;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Female - Skin Color
                                        return 17432;
                                    case 1: // Dwarf Female - Skin Color
                                        return 17433;
                                    case 2: // Dwarf Female - Skin Color
                                        return 17434;
                                    case 3: // Dwarf Female - Skin Color
                                        return 17435;
                                    case 4: // Dwarf Female - Skin Color
                                        return 17436;
                                    case 5: // Dwarf Female - Skin Color
                                        return 17437;
                                    case 6: // Dwarf Female - Skin Color
                                        return 17438;
                                    case 7: // Dwarf Female - Skin Color
                                        return 17439;
                                    case 8: // Dwarf Female - Skin Color
                                        return 17440;
                                    case 9: // Dwarf Female - Skin Color
                                        return 17441;
                                    case 10: // Dwarf Female - Skin Color
                                        return 17442;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Female - Face
                                        return 17443;
                                    case 1: // Dwarf Female - Face
                                        return 17444;
                                    case 2: // Dwarf Female - Face
                                        return 17445;
                                    case 3: // Dwarf Female - Face
                                        return 17446;
                                    case 4: // Dwarf Female - Face
                                        return 17447;
                                    case 5: // Dwarf Female - Face
                                        return 17448;
                                    case 6: // Dwarf Female - Face
                                        return 17449;
                                    case 7: // Dwarf Female - Face
                                        return 17450;
                                    case 8: // Dwarf Female - Face
                                        return 17451;
                                    case 9: // Dwarf Female - Face
                                        return 17452;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Female - Hair Style
                                        return 17453;
                                    case 1: // Dwarf Female - Hair Style
                                        return 17454;
                                    case 2: // Dwarf Female - Hair Style
                                        return 17455;
                                    case 3: // Dwarf Female - Hair Style
                                        return 17456;
                                    case 4: // Dwarf Female - Hair Style
                                        return 17457;
                                    case 5: // Dwarf Female - Hair Style
                                        return 17458;
                                    case 6: // Dwarf Female - Hair Style
                                        return 17459;
                                    case 7: // Dwarf Female - Hair Style
                                        return 17460;
                                    case 8: // Dwarf Female - Hair Style
                                        return 17461;
                                    case 9: // Dwarf Female - Hair Style
                                        return 17462;
                                    case 10: // Dwarf Female - Hair Style
                                        return 17463;
                                    case 11: // Dwarf Female - Hair Style
                                        return 17464;
                                    case 12: // Dwarf Female - Hair Style
                                        return 17465;
                                    case 13: // Dwarf Female - Hair Style
                                        return 17466;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Female - Hair Color
                                        return 17467;
                                    case 1: // Dwarf Female - Hair Color
                                        return 17468;
                                    case 2: // Dwarf Female - Hair Color
                                        return 17469;
                                    case 3: // Dwarf Female - Hair Color
                                        return 17470;
                                    case 4: // Dwarf Female - Hair Color
                                        return 17471;
                                    case 5: // Dwarf Female - Hair Color
                                        return 17472;
                                    case 6: // Dwarf Female - Hair Color
                                        return 17473;
                                    case 7: // Dwarf Female - Hair Color
                                        return 17474;
                                    case 8: // Dwarf Female - Hair Color
                                        return 17475;
                                    case 9: // Dwarf Female - Hair Color
                                        return 17476;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Dwarf Female - Piercings
                                        return 17477;
                                    case 1: // Dwarf Female - Piercings
                                        return 17478;
                                    case 2: // Dwarf Female - Piercings
                                        return 17479;
                                    case 3: // Dwarf Female - Piercings
                                        return 17480;
                                    case 4: // Dwarf Female - Piercings
                                        return 17481;
                                    case 5: // Dwarf Female - Piercings
                                        return 17482;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.NightElf:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Male - Skin Color
                                        return 17483;
                                    case 1: // Night Elf Male - Skin Color
                                        return 17484;
                                    case 2: // Night Elf Male - Skin Color
                                        return 17485;
                                    case 3: // Night Elf Male - Skin Color
                                        return 17486;
                                    case 4: // Night Elf Male - Skin Color
                                        return 17487;
                                    case 5: // Night Elf Male - Skin Color
                                        return 17488;
                                    case 6: // Night Elf Male - Skin Color
                                        return 17489;
                                    case 7: // Night Elf Male - Skin Color
                                        return 17490;
                                    case 8: // Night Elf Male - Skin Color
                                        return 17491;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Male - Face
                                        return 17492;
                                    case 1: // Night Elf Male - Face
                                        return 17493;
                                    case 2: // Night Elf Male - Face
                                        return 17494;
                                    case 3: // Night Elf Male - Face
                                        return 17495;
                                    case 4: // Night Elf Male - Face
                                        return 17496;
                                    case 5: // Night Elf Male - Face
                                        return 17497;
                                    case 6: // Night Elf Male - Face
                                        return 17498;
                                    case 7: // Night Elf Male - Face
                                        return 17499;
                                    case 8: // Night Elf Male - Face
                                        return 17500;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Male - Hair Style
                                        return 17501;
                                    case 1: // Night Elf Male - Hair Style
                                        return 17502;
                                    case 2: // Night Elf Male - Hair Style
                                        return 17503;
                                    case 3: // Night Elf Male - Hair Style
                                        return 17504;
                                    case 4: // Night Elf Male - Hair Style
                                        return 17505;
                                    case 5: // Night Elf Male - Hair Style
                                        return 17506;
                                    case 6: // Night Elf Male - Hair Style
                                        return 17507;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Male - Hair Color
                                        return 17508;
                                    case 1: // Night Elf Male - Hair Color
                                        return 17509;
                                    case 2: // Night Elf Male - Hair Color
                                        return 17510;
                                    case 3: // Night Elf Male - Hair Color
                                        return 17511;
                                    case 4: // Night Elf Male - Hair Color
                                        return 17512;
                                    case 5: // Night Elf Male - Hair Color
                                        return 17513;
                                    case 6: // Night Elf Male - Hair Color
                                        return 17514;
                                    case 7: // Night Elf Male - Hair Color
                                        return 17515;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Male - Facial Hair
                                        return 17516;
                                    case 1: // Night Elf Male - Facial Hair
                                        return 17517;
                                    case 2: // Night Elf Male - Facial Hair
                                        return 17518;
                                    case 3: // Night Elf Male - Facial Hair
                                        return 17519;
                                    case 4: // Night Elf Male - Facial Hair
                                        return 17520;
                                    case 5: // Night Elf Male - Facial Hair
                                        return 17521;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Female - Skin Color
                                        return 17522;
                                    case 1: // Night Elf Female - Skin Color
                                        return 17523;
                                    case 2: // Night Elf Female - Skin Color
                                        return 17524;
                                    case 3: // Night Elf Female - Skin Color
                                        return 17525;
                                    case 4: // Night Elf Female - Skin Color
                                        return 17526;
                                    case 5: // Night Elf Female - Skin Color
                                        return 17527;
                                    case 6: // Night Elf Female - Skin Color
                                        return 17528;
                                    case 7: // Night Elf Female - Skin Color
                                        return 17529;
                                    case 8: // Night Elf Female - Skin Color
                                        return 17530;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Female - Face
                                        return 17531;
                                    case 1: // Night Elf Female - Face
                                        return 17532;
                                    case 2: // Night Elf Female - Face
                                        return 17533;
                                    case 3: // Night Elf Female - Face
                                        return 17534;
                                    case 4: // Night Elf Female - Face
                                        return 17535;
                                    case 5: // Night Elf Female - Face
                                        return 17536;
                                    case 6: // Night Elf Female - Face
                                        return 17537;
                                    case 7: // Night Elf Female - Face
                                        return 17538;
                                    case 8: // Night Elf Female - Face
                                        return 17539;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Female - Hair Style
                                        return 17540;
                                    case 1: // Night Elf Female - Hair Style
                                        return 17541;
                                    case 2: // Night Elf Female - Hair Style
                                        return 17542;
                                    case 3: // Night Elf Female - Hair Style
                                        return 17543;
                                    case 4: // Night Elf Female - Hair Style
                                        return 17544;
                                    case 5: // Night Elf Female - Hair Style
                                        return 17545;
                                    case 6: // Night Elf Female - Hair Style
                                        return 17546;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Female - Hair Color
                                        return 17547;
                                    case 1: // Night Elf Female - Hair Color
                                        return 17548;
                                    case 2: // Night Elf Female - Hair Color
                                        return 17549;
                                    case 3: // Night Elf Female - Hair Color
                                        return 17550;
                                    case 4: // Night Elf Female - Hair Color
                                        return 17551;
                                    case 5: // Night Elf Female - Hair Color
                                        return 17552;
                                    case 6: // Night Elf Female - Hair Color
                                        return 17553;
                                    case 7: // Night Elf Female - Hair Color
                                        return 17554;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Night Elf Female - Markings
                                        return 17555;
                                    case 1: // Night Elf Female - Markings
                                        return 17556;
                                    case 2: // Night Elf Female - Markings
                                        return 17557;
                                    case 3: // Night Elf Female - Markings
                                        return 17558;
                                    case 4: // Night Elf Female - Markings
                                        return 17559;
                                    case 5: // Night Elf Female - Markings
                                        return 17560;
                                    case 6: // Night Elf Female - Markings
                                        return 17561;
                                    case 7: // Night Elf Female - Markings
                                        return 17562;
                                    case 8: // Night Elf Female - Markings
                                        return 17563;
                                    case 9: // Night Elf Female - Markings
                                        return 17564;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.Undead:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Male - Skin Color
                                        return 17565;
                                    case 1: // Undead Male - Skin Color
                                        return 17566;
                                    case 2: // Undead Male - Skin Color
                                        return 17567;
                                    case 3: // Undead Male - Skin Color
                                        return 17568;
                                    case 4: // Undead Male - Skin Color
                                        return 17569;
                                    case 5: // Undead Male - Skin Color
                                        return 17570;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Male - Face
                                        return 17571;
                                    case 1: // Undead Male - Face
                                        return 17572;
                                    case 2: // Undead Male - Face
                                        return 17573;
                                    case 3: // Undead Male - Face
                                        return 17574;
                                    case 4: // Undead Male - Face
                                        return 17575;
                                    case 5: // Undead Male - Face
                                        return 17576;
                                    case 6: // Undead Male - Face
                                        return 17577;
                                    case 7: // Undead Male - Face
                                        return 17578;
                                    case 8: // Undead Male - Face
                                        return 17579;
                                    case 9: // Undead Male - Face
                                        return 17580;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Male - Hair Style
                                        return 17581;
                                    case 1: // Undead Male - Hair Style
                                        return 17582;
                                    case 2: // Undead Male - Hair Style
                                        return 17583;
                                    case 3: // Undead Male - Hair Style
                                        return 17584;
                                    case 4: // Undead Male - Hair Style
                                        return 17585;
                                    case 5: // Undead Male - Hair Style
                                        return 17586;
                                    case 6: // Undead Male - Hair Style
                                        return 17587;
                                    case 7: // Undead Male - Hair Style
                                        return 17588;
                                    case 8: // Undead Male - Hair Style
                                        return 17589;
                                    case 9: // Undead Male - Hair Style
                                        return 17590;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Male - Hair Color
                                        return 17591;
                                    case 1: // Undead Male - Hair Color
                                        return 17592;
                                    case 2: // Undead Male - Hair Color
                                        return 17593;
                                    case 3: // Undead Male - Hair Color
                                        return 17594;
                                    case 4: // Undead Male - Hair Color
                                        return 17595;
                                    case 5: // Undead Male - Hair Color
                                        return 17596;
                                    case 6: // Undead Male - Hair Color
                                        return 17597;
                                    case 7: // Undead Male - Hair Color
                                        return 17598;
                                    case 8: // Undead Male - Hair Color
                                        return 17599;
                                    case 9: // Undead Male - Hair Color
                                        return 17600;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Male - Features
                                        return 17601;
                                    case 1: // Undead Male - Features
                                        return 17602;
                                    case 2: // Undead Male - Features
                                        return 17603;
                                    case 3: // Undead Male - Features
                                        return 17604;
                                    case 4: // Undead Male - Features
                                        return 17605;
                                    case 5: // Undead Male - Features
                                        return 17606;
                                    case 6: // Undead Male - Features
                                        return 17607;
                                    case 7: // Undead Male - Features
                                        return 17608;
                                    case 8: // Undead Male - Features
                                        return 17609;
                                    case 9: // Undead Male - Features
                                        return 17610;
                                    case 10: // Undead Male - Features
                                        return 17611;
                                    case 11: // Undead Male - Features
                                        return 17612;
                                    case 12: // Undead Male - Features
                                        return 17613;
                                    case 13: // Undead Male - Features
                                        return 17614;
                                    case 14: // Undead Male - Features
                                        return 17615;
                                    case 15: // Undead Male - Features
                                        return 17616;
                                    case 16: // Undead Male - Features
                                        return 17617;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Female - Skin Color
                                        return 17618;
                                    case 1: // Undead Female - Skin Color
                                        return 17619;
                                    case 2: // Undead Female - Skin Color
                                        return 17620;
                                    case 3: // Undead Female - Skin Color
                                        return 17621;
                                    case 4: // Undead Female - Skin Color
                                        return 17622;
                                    case 5: // Undead Female - Skin Color
                                        return 17623;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Female - Face
                                        return 17624;
                                    case 1: // Undead Female - Face
                                        return 17625;
                                    case 2: // Undead Female - Face
                                        return 17626;
                                    case 3: // Undead Female - Face
                                        return 17627;
                                    case 4: // Undead Female - Face
                                        return 17628;
                                    case 5: // Undead Female - Face
                                        return 17629;
                                    case 6: // Undead Female - Face
                                        return 17630;
                                    case 7: // Undead Female - Face
                                        return 17631;
                                    case 8: // Undead Female - Face
                                        return 17632;
                                    case 9: // Undead Female - Face
                                        return 17633;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Female - Hair Style
                                        return 17634;
                                    case 1: // Undead Female - Hair Style
                                        return 17635;
                                    case 2: // Undead Female - Hair Style
                                        return 17636;
                                    case 3: // Undead Female - Hair Style
                                        return 17637;
                                    case 4: // Undead Female - Hair Style
                                        return 17638;
                                    case 5: // Undead Female - Hair Style
                                        return 17639;
                                    case 6: // Undead Female - Hair Style
                                        return 17640;
                                    case 7: // Undead Female - Hair Style
                                        return 17641;
                                    case 8: // Undead Female - Hair Style
                                        return 17642;
                                    case 9: // Undead Female - Hair Style
                                        return 17643;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Female - Hair Color
                                        return 17644;
                                    case 1: // Undead Female - Hair Color
                                        return 17645;
                                    case 2: // Undead Female - Hair Color
                                        return 17646;
                                    case 3: // Undead Female - Hair Color
                                        return 17647;
                                    case 4: // Undead Female - Hair Color
                                        return 17648;
                                    case 5: // Undead Female - Hair Color
                                        return 17649;
                                    case 6: // Undead Female - Hair Color
                                        return 17650;
                                    case 7: // Undead Female - Hair Color
                                        return 17651;
                                    case 8: // Undead Female - Hair Color
                                        return 17652;
                                    case 9: // Undead Female - Hair Color
                                        return 17653;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Undead Female - Features
                                        return 17654;
                                    case 1: // Undead Female - Features
                                        return 17655;
                                    case 2: // Undead Female - Features
                                        return 17656;
                                    case 3: // Undead Female - Features
                                        return 17657;
                                    case 4: // Undead Female - Features
                                        return 17658;
                                    case 5: // Undead Female - Features
                                        return 17659;
                                    case 6: // Undead Female - Features
                                        return 17660;
                                    case 7: // Undead Female - Features
                                        return 17661;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.Tauren:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Male - Skin Color
                                        return 17662;
                                    case 1: // Tauren Male - Skin Color
                                        return 17663;
                                    case 2: // Tauren Male - Skin Color
                                        return 17664;
                                    case 3: // Tauren Male - Skin Color
                                        return 17665;
                                    case 4: // Tauren Male - Skin Color
                                        return 17666;
                                    case 5: // Tauren Male - Skin Color
                                        return 17667;
                                    case 6: // Tauren Male - Skin Color
                                        return 17668;
                                    case 7: // Tauren Male - Skin Color
                                        return 17669;
                                    case 8: // Tauren Male - Skin Color
                                        return 17670;
                                    case 9: // Tauren Male - Skin Color
                                        return 17671;
                                    case 10: // Tauren Male - Skin Color
                                        return 17672;
                                    case 11: // Tauren Male - Skin Color
                                        return 17673;
                                    case 12: // Tauren Male - Skin Color
                                        return 17674;
                                    case 13: // Tauren Male - Skin Color
                                        return 17675;
                                    case 14: // Tauren Male - Skin Color
                                        return 17676;
                                    case 15: // Tauren Male - Skin Color
                                        return 17677;
                                    case 16: // Tauren Male - Skin Color
                                        return 17678;
                                    case 17: // Tauren Male - Skin Color
                                        return 17679;
                                    case 18: // Tauren Male - Skin Color
                                        return 17680;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Male - Face
                                        return 17681;
                                    case 1: // Tauren Male - Face
                                        return 17682;
                                    case 2: // Tauren Male - Face
                                        return 17683;
                                    case 3: // Tauren Male - Face
                                        return 17684;
                                    case 4: // Tauren Male - Face
                                        return 17685;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Male - Horn Style
                                        return 17686;
                                    case 1: // Tauren Male - Horn Style
                                        return 17687;
                                    case 2: // Tauren Male - Horn Style
                                        return 17688;
                                    case 3: // Tauren Male - Horn Style
                                        return 17689;
                                    case 4: // Tauren Male - Horn Style
                                        return 17690;
                                    case 5: // Tauren Male - Horn Style
                                        return 17691;
                                    case 6: // Tauren Male - Horn Style
                                        return 17692;
                                    case 7: // Tauren Male - Horn Style
                                        return 17693;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Male - Horn Color
                                        return 17694;
                                    case 1: // Tauren Male - Horn Color
                                        return 17695;
                                    case 2: // Tauren Male - Horn Color
                                        return 17696;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Male - Facial Hair
                                        return 17697;
                                    case 1: // Tauren Male - Facial Hair
                                        return 17698;
                                    case 2: // Tauren Male - Facial Hair
                                        return 17699;
                                    case 3: // Tauren Male - Facial Hair
                                        return 17700;
                                    case 4: // Tauren Male - Facial Hair
                                        return 17701;
                                    case 5: // Tauren Male - Facial Hair
                                        return 17702;
                                    case 6: // Tauren Male - Facial Hair
                                        return 17703;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Female - Skin Color
                                        return 17704;
                                    case 1: // Tauren Female - Skin Color
                                        return 17705;
                                    case 2: // Tauren Female - Skin Color
                                        return 17706;
                                    case 3: // Tauren Female - Skin Color
                                        return 17707;
                                    case 4: // Tauren Female - Skin Color
                                        return 17708;
                                    case 5: // Tauren Female - Skin Color
                                        return 17709;
                                    case 6: // Tauren Female - Skin Color
                                        return 17710;
                                    case 7: // Tauren Female - Skin Color
                                        return 17711;
                                    case 8: // Tauren Female - Skin Color
                                        return 17712;
                                    case 9: // Tauren Female - Skin Color
                                        return 17713;
                                    case 10: // Tauren Female - Skin Color
                                        return 17714;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Female - Face
                                        return 17715;
                                    case 1: // Tauren Female - Face
                                        return 17716;
                                    case 2: // Tauren Female - Face
                                        return 17717;
                                    case 3: // Tauren Female - Face
                                        return 17718;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Female - Horn Style
                                        return 17719;
                                    case 1: // Tauren Female - Horn Style
                                        return 17720;
                                    case 2: // Tauren Female - Horn Style
                                        return 17721;
                                    case 3: // Tauren Female - Horn Style
                                        return 17722;
                                    case 4: // Tauren Female - Horn Style
                                        return 17723;
                                    case 5: // Tauren Female - Horn Style
                                        return 17724;
                                    case 6: // Tauren Female - Horn Style
                                        return 17725;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Female - Horn Color
                                        return 17726;
                                    case 1: // Tauren Female - Horn Color
                                        return 17727;
                                    case 2: // Tauren Female - Horn Color
                                        return 17728;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Tauren Female - Hair
                                        return 17729;
                                    case 1: // Tauren Female - Hair
                                        return 17730;
                                    case 2: // Tauren Female - Hair
                                        return 17731;
                                    case 3: // Tauren Female - Hair
                                        return 17732;
                                    case 4: // Tauren Female - Hair
                                        return 17733;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.Gnome:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Male - Skin Color
                                        return 17734;
                                    case 1: // Gnome Male - Skin Color
                                        return 17735;
                                    case 2: // Gnome Male - Skin Color
                                        return 17736;
                                    case 3: // Gnome Male - Skin Color
                                        return 17737;
                                    case 4: // Gnome Male - Skin Color
                                        return 17738;
                                    case 5: // Gnome Male - Skin Color
                                        return 17739;
                                    case 6: // Gnome Male - Skin Color
                                        return 17740;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Male - Face
                                        return 17741;
                                    case 1: // Gnome Male - Face
                                        return 17742;
                                    case 2: // Gnome Male - Face
                                        return 17743;
                                    case 3: // Gnome Male - Face
                                        return 17744;
                                    case 4: // Gnome Male - Face
                                        return 17745;
                                    case 5: // Gnome Male - Face
                                        return 17746;
                                    case 6: // Gnome Male - Face
                                        return 17747;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Male - Hair Style
                                        return 17748;
                                    case 1: // Gnome Male - Hair Style
                                        return 17749;
                                    case 2: // Gnome Male - Hair Style
                                        return 17750;
                                    case 3: // Gnome Male - Hair Style
                                        return 17751;
                                    case 4: // Gnome Male - Hair Style
                                        return 17752;
                                    case 5: // Gnome Male - Hair Style
                                        return 17753;
                                    case 6: // Gnome Male - Hair Style
                                        return 17754;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Male - Hair Color
                                        return 17755;
                                    case 1: // Gnome Male - Hair Color
                                        return 17756;
                                    case 2: // Gnome Male - Hair Color
                                        return 17757;
                                    case 3: // Gnome Male - Hair Color
                                        return 17758;
                                    case 4: // Gnome Male - Hair Color
                                        return 17759;
                                    case 5: // Gnome Male - Hair Color
                                        return 17760;
                                    case 6: // Gnome Male - Hair Color
                                        return 17761;
                                    case 7: // Gnome Male - Hair Color
                                        return 17762;
                                    case 8: // Gnome Male - Hair Color
                                        return 17763;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Male - Facial Hair
                                        return 17764;
                                    case 1: // Gnome Male - Facial Hair
                                        return 17765;
                                    case 2: // Gnome Male - Facial Hair
                                        return 17766;
                                    case 3: // Gnome Male - Facial Hair
                                        return 17767;
                                    case 4: // Gnome Male - Facial Hair
                                        return 17768;
                                    case 5: // Gnome Male - Facial Hair
                                        return 17769;
                                    case 6: // Gnome Male - Facial Hair
                                        return 17770;
                                    case 7: // Gnome Male - Facial Hair
                                        return 17771;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Female - Skin Color
                                        return 17772;
                                    case 1: // Gnome Female - Skin Color
                                        return 17773;
                                    case 2: // Gnome Female - Skin Color
                                        return 17774;
                                    case 3: // Gnome Female - Skin Color
                                        return 17775;
                                    case 4: // Gnome Female - Skin Color
                                        return 17776;
                                    case 5: // Gnome Female - Skin Color
                                        return 17777;
                                    case 6: // Gnome Female - Skin Color
                                        return 17778;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Female - Face
                                        return 17779;
                                    case 1: // Gnome Female - Face
                                        return 17780;
                                    case 2: // Gnome Female - Face
                                        return 17781;
                                    case 3: // Gnome Female - Face
                                        return 17782;
                                    case 4: // Gnome Female - Face
                                        return 17783;
                                    case 5: // Gnome Female - Face
                                        return 17784;
                                    case 6: // Gnome Female - Face
                                        return 17785;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Female - Hair Style
                                        return 17786;
                                    case 1: // Gnome Female - Hair Style
                                        return 17787;
                                    case 2: // Gnome Female - Hair Style
                                        return 17788;
                                    case 3: // Gnome Female - Hair Style
                                        return 17789;
                                    case 4: // Gnome Female - Hair Style
                                        return 17790;
                                    case 5: // Gnome Female - Hair Style
                                        return 17791;
                                    case 6: // Gnome Female - Hair Style
                                        return 17792;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Female - Hair Color
                                        return 17793;
                                    case 1: // Gnome Female - Hair Color
                                        return 17794;
                                    case 2: // Gnome Female - Hair Color
                                        return 17795;
                                    case 3: // Gnome Female - Hair Color
                                        return 17796;
                                    case 4: // Gnome Female - Hair Color
                                        return 17797;
                                    case 5: // Gnome Female - Hair Color
                                        return 17798;
                                    case 6: // Gnome Female - Hair Color
                                        return 17799;
                                    case 7: // Gnome Female - Hair Color
                                        return 17800;
                                    case 8: // Gnome Female - Hair Color
                                        return 17801;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Gnome Female - Earrings
                                        return 17802;
                                    case 1: // Gnome Female - Earrings
                                        return 17803;
                                    case 2: // Gnome Female - Earrings
                                        return 17804;
                                    case 3: // Gnome Female - Earrings
                                        return 17805;
                                    case 4: // Gnome Female - Earrings
                                        return 17806;
                                    case 5: // Gnome Female - Earrings
                                        return 17807;
                                    case 6: // Gnome Female - Earrings
                                        return 17808;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.Troll:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Male - Skin Color
                                        return 17809;
                                    case 1: // Troll Male - Skin Color
                                        return 17810;
                                    case 2: // Troll Male - Skin Color
                                        return 17811;
                                    case 3: // Troll Male - Skin Color
                                        return 17812;
                                    case 4: // Troll Male - Skin Color
                                        return 17813;
                                    case 5: // Troll Male - Skin Color
                                        return 17814;
                                    case 6: // Troll Male - Skin Color
                                        return 17815;
                                    case 7: // Troll Male - Skin Color
                                        return 17816;
                                    case 8: // Troll Male - Skin Color
                                        return 17817;
                                    case 9: // Troll Male - Skin Color
                                        return 17818;
                                    case 10: // Troll Male - Skin Color
                                        return 17819;
                                    case 11: // Troll Male - Skin Color
                                        return 17820;
                                    case 12: // Troll Male - Skin Color
                                        return 17821;
                                    case 13: // Troll Male - Skin Color
                                        return 17822;
                                    case 14: // Troll Male - Skin Color
                                        return 17823;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Male - Face
                                        return 17824;
                                    case 1: // Troll Male - Face
                                        return 17825;
                                    case 2: // Troll Male - Face
                                        return 17826;
                                    case 3: // Troll Male - Face
                                        return 17827;
                                    case 4: // Troll Male - Face
                                        return 17828;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Male - Hair Style
                                        return 17829;
                                    case 1: // Troll Male - Hair Style
                                        return 17830;
                                    case 2: // Troll Male - Hair Style
                                        return 17831;
                                    case 3: // Troll Male - Hair Style
                                        return 17832;
                                    case 4: // Troll Male - Hair Style
                                        return 17833;
                                    case 5: // Troll Male - Hair Style
                                        return 17834;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Male - Hair Color
                                        return 17835;
                                    case 1: // Troll Male - Hair Color
                                        return 17836;
                                    case 2: // Troll Male - Hair Color
                                        return 17837;
                                    case 3: // Troll Male - Hair Color
                                        return 17838;
                                    case 4: // Troll Male - Hair Color
                                        return 17839;
                                    case 5: // Troll Male - Hair Color
                                        return 17840;
                                    case 6: // Troll Male - Hair Color
                                        return 17841;
                                    case 7: // Troll Male - Hair Color
                                        return 17842;
                                    case 8: // Troll Male - Hair Color
                                        return 17843;
                                    case 9: // Troll Male - Hair Color
                                        return 17844;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Male - Tusks
                                        return 17845;
                                    case 1: // Troll Male - Tusks
                                        return 17846;
                                    case 2: // Troll Male - Tusks
                                        return 17847;
                                    case 3: // Troll Male - Tusks
                                        return 17848;
                                    case 4: // Troll Male - Tusks
                                        return 17849;
                                    case 5: // Troll Male - Tusks
                                        return 17850;
                                    case 6: // Troll Male - Tusks
                                        return 17851;
                                    case 7: // Troll Male - Tusks
                                        return 17852;
                                    case 8: // Troll Male - Tusks
                                        return 17853;
                                    case 9: // Troll Male - Tusks
                                        return 17854;
                                    case 10: // Troll Male - Tusks
                                        return 17855;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Female - Skin Color
                                        return 17856;
                                    case 1: // Troll Female - Skin Color
                                        return 17857;
                                    case 2: // Troll Female - Skin Color
                                        return 17858;
                                    case 3: // Troll Female - Skin Color
                                        return 17859;
                                    case 4: // Troll Female - Skin Color
                                        return 17860;
                                    case 5: // Troll Female - Skin Color
                                        return 17861;
                                    case 6: // Troll Female - Skin Color
                                        return 17862;
                                    case 7: // Troll Female - Skin Color
                                        return 17863;
                                    case 8: // Troll Female - Skin Color
                                        return 17864;
                                    case 9: // Troll Female - Skin Color
                                        return 17865;
                                    case 10: // Troll Female - Skin Color
                                        return 17866;
                                    case 11: // Troll Female - Skin Color
                                        return 17867;
                                    case 12: // Troll Female - Skin Color
                                        return 17868;
                                    case 13: // Troll Female - Skin Color
                                        return 17869;
                                    case 14: // Troll Female - Skin Color
                                        return 17870;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Female - Face
                                        return 17871;
                                    case 1: // Troll Female - Face
                                        return 17872;
                                    case 2: // Troll Female - Face
                                        return 17873;
                                    case 3: // Troll Female - Face
                                        return 17874;
                                    case 4: // Troll Female - Face
                                        return 17875;
                                    case 5: // Troll Female - Face
                                        return 17876;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Female - Hair Style
                                        return 17877;
                                    case 1: // Troll Female - Hair Style
                                        return 17878;
                                    case 2: // Troll Female - Hair Style
                                        return 17879;
                                    case 3: // Troll Female - Hair Style
                                        return 17880;
                                    case 4: // Troll Female - Hair Style
                                        return 17881;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Female - Hair Color
                                        return 17882;
                                    case 1: // Troll Female - Hair Color
                                        return 17883;
                                    case 2: // Troll Female - Hair Color
                                        return 17884;
                                    case 3: // Troll Female - Hair Color
                                        return 17885;
                                    case 4: // Troll Female - Hair Color
                                        return 17886;
                                    case 5: // Troll Female - Hair Color
                                        return 17887;
                                    case 6: // Troll Female - Hair Color
                                        return 17888;
                                    case 7: // Troll Female - Hair Color
                                        return 17889;
                                    case 8: // Troll Female - Hair Color
                                        return 17890;
                                    case 9: // Troll Female - Hair Color
                                        return 17891;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Troll Female - Tusks
                                        return 17892;
                                    case 1: // Troll Female - Tusks
                                        return 17893;
                                    case 2: // Troll Female - Tusks
                                        return 17894;
                                    case 3: // Troll Female - Tusks
                                        return 17895;
                                    case 4: // Troll Female - Tusks
                                        return 17896;
                                    case 5: // Troll Female - Tusks
                                        return 17897;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.BloodElf:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Male - Skin Color
                                        return 17906;
                                    case 1: // Blood Elf Male - Skin Color
                                        return 17907;
                                    case 2: // Blood Elf Male - Skin Color
                                        return 17908;
                                    case 3: // Blood Elf Male - Skin Color
                                        return 17909;
                                    case 4: // Blood Elf Male - Skin Color
                                        return 17910;
                                    case 5: // Blood Elf Male - Skin Color
                                        return 17911;
                                    case 6: // Blood Elf Male - Skin Color
                                        return 17912;
                                    case 7: // Blood Elf Male - Skin Color
                                        return 17913;
                                    case 8: // Blood Elf Male - Skin Color
                                        return 17914;
                                    case 9: // Blood Elf Male - Skin Color
                                        return 17915;
                                    case 10: // Blood Elf Male - Skin Color
                                        return 17916;
                                    case 11: // Blood Elf Male - Skin Color
                                        return 17917;
                                    case 12: // Blood Elf Male - Skin Color
                                        return 17918;
                                    case 13: // Blood Elf Male - Skin Color
                                        return 17919;
                                    case 14: // Blood Elf Male - Skin Color
                                        return 17920;
                                    case 15: // Blood Elf Male - Skin Color
                                        return 17921;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Male - Face
                                        return 17922;
                                    case 1: // Blood Elf Male - Face
                                        return 17923;
                                    case 2: // Blood Elf Male - Face
                                        return 17924;
                                    case 3: // Blood Elf Male - Face
                                        return 17925;
                                    case 4: // Blood Elf Male - Face
                                        return 17926;
                                    case 5: // Blood Elf Male - Face
                                        return 17927;
                                    case 6: // Blood Elf Male - Face
                                        return 17928;
                                    case 7: // Blood Elf Male - Face
                                        return 17929;
                                    case 8: // Blood Elf Male - Face
                                        return 17930;
                                    case 9: // Blood Elf Male - Face
                                        return 17931;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Male - Hair Style
                                        return 17932;
                                    case 1: // Blood Elf Male - Hair Style
                                        return 17933;
                                    case 2: // Blood Elf Male - Hair Style
                                        return 17934;
                                    case 3: // Blood Elf Male - Hair Style
                                        return 17935;
                                    case 4: // Blood Elf Male - Hair Style
                                        return 17936;
                                    case 5: // Blood Elf Male - Hair Style
                                        return 17937;
                                    case 6: // Blood Elf Male - Hair Style
                                        return 17938;
                                    case 7: // Blood Elf Male - Hair Style
                                        return 17939;
                                    case 8: // Blood Elf Male - Hair Style
                                        return 17940;
                                    case 9: // Blood Elf Male - Hair Style
                                        return 17941;
                                    case 10: // Blood Elf Male - Hair Style
                                        return 17942;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Male - Hair Color
                                        return 17943;
                                    case 1: // Blood Elf Male - Hair Color
                                        return 17944;
                                    case 2: // Blood Elf Male - Hair Color
                                        return 17945;
                                    case 3: // Blood Elf Male - Hair Color
                                        return 17946;
                                    case 4: // Blood Elf Male - Hair Color
                                        return 17947;
                                    case 5: // Blood Elf Male - Hair Color
                                        return 17948;
                                    case 6: // Blood Elf Male - Hair Color
                                        return 17949;
                                    case 7: // Blood Elf Male - Hair Color
                                        return 17950;
                                    case 8: // Blood Elf Male - Hair Color
                                        return 17951;
                                    case 9: // Blood Elf Male - Hair Color
                                        return 17952;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Male - Facial Hair
                                        return 17953;
                                    case 1: // Blood Elf Male - Facial Hair
                                        return 17954;
                                    case 2: // Blood Elf Male - Facial Hair
                                        return 17955;
                                    case 3: // Blood Elf Male - Facial Hair
                                        return 17956;
                                    case 4: // Blood Elf Male - Facial Hair
                                        return 17957;
                                    case 5: // Blood Elf Male - Facial Hair
                                        return 17958;
                                    case 6: // Blood Elf Male - Facial Hair
                                        return 17959;
                                    case 7: // Blood Elf Male - Facial Hair
                                        return 17960;
                                    case 8: // Blood Elf Male - Facial Hair
                                        return 17961;
                                    case 9: // Blood Elf Male - Facial Hair
                                        return 17962;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Female - Skin Color
                                        return 17963;
                                    case 1: // Blood Elf Female - Skin Color
                                        return 17964;
                                    case 2: // Blood Elf Female - Skin Color
                                        return 17965;
                                    case 3: // Blood Elf Female - Skin Color
                                        return 17966;
                                    case 4: // Blood Elf Female - Skin Color
                                        return 17967;
                                    case 5: // Blood Elf Female - Skin Color
                                        return 17968;
                                    case 6: // Blood Elf Female - Skin Color
                                        return 17969;
                                    case 7: // Blood Elf Female - Skin Color
                                        return 17970;
                                    case 8: // Blood Elf Female - Skin Color
                                        return 17971;
                                    case 9: // Blood Elf Female - Skin Color
                                        return 17972;
                                    case 10: // Blood Elf Female - Skin Color
                                        return 17973;
                                    case 11: // Blood Elf Female - Skin Color
                                        return 17974;
                                    case 12: // Blood Elf Female - Skin Color
                                        return 17975;
                                    case 13: // Blood Elf Female - Skin Color
                                        return 17976;
                                    case 14: // Blood Elf Female - Skin Color
                                        return 17977;
                                    case 15: // Blood Elf Female - Skin Color
                                        return 17978;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Female - Face
                                        return 17979;
                                    case 1: // Blood Elf Female - Face
                                        return 17980;
                                    case 2: // Blood Elf Female - Face
                                        return 17981;
                                    case 3: // Blood Elf Female - Face
                                        return 17982;
                                    case 4: // Blood Elf Female - Face
                                        return 17983;
                                    case 5: // Blood Elf Female - Face
                                        return 17984;
                                    case 6: // Blood Elf Female - Face
                                        return 17985;
                                    case 7: // Blood Elf Female - Face
                                        return 17986;
                                    case 8: // Blood Elf Female - Face
                                        return 17987;
                                    case 9: // Blood Elf Female - Face
                                        return 17988;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Female - Hair Style
                                        return 17989;
                                    case 1: // Blood Elf Female - Hair Style
                                        return 17990;
                                    case 2: // Blood Elf Female - Hair Style
                                        return 17991;
                                    case 3: // Blood Elf Female - Hair Style
                                        return 17992;
                                    case 4: // Blood Elf Female - Hair Style
                                        return 17993;
                                    case 5: // Blood Elf Female - Hair Style
                                        return 17994;
                                    case 6: // Blood Elf Female - Hair Style
                                        return 17995;
                                    case 7: // Blood Elf Female - Hair Style
                                        return 17996;
                                    case 8: // Blood Elf Female - Hair Style
                                        return 17997;
                                    case 9: // Blood Elf Female - Hair Style
                                        return 17998;
                                    case 10: // Blood Elf Female - Hair Style
                                        return 17999;
                                    case 11: // Blood Elf Female - Hair Style
                                        return 18000;
                                    case 12: // Blood Elf Female - Hair Style
                                        return 18001;
                                    case 13: // Blood Elf Female - Hair Style
                                        return 18002;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Female - Hair Color
                                        return 18004;
                                    case 1: // Blood Elf Female - Hair Color
                                        return 18005;
                                    case 2: // Blood Elf Female - Hair Color
                                        return 18006;
                                    case 3: // Blood Elf Female - Hair Color
                                        return 18007;
                                    case 4: // Blood Elf Female - Hair Color
                                        return 18008;
                                    case 5: // Blood Elf Female - Hair Color
                                        return 18009;
                                    case 6: // Blood Elf Female - Hair Color
                                        return 18010;
                                    case 7: // Blood Elf Female - Hair Color
                                        return 18011;
                                    case 8: // Blood Elf Female - Hair Color
                                        return 18012;
                                    case 9: // Blood Elf Female - Hair Color
                                        return 18013;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Blood Elf Female - Earrings
                                        return 18014;
                                    case 1: // Blood Elf Female - Earrings
                                        return 18015;
                                    case 2: // Blood Elf Female - Earrings
                                        return 18016;
                                    case 3: // Blood Elf Female - Earrings
                                        return 18017;
                                    case 4: // Blood Elf Female - Earrings
                                        return 18018;
                                    case 5: // Blood Elf Female - Earrings
                                        return 18019;
                                    case 6: // Blood Elf Female - Earrings
                                        return 18020;
                                    case 7: // Blood Elf Female - Earrings
                                        return 18021;
                                    case 8: // Blood Elf Female - Earrings
                                        return 18022;
                                    case 9: // Blood Elf Female - Earrings
                                        return 18023;
                                    case 10: // Blood Elf Female - Earrings
                                        return 18024;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
                case Race.Draenei:
                {
                    if (gender == Gender.Male)
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Male - Skin Color
                                        return 18025;
                                    case 1: // Draenei Male - Skin Color
                                        return 18026;
                                    case 2: // Draenei Male - Skin Color
                                        return 18027;
                                    case 3: // Draenei Male - Skin Color
                                        return 18028;
                                    case 4: // Draenei Male - Skin Color
                                        return 18029;
                                    case 5: // Draenei Male - Skin Color
                                        return 18030;
                                    case 6: // Draenei Male - Skin Color
                                        return 18031;
                                    case 7: // Draenei Male - Skin Color
                                        return 18032;
                                    case 8: // Draenei Male - Skin Color
                                        return 18033;
                                    case 9: // Draenei Male - Skin Color
                                        return 18034;
                                    case 10: // Draenei Male - Skin Color
                                        return 18035;
                                    case 11: // Draenei Male - Skin Color
                                        return 18036;
                                    case 12: // Draenei Male - Skin Color
                                        return 18037;
                                    case 13: // Draenei Male - Skin Color
                                        return 18038;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Male - Face
                                        return 18039;
                                    case 1: // Draenei Male - Face
                                        return 18040;
                                    case 2: // Draenei Male - Face
                                        return 18041;
                                    case 3: // Draenei Male - Face
                                        return 18042;
                                    case 4: // Draenei Male - Face
                                        return 18043;
                                    case 5: // Draenei Male - Face
                                        return 18044;
                                    case 6: // Draenei Male - Face
                                        return 18045;
                                    case 7: // Draenei Male - Face
                                        return 18046;
                                    case 8: // Draenei Male - Face
                                        return 18047;
                                    case 9: // Draenei Male - Face
                                        return 18048;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Male - Hair Style
                                        return 18049;
                                    case 1: // Draenei Male - Hair Style
                                        return 18050;
                                    case 2: // Draenei Male - Hair Style
                                        return 18051;
                                    case 3: // Draenei Male - Hair Style
                                        return 18052;
                                    case 4: // Draenei Male - Hair Style
                                        return 18053;
                                    case 5: // Draenei Male - Hair Style
                                        return 18054;
                                    case 6: // Draenei Male - Hair Style
                                        return 18055;
                                    case 7: // Draenei Male - Hair Style
                                        return 18056;
                                    case 8: // Draenei Male - Hair Style
                                        return 18057;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Male - Hair Color
                                        return 18058;
                                    case 1: // Draenei Male - Hair Color
                                        return 18059;
                                    case 2: // Draenei Male - Hair Color
                                        return 18060;
                                    case 3: // Draenei Male - Hair Color
                                        return 18061;
                                    case 4: // Draenei Male - Hair Color
                                        return 18062;
                                    case 5: // Draenei Male - Hair Color
                                        return 18063;
                                    case 6: // Draenei Male - Hair Color
                                        return 18064;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Male - Facial Hair
                                        return 18065;
                                    case 1: // Draenei Male - Facial Hair
                                        return 18066;
                                    case 2: // Draenei Male - Facial Hair
                                        return 18067;
                                    case 3: // Draenei Male - Facial Hair
                                        return 18068;
                                    case 4: // Draenei Male - Facial Hair
                                        return 18069;
                                    case 5: // Draenei Male - Facial Hair
                                        return 18070;
                                    case 6: // Draenei Male - Facial Hair
                                        return 18071;
                                    case 7: // Draenei Male - Facial Hair
                                        return 18072;
                                }
                                return 0;
                            }
                        }
                    }
                    else
                    {
                        switch (option)
                        {
                            case LegacyCustomizationOption.Skin:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Female - Skin Color
                                        return 18073;
                                    case 1: // Draenei Female - Skin Color
                                        return 18074;
                                    case 2: // Draenei Female - Skin Color
                                        return 18075;
                                    case 3: // Draenei Female - Skin Color
                                        return 18076;
                                    case 4: // Draenei Female - Skin Color
                                        return 18077;
                                    case 5: // Draenei Female - Skin Color
                                        return 18078;
                                    case 6: // Draenei Female - Skin Color
                                        return 18079;
                                    case 7: // Draenei Female - Skin Color
                                        return 18080;
                                    case 8: // Draenei Female - Skin Color
                                        return 18081;
                                    case 9: // Draenei Female - Skin Color
                                        return 18082;
                                    case 10: // Draenei Female - Skin Color
                                        return 18083;
                                    case 11: // Draenei Female - Skin Color
                                        return 18084;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.Face:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Female - Face
                                        return 18085;
                                    case 1: // Draenei Female - Face
                                        return 18086;
                                    case 2: // Draenei Female - Face
                                        return 18087;
                                    case 3: // Draenei Female - Face
                                        return 18088;
                                    case 4: // Draenei Female - Face
                                        return 18089;
                                    case 5: // Draenei Female - Face
                                        return 18090;
                                    case 6: // Draenei Female - Face
                                        return 18091;
                                    case 7: // Draenei Female - Face
                                        return 18092;
                                    case 8: // Draenei Female - Face
                                        return 18093;
                                    case 9: // Draenei Female - Face
                                        return 18094;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairStyle:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Female - Hair Style
                                        return 18095;
                                    case 1: // Draenei Female - Hair Style
                                        return 18096;
                                    case 2: // Draenei Female - Hair Style
                                        return 18097;
                                    case 3: // Draenei Female - Hair Style
                                        return 18098;
                                    case 4: // Draenei Female - Hair Style
                                        return 18099;
                                    case 5: // Draenei Female - Hair Style
                                        return 18100;
                                    case 6: // Draenei Female - Hair Style
                                        return 18101;
                                    case 7: // Draenei Female - Hair Style
                                        return 18102;
                                    case 8: // Draenei Female - Hair Style
                                        return 18103;
                                    case 9: // Draenei Female - Hair Style
                                        return 18104;
                                    case 10: // Draenei Female - Hair Style
                                        return 18105;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.HairColor:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Female - Hair Color
                                        return 18106;
                                    case 1: // Draenei Female - Hair Color
                                        return 18107;
                                    case 2: // Draenei Female - Hair Color
                                        return 18108;
                                    case 3: // Draenei Female - Hair Color
                                        return 18109;
                                    case 4: // Draenei Female - Hair Color
                                        return 18110;
                                    case 5: // Draenei Female - Hair Color
                                        return 18111;
                                    case 6: // Draenei Female - Hair Color
                                        return 18112;
                                }
                                return 0;
                            }
                            case LegacyCustomizationOption.FacialHair:
                            {
                                switch (value)
                                {
                                    case 0: // Draenei Female - Horn Style
                                        return 18113;
                                    case 1: // Draenei Female - Horn Style
                                        return 18114;
                                    case 2: // Draenei Female - Horn Style
                                        return 18115;
                                    case 3: // Draenei Female - Horn Style
                                        return 18116;
                                    case 4: // Draenei Female - Horn Style
                                        return 18117;
                                    case 5: // Draenei Female - Horn Style
                                        return 18118;
                                    case 6: // Draenei Female - Horn Style
                                        return 18119;
                                }
                                return 0;
                            }
                        }
                    }
                    return 0;
                }
            }
            return 0;
        }

        public static Array<ChrCustomizationChoice> ConvertLegacyCustomizationsToModern(Race raceId, Gender gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair)
        {
            var customizations = new Array<ChrCustomizationChoice>(5);
            customizations[0] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.Skin), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.Skin, skin));
            customizations[1] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.Face), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.Face, face));
            customizations[2] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.HairStyle), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.HairStyle, hairStyle));
            customizations[3] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.HairColor), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.HairColor, hairColor));
            customizations[4] = new ChrCustomizationChoice(GetModernCustomizationOption(raceId, gender, LegacyCustomizationOption.FacialHair), GetModernCustomizationChoice(raceId, gender, LegacyCustomizationOption.FacialHair, facialHair));
            return customizations;
        }
    }
}
