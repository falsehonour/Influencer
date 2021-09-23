using Mirror;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace HashtagChampion
{
    [Serializable]
    public class MatchData
    {

        [Flags]
        public enum StateFlags
        {
            Public = 1, Lobby = 2
        }

        public struct Description
        {
            public byte playerCount;
            public byte maxPlayerCount;
            public string id;
            public MatchData.StateFlags states;
            public uint hostNetId;
        }

        //TODO: Add settings
        public string id;
        public List<Player> players;
        public Player host;
        public StateFlags states;
        public MatchManager manager;
        public MatchSettings settings;

        public MatchData(string id, Player host, MatchManager manager, MatchSettings settings)
        {
            this.id = id;
            players = new List<Player>();
            //players.Add(host);
            this.host = host;
            states = StateFlags.Lobby;
            this.manager = manager;
            this.settings = settings;
        }

        public MatchData() { }

        public Description GetDescription()
        {
            return new Description
            {
                playerCount = (byte)players.Count,
                maxPlayerCount = this.settings.GetMaxPlayerCount(),
                id = this.id,
                states = this.states,
                hostNetId = host.netId
            };
        }
    }

}
//
public static class StringExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] input = Encoding.Default.GetBytes(id);
        byte[] hash = provider.ComputeHash(input);
        return new Guid(hash);
    }
}