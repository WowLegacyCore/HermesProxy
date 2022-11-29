// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Framework.Constants;
using Framework.Logging;
using Framework.Web;
using System;
using System.Net;

namespace BNetServer
{
    public class LoginServiceManager : Singleton<LoginServiceManager>
    {
        readonly FormInputs formInputs;
        IPEndPoint externalAddress;
        IPEndPoint localAddress;

        LoginServiceManager()
        {
            formInputs = new FormInputs();
        }

        public void Initialize()
        {
            int port = Framework.Settings.RestPort;
            if (port < 0 || port > 0xFFFF)
            {
                Log.Print(LogType.Error, $"Specified login service port ({port}) out of allowed range (1-65535), defaulting to 8081");
                port = 8081;
            }

            string configuredAddress = Framework.Settings.ExternalAddress;
            IPAddress address;
            if (!IPAddress.TryParse(configuredAddress, out address))
            {
                Log.Print(LogType.Error, $"Could not resolve LoginREST.ExternalAddress {configuredAddress}");
                return;
            }
            externalAddress = new IPEndPoint(address, port);

            configuredAddress = "127.0.0.1";
            if (!IPAddress.TryParse(configuredAddress, out address))
            {
                Log.Print(LogType.Error, $"Could not resolve local address.");
                return;
            }
            localAddress = new IPEndPoint(address, port);

            // set up form inputs
            formInputs.Type = "LOGIN_FORM";

            var input = new FormInput();
            input.Id = "account_name";
            input.Type = "text";
            input.Label = "E-mail";
            input.MaxLength = 320;
            formInputs.Inputs.Add(input);

            input = new FormInput();
            input.Id = "password";
            input.Type = "password";
            input.Label = "Password";
            input.MaxLength = 16;
            formInputs.Inputs.Add(input);

            input = new FormInput();
            input.Id = "log_in_submit";
            input.Type = "submit";
            input.Label = "Log In";
            formInputs.Inputs.Add(input);
        }

        public IPEndPoint GetAddressForClient(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
                return localAddress;

            return externalAddress;
        }

        public FormInputs GetFormInput()
        {
            return formInputs;
        }
    }

    public enum ServiceRequirement
    {
        Unauthorized,
        LoggedIn,
        Always,
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ServiceAttribute : Attribute
    {
        public ServiceRequirement Requirement { get; set; }
        public OriginalHash ServiceHash { get; set; }
        public uint MethodId { get; set; }

        public ServiceAttribute(ServiceRequirement requirement, OriginalHash serviceHash, uint methodId)
        {
            Requirement = requirement;
            ServiceHash = serviceHash;
            MethodId = methodId;
        }
    }
}
