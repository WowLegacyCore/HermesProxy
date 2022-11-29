﻿/*
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

using Framework.Networking;
using System.Net.Sockets;
using Framework.Logging;

namespace HermesProxy.World.Server
{
    public class WorldSocketManager : SocketManager<WorldSocket>
    {
        public override bool StartNetwork(string bindIp, int realmPort, int threadCount = 1)
        {
            _tcpNoDelay = true;

            // -1 means use default
            _socketSendBufferSize = -1;

            if (!base.StartNetwork(bindIp, realmPort, threadCount))
                return false;

            _instanceAcceptor = new AsyncAcceptor();
            if (!_instanceAcceptor.Start(bindIp, Framework.Settings.InstancePort))
            {
                Log.Print(LogType.Error, "StartNetwork failed to start instance AsyncAcceptor");
                return false;
            }

            _instanceAcceptor.AsyncAcceptSocket(OnSocketOpen);

            return true;
        }

        public override void StopNetwork()
        {
            _instanceAcceptor.Close();
            base.StopNetwork();

            _instanceAcceptor = null;
        }

        public override void OnSocketOpen(Socket sock)
        {
            Log.Print(LogType.Network, $"Instance socket open.");

            // set some options here
            try
            {
                if (_socketSendBufferSize >= 0)
                    sock.SendBufferSize = _socketSendBufferSize;

                // Set TCP_NODELAY.
                sock.NoDelay = _tcpNoDelay;
            }
            catch (SocketException ex)
            {
                Log.Print(LogType.Error, ex.ToString());
                return;
            }

            base.OnSocketOpen(sock);
        }

        AsyncAcceptor _instanceAcceptor;
        int _socketSendBufferSize;
        bool _tcpNoDelay;
    }
}
