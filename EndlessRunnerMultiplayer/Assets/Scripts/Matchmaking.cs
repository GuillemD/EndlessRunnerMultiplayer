using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class Matchmaking : NetworkBehaviour
{
    public Text newMatchName;
    float time = 9999.0f;

    public RectTransform listOfMatches;
    public GameObject matchEntryPrefab;


    // Update is called once per frame
    private void Update()
    {
        time += Time.deltaTime;

        if (time > 10.0f)
        {
            time = 0.0f;

            NetworkManager.singleton.StartMatchMaker();
            NetworkManager.singleton.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);


        }
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            // Destroy previous list
            int childCount = listOfMatches.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Destroy(listOfMatches.GetChild(i).gameObject);
            }

            //Insert new list of matches

            for (int i = 0; i < matches.Count; ++i)
            {
                MatchInfoSnapshot match = matches[i];
                string matchName = match.name;
                UnityEngine.Networking.Types.NetworkID networkId = match.networkId;

                GameObject matchEntry = GameObject.Instantiate(matchEntryPrefab, listOfMatches);
                RectTransform rect = matchEntry.GetComponent<RectTransform>();

                rect.localPosition = new Vector3(10.0f, -(float)1 * 50.0f, 0.0f);

                Text text = matchEntry.GetComponentInChildren<Text>();
                text.text = "Match: " + matchName;
                Button button = matchEntry.GetComponentInChildren<Button>();
                button.onClick.AddListener(delegate { OnJoinMatchClicked(networkId); });
            }
        }
        else
        {
            Debug.Log("OnMatchList failed");
        }
    }

    public void OnCreateMatchClicked()
    {
        Debug.Log("OnCreateMatchClicked" + newMatchName.text);
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.CreateMatch(newMatchName.text, 4, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            NetworkManager.singleton.StartHost(matchInfo);

        }
        else
        {
            Debug.Log("OnMatchCreate failed");
        }

    }

    public void OnJoinMatchClicked(UnityEngine.Networking.Types.NetworkID networkId)
    {
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.JoinMatch(networkId, "", "", "", 0, 0, OnMatchJoin);

    }

    public void OnMatchJoin(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            NetworkManager.singleton.StartClient(matchInfo);
        }
        else
        {
            Debug.Log("OnMatchJoin failed");
        }
    }

   
}
