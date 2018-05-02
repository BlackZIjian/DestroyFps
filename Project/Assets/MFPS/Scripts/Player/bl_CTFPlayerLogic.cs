using UnityEngine;

public class bl_CTFPlayerLogic : bl_PhotonHelper {

    public Team m_PlayerTeam = Team.All;
    public Transform FlagPosition;
    public bool isLocal;

    void Start()
    {
        m_PlayerTeam = GetTeamEnum(photonView.owner);
        isLocal = photonView.isMine;
    }
}
