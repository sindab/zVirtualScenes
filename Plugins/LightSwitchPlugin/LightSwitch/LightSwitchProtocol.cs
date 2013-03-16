﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightSwitchPlugin.LightSwitch
{
    internal static class LightSwitchProtocol
    {
        internal static void DecodeIncoming(string incomingMessage, ILightSwitchChannel protocol)
        {
            if (incomingMessage.Length == 0)
                return;

            string cmd = incomingMessage.TrimEnd(Environment.NewLine.ToCharArray()).ToUpper();
            string[] commandSegments = cmd.Split('~');

            if (commandSegments[0].Equals("IPHONE"))
                protocol.onIphone();
            else if (commandSegments[0].Equals("PASSWORD") && commandSegments.Length == 2)
                protocol.onPassword(commandSegments[1]);
            else if (commandSegments[0].Equals("VERSION"))
                protocol.onVersion();
            else if (commandSegments[0].Equals("SERVER"))
                protocol.onServer();
            else if (commandSegments[0].Equals("TERMINATE"))
                protocol.onTerminate();
            else if (commandSegments[0].Equals("ALIST"))  //DEVICES, SCENES AND ZONES.
                protocol.onAList();
            else if (commandSegments[0].Equals("LIST")) //DEVICES
                protocol.onList();
            else if (commandSegments[0].Equals("SLIST"))  //SCENES
                protocol.onSList();
            else if (commandSegments[0].Equals("ZLIST")) //ZONES
                protocol.onZList();
            else if (commandSegments[0].Equals("DEVICE") && commandSegments.Length == 4)
                protocol.onDevice(commandSegments[1], commandSegments[2], commandSegments[3]);
            else if (commandSegments[0].Equals("SCENE") && commandSegments.Length == 2)
                protocol.onScene(commandSegments[1]);
            else if (commandSegments[0].Equals("ZONE") && commandSegments.Length == 3)
                protocol.onZone(commandSegments[1], commandSegments[2]);
            else if (commandSegments[0].Equals("THERMMODE") && commandSegments.Length == 4)
                protocol.onThermMode(commandSegments[1], commandSegments[2], commandSegments[3]);
            else if (commandSegments[0].Equals("THERMTEMP") && commandSegments.Length == 5)
                protocol.onThermTemp(commandSegments[1], commandSegments[2], commandSegments[3], commandSegments[4]);
        }

        internal static LightSwitchCommand CreateMsgCmd(string msg)
        {
            return new LightSwitchCommand(string.Format("MSG~{0}{1}", msg, Environment.NewLine));
        }

        internal static LightSwitchCommand CreateErrorMsgCmd(string msg)
        {
            return new LightSwitchCommand(string.Format("ERR~{0}{1}", msg, Environment.NewLine));
        }

        internal static LightSwitchCommand CreateMsgCmdFormat(string msg, params object[] parameters)
        {
            return LightSwitchProtocol.CreateMsgCmd(string.Format(msg, parameters));
        }

        internal static LightSwitchCommand CreateInfoCmd(string msg)
        {
            return new LightSwitchCommand(msg);
        }

        internal static LightSwitchCommand CreateInfoCmdFormat(string msg, params object[] parameters)
        {
            return LightSwitchProtocol.CreateInfoCmd(string.Format(msg, parameters));
        }

        internal static LightSwitchCommand CreateCookieCmd(string nonce)
        {
            return new LightSwitchCommand(string.Format("COOKIE~{0}{1}", nonce, Environment.NewLine));
        }

        internal static LightSwitchCommand CreateVersionCmd(string AppNameAndVersion)
        {
            return new LightSwitchCommand(string.Format("VER~{0}{1}", AppNameAndVersion, Environment.NewLine));
        }

        internal static LightSwitchCommand CreateDeviceCmd(string name, string id, string level, DeviceTypes type)
        {
            return new LightSwitchCommand(string.Format("DEVICE~{0}~{1}~{2}~{3}{4}",
                name,
                id,
                level,
                type.ToString(),
                Environment.NewLine));
        }

        internal static LightSwitchCommand CreateUpdateCmd(string name, string id, string level, DeviceTypes type)
        {
            return new LightSwitchCommand(string.Format("UPDATE~{0}~{1}~{2}~{3}{4}",
                name,
                id,
                level,
                type.ToString(),
                Environment.NewLine));
        }

        internal static LightSwitchCommand CreateSceneCmd(string name, string id)
        {
            return new LightSwitchCommand(string.Format("SCENE~{0}~{1}~{2}",
                name,
                id,
                Environment.NewLine));
        }

        internal static LightSwitchCommand CreateZoneCmd(string name, string id)
        {
            return new LightSwitchCommand(string.Format("ZONE~{0}~{1}~{2}",
                name,
                id,
                Environment.NewLine));
        }

        internal static LightSwitchCommand CreateEndListCmd()
        {
            return new LightSwitchCommand(string.Format("ENDLIST{0}", Environment.NewLine));
        }

        public enum DeviceTypes
        {
            BinarySwitch,
            MultiLevelSwitch,
            Thermostat,
            Sensor,
            WindowCovering,
            Status
        }
    }

    public sealed class LightSwitchCommand
    {
        public string RawCommand { get; private set; }

        public LightSwitchCommand(string rawCommand)
        {
            RawCommand = rawCommand;
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(this.RawCommand);
        }
    }

    
}
