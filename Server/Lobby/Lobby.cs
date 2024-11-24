using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Lobby
{
    public class Lobby : JobSerializer
    {
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public void EnterLobby()
        {

        }
    }
}