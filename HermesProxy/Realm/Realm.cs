/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.Constants;
using Framework.Realm;
using System;
using System.Net;
using System.Net.Sockets;

public class Realm : IEquatable<Realm>
{
    public void SetName(string name)
    {
        Name = name;
        NormalizedName = name;
        NormalizedName = NormalizedName.Replace(" ", "");
    }

    public IPEndPoint GetAddressForClient(IPAddress clientAddr)
    {
        IPAddress realmIp;

        if (IPAddress.IsLoopback(clientAddr))
            realmIp = IPAddress.Parse("127.0.0.1");
        else
            realmIp = IPAddress.Parse(Framework.Settings.ExternalAddress);

        IPEndPoint endpoint = new IPEndPoint(realmIp, Framework.Settings.RealmPort);

        // Return external IP
        return endpoint;
    }

    public uint GetConfigId()
    {
        return ConfigIdByType[Type];
    }

    uint[] ConfigIdByType =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14
    };

    public override bool Equals(object obj)
    {
        return obj != null && obj is Realm && Equals((Realm)obj);
    }

    public bool Equals(Realm other)
    {
        return other.ExternalAddress.Equals(ExternalAddress)
            && other.Port == Port
            && other.Name == Name
            && other.Type == Type
            && other.Flags == Flags
            && other.Timezone == Timezone
            && other.PopulationLevel == PopulationLevel;
    }

    public override int GetHashCode()
    {
        return new { ExternalAddress, Port, Name, Type, Flags, Timezone, PopulationLevel }.GetHashCode();
    }

    public RealmId Id;
    public uint Build;
    public IPAddress ExternalAddress;
    public ushort Port;
    public string Name;
    public string NormalizedName;
    public byte Type;
    public RealmFlags Flags;
    public byte Timezone;
    public float PopulationLevel;
}

