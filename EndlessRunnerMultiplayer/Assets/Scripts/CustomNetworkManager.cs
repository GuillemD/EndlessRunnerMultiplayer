using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;


public class MsgTypes
{
    public const short PlayerPrefabSelect = MsgType.Highest + 1;
    public class PlayerPrefabMsg : MessageBase
    {
        public short controllerId;
        public short prefabIndex;
    }
}
public class CustomNetworkManager : NetworkManager
{
   
    public short playerPrefabIndex;

    public string[] playerNames = new string[] { "Boy", "Girl", "Robot" };

  
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler(MsgTypes.PlayerPrefabSelect, OnPrefabResponse);
        base.OnStartServer();

    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        client.RegisterHandler(MsgTypes.PlayerPrefabSelect, OnPrefabRequest);
        base.OnClientConnect(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
        msg.controllerId = playerControllerId;
        NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefabSelect, msg);
    }

    private void OnPrefabRequest(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        msg.prefabIndex = playerPrefabIndex;
        client.Send(MsgTypes.PlayerPrefabSelect, msg);
    }

    private void OnPrefabResponse(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        playerPrefab = spawnPrefabs[msg.prefabIndex];
        base.OnServerAddPlayer(netMsg.conn, msg.controllerId);
    }

    private void OnGUI()
    {
        if (!isNetworkActive)
        {
            playerPrefabIndex = (short)GUI.SelectionGrid(new Rect(Screen.width - 200, 10, 200, 50), playerPrefabIndex, playerNames, 3);
        }
    }

    //CHANGE PLAYER PREFAB

    public void ChangePlayerPrefab(PlayerController currentPlayer, int id)
    {
        GameObject newPlayer = Instantiate(spawnPrefabs[id], currentPlayer.gameObject.transform.position, currentPlayer.gameObject.transform.rotation);

        NetworkServer.Destroy(currentPlayer.gameObject);

        NetworkServer.ReplacePlayerForConnection(currentPlayer.connectionToClient, newPlayer, 0);
    }

    //Add level section

    public void SpawnLevelSection(int id, Vector3 spawnPos)
    {
        GameObject newLevelSection = Instantiate(spawnPrefabs[id], spawnPos, Quaternion.identity);

        NetworkServer.Spawn(newLevelSection);
    }

   //Add obstacle

    public void AddObstacleForEnemy(int id, GameObject enemyPlayer)
    {
        int rand = 0;

        rand = Random.Range(0, 3);
        if(enemyPlayer != null)
        {
            switch(rand)
            {
                case 0:
                    {
                        GameObject obstacle = Instantiate(spawnPrefabs[id], (enemyPlayer.gameObject.transform.position + (new Vector3(0, 1, 10))), Quaternion.identity);
                        NetworkServer.Spawn(obstacle);
                        break;
                    }
                case 1:
                    {
                        GameObject obstacle = Instantiate(spawnPrefabs[id], (enemyPlayer.gameObject.transform.position + (new Vector3(1.5f, 1, 10))), Quaternion.identity);
                        NetworkServer.Spawn(obstacle);
                        break;
                    }
                case 2:
                    {
                        GameObject obstacle = Instantiate(spawnPrefabs[id], (enemyPlayer.gameObject.transform.position + (new Vector3(-1.5f, 1, 10))), Quaternion.identity);
                        NetworkServer.Spawn(obstacle);
                        break;
                    }
                default:
                    {
                        GameObject obstacle = Instantiate(spawnPrefabs[id], (enemyPlayer.gameObject.transform.position + (new Vector3(0, 1, 10))), Quaternion.identity);
                        NetworkServer.Spawn(obstacle);
                        break;
                    }
            }   
        }
    }

   
}

