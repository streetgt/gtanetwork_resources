using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

public class PlayerIds : Script
{
    private List<Client> players = new List<Client>();
    private Dictionary<Client, NetHandle> playerLabels = new Dictionary<Client, NetHandle>();

    public PlayerIds()
    {
        API.onResourceStart += onResourceStart;
        API.onPlayerConnected += onPlayerConnect;
        API.onPlayerDisconnected += onPlayerDisconnect;
    }

    public void onResourceStart()
    {
        for (var i = 0; i < API.getSetting<int>("max_players"); i++)
        {
            players.Add(null);
        }
    }

    public void onPlayerConnect(Client player)
    {
        int index = getFreeId();
        if (index != -1)
        {
            players[index] = player;
        }

        if (API.getSetting<bool>("player_tags"))
        {
            var label = API.createTextLabel(string.Format("ID:{0}", index), API.getEntityPosition(player.handle), 50f, 0.4f, true);
            API.attachEntityToEntity(label, player.handle, null, new Vector3(0, 0, 1f), new Vector3());
            playerLabels.Add(player, label);
        }
    }

    public void onPlayerDisconnect(Client player, string reason)
    {
        int index = players.IndexOf(player);
        if (index != -1)
        {
            players[players.IndexOf(player)] = null;
        }

        if (API.getSetting<bool>("player_tags"))
        {
            if (playerLabels.ContainsKey(player))
            {
                API.deleteEntity(playerLabels[player]);
                playerLabels.Remove(player);
            }
        }

    }

    [Command("id", "~y~USAGE: ~w~/id [id/PartOfName]")]
    public void GetPlayerId(Client sender, string idOrName)
    {
        Client target = findPlayer(sender, idOrName);
        if (target != null)
        {
            API.sendChatMessageToPlayer(sender, string.Format("~y~Player found: ~w~{0} - ID:{1}", target.name, getIdFromClient(target)));
        }
    }

    /// <summary>
    /// Find a player given a partial name or a ID
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="idOrName"></param>
    /// <returns>null or Client</returns>
    public Client findPlayer(Client sender, string idOrName)
    {
        int id;

        // If idOrName is Numeric
        if (int.TryParse(idOrName, out id))
        {
            return getClientFromId(sender, id);
        }

        Client returnClient = null;
        int playersCount = 0;
        foreach (var player in players)
        {
            // Skip if list element is null
            if (player == null) continue;


            // If player name contains provided name
            if (player.name.ToLower().Contains(idOrName.ToLower()))
            {
                // If player name == provided name
                if ((player.name.Equals(idOrName, StringComparison.OrdinalIgnoreCase)))
                {
                    return player;
                }
                else
                {
                    playersCount++;
                    returnClient = player;
                }
            } 
        }


        if (playersCount != 1)
        {
            if (playersCount > 0)
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Multiple users found.");
            }
            else
            {
                API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Player name not found.");
            }
            return null;
        }

        return returnClient;
    }

    /// <summary>
    /// Gets the Client from a give ID
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="id"></param>
    /// <returns>null or Client</returns>
    public Client getClientFromId(Client sender, int id)
    {
        if (players[id] == null)
        {
            API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Player ID not found.");
            return null;
        }

        return players[id];
    }

    /// <summary>
    /// Gets the ID from Client
    /// </summary>
    /// <param name="target"></param>
    /// <returns>id or -1 in case of don't find the player</returns>
    public int getIdFromClient(Client target)
    {
        return players.IndexOf(target);;
    }

    /// <summary>
    /// Gets the first null element in the player list
    /// </summary>
    /// 
    /// <returns>index of the first element null or -1 in case of don't find any null element</returns>
    private int getFreeId()
    {
        foreach (var item in players)
        {
            if (item == null)
            {
                return this.players.IndexOf(item);
            }
        }

        return -1;
    }
}
